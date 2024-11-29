using static remEDIFIER.Configuration;
using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER.Bluetooth;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Bluetooth discovery window
/// </summary>
public class DiscoveryWindow : GuiWindow {
    /// <summary>
    /// Bluetooth adapter instance
    /// </summary>
    private readonly BluetoothAdapter _adapter = new();

    /// <summary>
    /// Bluetooth discovery instance
    /// </summary>
    private readonly BluetoothDiscovery _discovery = new();

    /// <summary>
    /// List of discovered devices
    /// </summary>
    private readonly List<DiscoveredDevice> _discovered = [];

    /// <summary>
    /// Selected discovered device
    /// </summary>
    private DiscoveredDevice? _selectedDevice;

    /// <summary>
    /// ImGui renderer instance
    /// </summary>
    private readonly ImGuiRenderer _renderer;
    
    /// <summary>
    /// Is bluetooth available
    /// </summary>
    private bool _bluetoothAvailable;
    
    /// <summary>
    /// Is currently discovering
    /// </summary>
    private bool _discovering;
    
    /// <summary>
    /// Creates a new discovery window
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    public DiscoveryWindow(ImGuiRenderer renderer) {
        _renderer = renderer;
        _discovery.DiscoveryFinished += () => {
            if (!_discovering) return;
            _discovered.Clear();
            _selectedDevice = null;
            _discovery.StartDiscovery();
        };

        _discovery.DeviceDiscovered += info => {
            _discovered.Add(new DiscoveredDevice(info));
            var connected = _adapter.GetConnectedDevices();
            if (connected.All(x => _discovered.Any(y => y.Info.MacAddress == x)))
                AutoConnect();
        };
        
        _adapter.AdapterDisabled += _ => {
            _bluetoothAvailable = false;
            _discovery.StopDiscovery();
        };
        
        _adapter.AdapterEnabled += _ => {
            _bluetoothAvailable = true; 
            _discovery.StartDiscovery();
            _discovering = true;
        };
        
        if (_adapter.BluetoothAvailable) {
            _discovery.StartDiscovery();
            _discovering = true;
        }
        
        _bluetoothAvailable = _adapter.BluetoothAvailable;
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    /// <param name="renderer">Renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        if (ImGui.Begin($"Bluetooth Discovery ##{ID}", ImGuiWindowFlags.AlwaysAutoResize)) {
            if (_bluetoothAvailable) {
                ImGui.Text("Select a device to connect to:");
                foreach (var device in _discovered.ToList()) {
                    ImGui.BeginGroup();
                    ImGui.Image(device.Icon, new Vector2(24, 24));
                    ImGui.SameLine();
                    if (ImGui.Selectable(device.DisplayName, _selectedDevice == device))
                        _selectedDevice = device;
                    ImGui.EndGroup();
                }
                ImGui.BeginDisabled(_selectedDevice == null);
                if (ImGui.Button("Connect"))
                    Connect(_selectedDevice!);
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
        foreach (var device in _discovered.OrderBy(x => !x.Info.IsLowEnergyDevice)
                     .Where(x => Config.ConnectedToBefore.Contains(x.Info.MacAddress))) {
            if ((!Config.AutoConnectOverClassic || device.Info.IsLowEnergyDevice)
                && (!Config.AutoConnectOverLowEnergy || !device.Info.IsLowEnergyDevice)) continue;
            Log.Information("Found device for automatic connection");
            Connect(device);
            return;
        }
    }
    
    /// <summary>
    /// Connects to a device
    /// </summary>
    private void Connect(DiscoveredDevice device) {
        Log.Information("Connecting to {0} ({1}, BLE: {2})", device.Info.DeviceName, device.Info.MacAddress, device.Info.IsLowEnergyDevice);
        _renderer?.OpenWindow(new DeviceWindow(_renderer, device.Info, device.DisplayName));
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