using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JwtDecoder;

public class JwtTokenDecoder
{
    public string? TryDecode(string token)
    {
        try
        {
            var parts = token.Trim().Split('.', 3);

            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
            {
                return null;
            }
            
            var headers = DecodeTokenPart(parts[0]);
            var claims = DecodeTokenPart(parts[1]);
            
            JsonObject jsonObject = new();

            foreach (var pair in headers.Concat(claims))
            {
                jsonObject[pair.key] = pair.value?.DeepClone();
            }

            return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private IReadOnlyCollection<(string key, JsonNode? value)> DecodeTokenPart(string part)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(part);
        byte[] decoded = Base64Url.DecodeFromUtf8(bytes);
        string json = Encoding.UTF8.GetString(decoded);
        
        JsonObject? jsonObj = JsonNode.Parse(json)?.AsObject();
        if (jsonObj is null)
        {
            return Array.Empty<(string, JsonNode?)>();
        }

        return jsonObj
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToArray();
    }
}