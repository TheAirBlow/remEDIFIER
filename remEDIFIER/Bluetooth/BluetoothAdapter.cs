using System.Runtime.InteropServices;

namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth adapter
/// </summary>
public partial class BluetoothAdapter {
    /// <summary>
    /// Unmanaged class instance
    /// </summary>
    private readonly IntPtr _wrapper;
    
    /// <summary>
    /// Is Bluetooth is available
    /// </summary>
    public bool BluetoothAvailable => IsBluetoothAvailable(_wrapper);

    /// <summary>
    /// Bluetooth adapter mac address
    /// </summary>
    public string? AdapterAddress {
        get {
            var ptr = GetAdapterAddress(_wrapper);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAuto(ptr);
        }
    }
    
    /// <summary>
    /// Adapter enabled event
    /// </summary>
    public event AddressDelegate? AdapterEnabled;
    
    /// <summary>
    /// Adapter disabled event
    /// </summary>
    public event AddressDelegate? AdapterDisabled;

    /// <summary>
    /// Creates a new bluetooth adapter
    /// </summary>
    public BluetoothAdapter() {
        _wrapper = CreateBluetoothAdapter();
        SetAdapterDisabledCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new AddressCallback(DisabledCallback)));
        SetAdapterEnabledCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new AddressCallback(EnabledCallback)));
    }
    
    /// <summary>
    /// Fetches mac addresses of connected devices
    /// </summary>
    /// <returns>Mac address array</returns>
    public string[] GetConnectedDevices() {
        var st = GetConnectedDevices(_wrapper);
        if (st == IntPtr.Zero) return [];
        var info = Marshal.PtrToStructure<ConnectedDevices>(st);
        var addresses = new string[info.Length];
        for (var i = 0; i < info.Length; i++) {
            var ptr = Marshal.ReadIntPtr(info.Addresses, i * IntPtr.Size);
            addresses[i] = Marshal.PtrToStringAuto(ptr)!;
        }

        return addresses;
    }
    
    /// <summary>
    /// Internal adapter enabled callback
    /// </summary>
    /// <param name="address">Address</param>
    private void EnabledCallback(IntPtr address) {
        var str = Marshal.PtrToStringAuto(address)!;
        AdapterEnabled?.Invoke(str);
    }
    
    /// <summary>
    /// Internal adapter Disabled callback
    /// </summary>
    /// <param name="address">Address</param>
    private void DisabledCallback(IntPtr address) {
        var str = Marshal.PtrToStringAuto(address)!;
        AdapterDisabled?.Invoke(str);
    }
    
    /// <summary>
    /// Adapter address delegate
    /// </summary>
    public delegate void AddressDelegate(string address);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void AddressCallback(IntPtr address);
    
    [LibraryImport("comhelper")]
    private static partial IntPtr CreateBluetoothAdapter();
    
    [LibraryImport("comhelper")]
    private static partial IntPtr SetAdapterEnabledCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper")]
    private static partial IntPtr SetAdapterDisabledCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper")]
    private static partial bool IsBluetoothAvailable(IntPtr wrapper);
    
    [LibraryImport("comhelper")]
    private static partial IntPtr GetAdapterAddress(IntPtr wrapper);
    
    [LibraryImport("comhelper")]
    private static partial IntPtr GetConnectedDevices(IntPtr wrapper);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct ConnectedDevices {
        public IntPtr Addresses;
        public uint Length;
    }
}