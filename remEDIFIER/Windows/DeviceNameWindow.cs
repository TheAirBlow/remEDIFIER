using System.Drawing;
using System.Numerics;
using ImGuiNET;
using remEDIFIER.Device;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Windows;

/// <summary>
/// Device name window
/// </summary>
public class DeviceNameWindow : ManagedWindow {
    /// <summary>
    /// Icon to show in the top bar
    /// </summary>
    public override string Icon => "edit";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Device name";
    
    /// <summary>
    /// Connected device
    /// </summary>
    private EdifierDevice Device { get; }

    /// <summary>
    /// New device name
    /// </summary>
    private string _deviceName;

    /// <summary>
    /// Was a response packet received
    /// </summary>
    private bool? _received;
    
    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DeviceNameWindow(EdifierDevice device) {
        _deviceName = device.State?.DeviceName ?? "";
        device.Client.PacketReceived += PacketReceived;
        device.Client.PacketTimedOut += PacketTimedOut;
        Device = device;
    }

    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        if (_received == true) Closed = true;
        MyGui.Text("Enter a new name for your device:");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##name", ref _deviceName, (uint)(Device.Client.Support?.MaxDeviceName ?? 255));
        MyGui.Text("(changes will only be applied after re-pairing)", 18, Color.DarkGray);
        ImGui.Dummy(new Vector2(0, 5));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));
        if (_received == null && ImGui.Button("Save changes", new Vector2(ImGui.GetContentRegionAvail().X, 30))) {
            Device.Client.Send(PacketType.SetDeviceName, new StringData(_deviceName), notify: true, wantResponse: false);
            Processing = true; _received = false;
        }
        if (_received == false)
            MyGui.LoadingText("Waiting for response... %s", null, 20);
    }

    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <param name="payload">Payload</param>
    private void PacketReceived(PacketType type, IPacketData? data, byte[] payload) {
        if (type != PacketType.SetDeviceName) return;
        Device.State!.DeviceName = _deviceName;
        _received = true;
    }
    
    /// <summary>
    /// Handles timed out packet
    /// </summary>
    /// <param name="type">Type</param>
    private void PacketTimedOut(PacketType type) {
        if (type != PacketType.SetDeviceName) return;
        Device.Client.Send(PacketType.SetDeviceName, new StringData(_deviceName), notify: true, wantResponse: false);
    }

    /// <summary>
    /// Unregisters event handlers
    /// </summary>
    public override void OnClosed() {
        Device.Client.PacketReceived -= PacketReceived;
        Device.Client.PacketTimedOut -= PacketTimedOut;
    }
}