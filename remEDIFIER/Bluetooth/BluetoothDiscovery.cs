using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth device discovery agent
/// </summary>
[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
public partial class BluetoothDiscovery {
    /// <summary>
    /// Unmanaged class instance
    /// </summary>
    private readonly IntPtr _wrapper;
    
    /// <summary>
    /// New device discovered event
    /// </summary>
    public event DeviceDiscoveredDelegate? DeviceDiscovered;
    
    /// <summary>
    /// Device discovery finished event
    /// </summary>
    public event DiscoveryFinishedDelegate? DiscoveryFinished;
    
    /// <summary>
    /// Device discovered callback
    /// </summary>
    private readonly DeviceDiscoveredCallback _deviceDiscoveredCallback;
    
    /// <summary>
    /// Discovery finished callback
    /// </summary>
    private readonly DiscoveryFinishedCallback _discoveryFinishedCallback;

    /// <summary>
    /// Creates a new bluetooth discovery agent
    /// </summary>
    public BluetoothDiscovery() {
        _wrapper = CreateDiscovery();
        _deviceDiscoveredCallback = DeviceDiscoveredHandler;
        _discoveryFinishedCallback = DiscoveryFinishedHandler;
        SetDeviceDiscoveredCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_deviceDiscoveredCallback));
        SetDiscoveryFinishedCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_discoveryFinishedCallback));
    }

    /// <summary>
    /// Starts device discovery
    /// </summary>
    public void StartDiscovery()
        => StartDiscovery(_wrapper);
    
    /// <summary>
    /// Stops device discovery
    /// </summary>
    public void StopDiscovery()
        => StopDiscovery(_wrapper);

    /// <summary>
    /// Internal device discovered handler
    /// </summary>
    /// <param name="info">Device Information</param>
    private void DeviceDiscoveredHandler(DeviceInfoStruct info)
        => DeviceDiscovered?.Invoke(new DeviceInfo(info));

    /// <summary>
    /// Internal discovery finished handler
    /// </summary>
    private void DiscoveryFinishedHandler()
        => DiscoveryFinished?.Invoke();
    
    /// <summary>
    /// Device discovered delegate
    /// </summary>
    public delegate void DeviceDiscoveredDelegate(DeviceInfo info);

    /// <summary>
    /// Discovery finished delegate
    /// </summary>
    public delegate void DiscoveryFinishedDelegate();
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DeviceDiscoveredCallback(DeviceInfoStruct info);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DiscoveryFinishedCallback();
    
    [LibraryImport("comhelper")]
    private static partial IntPtr CreateDiscovery();
    
    [LibraryImport("comhelper")]
    private static partial void SetDeviceDiscoveredCallback(IntPtr wrapper, IntPtr callback);

    [LibraryImport("comhelper")]
    private static partial void SetDiscoveryFinishedCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper")]
    private static partial void StartDiscovery(IntPtr wrapper);
    
    [LibraryImport("comhelper")]
    private static partial void StopDiscovery(IntPtr wrapper);
}

/// <summary>
/// Managed device information class
/// </summary>
public class DeviceInfo {
    /// <summary>
    /// Display name
    /// </summary>
    public string DeviceName { get; }
        
    /// <summary>
    /// Mac address
    /// </summary>
    public string MacAddress { get; }
        
    /// <summary>
    /// Whether this device is BLE or Classic
    /// </summary>
    public bool IsLowEnergyDevice { get; }
        
    /// <summary>
    /// Service UUID for BLE device
    /// </summary>
    public string[]? ServiceUuids { get; }
        
    /// <summary>
    /// First manufacturer data for BLE device
    /// </summary>
    public byte[]? ManufacturerData { get; }
        
    /// <summary>
    /// First manufacturer ID for BLE device
    /// </summary>
    public ushort? ManufacturerId { get; }
    
    /// <summary>
    /// Major device type for Classic device
    /// </summary>
    public uint? MajorDeviceType { get; }
    
    /// <summary>
    /// Minor device type for Classic device
    /// </summary>
    public uint? MinorDeviceType { get; }

    /// <summary>
    /// Creates a managed version of the device info struct
    /// </summary>
    /// <param name="info">Device Information</param>
    public DeviceInfo(DeviceInfoStruct info) {
        DeviceName = info.DeviceName;
        MacAddress = info.MacAddress;
        IsLowEnergyDevice = info.IsLowEnergyDevice;
        if (!info.IsLowEnergyDevice) {
            MajorDeviceType = info.MajorDeviceType;
            MinorDeviceType = info.MinorDeviceType;
            return;
        }
        ServiceUuids = new string[info.ServiceUuidsLength];
        for (var i = 0; i < info.ServiceUuidsLength; i++) {
            var ptr = Marshal.ReadIntPtr(info.ServiceUuids, i * IntPtr.Size);
            var str = Marshal.PtrToStringAuto(ptr)!;
            ServiceUuids[i] = str[1..^1];
        }
        ManufacturerId = info.ManufacturerId;
        ManufacturerData = new byte[info.ManufacturerDataLength];
        Marshal.Copy(info.ManufacturerData, ManufacturerData, 0, ManufacturerData.Length);
    }
}
    
/// <summary>
/// Unmanaged device information struct
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DeviceInfoStruct {
    [MarshalAs(UnmanagedType.LPStr)] public string DeviceName;
    [MarshalAs(UnmanagedType.LPStr)] public string MacAddress;
    public bool IsLowEnergyDevice;
    public IntPtr ServiceUuids;
    public uint ServiceUuidsLength;
    public IntPtr ManufacturerData;
    public uint ManufacturerDataLength;
    public ushort ManufacturerId;
    public uint MajorDeviceType;
    public uint MinorDeviceType;
}