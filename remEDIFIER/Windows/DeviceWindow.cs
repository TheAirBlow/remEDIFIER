using System.Text.Json;
using static remEDIFIER.Configuration;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Widgets;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Device management window
/// </summary>
public class DeviceWindow : ManagedWindow {
    /// <summary>
    /// con to show in the top bar
    /// </summary>
    public override string Icon => "headphones";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => Device.Product?.Name ?? Device.Info.DeviceName;
    
    /// <summary>
    /// List of all widgets
    /// </summary>
    private readonly List<Type> _allWidgets = [
        typeof(InfoWidget), typeof(PlaybackWidget), typeof(EqualizerWidget), 
        typeof(DeviceNameWidget), typeof(AudioWidget), typeof(ShutdownWidget),
        typeof(VolumeWidget), typeof(DebugWidget)
    ];
    
    /// <summary>
    /// Connected device
    /// </summary>
    public DiscoveredDevice Device { get; private set; }

    /// <summary>
    /// Edifier client
    /// </summary>
    public EdifierClient Client => Device.Client;
    
    /// <summary>
    /// Device configuration
    /// </summary>
    public Device? DeviceConfig { get; private set; }

    /// <summary>
    /// Widgets to render
    /// </summary>
    private readonly List<IWidget> _widgets;
    
    /// <summary>
    /// List of not supported features
    /// </summary>
    private readonly List<Feature> _notSupported;

    /// <summary>
    /// Frame counter for auto resize
    /// </summary>
    private int _frames;

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DeviceWindow(DiscoveredDevice device) {
        Device = device;
        Client.PacketReceived += PacketReceived;
        Client.DeviceDisconnected += OnClosed;
        DeviceConfig = Config.Devices.FirstOrDefault(x => x.MacAddress == Device.Info.MacAddress);
        if (DeviceConfig == null) {
            DeviceConfig = new Device {
                MacAddress = Device.Info.MacAddress,
                ProtocolVersion = Device.ProtocolVersion!.Value,
                EncryptionType = Device.EncryptionType!.Value
            };
            Config.Devices.Add(DeviceConfig);
            Config.Save();
        }
        
        _widgets = _allWidgets
            .Select(x => DeviceConfig.Widgets.TryGetValue(x.Name, out var json) 
                ? (IWidget)json.Deserialize(x, JsonContext.Default)!
                : (IWidget)Activator.CreateInstance(x)!)
            .Where(x => x.Features.Length == 0 || Client.Support!.Features.Any(y => x.Features.Contains(y))).ToList();
        var supported = _widgets.SelectMany(x => x.Features);
        _notSupported = Client.Support!.Features.Where(x => !supported.Contains(x)).ToList();
        if (_notSupported.Count > 0)
            Log.Warning("Some features are not supported: {0}", string.Join(", ", _notSupported));
        foreach (var widget in _widgets)
            try { widget.ReadSettings(this); }
            catch (Exception e) { Log.Error("Widget threw an exception: {0}", e); }
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        if (Client.Connected) {
            ImGui.Text("TODO: redo this whole thing");
            foreach (var widget in _widgets)
                try { widget.Render(this, MyGui.Renderer); }
                catch (Exception e) { Log.Error("Widget threw an exception: {0}", e); }
            
            if (_notSupported.Count > 0) {
                ImGui.SeparatorText("Missing features");
                ImGui.TextWrapped(string.Join(", ", _notSupported));
            }

            if (_frames < 2) _frames++;
            var check = DeviceConfig!.RestoreSettings;
            ImGui.Checkbox("Restore settings", ref check);
            if (!check && DeviceConfig.RestoreSettings) {
                DeviceConfig.RestoreSettings = false;
                Config.Save();
            }
            if (check && !DeviceConfig.RestoreSettings) {
                DeviceConfig.RestoreSettings = true;
                Config.Save();
            }
                    
            ImGui.End();
            return;
        } 
            
        ImGui.Text("Connecting, please wait...");
        ImGui.End();
    }
    
    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    private void PacketReceived(PacketType type, IPacketData? data) {
        foreach (var widget in _widgets)
            try { if (widget.PacketReceived(this, type, data)) return; }
            catch (Exception e) { Log.Error("Widget threw an exception: {0}", e); }
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