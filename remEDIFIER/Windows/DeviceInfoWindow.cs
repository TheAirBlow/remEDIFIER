using System.Numerics;
using ImGuiNET;
using Raylib_CsLo;
using remEDIFIER.Device;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Device management window
/// </summary>
public class DeviceInfoWindow : ManagedWindow {
    /// <summary>
    /// Array of currently supported features
    /// </summary>
    private static Feature[] _supported = [
        Feature.GetMacAddress, Feature.GetFirmwareVersion, Feature.GetDeviceName, Feature.SetDeviceName,
        Feature.ShowBattery, Feature.MusicInfo, Feature.ManualShutdown, Feature.Disconnect, Feature.RePair,
        Feature.Disconnect
    ];
    
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
    public EdifierDevice Device { get; }

    /// <summary>
    /// Device information
    /// </summary>
    public DeviceState State => Device.State!;

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DeviceInfoWindow(EdifierDevice device) {
        Device = device;
        Device.State = new DeviceState(Client);
        Client.PacketReceived += PacketReceived;
        Client.DeviceDisconnected += OnClosed;
        Log.Information("Detected features: {0}", string.Join(", ", Client.Support!.Features));
        var notSupported = Client.Support!.Features.Where(x => !_supported.Contains(x)).ToList();
        if (notSupported.Count > 0)
            Log.Warning("Some features are not supported: {0}", string.Join(", ", notSupported));
        State.Request();
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        ImGui.BeginChild("Information",
            new Vector2(ImGui.GetContentRegionAvail().X, 110));
        ImGui.BeginGroup();
        MyGui.LoadingText("%s", State.DeviceName, 32);
        ImGui.SameLine();
        MyGui.SetNextMargin(6);
        if (Client.Supports(Feature.SetDeviceName)) {
            if (MyGui.ImageButton("edit", Scaler.Fit(20, 20)))
                Manager.OpenWindow(new DeviceNameWindow(Device));
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Change device name");
        }
        MyGui.LoadingText("MAC address: %s", State.MacAddress, 18);
        MyGui.LoadingText("Firmware version: %s", State.FirmwareVersion, 18);
        MyGui.LoadingText("Battery charge: %s%", State.Battery, 18);
        ImGui.EndGroup();
        ImGui.SameLine();
        MyGui.SetNextCentered(1f);
        var image = Images.Get(Device.Extra!.Product.ProductImageLink);
        MyGui.Image(image, Scaler.Fit(105, 105, ratio: new Vector2(1f, 0.5f)));
        ImGui.EndChild();
        ImGui.Separator();
        if (Client.Supports(Feature.ManualShutdown) && ButtonPanel("power-off", "Power off"))
            Client.Send(PacketType.Shutdown, wait: false);
        if (Client.Supports(Feature.Disconnect) && ButtonPanel("disconnect", "Disconnect"))
            Client.Send(PacketType.Disconnect, wait: false);
        if (Client.Supports(Feature.RePair) && ButtonPanel("unpair", "Disconnect and re-pair"))
            Client.Send(PacketType.RePair, wait: false);
        if (Client.Supports(Feature.FactoryReset) && ButtonPanel("erase", "Reset to factory settings"))
            Client.Send(PacketType.FactoryReset, wait: false);
        if (ButtonPanel("bug", "Debugging toolbox"))
            Manager.OpenWindow(new DebugWindow(Device));
        ImGui.Dummy(new Vector2(0, 45));
        
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetWindowPos().X - 1, ImGui.GetWindowPos().Y + ImGui.GetWindowHeight() - 44));
        ImGui.SetNextWindowSize(new Vector2(ImGui.GetWindowSize().X + 2, 45));
        ImGui.Begin("Playback information", ImGuiWindowFlags.NoDecoration);
        MyGui.PushContentRegion();
        if (MyGui.ImageButton("backward", new Vector2(24, 24)))
            Client.Send(PacketType.AVCRPCommand, 
                new AVCRPCommandData(AVCRPCommand.Previous), 
                wait: false);
        ImGui.SameLine();
        if (MyGui.ImageButton(State.Playing ? "pause" : "play", new Vector2(24, 24)))
            Client.Send(PacketType.AVCRPCommand, 
                new AVCRPCommandData(State.Playing ? AVCRPCommand.Pause : AVCRPCommand.Play), 
                wait: false);
        ImGui.SameLine();
        if (MyGui.ImageButton("forward", new Vector2(24, 24)))
            Client.Send(PacketType.AVCRPCommand, 
                new AVCRPCommandData(AVCRPCommand.Next), 
                wait: false);
        ImGui.SameLine();
        MyGui.SetNextCentered(1f);
        MyGui.ScrollingText(State.SongName != null 
            ? $"{State.SongAuthor} - {State.SongName}"
            : "Song title is not available",
            ImGui.GetContentRegionAvail().X);
        MyGui.PopContentRegion();
        ImGui.End();
    }

    /// <summary>
    /// Draws a button panel
    /// </summary>
    /// <param name="icon">Icon</param>
    /// <param name="title">Title</param>
    /// <returns>True if clocked</returns>
    private bool ButtonPanel(string icon, string title) {
        ImGui.BeginChild(title, new Vector2(ImGui.GetIO().DisplaySize.X - 10, 50));
        MyGui.PushContentRegion();
        MyGui.SetNextCentered(0f, 0.5f);
        MyGui.Image(icon, Scaler.Fit(28, 28));
        ImGui.SameLine();
        MyGui.SetNextCentered(0f, 0.5f);
        MyGui.Text(title, 24);
        ImGui.SameLine();
        MyGui.SetNextMargin(right: 5);
        MyGui.SetNextCentered(1f, 0.5f);
        MyGui.Image("angle-right", Scaler.Fit(18, 18));
        MyGui.PopContentRegion();
        ImGui.EndChild();
        var clicked = ImGui.IsItemClicked();
        ImGui.Separator();
        return clicked;
    }
    
    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <param name="payload">Payload</param>
    private void PacketReceived(PacketType type, IPacketData? data, byte[] payload)
        => State.Update(type, data);

    /// <summary>
    /// Disconnects from the device
    /// </summary>
    public override void OnClosed() {
        Client.PacketReceived -= PacketReceived;
        Client.DeviceDisconnected -= OnClosed;
        Device.Client.Disconnect();
        Closed = true;
    }
}