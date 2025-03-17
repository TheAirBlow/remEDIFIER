using remEDIFIER.Bluetooth;

namespace remEDIFIER.Device;

/// <summary>
/// Information parsed from BLE manufacturer data
/// </summary>
public class LowEnergyInfo {
    /// <summary>
    /// Protocol version
    /// </summary>
    public int ProtocolVersion { get; private set; }

    /// <summary>
    /// Encryption type
    /// </summary>
    public EncryptionType EncryptionType { get; private set; }

    /// <summary>
    /// Bluetooth classic MAC address
    /// </summary>
    public string MacAddress { get; private set; } = null!;

    /// <summary>
    /// Product information
    /// </summary>
    public Product Product { get; private set; } = null!;

    /// <summary>
    /// Private empty constructor
    /// </summary>
    private LowEnergyInfo() { }

    /// <summary>
    /// Parses BLE manufacturer data
    /// </summary>
    /// <param name="device">Device</param>
    /// <returns>Low energy info</returns>
    public static LowEnergyInfo? Parse(BluetoothDevice device) {
        if (!device.IsLowEnergyDevice) return null;
        var valid = device.ServiceUuids!.FirstOrDefault(x => Product.Products.Any(y => y.ProductSearchUuid == x));
        var data = device.ManufacturerData;
        if (valid == null || data == null) return null;
        var info = new LowEnergyInfo {
            Product = Product.Products.First(x => x.ProductSearchUuid == valid)
        };

        var id = device.ManufacturerId!;
        if (id != 2016) data = [(byte)(id & 255), (byte)((id >> 8) & 255), ..data];
        if (data.Length < 6) return null;
        if (data.Length > 6) {
            info.MacAddress = string.Join(":", data[..6].Select(x => Convert.ToHexString([x])));
            info.EncryptionType = (EncryptionType)data[^1];
            info.ProtocolVersion = data[^2];
        } else {
            info.MacAddress = string.Join(":", data[..6].Select(x => Convert.ToHexString([x])));
            info.EncryptionType = EncryptionType.None;
            info.ProtocolVersion = 1;
        }

        return info;
    }

    /// <summary>
    /// Copies BLE information from device config
    /// </summary>
    /// <param name="config">Device config</param>
    /// <returns>Low energy info</returns>
    public static LowEnergyInfo? FromConfig(DeviceConfig? config) {
        if (config == null) return null;
        var product = Product.Products.FirstOrDefault(x => x.Id == config.ProductId);
        return product == null ? null : new LowEnergyInfo {
            ProtocolVersion = config.ProtocolVersion,
            EncryptionType = config.EncryptionType,
            Product = product
        };
    }
}