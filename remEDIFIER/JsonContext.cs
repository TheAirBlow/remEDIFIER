using System.Text.Json.Serialization;
using remEDIFIER.Widgets;

namespace remEDIFIER; 

[JsonSourceGenerationOptions(WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = [ typeof(JsonStringEnumConverter) ])]
[JsonSerializable(typeof(DeviceNameWidget))]
[JsonSerializable(typeof(EqualizerWidget))]
[JsonSerializable(typeof(ShutdownWidget))]
[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(VolumeWidget))]
[JsonSerializable(typeof(AudioWidget))]
[JsonSerializable(typeof(Product[]))]
public partial class JsonContext : JsonSerializerContext;