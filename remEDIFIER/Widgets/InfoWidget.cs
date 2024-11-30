using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Windows;

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
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Basic information");
        if (window.Client.Support!.Supports(Feature.GetFirmwareVersion))
            ImGui.TextUnformatted($"Firmware version: {_version ?? "(loading)"}");
        if (window.Client.Support!.Supports(Feature.GetMacAddress))
            ImGui.TextUnformatted($"MAC address: {_macAddress ?? "(loading)"}");
        if (window.Client.Support!.Supports(Feature.ShowBattery))
            ImGui.TextUnformatted($"Battery charge: {_battery ?? "(loading)"}");
        var sameLine = false;
        if (window.Client.Support!.Supports(Feature.RePair)) {
            if (ImGui.Button("Re-pair"))
                window.Client.Send(PacketType.RePair, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (window.Client.Support!.Supports(Feature.ManualShutdown)) {
            if (ImGui.Button("Shutdown"))
                window.Client.Send(PacketType.Shutdown, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (window.Client.Support!.Supports(Feature.Disconnect)) {
            if (ImGui.Button("Disconnect"))
                window.Client.Send(PacketType.Disconnect, wait: false);
            sameLine = true;
        }
        if (sameLine) ImGui.SameLine();
        if (window.Client.Support!.Supports(Feature.DeviceResetSettings)) {
            if (ImGui.Button("Factory Reset"))
                window.Client.Send(PacketType.FactoryReset, wait: false);
            sameLine = true;
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
            case PacketType.GetBattery:
                _battery = $"{(int)((ByteData)data!).Value}%";
                return true;
            case PacketType.GetMacAddress:
                _macAddress = ((MacAddressData)data!).Value;
                return true;
            case PacketType.GetFirmwareVersion:
                _version = ((VersionData)data!).Version.ToString();
                return true;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Sends all the packets necessary
    /// </summary>
    /// <param name="window">Window</param>
    public void ReadSettings(DeviceWindow window) {
        if (window.Client.Support!.Supports(Feature.GetFirmwareVersion))
            window.Client.Send(PacketType.GetFirmwareVersion, notify: true);
        if (window.Client.Support!.Supports(Feature.GetMacAddress))
            window.Client.Send(PacketType.GetMacAddress, notify: true);
        if (window.Client.Support!.Supports(Feature.ShowBattery))
            window.Client.Send(PacketType.GetBattery, notify: true);
    }
}