using System.Numerics;
using ImGuiNET;
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
    /// Array of all device windows
    /// </summary>
    private static readonly Type[] _allWindows = [
        typeof(DebugWindow)
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
    /// Array of supported device windows
    /// </summary>
    private DeviceWindow[] _windows;

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
        MyGui.LoadingText("MAC address: %s", State.MacAddress, 18);
        MyGui.LoadingText("Firmware version: %s", State.FirmwareVersion, 18);
        MyGui.LoadingText("Battery charge: %s%", State.Battery, 18);
        ImGui.EndGroup();
        ImGui.SameLine();
        MyGui.SetNextCentered(1f);
        var image = Images.Get(Device.Extra!.Product.ProductImageLink);
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