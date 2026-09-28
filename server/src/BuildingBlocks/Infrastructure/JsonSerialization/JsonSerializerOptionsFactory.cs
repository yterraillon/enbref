using System.Text.Json;

namespace Infrastructure.JsonSerialization;

public static class JsonSerializerOptionsFactory
{
    public static JsonSerializerOptions CreateCamelCaseOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };    
    
    public static JsonSerializerOptions CreateCamelCaseAndWriteIntendedOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
}