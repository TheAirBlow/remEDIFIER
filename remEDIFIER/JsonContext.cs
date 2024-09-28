using System.Text.Json.Serialization;

namespace remEDIFIER; 

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Configuration))] [JsonSerializable(typeof(Product[]))]
public partial class JsonContext : JsonSerializerContext;