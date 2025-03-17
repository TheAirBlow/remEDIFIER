using remEDIFIER.Bluetooth;

namespace remEDIFIER.Device;

/// <summary>
/// Edifier device information
/// </summary>
public class EdifierDevice {
    /// <summary>
    /// Display name in discovery
    /// </summary>
    public string DisplayName { get; private set; }
    
    /// <summary>
    /// Icon to show in discovery
    /// </summary>
    public string Icon { get; private set; }
    
    /// <summary>
    /// Display status in discovery
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Bluetooth device information
    /// </summary>
    public BluetoothDevice Info { get; }
    
    /// <summary>
    /// Device configuration
    /// </summary>
    public DeviceConfig? Config { get; private set; }
        
    /// <summary>
    /// Bluetooth low energy information
    /// </summary>
    public LowEnergyInfo? Extra { get; private set; }
    
    /// <summary>
    /// Device state
    /// </summary>
    public DeviceState? State { get; set; }

    /// <summary>
    /// Edifier client for this device
    /// </summary>
    public EdifierClient Client { get; } = new();

    /// <summary>
    /// Creates a new discovered device
    /// </summary>
    /// <param name="device">Device Info</param>
    public EdifierDevice(BluetoothDevice device) {
        Info = device;
        Config = Configuration.Config.Devices.FirstOrDefault(
            x => x.MacAddress == Info.MacAddress);
        if (device.IsLowEnergyDevice) {
            Extra = LowEnergyInfo.Parse(device);
            if (Extra != null) {
                DisplayName = $"{Extra.Product.ProductName} (BLE)";
                Icon = "edifier"; CreateConfig();
                return;
            }

            DisplayName = device.DeviceName;
            Icon = "bluetooth";
            return;
        }

        Extra ??= LowEnergyInfo.FromConfig(Config);
        if (Extra != null) {
            DisplayName = $"{Extra.Product.ProductName} (SPP)";
            Icon = "edifier";
            return;
        }

        DisplayName = device.DeviceName;
        Icon = device.MajorDeviceType == 4 ? "headphones" : "bluetooth";
    }

    /// <summary>
    /// Updates device with information from BLE device
    /// </summary>
    public void UpdateFromBLE(EdifierDevice device) {
        if (device.Extra?.MacAddress != Info.MacAddress
            || !device.Info.IsLowEnergyDevice
            || device.Extra?.Product == null) return;
        DisplayName = $"{device.Extra.Product.ProductName} (SPP)";
        Extra = device.Extra;
        Icon = "edifier";
        CreateConfig();
    }
    
    /// <summary>
    /// Creates device config
    /// </summary>
    private void CreateConfig() {
        if (Extra == null || Config != null) return;
        Config = new DeviceConfig {
            ProtocolVersion = Extra.ProtocolVersion,
            EncryptionType = Extra.EncryptionType,
            ProductId = Extra.Product.Id,
            MacAddress = Info.MacAddress
        };
        Configuration.Config.Devices.Add(Config);
        Configuration.Config.Save();
    }
}

/// <summary>
/// Encryption type
/// </summary>
public enum EncryptionType {
    /// <summary>
    /// No encryption
    /// </summary>
    None = 0x00,
    
    /// <summary>
    /// XOR payload encryption
    /// </summary>
    XOR = 0x10
}