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
    /// Data received event
    /// </summary>
    public event DataReceivedDelegate? DataReceived;
    
    /// <summary>
    /// Data received delegate
    /// </summary>
    public delegate void DataReceivedDelegate(byte[] buf);
}