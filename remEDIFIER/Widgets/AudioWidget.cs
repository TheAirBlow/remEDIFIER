using System.Text.Json.Serialization;
using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Windows;

namespace remEDIFIER.Widgets;

/// <summary>
/// Audio codec settings widget
/// </summary>
public class AudioWidget : SerializableWidget, IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    [JsonIgnore]
    public Feature[] Features => [ Feature.Ldac, Feature.HiRes, Feature.GameMode ];
    
    /// <summary>
    /// LDAC state
    /// </summary>
    public LDACState? State { get; set; }
    
    /// <summary>
    /// Is game mode enabled
    /// </summary>
    public bool? GameMode { get; set; }
    
    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Audio settings");
        if (window.Client.Supports(Feature.Ldac)) {
            ImGui.Text("LDAC");
            ImGui.SameLine();
            var states = Enum.GetValues<LDACState>();
            for (var i = 0; i < states.Length; i++) {
                if (states[i] == LDACState.On192K && 
                    !window.Client.Supports(Feature.Allow192K))
                    continue;
                if (i != 0) ImGui.SameLine();
                var name = states[i].ToString();
                if (name.StartsWith("On")) name = name[2..];
                if (ImGui.RadioButton($"{name}##{i}", State == states[i]))
                    window.Client.Send(PacketType.SetLDAC, new LdacData { Value = states[i] }, wait: false);
            }
        }

        if (window.Client.Supports(Feature.GameMode)) {
            var temp = GameMode ?? true;
            ImGui.Checkbox("Game Mode", ref temp);
            if (GameMode != null && temp != GameMode) window.Client.Send(
                PacketType.SetGameMode, new BooleanData { Value = temp }, wait: false);
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
            case PacketType.GetLDAC: {
                var value = ((LdacData)data!).Value;
                State = value; SaveSettings(window);
                return true;
            }
            case PacketType.SetLDAC: {
                var value = ((LdacData)data!).Value;
                State = value; SaveSettings(window);
                return true;
            }
            case PacketType.GetGameMode: {
                var value = ((BooleanData)data!).Value;
                if (GameMode != null && value != GameMode) window.Client.Send(
                    PacketType.SetGameMode, new BooleanData { Value = GameMode.Value }, wait: false);
                GameMode = value; SaveSettings(window);
                return true;
            }
            case PacketType.SetGameMode: {
                var value = ((BooleanData)data!).Value;
                GameMode = value; SaveSettings(window);
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
        window.Client.Send(PacketType.GetGameMode, notify: true);
        window.Client.Send(PacketType.GetLDAC, notify: true);
    }
}
