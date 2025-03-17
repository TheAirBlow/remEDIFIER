using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Protocol.Values;

/// <summary>
/// Equalizer value mapping
/// </summary>
public class EqualizerValue {
    /// <summary>
    /// Actual equalizer value
    /// </summary>
    public byte Value { get; }
    
    /// <summary>
    /// Array of supported equalizer presets
    /// </summary>
    public EqualizerPreset[] Presets { get; set; }
    
    /// <summary>
    /// Whether equalizer is supported or not
    /// </summary>
    public bool Supported => Value != 0;

    /// <summary>
    /// Creates a new equalizer value
    /// </summary>
    /// <param name="value">Value</param>
    /// <param name="data">Support data</param>
    public EqualizerValue(byte value, SupportData? data) {
        if (!_valueMapping.TryGetValue(value, out var types)) types = [];
        Value = value; Presets = types;
        if (!PatchManager.ShowCustomEq(data))
            Presets = Presets.Where(x => x != EqualizerPreset.Customized).ToArray();
    }
    
    /// <summary>
    /// Maps equalizer preset to index
    /// </summary>
    /// <param name="preset">Preset</param>
    /// <returns>Index</returns>
    public byte Map(EqualizerPreset preset) {
        if (preset == EqualizerPreset.Disable) return 0xFF;
        var index = Array.IndexOf(Presets, preset);
        if (index != -1) return (byte)index;
        Log.Warning("Equalizer preset {0} is not supported", preset);
        return 0xFF;
    }
    
    /// <summary>
    /// Maps index to equalizer preset
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>Preset</returns>
    public EqualizerPreset Map(byte index) {
        if (index == 0xFF) return EqualizerPreset.Disable;
        if (index < Presets.Length) return Presets[index];
        Log.Warning("Unknown equalizer preset: {0:X2}", index);
        return EqualizerPreset.Unknown;
    }

    /// <summary>
    /// Equalizer value to equalizer presets mapping
    /// </summary>
    private readonly Dictionary<byte, EqualizerPreset[]> _valueMapping = new() {
        [0x00] = [],
        [0x01] = [EqualizerPreset.Classic, EqualizerPreset.Pop, EqualizerPreset.Classical, EqualizerPreset.Rock, EqualizerPreset.Disable],
        [0x02] = [EqualizerPreset.Classic, EqualizerPreset.Pop, EqualizerPreset.Classical, EqualizerPreset.Rock, EqualizerPreset.Rock, EqualizerPreset.Rock],
        [0x03] = [EqualizerPreset.Classic, EqualizerPreset.Pop, EqualizerPreset.Rock],
        [0x04] = [EqualizerPreset.Classic, EqualizerPreset.Pop, EqualizerPreset.Classical, EqualizerPreset.Rock, EqualizerPreset.Customized],
        [0x06] = [EqualizerPreset.Classic, EqualizerPreset.Surround, EqualizerPreset.Game],
        [0x07] = [EqualizerPreset.Classic, EqualizerPreset.Dynamic, EqualizerPreset.Customized],
        [0x08] = [EqualizerPreset.Classic, EqualizerPreset.Dynamic, EqualizerPreset.Surround, EqualizerPreset.Customized],
        [0x09] = [EqualizerPreset.Classic, EqualizerPreset.HiFi, EqualizerPreset.Stax],
        [0x0A] = [EqualizerPreset.Classic, EqualizerPreset.Monitor, EqualizerPreset.Dynamic, EqualizerPreset.Vocal, EqualizerPreset.Customized],
        [0x0B] = [EqualizerPreset.Classic, EqualizerPreset.Game, EqualizerPreset.Classical, EqualizerPreset.Dynamic, EqualizerPreset.Customized],
        [0x0C] = [EqualizerPreset.Music, EqualizerPreset.Game, EqualizerPreset.Movie, EqualizerPreset.Customized],
        [0x0D] = [EqualizerPreset.Music, EqualizerPreset.Game, EqualizerPreset.Customized],
        [0x0E] = [EqualizerPreset.Music, EqualizerPreset.Game, EqualizerPreset.Movie],
        [0x0F] = [EqualizerPreset.Music, EqualizerPreset.Game, EqualizerPreset.Movie, EqualizerPreset.Customized],
        [0x10] = [EqualizerPreset.Classic, EqualizerPreset.Monitor, EqualizerPreset.Dynamic, EqualizerPreset.Vocal, EqualizerPreset.Customized],
        [0x11] = [EqualizerPreset.Classic, EqualizerPreset.Monitor, EqualizerPreset.Game, EqualizerPreset.Vocal, EqualizerPreset.Customized],
        [0x12] = [EqualizerPreset.Music, EqualizerPreset.Monitor, EqualizerPreset.Game, EqualizerPreset.Movie, EqualizerPreset.Customized],
        [0x16] = [EqualizerPreset.Original, EqualizerPreset.Dynamic, EqualizerPreset.Monitor, EqualizerPreset.Customized],
        [0x17] = [EqualizerPreset.Original, EqualizerPreset.Dynamic, EqualizerPreset.Electrostatic, EqualizerPreset.Customized],
        [0x18] = [EqualizerPreset.Classic, EqualizerPreset.Dynamic, EqualizerPreset.Customized],
        [0x19] = [EqualizerPreset.Classic, EqualizerPreset.Bassy, EqualizerPreset.Vocal, EqualizerPreset.Customized],
        [0x1A] = [EqualizerPreset.Classic, EqualizerPreset.Pop, EqualizerPreset.Classic, EqualizerPreset.Rock, EqualizerPreset.Movie],
        [0x1B] = [EqualizerPreset.Classic, EqualizerPreset.Classical, EqualizerPreset.Bassy, EqualizerPreset.Rock, EqualizerPreset.Customized],
        [0x1D] = [EqualizerPreset.Classic, EqualizerPreset.Dyzj, EqualizerPreset.Vocal, EqualizerPreset.Gyzq],
        [0x1E] = [EqualizerPreset.Monitor, EqualizerPreset.Music, EqualizerPreset.Customized],
        [0x20] = [EqualizerPreset.Classic, EqualizerPreset.Dynamic, EqualizerPreset.Vocal, EqualizerPreset.Customized],
        [0x23] = [EqualizerPreset.Classic, EqualizerPreset.Popular, EqualizerPreset.Classical, EqualizerPreset.Rock, EqualizerPreset.Theatre, EqualizerPreset.Customized]
    };

