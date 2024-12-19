using static remEDIFIER.Configuration;
using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Bluetooth discovery window
/// </summary>
public class DiscoveryWindow : GuiWindow {
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
    private readonly List<DiscoveredDevice> _discovered = [];

    /// <summary>
    /// Devices we are connecting or connected to
    /// </summary>
    public Dictionary<string, (EdifierClient, DeviceWindow?)> Connected { get; } = [];

    /// <summary>
    /// ImGui renderer instance
    /// </summary>
    public ImGuiRenderer Renderer { get; }
    
    /// <summary>
    /// Is bluetooth available
    /// </summary>
    private bool _bluetoothAvailable;
    
    /// <summary>
    /// Selected discovered device
    /// </summary>
    private string _selectedDevice = "";
    
    /// <summary>
    /// Is currently discovering
    /// </summary>
    private bool _discovering;

    /// <summary>
    /// Was there a successful auto connection
    /// </summary>
    private bool _autoConnected;
    
    /// <summary>
    /// Creates a new discovery window
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    public DiscoveryWindow(ImGuiRenderer renderer) {
        Renderer = renderer;
        _discovery.DiscoveryFinished += () => {
            if (!_discovering) return;
            lock (_discovered)
                _discovered.Clear();
            _autoConnected = false;
            _discovery.StartDiscovery();
        };

        _discovery.DeviceDiscovered += info => {
            lock(_discovered)
                _discovered.Add(new DiscoveredDevice(info));
            if (_autoConnected) return;
            var connected = Adapter.GetConnectedDevices();
            if (connected.All(x => _discovered.Any(y => y.Info.MacAddress == x)))
                AutoConnect();
        };
        
        Adapter.AdapterDisabled += _ => {
            _bluetoothAvailable = false;
            _discovery.StopDiscovery();
        };
        
        Adapter.AdapterEnabled += _ => {
            _bluetoothAvailable = true; 
            _discovery.StartDiscovery();
            _discovering = true;
        };
        
        if (Adapter.BluetoothAvailable) {
            _discovery.StartDiscovery();
            _discovering = true;
        }
        
        _bluetoothAvailable = Adapter.BluetoothAvailable;
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    /// <param name="renderer">Renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        if (ImGui.Begin($"Bluetooth Discovery ##{ID}", ImGuiWindowFlags.AlwaysAutoResize)) {
            if (_bluetoothAvailable) {
                ImGui.Text("Select a device to connect to:");
                List<DiscoveredDevice> discovered;
                lock (_discovered) discovered = _discovered.ToList();
                foreach (var device in discovered) {
                    ImGui.BeginGroup();
                    ImGui.Image(device.Icon, new Vector2(24, 24));
                    ImGui.SameLine();
                    var name = device.DisplayName;
                    var contains = Connected.TryGetValue(device.Info.MacAddress, out var pair);
                    if (contains && pair.Item2 == null) name += " " + new string('.', (int)ImGui.GetTime() % 3 + 1);
                    ImGui.BeginDisabled(contains);
                    if (ImGui.Selectable(name, _selectedDevice == device.Info.MacAddress))
                        _selectedDevice = device.Info.MacAddress;
                    ImGui.EndDisabled();
                    ImGui.EndGroup();
                }
                ImGui.BeginDisabled(
                    discovered.All(x => x.Info.MacAddress != _selectedDevice) ||
                    Connected.ContainsKey(_selectedDevice));
                if (ImGui.Button("Connect"))
                    Connect(discovered.First(x => x.Info.MacAddress == _selectedDevice));
                ImGui.EndDisabled();
            } else {
                ImGui.Text("Bluetooth is not available!");
                ImGui.Text("Make sure you have Bluetooth enabled.");
            }
            
            ImGui.End();
        }
    }

    /// <summary>
    /// Attempts to automatically connect
    /// </summary>
    private void AutoConnect() {
        var connected = Adapter.GetConnectedDevices();
        foreach (var device in _discovered
                     .Where(x => !Connected.ContainsKey(x.Info.MacAddress) && Config.Devices.Any(y => x.Info.MacAddress == y.MacAddress && y.AutoConnect))
                     .OrderBy(x => x.Info.IsLowEnergyDevice).ThenByDescending(x => connected.Contains(x.Info.MacAddress))) {
            if ((device.Info.IsLowEnergyDevice && !Config.AutoConnectOverLowEnergy)
                || (!device.Info.IsLowEnergyDevice && !Config.AutoConnectOverClassic)) continue;
            Log.Information("Found device for automatic connection");
            Connect(device, false);
            _autoConnected = true;
            return;
        }
    }
    
    /// <summary>
    /// Connects to a device
    /// </summary>
    /// <param name="device">Device Info</param>
    /// <param name="manual">Is Manual</param>
    private void Connect(DiscoveredDevice device, bool manual = true) {
        var client = new EdifierClient();
        Connected.Add(device.Info.MacAddress, (client, null));
        client.DeviceConnected += () => {
            var data = client.Send(PacketType.GetSupportedFeatures);
            if (data is not SupportData) {
                if (manual) Renderer.OpenWindow(new PopupWindow("An error occured", "Failed to receive support data"));
                client.Disconnect();
                return;
            }
            
            var window = new DeviceWindow(client, device.DisplayName);
            Connected[device.Info.MacAddress] = (client, window);
            Renderer.OpenWindow(window);
        };
        client.DeviceDisconnected += () => {
            if (!Connected.TryGetValue(device.Info.MacAddress, out var pair)) return;
            if (pair.Item2 != null) pair.Item2.IsOpen = false;
            Connected.Remove(device.Info.MacAddress);
        };
        client.ErrorOccured += (err, code) => {
            if (manual) Renderer.OpenWindow(new PopupWindow("An error occured", $"{err} ({code})"));
            if (!Connected.TryGetValue(device.Info.MacAddress, out var pair)) return;
            if (pair.Item2 != null) pair.Item2.IsOpen = false;
            Connected.Remove(device.Info.MacAddress);
        };
        client.Connect(device.Info, Adapter);
    }

    /// <summary>
    /// Discovered device information
    /// </summary>
    private class DiscoveredDevice {
        /// <summary>
        /// Display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Device information
        /// </summary>
        public DeviceInfo Info { get; }
        
        /// <summary>
        /// Icon handle to show
        /// </summary>
        public IntPtr Icon { get; }

        /// <summary>
        /// Creates a new discovered device
        /// </summary>
        /// <param name="info">Device Info</param>
        public DiscoveredDevice(DeviceInfo info) {
            Info = info;
            if (info.IsLowEnergyDevice) {
                var valid = info.ServiceUuids!.FirstOrDefault(x => Product.Products.Any(y => y.ProductSearchUuid == x));
                if (valid != null) {
                    var product = Product.Products.First(x => x.ProductSearchUuid == valid);
                    DisplayName = $"{product.ProductName} (BLE)";
                    Icon = Images.Get("edifier");
                    return;
                }

                DisplayName = info.DeviceName;
                Icon = Images.Get("bluetooth");
                return;
            }
            
            DisplayName = info.DeviceName;
            Icon = Images.Get(info.MajorDeviceType == 4 ? "audio" : "bluetooth");
        }
    }
}