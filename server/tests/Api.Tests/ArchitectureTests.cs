using System.Reflection;
using EnBref.Api.Features.GenerateRecap;
using EnBref.Infrastructure.Collection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EnBref.Api.Tests;

/// <summary>Fitness functions : les décisions d'architecture tenues par un test plutôt que par la revue.</summary>
public class ArchitectureTests
{
    private static readonly Assembly ApiAssembly = typeof(GenerateRecapHandler).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(IFeedReader).Assembly;

    private const string FeaturesNamespace = "EnBref.Api.Features.";

    // Dépendances autorisées entre slices. Toute nouvelle entrée est une décision visible en revue.
    private static readonly HashSet<(string From, string To)> AllowedSliceDependencies =
    [
        ("GenerateRecap", "CollectHeadlines"),
    ];

    [Test]
    public async Task Infrastructure_never_references_Api()
    {
        var references = InfrastructureAssembly.GetReferencedAssemblies().Select(reference => reference.Name);

        await Assert.That(references).DoesNotContain(ApiAssembly.GetName().Name);
    }

    [Test]
    [Arguments("MediatR")]
    [Arguments("MassTransit")]
    [Arguments("Wolverine")]
    public async Task No_in_memory_bus(string forbiddenAssemblyPrefix)
    {
        var references = new[] { ApiAssembly, InfrastructureAssembly }
            .SelectMany(assembly => assembly.GetReferencedAssemblies())
            .Select(reference => reference.Name ?? "");

        await Assert.That(references.Where(name => name.StartsWith(forbiddenAssemblyPrefix, StringComparison.Ordinal)))
            .IsEmpty();
    }

    [Test]
    public async Task Slices_only_depend_on_allowed_slices()
    {
        var violations = ApiAssembly.GetTypes()
            .Select(type => (Type: type, Slice: SliceOf(type)))
            .Where(item => item.Slice is not null)
            .SelectMany(item => ReferencedTypes(item.Type)
                .Select(SliceOf)
                .Where(target => target is not null && target != item.Slice
                                 && !AllowedSliceDependencies.Contains((item.Slice!, target)))
                .Select(target => $"{item.Type.FullName} → {target}"))
            .Distinct()
            .ToList();

        await Assert.That(violations).IsEmpty();
    }

    [Test]
    public async Task No_GET_route_triggers_a_generation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.Contains("generations", StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        var methods = endpoints
            .SelectMany(endpoint => endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? ["*"])
            .ToList();

        await Assert.That(endpoints).IsNotEmpty();
        await Assert.That(methods).IsEquivalentTo(["POST"]);
    }

    private static string? SliceOf(Type type)
    {
        var ns = type.Namespace;
        if (ns is null || !ns.StartsWith(FeaturesNamespace, StringComparison.Ordinal))
        {
            return null;
        }

        return ns[FeaturesNamespace.Length..].Split('.')[0];
    }

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                 | BindingFlags.Static | BindingFlags.DeclaredOnly;

        var types = type.GetFields(all).Select(field => field.FieldType)
            .Concat(type.GetProperties(all).Select(property => property.PropertyType))
            .Concat(type.GetConstructors(all).SelectMany(ctor => ctor.GetParameters()).Select(parameter => parameter.ParameterType))
            .Concat(type.GetMethods(all).SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .Append(method.ReturnType)));

        return types.SelectMany(Unwrap);
    }

    // Task<CollectionResult>, IReadOnlyList<FeedResult>… : on remonte aux arguments génériques.
    private static IEnumerable<Type> Unwrap(Type type)
    {
        if (type.HasElementType)
        {
            return Unwrap(type.GetElementType()!);
        }

        return type.IsGenericType
            ? type.GetGenericArguments().SelectMany(Unwrap).Prepend(type)
            : [type];
    }
}
