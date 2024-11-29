using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER;

/// <summary>
/// Edifier bluetooth client
/// </summary>
public class EdifierClient {
    /// <summary>
    /// Dictionary of packets waiting to be passed over
    /// </summary>
    private readonly List<PacketWrapper> _packets = [];
    
    /// <summary>
    /// Bluetooth connection
    /// </summary>
    private IBluetooth? _bluetooth;
    
    /// <summary>
    /// Support data if available
    /// </summary>
    public SupportData? Support { get; private set; }
    
    /// <summary>
    /// Is client currently connected
    /// </summary>
    public bool Connected { get; private set; }
    
    /// <summary>
    /// Data received event
    /// </summary>
    public event PacketReceivedDelegate? PacketReceived;

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
    /// <param name="info">Device Info</param>
    public void Connect(DeviceInfo info) {
        if (_bluetooth != null) throw new InvalidOperationException("Connection is already in progress");
        if (Connected) throw new InvalidOperationException("Device is already connected");
        if (info.IsLowEnergyDevice) {
            var lowEnergy = new BluetoothLowEnergy();
            _bluetooth = lowEnergy; RegisterEvents();
            var valid = info.ServiceUuids!.FirstOrDefault(x => Product.Products.Any(y => y.ProductSearchUuid == x));
            if (valid == null) throw new InvalidDataException($"{info.DeviceName} is not an Edifier product");
            var product = Product.Products.First(x => x.ProductSearchUuid == valid);
            lowEnergy.Connect(info.MacAddress, product.ProductServiceUuid, 
                product.ProductWriteUuid, product.ProductReadUuid);
            return;
        }

        var classic = new BluetoothClassic();
        _bluetooth = classic; RegisterEvents();
        classic.Connect(info.MacAddress);
    }

    /// <summary>
    /// Disconnects from current device
    /// </summary>
    public void Disconnect() {
        if (!Connected) return;
        _bluetooth!.Disconnect();
    }
    
    /// <summary>
    /// Registers event handlers
    /// </summary>
    private void RegisterEvents() {
        _bluetooth!.DeviceConnected += () => {
            Connected = true;
            new Thread(() => DeviceConnected?.Invoke()).Start();
        };
        _bluetooth.DeviceDisconnected += () => {
            Connected = false; _bluetooth = null;
            new Thread(() => DeviceDisconnected?.Invoke()).Start();
        };
        _bluetooth.ErrorOccured += (err, code) => {
            Disconnect();
            new Thread(() => ErrorOccured?.Invoke(err, code)).Start();
        };
        _bluetooth.DataReceived += buf => {
            var (type, data) = Packet.Deserialize(buf, Support);
            Log.Information("Received {0}, data: {1}", type, Convert.ToHexString(buf[3..^2]));
            if (type == PacketType.GetSupportedFeatures) Support = (SupportData)data!;
            var target = _packets.FirstOrDefault(x => x.Type == type && !x.Received);
            if (target == null) {
                PacketReceived?.Invoke(type, data);
                return;
            }
            target.Data = data; target.Received = true;
            _packets.Remove(target);
        };
    }

    /// <summary>
    /// Sends a packet and reads the response
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="data">Packet Data</param>
    /// <param name="notify">Notify event listeners</param>
    /// <param name="wait">Wait for response</param>
    /// <returns>Packet data</returns>
    public IPacketData? Send(PacketType type, IPacketData? data = null, bool notify = false, bool wait = true) {
        if (!Connected) throw new InvalidOperationException("No device is connected");
        _bluetooth!.Write(Packet.Serialize(type, Support, data));
        if (!wait) return null;
        var wrapper = new PacketWrapper { Type = type };
        _packets.Add(wrapper);
        var token = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
        while (!wrapper.Received) {
            if (token.IsCancellationRequested) {
                Log.Warning("{0} has timed out", type);
                return null;
            }
            Thread.Sleep(10);
        }
        
        if (notify) PacketReceived?.Invoke(type, wrapper.Data);
        return wrapper.Data;
    }
    
    /// <summary>
    /// Packet received delegate
    /// </summary>
    public delegate void PacketReceivedDelegate(PacketType type, IPacketData? data);

    /// <summary>
    /// Simple packet wrapper
    /// </summary>
    private class PacketWrapper {
        /// <summary>
        /// Packet type
        /// </summary>
        public PacketType Type { get; set; }
        
        /// <summary>
        /// Packet data
        /// </summary>
        public IPacketData? Data { get; set; }
        
        /// <summary>
        /// Was the packet received
        /// </summary>
        public bool Received { get; set; }
    }
}