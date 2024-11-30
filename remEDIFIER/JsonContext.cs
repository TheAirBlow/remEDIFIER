using System.Text.Json.Serialization;
using remEDIFIER.Widgets;

namespace remEDIFIER; 

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(DeviceNameWidget))]
[JsonSerializable(typeof(EqualizerWidget))]
[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(Product[]))]
public partial class JsonContext : JsonSerializerContext;