using static remEDIFIER.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Windows;

namespace remEDIFIER.Widgets;

/// <summary>
/// Prompt volume widget
/// </summary>
public class VolumeWidget : IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    [JsonIgnore]
    public Feature[] Features => [ Feature.BeepVolumeSettings ];
    
    /// <summary>
    /// Prompt volume
    /// </summary>
    public byte? PromptVolume { get; set; }
    
    /// <summary>
    /// Should volume be uncapped from 10 to 100
    /// </summary>
    public bool UncapVolume { get; set; }
    
    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Prompt volume");
        int value = PromptVolume ?? 0; var max = UncapVolume ? 0xFF : 10;
        ImGui.SliderInt("##volume", ref value, 0, max);
        if (PromptVolume != null && value != PromptVolume) window.Client.Send(
            PacketType.SetPromptVolume, new ByteData { Value = (byte)value }, notify: true);
        var temp = UncapVolume;
        ImGui.Checkbox("Remove volume limit", ref temp);
        UncapVolume = temp;
        if (UncapVolume) ImGui.Text(
            "WARNING: Anything above 10 will be EXTREMELY loud!\n" +
            "Take off the headphones before it's too late!");
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
            case PacketType.GetPromptVolume: {
                var value = ((ByteData)data!).Value;
                if (PromptVolume != null && value != PromptVolume)
                    window.Client.Send(PacketType.SetPromptVolume, 
                        new ByteData { Value = PromptVolume.Value });
                PromptVolume = value; SaveSettings(window);
                return true;
            }
            case PacketType.SetPromptVolume: {
                var value = ((ByteData)data!).Value;
                PromptVolume = value; SaveSettings(window);
                return true;
            }
            default:
                return false;
        }
    }

    /// <summary>
    /// Saves settings to the configuration
    /// </summary>
    private void SaveSettings(DeviceWindow window) {
        const string key = nameof(VolumeWidget);
        var node = JsonSerializer.SerializeToNode(this, JsonContext.Default.VolumeWidget)!;
        window.Device!.Widgets[key] = node.AsObject(); Config.Save();
    }
    
    /// <summary>
    /// Sends all the packets necessary
    /// </summary>
    /// <param name="window">Window</param>
    public void ReadSettings(DeviceWindow window) {
        window.Client.Send(PacketType.GetPromptVolume, notify: true);
    }
}