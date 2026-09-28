using System.Text;
using Infrastructure.JsonSerialization;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure.HttpClients;

public static class HttpClientExtensions
{
    public static async Task<TResult?> GetRequest<TResult>(this HttpClient client, string relativeUri)
    {
        try
        {
            var response = await client.GetAsync($"{relativeUri}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            
            return JsonConvert.DeserializeObject<TResult>(result);
        }
 
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
 
        return default;
    }
    
    public static async Task<TResult?> PostRequest<TArgument, TResult>(this HttpClient client, string relativeUri,
        TArgument request)
    {
        try
        {
            var jsonCommand = JsonSerializer.Serialize(request, JsonSerializerOptionsFactory.CreateCamelCaseAndWriteIntendedOptions());
            var content = new StringContent(jsonCommand, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(relativeUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
 
            return JsonConvert.DeserializeObject<TResult>(result);
        }
 
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
 
        return default;
    } 
    
    public static async Task<TResult?> PutRequest<TArgument, TResult>(this HttpClient client, string relativeUri,
        TArgument request)
    {
        try
        {
            var jsonCommand = JsonSerializer.Serialize(request, JsonSerializerOptionsFactory.CreateCamelCaseAndWriteIntendedOptions());
            var content = new StringContent(jsonCommand, Encoding.UTF8, "application/json");
            
            var response = await client.PutAsync(relativeUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
            }
            
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
 
            return JsonConvert.DeserializeObject<TResult>(result);
        }
 
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
 
        return default;
    } 
    
    public static void AddBearerTokenHeader(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }
}