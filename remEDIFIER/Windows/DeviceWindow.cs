using System.Numerics;
using static remEDIFIER.Configuration;
using ImGuiNET;
using remEDIFIER.Protocol;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Device management window
/// </summary>
public class DeviceWindow : ManagedWindow {
    /// <summary>
    /// Icon to show in the top bar
    /// </summary>
    public override string Icon => "headphones";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Device info";
    
    /// <summary>
    /// Edifier client
    /// </summary>
    public EdifierClient Client => Device.Client;
    
    /// <summary>
    /// Connected device
    /// </summary>
    public DiscoveredDevice Device { get; }
    
    /// <summary>
    /// Device configuration
    /// </summary>
    public Device? DeviceConfig { get; }
    
    /// <summary>
    /// Device information
    /// </summary>
    public DeviceInformation Info { get; }

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DeviceWindow(DiscoveredDevice device) {
        Device = device;
        Info = new DeviceInformation(Client);
        Client.PacketReceived += PacketReceived;
        Client.DeviceDisconnected += OnClosed;
        DeviceConfig = Config.Devices.FirstOrDefault(x => x.MacAddress == Device.Info.MacAddress);
        if (DeviceConfig == null) {
            DeviceConfig = new Device {
                MacAddress = Device.Info.MacAddress,
                ProtocolVersion = Device.ProtocolVersion!.Value,
                EncryptionType = Device.EncryptionType!.Value
            };
            if (device.Product != null)
                DeviceConfig.ProductId = device.Product.Id;
            Config.Devices.Add(DeviceConfig);
            Config.Save();
        }
        
        Log.Information("Detected features: {0}", string.Join(", ", Client.Support!.Features));
        Info.Request();
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        ImGui.BeginChild("Information",
            new Vector2(ImGui.GetContentRegionAvail().X, 110));
        ImGui.BeginGroup();
        MyGui.LoadingText("%s", Info.DeviceName, 32);
        MyGui.LoadingText("MAC address: %s", Info.MacAddress, 18);
        MyGui.LoadingText("Firmware version: %s", Info.FirmwareVersion, 18);
        MyGui.LoadingText("Battery charge: %s%", Info.Battery, 18);
        ImGui.EndGroup();
        ImGui.SameLine();
        MyGui.SetNextCentered(1f);
        var link = Device.Product?.ProductImageLink;
        var image = link != null ? Images.Get(link) : Images.Get("unknown");
        MyGui.Image(image, Scaler.Fit(105, 105));
        ImGui.EndChild();
        ImGui.Separator();
        ImGui.BeginChild("Debugging",
            new Vector2(ImGui.GetIO().DisplaySize.X - 10, 50));
        MyGui.PushContentRegion();
        MyGui.SetNextCentered(0f, 0.5f);
        MyGui.Image("bug", Scaler.Fit(28, 28));
        ImGui.SameLine();
        MyGui.SetNextCentered(0f, 0.5f);
        MyGui.Text("Debugging toolbox", 24);
        ImGui.SameLine();
        MyGui.SetNextMargin(right: 5);
        MyGui.SetNextCentered(1f, 0.5f);
        MyGui.Image("angle-right", Scaler.Fit(18, 18));
        MyGui.PopContentRegion();
        ImGui.EndChild();
        ImGui.Separator();
    }

    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <param name="payload">Payload</param>
    private void PacketReceived(PacketType type, IPacketData? data, byte[] payload) {
        Info.Update(type, data);
    }

    /// <summary>
    /// Disconnects from the device
    /// </summary>
    public override void OnClosed() {
        Device.Client.Disconnect();
        Device.Client = new EdifierClient();
        Closed = true;
    }
}