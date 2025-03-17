using System.Text.Json.Serialization;

namespace remEDIFIER; 

[JsonSourceGenerationOptions(WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [ typeof(JsonStringEnumConverter) ])]
[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(Product[]))]
public partial class JsonContext : JsonSerializerContext;