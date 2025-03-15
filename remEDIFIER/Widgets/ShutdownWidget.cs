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
/// Automatic or timer shutdown widget
/// </summary>
public class ShutdownWidget : SerializableWidget, IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    [JsonIgnore]
    public Feature[] Features => [ Feature.ShutdownTimer, Feature.NoAudioAutoShutdown ];
    
    /// <summary>
    /// Shutdown timer value
    /// </summary>
    public ushort? ShutdownTimer { get; set; }
    
    /// <summary>
    /// Automatically shut down after 20 minutes without audio
    /// </summary>
    private bool? NoAudioAutoShutdown { get; set; }
    
    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Shutdown timer");
        if (window.Client.Support!.Supports(Feature.ShutdownTimer)) {
            int value = ShutdownTimer ?? 0;
            var format = $"{value % 60} seconds";
            if (value > 60) format = $"{Math.Floor(value / 60f) % 60} minutes {format}";
            if (value > 3600) format = $"{Math.Floor(value / 3600f) % 60} hours {format}";
            if (value > 86400) format = $"{Math.Floor(value / 86400f) % 24} days {format}";
            ImGui.SliderInt("##timer", ref value, 0, 0xFFFF, format);
            ImGui.Text("The shutdown timer is disabled if set to 0 seconds");
            if (ShutdownTimer != null && value != ShutdownTimer) {
                // apply immediately as there is no arbitrary delay
                ShutdownTimer = (ushort)value;
                if (value == 0) window.Client.Send(PacketType.DisableShutdownTimer);
                else window.Client.Send(PacketType.EnableShutdownTimer,
                    new ShortData { Value = ShutdownTimer.Value });
            }
        }

        if (window.Client.Support!.Supports(Feature.NoAudioAutoShutdown)) {
            var temp = NoAudioAutoShutdown ?? true;
            ImGui.Checkbox("Shutdown after 20 minutes without audio", ref temp);
            if (NoAudioAutoShutdown != null && temp != NoAudioAutoShutdown) window.Client.Send(
                PacketType.SetShutdownWithNoAudio, new BooleanData { Value = temp }, wait: false);
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
            case PacketType.GetShutdownTimer: {
                var value = ((ShortData)data!).Value;
                if (ShutdownTimer != null && value != ShutdownTimer) {
                    if (value == 0) window.Client.Send(PacketType.DisableShutdownTimer);
                    else window.Client.Send(PacketType.EnableShutdownTimer, new ShortData { Value = value });
                }
                ShutdownTimer = value; SaveSettings(window);
                return true;
            }
            case PacketType.GetShutdownWithNoAudio: {
                var value = ((BooleanData)data!).Value;
                if (NoAudioAutoShutdown != null && value != NoAudioAutoShutdown) window.Client.Send(
                    PacketType.SetShutdownWithNoAudio, new BooleanData { Value = NoAudioAutoShutdown.Value }, wait: false);
                NoAudioAutoShutdown = value; SaveSettings(window);
                return true;
            }
            case PacketType.SetShutdownWithNoAudio: {
                var value = ((BooleanData)data!).Value;
                NoAudioAutoShutdown = value; SaveSettings(window);
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
        if (window.Client.Support!.Supports(Feature.NoAudioAutoShutdown))
            window.Client.Send(PacketType.GetShutdownWithNoAudio, notify: true);
        if (window.Client.Support!.Supports(Feature.ShutdownTimer))
            window.Client.Send(PacketType.GetShutdownTimer, notify: true);
    }
}