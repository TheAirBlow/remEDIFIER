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
    /// Array of ANC types names
    /// </summary>
    public string[] Names { get; set; }

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
            throw new ArgumentOutOfRangeException(nameof(value), "Unknown ANC value specified");
        Names = types.Select(x => _nameMapping[x]).ToArray();
        Value = value; Modes = types;
    }

    /// <summary>
    /// Maps ANC mode to index
    /// </summary>
    /// <param name="mode">ANC mode</param>
    /// <returns>Index</returns>
    public byte Map(ANCMode mode) {
        var index = Array.IndexOf(Modes, mode);
        if (index == -1) throw new ArgumentOutOfRangeException(nameof(mode),
            "Specified ANC mode is not supported by current headset");
        return (byte)index;
    }
    
    /// <summary>
    /// Maps index to ANC mode
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>ANC mode</returns>
    public ANCMode Map(byte index) {
        if (index >= Modes.Length) throw new ArgumentOutOfRangeException(nameof(index),
            "Specified index is not supported by current headset");
        return Modes[index];
    }

    /// <summary>
    /// ANC value to ANC types mapping
    /// </summary>
    private readonly Dictionary<byte, ANCMode[]> _valueMapping = new() {
        [0x00] = [],
        [0x06] = [ANCMode.Normal, ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.AmbientNoise],
        [0x0C] = [ANCMode.Normal, ANCMode.NoiseCancellation, ANCMode.AmbientNoise],
        [0x13] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.AmbientNoise, ANCMode.WindReduction, ANCMode.Normal],
        [0x17] = [ANCMode.NoiseCancellation, ANCMode.AmbientNoise, ANCMode.Normal],
        [0x1A] = [ANCMode.HighNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.WindReduction, ANCMode.AmbientNoise, ANCMode.Normal],
        [0x1F] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.LowNoiseCancellation, ANCMode.AmbientNoise, ANCMode.WindReduction, ANCMode.Normal],
        [0x10] = [ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.WindReduction, ANCMode.AmbientNoise, ANCMode.AmbientNoise, ANCMode.Normal],
        [0x11] = [ANCMode.AdaptiveNoiseCancellation, ANCMode.AmbientNoise, ANCMode.Normal],
        [0x1C] = [ANCMode.NoiseCancellation, ANCMode.AmbientNoise, ANCMode.Normal],
        [0x1D] = [ANCMode.AdaptiveNoiseCancellation, ANCMode.HighNoiseCancellation, ANCMode.MediumNoiseCancellation, ANCMode.LegacyAmbientNoise, ANCMode.Normal]
    };

    /// <summary>
    /// ANC type to display name mapping
    /// </summary>
    private readonly Dictionary<ANCMode, string> _nameMapping = new() {
        [ANCMode.AdaptiveNoiseCancellation] = "Adaptive Noise Cancellation",
        [ANCMode.MediumNoiseCancellation] = "Medium Noise Cancellation",
        [ANCMode.HighNoiseCancellation] = "High Noise Cancellation",
        [ANCMode.LowNoiseCancellation] = "Low Noise Cancellation",
        [ANCMode.NoiseCancellation] = "Noise Cancellation",
        [ANCMode.LegacyAmbientNoise] = "Ambient Noise",
        [ANCMode.WindReduction] = "Wind Reduction",
        [ANCMode.AmbientNoise] = "Ambient Noise",
        [ANCMode.Normal] = "Normal",
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
    LegacyAmbientNoise, // TODO: specify adaptive transparency, highly human voice, balanced, background sound
    NoiseCancellation,
    WindReduction, // TODO: find out how to specify values
    AmbientNoise,
    Normal
}