using System.Runtime.InteropServices;

namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth low energy agent
/// </summary>
public partial class BluetoothLowEnergy : IBluetooth {
    /// <summary>
    /// Unmanaged class instance
    /// </summary>
    private readonly IntPtr _wrapper;
    
    /// <summary>
    /// Is currently connected
    /// </summary>
    private bool _isConnected;
    
    /// <summary>
    /// Data received event
    /// </summary>
    public event IBluetooth.DataReceivedDelegate? DataReceived;

    /// <summary>
    /// Device connected event
    /// </summary>
    public event GenericDelegate? DeviceConnected;
    
    /// <summary>
    /// Device disconnected event
    /// </summary>
    public event GenericDelegate? DeviceDisconnected;

    /// <summary>
    /// Error occured event
    /// </summary>
    public event ErrorDelegate? ErrorOccured;

    /// <summary>
    /// Creates a new bluetooth low energy agent
    /// </summary>
    public BluetoothLowEnergy() {
        _wrapper = CreateBluetoothLowEnergy();
        SetConnectedCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new GenericCallback(ConnectedHandler)));
        SetDisconnectedCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new GenericCallback(DisconnectedHandler)));
        SetErrorCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new ErrorCallback(ErrorHandler)));
        SetDataCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(new DataCallback(DataHandler)));
    }

    /// <summary>
    /// Connects to a bluetooth device
    /// </summary>
    /// <param name="address">Mac address</param>
    /// <param name="serviceUuid">Service UUID</param>
    /// <param name="writeUuid">Write UUID</param>
    /// <param name="readUuid">Read UUID</param>
    public void Connect(string address, string serviceUuid, string writeUuid, string readUuid) {
        if (_isConnected) throw new InvalidOperationException(
            "Agent is already connected to a bluetooth device");
        Connect(_wrapper, Marshal.StringToHGlobalAuto(address), Marshal.StringToHGlobalAuto(serviceUuid),
            Marshal.StringToHGlobalAuto(writeUuid), Marshal.StringToHGlobalAuto(readUuid));
    }

    /// <summary>
    /// Disconnects from current device
    /// </summary>
    public void Disconnect() {
        if (!_isConnected) throw new InvalidOperationException(
            "Agent is not connected to a bluetooth device");
        Disconnect(_wrapper);
    }
    
    /// <summary>
    /// Writes a packet
    /// </summary>
    /// <param name="buf">Buffer</param>
    public unsafe void Write(byte[] buf) {
        if (!_isConnected) throw new InvalidOperationException(
            "Agent is not connected to a bluetooth device");
        fixed (byte* ptr = buf) Write(_wrapper, (IntPtr)ptr, (uint)buf.Length);
    }

    /// <summary>
    /// Internal device disconnected handler
    /// </summary>
    private void DisconnectedHandler() {
        _isConnected = false;
        DeviceDisconnected?.Invoke();
    }
    
    /// <summary>
    /// Internal device connected handler
    /// </summary>
    private void ConnectedHandler() {
        _isConnected = true;
        DeviceConnected?.Invoke();
    }

    /// <summary>
    /// Internal error occured handler
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="code">Code</param>
    private void ErrorHandler(IntPtr message, int code) {
        _isConnected = false;
        var str = Marshal.PtrToStringAuto(message);
        ErrorOccured?.Invoke(str!, code);
    }

    /// <summary>
    /// Internal data received handler
    /// </summary>
    /// <param name="data">Buffer</param>
    /// <param name="length">Length</param>
    private void DataHandler(IntPtr data, uint length) {
        var buf = new byte[length];
        Marshal.Copy(data, buf, 0, buf.Length);
        DataReceived?.Invoke(buf);
    }
    
    /// <summary>
    /// Generic empty delegate
    /// </summary>
    public delegate void GenericDelegate();
    
    /// <summary>
    /// Error message delegate
    /// </summary>
    public delegate void ErrorDelegate(string message, int code);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DataCallback(IntPtr data, uint length);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ErrorCallback(IntPtr message, int code);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GenericCallback();
    
    [LibraryImport("comhelper")]
    private static partial IntPtr CreateBluetoothLowEnergy();
    
    [DllImport("comhelper", EntryPoint = "LowEnergyConnect")]
    private static extern void Connect(IntPtr wrapper, IntPtr address, IntPtr serviceUuid, IntPtr writeUuid, IntPtr readUuid);
    
    [LibraryImport("comhelper", EntryPoint = "LowEnergyDisconnect")]
    private static partial void Disconnect(IntPtr wrapper);
    
    [LibraryImport("comhelper", EntryPoint = "SetLowEnergyDisconnectedCallback")]
    private static partial void SetDisconnectedCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetLowEnergyConnectedCallback")]
    private static partial void SetConnectedCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetLowEnergyErrorCallback")]
    private static partial void SetErrorCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetLowEnergyDataCallback")]
    private static partial void SetDataCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "LowEnergyWrite")]
    private static partial void Write(IntPtr wrapper, IntPtr data, uint length);
}