using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Device;

/// <summary>
/// Edifier bluetooth client
/// </summary>
public class EdifierClient {
    /// <summary>
    /// Discovered device
    /// </summary>
    public EdifierDevice? Device { get; private set; }
    
    /// <summary>
    /// Support data if available
    /// </summary>
    public SupportData? Support { get; private set; }
    
    /// <summary>
    /// Is client currently connected
    /// </summary>
    public bool Connected { get; private set; }
    
    /// <summary>
    /// Packet queue
    /// </summary>
    private readonly Queue<PacketWrapper> _packets = new();

    /// <summary>
    /// Packet wrapper we are currently waiting for
    /// </summary>
    private PacketWrapper? _waitingFor;
    
    /// <summary>
    /// Bluetooth connection
    /// </summary>
    private IBluetooth? _bluetooth;
    
    /// <summary>
    /// Data received event
    /// </summary>
    public event PacketReceivedDelegate? PacketReceived;
    
    /// <summary>
    /// Data timed out event
    /// </summary>
    public event PacketTimedOutDelegate? PacketTimedOut;

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
    /// Connects to an edifier device
    /// </summary>
    /// <param name="device">Device</param>
    /// <param name="adapter">Adapter</param>
    public void Connect(EdifierDevice device, BluetoothAdapter adapter) {
        if (device.Extra == null)
            throw new ArgumentException("BLE information must be available", nameof(device));
        Device = device;
        Support = new SupportData {
            Extra = device.Extra
        };
        Log.Information("Connecting to {0} ({1}, BLE: {2})",
            device.Info.DeviceName, device.Info.MacAddress, device.Info.IsLowEnergyDevice);
        if (_bluetooth != null) throw new InvalidOperationException("Connection is already in progress");
        if (Connected) throw new InvalidOperationException("Device is already connected");
        if (device.Info.IsLowEnergyDevice) {
            var lowEnergy = new BluetoothLowEnergy();
            _bluetooth = lowEnergy; RegisterEvents();
            if (device.Extra.Product == null) throw new InvalidDataException($"{device.Info.DeviceName} is not an Edifier product");
            lowEnergy.Connect(adapter.AdapterAddress!, device.Info.MacAddress,
                device.Extra.Product.ProductServiceUuid, device.Extra.Product.ProductWriteUuid,
                device.Extra.Product.ProductReadUuid);
            return;
        }

        var classic = new BluetoothClassic();
        _bluetooth = classic; RegisterEvents();
        classic.Connect(device.Info.MacAddress);
    }

    /// <summary>
    /// Disconnects from current device
    /// </summary>
    public void Disconnect() {
        if (_bluetooth == null || Device == null) return;
        Log.Information("Disconnected from {0} ({1}, BLE: {2})",
            Device.Info.DeviceName, Device.Info.MacAddress, Device.Info.IsLowEnergyDevice);
        Connected = false;
        _bluetooth.Disconnect();
        _bluetooth = null;
    }
    
    /// <summary>
    /// Registers event handlers
    /// </summary>
    private void RegisterEvents() {
        if (Device == null) return;
        _bluetooth!.DeviceConnected += () => {
            Connected = true;
            new Thread(SenderThread).Start();
            new Thread(() => DeviceConnected?.Invoke()).Start();
        };
        _bluetooth.DeviceDisconnected += () => {
            _bluetooth = null;
            Log.Information("Disconnected from {0} ({1}, BLE: {2})",
                Device.Info.DeviceName, Device.Info.MacAddress, Device.Info.IsLowEnergyDevice);
            new Thread(() => DeviceDisconnected?.Invoke()).Start();
        };
        _bluetooth.ErrorOccured += (err, code) => {
            _bluetooth = null; Connected = false;
            Log.Information("Disconnected with error {0} ({1})", err, code);
            new Thread(() => ErrorOccured?.Invoke(err, code)).Start();
            new Thread(() => DeviceDisconnected?.Invoke()).Start();
        };
        _bluetooth.DataReceived += buf => {
            var (type, data, payload) = Packet.Deserialize(buf, Support);
            Log.Information("Received {0} with payload {1}", type, Convert.ToHexString(payload));
            if (type == PacketType.GetSupportedFeatures) {
                Support = (SupportData)data!;
                Support.Extra = Device.Extra;
            }
            
            if (_waitingFor == null || _waitingFor.Type != type) {
                PacketReceived?.Invoke(type, data, payload);
                return;
            }
            
            _waitingFor.ReceivedPayload = payload;
            _waitingFor.ReceivedData = data;
            _waitingFor.State = PacketState.Received;
            if (_waitingFor.NotifyListeners)
                PacketReceived?.Invoke(type, data, payload);
            _waitingFor = null;
        };
    }

