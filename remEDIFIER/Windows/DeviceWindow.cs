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
public class DeviceWindow : GuiWindow {
    /// <summary>
    /// List of all widgets
    /// </summary>
    private readonly List<Type> _allWidgets = [
        typeof(InfoWidget), typeof(PlaybackWidget), typeof(EqualizerWidget), 
        typeof(DeviceNameWidget), typeof(DebugWidget)
    ];
    
    /// <summary>
    /// Bluetooth connection
    /// </summary>
    public EdifierClient Client { get; }
    
    /// <summary>
    /// Device configuration
    /// </summary>
    public Device? Device { get; private set; }

    /// <summary>
    /// List of not supported features
    /// </summary>
    private List<Feature>? _notSupported;
    
    /// <summary>
    /// Widgets to render
    /// </summary>
    private List<IWidget>? _widgets;

    /// <summary>
    /// Display name
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Frame counter for auto resize
    /// </summary>
    private int _frames = 0;

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="discovery">Discovery Window</param>
    /// <param name="info">Device Info</param>
    /// <param name="name">Display Name</param>
    public DeviceWindow(DiscoveryWindow discovery, DeviceInfo info, string name) {
        _name = name;
        Client = new EdifierClient();
        Client.DeviceConnected += () => {
            var data = Client.Send(PacketType.GetSupportedFeatures);
            if (data is not SupportData support) {
                discovery.Renderer.OpenWindow(new PopupWindow("An error occured", "Failed to receive support data"));
                Client.Disconnect();
                return;
            }

            Device = Config.Devices.FirstOrDefault(x => x.MacAddress == info.MacAddress);
            if (Device == null) {
                Device = new Device { MacAddress = info.MacAddress };
                Config.Devices.Add(Device);
                Config.Save();
            }
            _widgets = _allWidgets
                .Select(x => Device.Widgets.TryGetValue(x.Name, out var json) 
                    ? (IWidget)json.Deserialize(x, JsonContext.Default)!
                    : (IWidget)Activator.CreateInstance(x)!)
                .Where(x => x.Features.Length == 0 || support.Features.Any(y => x.Features.Contains(y))).ToList();
            Client.Send(PacketType.GetDeviceName, notify: true);
            Client.Send(PacketType.GetFirmwareVersion, notify: true);
            Client.Send(PacketType.GetMacAddress, notify: true);
            Client.Send(PacketType.AVCRPState, notify: true);
            if (support.Supports(Feature.ShowBattery))
                Client.Send(PacketType.GetBattery, notify: true);
            if (support.Supports(Feature.Equalizer))
                Client.Send(PacketType.GetEqualizer, notify: true);
            if (support.Supports(Feature.HiRes) || support.Supports(Feature.LineHiRes))
                Client.Send(PacketType.GetAudioDecoding, notify: true);
        };
        Client.DeviceDisconnected += () => {
            if (discovery.Connected.Contains(info.MacAddress))
                discovery.Connected.Remove(info.MacAddress);
            IsOpen = false;
        };
        Client.ErrorOccured += (err, code) => {
            discovery.Renderer.OpenWindow(new PopupWindow("An error occured", $"{err} ({code})"));
            if (discovery.Connected.Contains(info.MacAddress))
                discovery.Connected.Remove(info.MacAddress);
            IsOpen = false;
        };
        Client.PacketReceived += PacketReceived;
        Client.Connect(info);
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    /// <param name="renderer">Renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        if (ImGui.Begin($"{_name} ##{ID}", ref IsOpen, _frames < 2 ? ImGuiWindowFlags.AlwaysAutoResize : ImGuiWindowFlags.None)) {
            if (!IsOpen) Client.Disconnect();
            if (Client is { Connected: true, Support: null }) {
                ImGui.Text("Waiting for support data...");
                ImGui.End();
                return;
            }
                
            if (Client.Connected) {
                if (_widgets == null) {
                    ImGui.Text("Waiting for widgets...");
                    ImGui.End();
                    return;
                }
                
                foreach (var widget in _widgets)
                    widget.Render(this, renderer);

                if (_notSupported == null) {
                    var supported = _widgets.SelectMany(x => x.Features);
                    _notSupported = Client.Support!.Features.Where(x => !supported.Contains(x)).ToList();
                }
                
                if (_notSupported.Count > 0) {
                    ImGui.SeparatorText("Missing features");
                    ImGui.TextWrapped(string.Join(", ", _notSupported));
                }

                if (_frames < 2) _frames++;
                var check = Device!.AutoConnect;
                ImGui.Checkbox("Auto-connect", ref check);
                if (!check && Device.AutoConnect) {
                    Device.AutoConnect = false;
                    Config.Save();
                }
                if (check && !Device.AutoConnect) {
                    Device.AutoConnect = true;
                    Config.Save();
                }
                
                ImGui.SameLine();
                check = Device!.RestoreSettings;
                ImGui.Checkbox("Restore settings", ref check);
                if (!check && Device.RestoreSettings) {
                    Device.RestoreSettings = false;
                    Config.Save();
                }
                if (check && !Device.RestoreSettings) {
                    Device.RestoreSettings = true;
                    Config.Save();
                }
                    
                ImGui.End();
                return;
            } 
            
            ImGui.Text("Connecting, please wait...");
            ImGui.End();
        }
    }
    
    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    private void PacketReceived(PacketType type, IPacketData? data) {
        if (_widgets == null) return;
        foreach (var widget in _widgets)
            try { if (widget.PacketReceived(this, type, data)) return; }
            catch (Exception e) { Log.Error("Widget threw an exception: {0}", e); }
    }
}