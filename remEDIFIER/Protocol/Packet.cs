using System.Data;
using System.Reflection;
using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol.Packets;
using Serilog;

namespace remEDIFIER.Protocol;

/// <summary>
/// Edifier packet serializer
/// </summary>
public static class Packet {
    /// <summary>
    /// Packet type to packet data mapping
    /// </summary>
    private static readonly Dictionary<PacketType, IPacketData> _mapping = [];

    /// <summary>
    /// Initializes mapping
    /// </summary>
    static Packet() {
        var objs = Assembly.GetCallingAssembly().GetTypes()
            .Where(x => typeof(IPacketData).IsAssignableFrom(x) && x != typeof(IPacketData))
            .Select(x => (IPacketData)Activator.CreateInstance(x)!);
        foreach (var data in objs)
        foreach (var type in data.Types)
            _mapping[type] = data;
    }
    
    /// <summary>
    /// Serializes an edifier packet
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support Data</param>
    /// <param name="data">Packet Data</param>
    /// <returns>Data Buffer</returns>
    public static byte[] Serialize(PacketType type, SupportData? support = null, IPacketData? data = null) {
        if (data != null && !data.Types.Contains(type)) throw new ArgumentOutOfRangeException(
            nameof(data), $"Specified packet data type does not support {type.ToString()}");
        var dataBuf = data != null ? data.Serialize(type, support) : [];
        
        // TODO: move this somewhere else
        Log.Information(dataBuf.Length > 0 
                ? "Sent {0} with payload {1}" 
                : "Sent {0} without payload",
            type, Convert.ToHexString(dataBuf));
        
        if (support?.EncryptionType == EncryptionType.XOR)
            for (var i = 0; i < dataBuf.Length; i++)
                dataBuf[i] ^= 0xA5;
        if (support?.ProtocolVersion <= 1) {
            var buf = new byte[dataBuf.Length + 5];
            if (dataBuf.Length != 0)
                Array.Copy(dataBuf, 0, buf, 3, dataBuf.Length);
            buf[0] = 0xAA;
            buf[1] = (byte)(dataBuf.Length + 1);
            buf[2] = (byte)type;
            Hash(buf, support);
            return buf;
        } else {
            var buf = new byte[dataBuf.Length + 6];
            if (dataBuf.Length != 0)
                Array.Copy(dataBuf, 0, buf, 5, dataBuf.Length);
            buf[0] = 0xAA;
            buf[1] = 0xEC;
            buf[2] = (byte)type;
            buf[3] = (byte)(dataBuf.Length >> 8);
            buf[4] = (byte)(dataBuf.Length & 0xFF);
            Hash(buf, support);
            return buf;
        }
    }

    /// <summary>
    /// Deserializes an edifier packet
    /// </summary>
    /// <param name="buf">Packet Buffer</param>
    /// <param name="support">Support Data</param>
    /// <returns>Packet Type, Data, Raw Payload</returns>
    public static (PacketType, IPacketData?, byte[]) Deserialize(byte[] buf, SupportData? support = null) {
        if (support?.ProtocolVersion <= 1) {
            if (buf.Length < 5) throw new ArgumentOutOfRangeException(
                nameof(buf), "There must be at least 5 bytes in a packet");
            if (buf[0] != 0xBB && buf[0] != 0xCC) throw new ArgumentOutOfRangeException(
                nameof(buf), $"Invalid packet header, expected BB or CC but found {buf[0]:X2}");
            if (buf.Length != buf[1] + 4) throw new ArgumentOutOfRangeException(
                nameof(buf), $"Invalid packet length, expected {buf[1] + 4} but found {buf.Length}");
            Hash(buf, support, true);
            var type = (PacketType)buf[2];
            _mapping.TryGetValue(type, out var data);
            var payload = new byte[buf[1] - 1];
            Array.Copy(buf, 3, payload, 0, payload.Length);
            if (data == null) return (type, data, payload);
            data.Deserialize(type, support, payload);
            return (type, data, payload);
        } else {
            if (buf.Length < 6) throw new ArgumentOutOfRangeException(
                nameof(buf), "There must be at least 6 bytes in a packet");
            if (buf[0] != 0xBB && buf[0] != 0xCC) throw new ArgumentOutOfRangeException(
                nameof(buf), $"Invalid packet header, expected BB or CC but found {buf[0]:X2}");
            if (buf[1] != 0xEC) throw new ArgumentOutOfRangeException(
                nameof(buf), $"Invalid app code, expected EC but found {buf[1]:X2}");
            var length = (buf[3] << 8) | buf[4];
            if (buf.Length != length + 6) throw new ArgumentOutOfRangeException(
                nameof(buf), $"Invalid packet length, expected {length + 6} but found {buf.Length}");
            Hash(buf, support, true);
            var type = (PacketType)buf[2];
            _mapping.TryGetValue(type, out var data);
            var payload = new byte[length];
            Array.Copy(buf, 5, payload, 0, payload.Length);
            if (support?.EncryptionType == EncryptionType.XOR)
                for (var i = 0; i < payload.Length; i++)
                    payload[i] ^= 0xA5;
            if (data == null) return (type, data, payload);
            data.Deserialize(type, support, payload);
            return (type, data, payload);
        }
    }

    /// <summary>
    /// Sets or verifies signature
    /// </summary>
    /// <param name="buf">Buffer</param>
    /// <param name="support">Support data</param>
    /// <param name="verify">Verify</param>
    public static void Hash(byte[] buf, SupportData? support = null, bool verify = false) {
        var signSize = support?.ProtocolVersion <= 1 ? 2 : 1;
        var sum = (ushort)(signSize == 2 ? 8217 : 0);
        for (var i = 0; i < buf.Length - signSize; i++)
            sum += buf[i];
        var val1 = (sum >> 8) & 255;
        var val2 = sum & 255;
        if (verify) {
            if (buf[^1] != val2 || (signSize > 1 && buf[^2] != val1))
                Log.Warning("Invalid signature, expected {0:X2}{1:X2} but found {2:X2}{3:X2} in {4} (sign size: {5})",
                    val1, val2, buf[^2], buf[^1], Convert.ToHexString(buf), signSize);
            return;
        }

        if (signSize > 1)
            buf[^2] = (byte)val1;
        buf[^1] = (byte)val2;
    }
}