    /// <summary>
    /// Packet sender thread
    /// </summary>
    private void SenderThread() {
        while (true) {
            while (_packets.Count == 0) {
                if (!Connected) return;
                Thread.Sleep(10);
            }
            
            if (!Connected) return;
            var packet = _packets.Dequeue();
            if (packet.WaitForResponse)
                _waitingFor = packet;
            
            try {
                _bluetooth!.Write(packet.Sent);
            } catch (Exception e) {
                Log.Warning("Failed to send packet: {0}", e);
                continue;
            }
            
            packet.State = PacketState.Sent;

            // Trying to send multiple packets without waiting
            // for a response will cause the headphones to disconnect
            if (packet.WaitForResponse) {
                var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
                while (packet.State != PacketState.Received) {
                    if (!Connected) return;
                    if (token.IsCancellationRequested) {
                        Log.Warning("Receiving {0} has timed out", packet.Type);
                        new Thread(() => PacketTimedOut?.Invoke(packet.Type)).Start();
                        break;
                    }
                
                    Thread.Sleep(10);
                }
            }
        }
    }

    /// <summary>
    /// Sends a raw packet
    /// </summary>
    /// <param name="buf">Buffer</param>
    /// <returns>Packet data</returns>
    public void Send(byte[] buf) {
        if (!Connected) throw new InvalidOperationException("No device is connected");
        _bluetooth!.Write(buf);
    }

    /// <summary>
    /// Sends a packet and reads the response
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="data">Packet Data</param>
    /// <param name="notify">Notify event listeners</param>
    /// <param name="wait">Wait for response</param>
    /// <param name="wantResponse">Wants response</param>
    /// <returns>Packet data</returns>
    public IPacketData? Send(PacketType type, IPacketData? data = null, bool notify = false, bool wait = true, bool wantResponse = true)
        => Send(type, Packet.Serialize(type, Support, data), notify, wait, wantResponse).ReceivedData;

    /// <summary>
    /// Sends a packet and reads the response
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="data">Packet Data</param>
    /// <param name="notify">Notify event listeners</param>
    /// <param name="wait">Wait for response</param>
    /// <param name="wantResponse">Wants response</param>
    /// <returns>Packet data</returns>
    public PacketWrapper Send(PacketType type, byte[] data, bool notify = false, bool wait = true, bool wantResponse = true) {
        var wrapper = new PacketWrapper(type, data, notify, wait);
        _packets.Enqueue(wrapper);
        if (!wantResponse || !wait) return wrapper;
        var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
        while (wrapper.State != PacketState.Received) {
            if (token.IsCancellationRequested) return wrapper;
            Thread.Sleep(10);
        }
        
        return wrapper;
    }
    
    /// <summary>
    /// Does device support feature
    /// </summary>
    /// <param name="feature">Feature</param>
    /// <returns>True if supports</returns>
    public bool Supports(Feature feature)
        => Support?.Features.Contains(feature) ?? false;
    
    /// <summary>
    /// Packet received delegate
    /// </summary>
    public delegate void PacketReceivedDelegate(PacketType type, IPacketData? data, byte[] payload);
    
    /// <summary>
    /// Packet timed out delegate
    /// </summary>
    public delegate void PacketTimedOutDelegate(PacketType type);

    /// <summary>
    /// Simple packet wrapper
    /// </summary>
    public class PacketWrapper {
        /// <summary>
        /// Packet type
        /// </summary>
        public PacketType Type { get; set; }
        
        /// <summary>
        /// Sent data buffer
        /// </summary>
        public byte[] Sent { get; set; }
        
        /// <summary>
        /// Received payload (decrypted)
        /// </summary>
        public byte[]? ReceivedPayload { get; set; }
        
        /// <summary>
        /// Deserialized received data
        /// </summary>
        public IPacketData? ReceivedData { get; set; }
        
        /// <summary>
        /// Packet state
        /// </summary>
        public PacketState State { get; set; }
        
        /// <summary>
        /// Notify event listeners
        /// </summary>
        public bool NotifyListeners { get; set; }
        
        /// <summary>
        /// Should we wait for a response
        /// </summary>
        public bool WaitForResponse { get; set; }

        /// <summary>
        /// Creates a new packet wrapper
        /// </summary>
        /// <param name="type">Packet type</param>
        /// <param name="buf">Packet buffer</param>
        /// <param name="notify">Notify event listeners</param>
        /// <param name="wait">Wait for response</param>
        public PacketWrapper(PacketType type, byte[] buf, bool notify = false, bool wait = true) {
            Type = type; Sent = buf; NotifyListeners = notify; WaitForResponse = wait;
        }
    }
    
    /// <summary>
    /// Packet state
    /// </summary>
    public enum PacketState {
        /// <summary>
        /// Packet is queued for sending
        /// </summary>
        Queued,
        
        /// <summary>
        /// Packet was sent
        /// </summary>
        Sent,
        
        /// <summary>
        /// Received response
        /// </summary>
        Received
    }
}