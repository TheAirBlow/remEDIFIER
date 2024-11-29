namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth communication method
/// </summary>
public interface IBluetooth {
    /// <summary>
    /// Writes a packet
    /// </summary>
    /// <param name="buf">Buffer</param>
    public void Write(byte[] buf);

    /// <summary>
    /// Disconnects from the device
    /// </summary>
    public void Disconnect();

    /// <summary>
    /// Data received event
    /// </summary>
    public event DataReceivedDelegate? DataReceived;

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
    /// Generic empty delegate
    /// </summary>
    public delegate void GenericDelegate();
    
    /// <summary>
    /// Error message delegate
    /// </summary>
    public delegate void ErrorDelegate(string message, int code);
    
    /// <summary>
    /// Data received delegate
    /// </summary>
    public delegate void DataReceivedDelegate(byte[] buf);
}