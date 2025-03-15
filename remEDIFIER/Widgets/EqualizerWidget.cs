using System.Text.Json.Serialization;
using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Protocol.Values;
using remEDIFIER.Windows;

namespace remEDIFIER.Widgets;

/// <summary>
/// Equalizer widget
/// </summary>
public class EqualizerWidget : SerializableWidget, IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    [JsonIgnore]
    public Feature[] Features => [ Feature.Equalizer ];
    
    /// <summary>
    /// Equalizer preset
    /// </summary>
    public EqualizerPreset? Preset { get; set; }
    
    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Equalizer");
        var eq = window.Client.Support!.EqualizerValue!;
        for (var i = 0; i < eq.Names.Length; i++) {
            if (i != 0) ImGui.SameLine();
            if (ImGui.RadioButton($"{eq.Names[i]}##{i}", Preset == eq.Presets[i])) {
                window.Client.Send(PacketType.SetEqualizer, 
                    new EqualizerData { Preset = eq.Presets[i] }, wait: false);
            }
        }
    }

    /// <summary>
    /// Process a received packet
    /// </summary>
    /// <param name="window">Window</param>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <returns>True if processed</returns>
    public bool PacketReceived(DeviceWindow window, PacketType type, IPacketData? data) {
        switch (type) {
            case PacketType.GetEqualizer: {
                var value = ((EqualizerData)data!).Preset;
                if (Preset != null && value != Preset) window.Client.Send(
                    PacketType.SetEqualizer, new EqualizerData { Preset = Preset.Value }, wait: false);
                Preset = value; SaveSettings(window);
                return true;
            }
            case PacketType.SetEqualizer: {
                var value = ((EqualizerData)data!).Preset;
                Preset = value; SaveSettings(window);
                return true;
            }
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Sends all the packets necessary
    /// </summary>
    /// <param name="window">Window</param>
    public void ReadSettings(DeviceWindow window) {
        window.Client.Send(PacketType.GetEqualizer, notify: true);
    }
}