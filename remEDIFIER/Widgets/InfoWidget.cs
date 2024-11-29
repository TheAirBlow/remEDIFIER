using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Widgets;

/// <summary>
/// Information widget
/// </summary>
public class InfoWidget : IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    public Feature[] Features => [
        Feature.ShowBattery, Feature.GetFirmwareVersion, Feature.RePair, 
        Feature.ManualShutdown, Feature.Disconnect, Feature.DeviceResetSettings, 
        Feature.GetMacAddress
    ];
    
    /// <summary>
    /// Device name
    /// </summary>
    private string? _deviceName;
    
    /// <summary>
    /// Mac address
    /// </summary>
    private string? _macAddress;
    
    /// <summary>
    /// Firmware version
    /// </summary>
    private string? _version;
    
    /// <summary>
    /// Battery percentage
    /// </summary>
    private string? _battery;

    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="client">Edifier client</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(EdifierClient client, ImGuiRenderer renderer) {
        ImGui.SeparatorText(_deviceName ?? "(loading)");
        ImGui.TextUnformatted($"Firmware version: {_version ?? "(loading)"}");
        ImGui.TextUnformatted($"MAC address: {_macAddress ?? "(loading)"}");
        ImGui.TextUnformatted($"Battery charge: {_battery ?? "(loading)"}");
        var sameLine = false;
        if (client.Support!.Supports(Feature.RePair)) {
            if (ImGui.Button("Re-pair"))
                client.Send(PacketType.RePair, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (client.Support!.Supports(Feature.ManualShutdown)) {
            if (ImGui.Button("Shutdown"))
                client.Send(PacketType.Shutdown, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (client.Support!.Supports(Feature.Disconnect)) {
            if (ImGui.Button("Disconnect"))
                client.Send(PacketType.Disconnect, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (client.Support!.Supports(Feature.DeviceResetSettings)) {
            if (ImGui.Button("Factory Reset"))
                client.Send(PacketType.FactoryReset, wait: false);
            sameLine = true;
        }
    }

    /// <summary>
    /// Process a received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <returns>True if processed</returns>
    public bool PacketReceived(PacketType type, IPacketData? data) {
        switch (type) {
            case PacketType.GetBattery:
                _battery = $"{(int)((ByteData)data!).Value}%";
                return true;
            case PacketType.GetMacAddress:
                _macAddress = ((MacAddressData)data!).Value;
                return true;
            case PacketType.GetDeviceName:
                _deviceName = ((StringData)data!).Value;
                return true;
            case PacketType.GetFirmwareVersion:
                _version = ((VersionData)data!).Version.ToString();
                return true;
            default:
                return false;
        }
    }
}