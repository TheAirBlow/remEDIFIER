using remEDIFIER.Bluetooth;

namespace remEDIFIER;

/// <summary>
/// Discovered device information
/// </summary>
public class DiscoveredDevice {
    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; private set; }
        
    /// <summary>
    /// Device information
    /// </summary>
    public DeviceInfo Info { get; private set; }
        
    /// <summary>
    /// Icon name to show
    /// </summary>
    public string Icon { get; private set; }
    
    /// <summary>
    /// Display status
    /// </summary>
    public string? Status { get; set; }
        
    /// <summary>
    /// Protocol version
    /// </summary>
    public int? ProtocolVersion { get; set; }
    
    /// <summary>
    /// Encryption type
    /// </summary>
    public EncryptionType? EncryptionType { get; set; }
    
    /// <summary>
    /// Bluetooth classic MAC address
    /// </summary>
    public string? ClassicAddress { get; private set; }
    
    /// <summary>
    /// Product information
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// Edifier client for this device
    /// </summary>
    public EdifierClient Client { get; set; } = new();

    /// <summary>
    /// Creates a new discovered device
    /// </summary>
    /// <param name="info">Device Info</param>
    public DiscoveredDevice(DeviceInfo info) {
        Info = info;
        if (info.IsLowEnergyDevice) {
            var valid = info.ServiceUuids!.FirstOrDefault(x => Product.Products.Any(y => y.ProductSearchUuid == x));
            if (valid != null) {
                if (info.ManufacturerData != null)
                    if (info.ManufacturerData.Length < 6) {
                        EncryptionType = remEDIFIER.EncryptionType.None;
                        ProtocolVersion = 1;
                    } else if (info.ManufacturerData.Length > 6) {
                        ClassicAddress = string.Join(":", info.ManufacturerData[..6].Select(x => Convert.ToHexString([x])));
                        EncryptionType = (EncryptionType)info.ManufacturerData[^1];
                        ProtocolVersion = info.ManufacturerData[^2];
                    } else {
                        ClassicAddress = string.Join(":", info.ManufacturerData[..6].Select(x => Convert.ToHexString([x])));
                        EncryptionType = remEDIFIER.EncryptionType.None;
                        ProtocolVersion = 1;
                    }
                
                Product = Product.Products.First(x => x.ProductSearchUuid == valid);
                DisplayName = $"{Product.ProductName} (BLE)";
                Icon = "edifier";
                return;
            }

            DisplayName = info.DeviceName;
            Icon = "bluetooth";
            return;
        }
            
        DisplayName = info.DeviceName;
        Icon = info.MajorDeviceType == 4 ? "headphones" : "bluetooth";
    }

    /// <summary>
    /// Updates device with information from BLE device
    /// </summary>
    public void UpdateFromBLE(DiscoveredDevice device) {
        if (device.Info.IsLowEnergyDevice || device.Product == null) return;
        DisplayName = $"{device.Product.ProductName} (SPP)";
        ClassicAddress = device.ClassicAddress;
        EncryptionType = device.EncryptionType;
        ProtocolVersion = device.ProtocolVersion;
        Product = device.Product;
        Icon = "edifier";
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