    /// <summary>
    /// Equalizer preset to display name mapping
    /// </summary>
    public readonly Dictionary<EqualizerPreset, string> Names = new() {
        [EqualizerPreset.Classic] = "Classic",
        [EqualizerPreset.Classical] = "Classical",
        [EqualizerPreset.Pop] = "Pop",
        [EqualizerPreset.Rock] = "Rock",
        [EqualizerPreset.Customized] = "Customized",
        [EqualizerPreset.Surround] = "Surround",
        [EqualizerPreset.Game] = "Gaming",
        [EqualizerPreset.Dynamic] = "Dynamic",
        [EqualizerPreset.HiFi] = "Hi-Fi",
        [EqualizerPreset.Stax] = "Stax",
        [EqualizerPreset.Monitor] = "Monitor",
        [EqualizerPreset.Vocal] = "Vocal",
        [EqualizerPreset.Music] = "Music",
        [EqualizerPreset.Gyzq] = "GYZQ",
        [EqualizerPreset.Theatre] = "Theatre",
        [EqualizerPreset.Movie] = "Movie",
        [EqualizerPreset.Electrostatic] = "Electrostatic",
        [EqualizerPreset.Bassy] = "Bassy",
        [EqualizerPreset.Popular] = "Popular",
        [EqualizerPreset.Dyzj] = "DYZJ",
        [EqualizerPreset.Original] = "Original",
        [EqualizerPreset.Unknown] = "Unknown",
        [EqualizerPreset.Disable] = "Disable"
    };
}

/// <summary>
/// Equalizer preset enum
/// </summary>
public enum EqualizerPreset {
    Classic, Classical, Pop, Rock, Customized, Surround, Game, 
    Dynamic, HiFi, Stax, Monitor, Vocal, Music, Gyzq, Theatre,
    Movie, Electrostatic, Bassy, Popular, Dyzj, Original, 
    Disable, Unknown
}