using static remEDIFIER.Configuration;
using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Raylib_ImGui.Windows;
using remEDIFIER.Bluetooth;
using remEDIFIER.Device;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Bluetooth discovery window
/// </summary>
public class DiscoveryWindow : ManagedWindow {
    /// <summary>
    /// con to show in the top bar
    /// </summary>
    public override string Icon => "bluetooth";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Discovery";
    
    /// <summary>
    /// Bluetooth adapter instance
    /// </summary>
    public BluetoothAdapter Adapter { get; } = new();

    /// <summary>
    /// Bluetooth discovery instance
    /// </summary>
    private readonly BluetoothDiscovery _discovery = new();

    /// <summary>
    /// List of discovered devices
    /// </summary>
    private readonly List<EdifierDevice> _discovered = [];

    /// <summary>
    /// Device that we're connecting to
    /// </summary>
    private EdifierDevice? _device;
    
    /// <summary>
    /// Is bluetooth available
    /// </summary>
    private bool _bluetoothAvailable;

    /// <summary>
    /// Should all bluetooth devices be shown
    /// </summary>
    private bool _showAll;
    
    /// <summary>
    /// Creates a new discovery window
    /// </summary>
    public DiscoveryWindow() {
        _discovery.DiscoveryFinished += () => {
            if (Hidden) return;
            _discovery.StartDiscovery();
        };
        
        _discovery.DeviceDiscovered += info => {
            Log.Information("Discovered {0} ({1}, BLE: {2})",
                info.DeviceName, info.MacAddress, info.IsLowEnergyDevice);
            lock (_discovered) {
                if (_discovered.Any(x => x.Info.MacAddress == info.MacAddress)) return;
                var device = new EdifierDevice(info);
                if (device.Extra != null)
                    _discovered.FirstOrDefault(x => x.Info.MacAddress == device.Extra.MacAddress)?.UpdateFromBLE(device);
                var ble = _discovered.FirstOrDefault(x => x.Extra?.MacAddress == info.MacAddress);
                if (ble != null) device.UpdateFromBLE(ble);
                _discovered.Add(device);
            }
        };
        
        Adapter.AdapterDisabled += _ => {
            _bluetoothAvailable = false;
            _discovery.StopDiscovery();
        };
        
        Adapter.AdapterEnabled += _ => {
            _bluetoothAvailable = true; 
            _discovery.StartDiscovery();
        };
        
        if (Adapter.BluetoothAvailable) {
            _discovery.StartDiscovery();
        }
        
        _bluetoothAvailable = Adapter.BluetoothAvailable;
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        if (_bluetoothAvailable) {
            MyGui.PushContentRegion();
            MyGui.Text(_showAll ? "Show only compatible devices >>" : "Show incompatible devices >>", 20, Color.DarkGray);
            if (ImGui.IsItemClicked()) _showAll = !_showAll;
            ImGui.SameLine();
            MyGui.SetNextMargin(right: 5);
            MyGui.SetNextCentered(1f);
            MyGui.Image("refresh", Scaler.Fit(16, 16));
            if (ImGui.IsItemClicked()) {
                _discovery.StopDiscovery();
                _discovery.StartDiscovery();
                lock (_discovered)
                    _discovered.Clear();
            }
            
            List<EdifierDevice> discovered;
            lock (_discovered) discovered = _discovered.ToList();
            if (!discovered.Any(x => _showAll || x is { Extra: not null })) {
                MyGui.SetNextCentered(0.5f, 0.5f);
                MyGui.TextWrapped(
                    "No devices have been found yet!\n" +
                    "This might take a bit of time,\n" +
                    "so please be patient.");
            } else {
                foreach (var device in discovered.Where(x => _showAll || x is { Extra: not null })) {
                    ImGui.BeginChild(device.Info.MacAddress,
                        new Vector2(ImGui.GetIO().DisplaySize.X - 10, 40 + (device.Status != null ? 18 : 0)));
                    MyGui.PushContentRegion();
                    MyGui.SetNextCentered(0f, 0.5f);
                    MyGui.Image(device.Icon, Scaler.Fit(28, 28));
                    ImGui.SameLine();
                    MyGui.SetNextCentered(0f, 0.5f);
                    MyGui.Wrapped(() => {
                        ImGui.BeginGroup();
                        MyGui.Text(device.DisplayName, 24);
                        if (device.Status != null)
                            MyGui.Text(device.Status, 18, Color.DarkGray);
                        ImGui.EndGroup();
                    });
                    ImGui.SameLine();
                    MyGui.SetNextMargin(right: 5);
                    MyGui.SetNextCentered(1f, 0.5f);
                    MyGui.Image("angle-right", Scaler.Fit(18, 18));
                    MyGui.PopContentRegion();
                    ImGui.EndChild();
                    if (ImGui.IsItemClicked()) 
                        Connect(device);
                    ImGui.Separator();
                }
            }
            
            MyGui.PopContentRegion();
        } else {
            MyGui.SetNextCentered(0.5f, 0.5f);
            MyGui.Text("Bluetooth is not available!\nMake sure you have it enabled.");
        }
    }
    
    /// <summary>
    /// Connects to a device
    /// </summary>
    /// <param name="device">Device Info</param>
    private void Connect(EdifierDevice device) {
        if (_device != null)
            if (_device.Client.Connected)
                _device.Client.Disconnect();
            else DeviceDisconnected();
        
        _device = device;
        RegisterEvents(device);
        device.Status = "Connecting, please wait...";
        device.Client.Connect(device, Adapter);
    }

    /// <summary>
    /// Handles device connection
    /// </summary>
    private void DeviceConnected() {
        if (_device == null) {
            Log.Warning("Received connected event with no device chosen");
            return;
        } 
        
        var data = _device.Client.Send(PacketType.GetSupportedFeatures);
        if (data is not SupportData) {
            MyGui.Renderer.OpenWindow(new PopupWindow("Failed to connect", "Failed to receive support data"));
            _device.Client.Disconnect();
            _device.Status = null;
            _device = null;
            return;
        }
            
        var window = new DeviceInfoWindow(_device);
        Manager.OpenWindow(window);
        _device.Status = "Connected";
    }
    
    /// <summary>
    /// Handles device disconnection
    /// </summary>
    private void DeviceDisconnected() {
        if (_device == null) return;
        UnregisterEvents(_device);
        _device.Status = null;
        _device = null;
    }
    
    /// <summary>
    /// Handles bluetooth errors
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="code">Error code</param>
    private void ErrorOccured(string message, int code) {
        MyGui.Renderer.OpenWindow(new PopupWindow("Failed to connect", $"{message} ({code})"));
    }

    /// <summary>
    /// Registers events for edifier device
    /// </summary>
    /// <param name="device">Device</param>
    private void RegisterEvents(EdifierDevice device) {
        device.Client.DeviceDisconnected += DeviceDisconnected;
        device.Client.DeviceConnected += DeviceConnected;
        device.Client.ErrorOccured += ErrorOccured;
    }
    
    /// <summary>
    /// Unregisters events for edifier device
    /// </summary>
    /// <param name="device">Device</param>
    private void UnregisterEvents(EdifierDevice device) {
        device.Client.DeviceDisconnected -= DeviceDisconnected;
        device.Client.DeviceConnected -= DeviceConnected;
        device.Client.ErrorOccured -= ErrorOccured;
    }

    /// <summary>
    /// Starts discovery
    /// </summary>
    public override void OnShown()
        => _discovery.StartDiscovery();

    /// <summary>
    /// Starts discovery
    /// </summary>
    public override void OnHidden()
        => _discovery.StopDiscovery();
}