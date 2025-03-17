using Serilog;

namespace remEDIFIER.Protocol.Values;

/// <summary>
/// ANC value mapping
/// </summary>
public class ANCValue {
    /// <summary>
    /// Actual ANC value
    /// </summary>
    public byte Value { get; }
    
    /// <summary>
    /// Array of supported ANC modes
    /// </summary>
    public ANCMode[] Modes { get; set; }
    
    /// <summary>
    /// Whether ANC is supported or not
    /// </summary>
    public bool Supported => Value != 0;

    /// <summary>
    /// Creates a new ANC value
    /// </summary>
    /// <param name="value">Value</param>
    public ANCValue(byte value) {
        if (!_valueMapping.TryGetValue(value, out var types))
            types = [];
        Value = value; Modes = types;
    }

    /// <summary>
    /// Maps ANC mode to index
    /// </summary>
    /// <param name="mode">ANC mode</param>
    /// <returns>Index</returns>
    public byte Map(ANCMode mode) {
        var index = Array.IndexOf(Modes, mode);
        if (index != -1) return (byte)(index + 1);
        Log.Warning("ANC mode {0} is not supported", mode);
        return 0xFF;
    }
    
    /// <summary>
    /// Maps index to ANC mode
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>ANC mode</returns>
    public ANCMode Map(byte index) {
        if (index < Modes.Length) return Modes[index];
        Log.Warning("Unknown ANC value: {0:X2}", index);
        return ANCMode.Unknown;
    }

    /// <summary>
    /// ANC value to ANC types mapping
    /// </summary>
    private readonly Dictionary<byte, ANCMode[]> _valueMapping = new() {
        [0x00] = [],
        [0x06] = [ANCMode.Normal, ANCMode.HighNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.AmbientSound],
        [0xF6] = [ANCMode.Normal, ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.AmbientSound],
        [0x0C] = [ANCMode.Normal, ANCMode.NoiseCancellation, ANCMode.AmbientSound],
        [0x13] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.AmbientSoundChoice1, ANCMode.WindReduction, ANCMode.Normal],
        [0x17] = [ANCMode.NoiseCancellation, ANCMode.AmbientSound, ANCMode.Normal],
        [0x1A] = [ANCMode.HighNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.WindReduction, ANCMode.AmbientSound, ANCMode.Normal],
        [0x1F] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.AmbientSound, ANCMode.WindReduction, ANCMode.Normal],
        [0x10] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.WindReduction, ANCMode.AmbientSound, ANCMode.Normal],
        [0x11] = [ANCMode.AdaptiveNoiseCancellation, ANCMode.AmbientSound, ANCMode.Normal],
        [0x1C] = [ANCMode.NoiseCancellation, ANCMode.AmbientSound, ANCMode.Normal],
        [0x1D] = [ANCMode.AdaptiveNoiseCancellation, ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.AmbientSoundChoice2, ANCMode.WindReduction, ANCMode.Normal]
    };

    /// <summary>
    /// ANC type to display name mapping
    /// </summary>
    public readonly Dictionary<ANCMode, string> Names = new() {
        [ANCMode.AdaptiveNoiseCancellation] = "Adaptive Noise Cancellation",
        [ANCMode.MediumNoiseCancellation] = "Medium Noise Cancellation",
        [ANCMode.HighNoiseCancellation] = "High Noise Cancellation",
        [ANCMode.LowNoiseCancellation] = "Low Noise Cancellation",
        [ANCMode.NoiseCancellation] = "Noise Cancellation",
        [ANCMode.AmbientSoundChoice1] = "Ambient Sound",
        [ANCMode.AmbientSoundChoice2] = "Ambient Sound",
        [ANCMode.AmbientSound] = "Ambient Sound",
        [ANCMode.WindReduction] = "Wind Reduction",
        [ANCMode.Normal] = "Normal"
    };
}

/// <summary>
/// ANC type enum
/// </summary>
public enum ANCMode {
    AdaptiveNoiseCancellation,
    MediumNoiseCancellation,
    HighNoiseCancellation,
    LowNoiseCancellation,
    AmbientSoundChoice1, // TODO: specify voice enhancement, balanced, background sound
    AmbientSoundChoice2, // TODO: specify adaptive transparency, highly human voice, balanced, background sound
    NoiseCancellation,
    WindReduction, // TODO: find out how to specify values
    AmbientSound,
    Unknown,
    Normal
}