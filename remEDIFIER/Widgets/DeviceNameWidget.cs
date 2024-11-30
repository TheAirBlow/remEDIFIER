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
/// Device name widget
/// </summary>
public class DeviceNameWidget : IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    [JsonIgnore]
    public Feature[] Features => [
        Feature.GetDeviceName, Feature.SetDeviceName
    ];
    
    /// <summary>
    /// Device name
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Device name field
    /// </summary>
    private string _nameField = "(loading)";

    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Device name");
        ImGui.Text("Takes effect after re-pairing and restarting the device");
        ImGui.InputText("##name", ref _nameField, (uint)(window.Client.Support?.MaxDeviceName ?? 10));
        ImGui.SameLine();
        if (window.Client.Support!.Supports(Feature.SetDeviceName) && ImGui.Button("Save")) {
            DeviceName = _nameField; SaveSettings(window);
            window.Client.Send(PacketType.SetDeviceName, 
                new StringData { Value = _nameField }, wait: false);
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
            case PacketType.GetDeviceName:
                var value = ((StringData)data!).Value;
                if (DeviceName != null && value != DeviceName) window.Client.Send(
                    PacketType.SetDeviceName, new StringData { Value = _nameField }, wait: false);
                DeviceName = _nameField = value;
                SaveSettings(window);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Saves settings to the configuration
    /// </summary>
    private void SaveSettings(DeviceWindow window) {
        const string key = nameof(DeviceNameWidget);
        var node = JsonSerializer.SerializeToNode(this, JsonContext.Default.DeviceNameWidget)!;
        window.Device!.Widgets[key] = node.AsObject(); Config.Save();
    }
}