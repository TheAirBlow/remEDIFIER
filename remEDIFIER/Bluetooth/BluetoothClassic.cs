using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth classic agent
/// </summary>
[SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
public partial class BluetoothClassic : IBluetooth {
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
    public event IBluetooth.GenericDelegate? DeviceConnected;
    
    /// <summary>
    /// Device disconnected event
    /// </summary>
    public event IBluetooth.GenericDelegate? DeviceDisconnected;

    /// <summary>
    /// Error occured event
    /// </summary>
    public event IBluetooth.ErrorDelegate? ErrorOccured;

    /// <summary>
    /// Connected callback
    /// </summary>
    private readonly GenericCallback _connectedCallback;
    
    /// <summary>
    /// Connected callback
    /// </summary>
    private readonly GenericCallback _disconnectedCallback;
    
    /// <summary>
    /// Connected callback
    /// </summary>
    private readonly ErrorCallback _errorCallback;
    
    /// <summary>
    /// Connected callback
    /// </summary>
    private readonly DataCallback _dataCallback;

    /// <summary>
    /// Creates a new bluetooth classic agent
    /// </summary>
    public BluetoothClassic() {
        _wrapper = CreateBluetoothClassic();
        _errorCallback = ErrorHandler; _dataCallback = DataHandler;
        _connectedCallback = ConnectedHandler; _disconnectedCallback = DisconnectedHandler;
        SetConnectedCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_connectedCallback));
        SetDisconnectedCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_disconnectedCallback));
        SetErrorCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_errorCallback));
        SetDataCallback(_wrapper, Marshal.GetFunctionPointerForDelegate(_dataCallback));
    }
    
    /// <summary>
    /// Connects to a bluetooth device
    /// </summary>
    /// <param name="address">Mac Address</param>
    public void Connect(string address) {
        if (_isConnected) throw new InvalidOperationException(
            "Agent is already connected to a bluetooth device");
        Connect(_wrapper, Marshal.StringToHGlobalAuto(address));
    }

    /// <summary>
    /// Disconnects from current device
    /// </summary>
    public void Disconnect() {
        Disconnect(_wrapper);
        DisconnectedHandler();
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
    private static partial IntPtr CreateBluetoothClassic();
    
    [LibraryImport("comhelper", EntryPoint = "ClassicConnect")]
    private static partial void Connect(IntPtr wrapper, IntPtr address);
    
    [LibraryImport("comhelper", EntryPoint = "ClassicDisconnect")]
    private static partial void Disconnect(IntPtr wrapper);
    
    [LibraryImport("comhelper", EntryPoint = "SetClassicDisconnectedCallback")]
    private static partial void SetDisconnectedCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetClassicConnectedCallback")]
    private static partial void SetConnectedCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetClassicErrorCallback")]
    private static partial void SetErrorCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "SetClassicDataCallback")]
    private static partial void SetDataCallback(IntPtr wrapper, IntPtr callback);
    
    [LibraryImport("comhelper", EntryPoint = "ClassicWrite")]
    private static partial void Write(IntPtr wrapper, IntPtr data, uint length);
}