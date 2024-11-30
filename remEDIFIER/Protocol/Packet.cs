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
        var buf = new byte[dataBuf.Length + 5];
        if (dataBuf.Length != 0)
            Array.Copy(dataBuf, 0, buf, 3, dataBuf.Length);
        buf[0] = 0xAA;
        buf[1] = (byte)(dataBuf.Length + 1);
        buf[2] = (byte)type;
        Sign(buf);
        return buf;
    }

    /// <summary>
    /// Deserializes an edifier packet
    /// </summary>
    /// <param name="buf">Packet Buffer</param>
    /// <param name="support">Support Data</param>
    /// <returns>Packet Type and Data</returns>
    public static (PacketType, IPacketData?) Deserialize(byte[] buf, SupportData? support = null) {
        if (buf.Length < 5) throw new ArgumentOutOfRangeException(
            nameof(buf), "There must be at least 5 bytes in a packet");
        if (buf[0] != 0xBB && buf[0] != 0xCC) throw new ArgumentOutOfRangeException(
            nameof(buf), $"Invalid packet header, expected BB or CC but found {buf[0]:X2}");
        if (buf.Length != buf[1] + 4) throw new ArgumentOutOfRangeException(
            nameof(buf), $"Invalid packet length, expected {buf[1] + 4} but found {buf.Length}");
        Sign(buf, true);
        var type = (PacketType)buf[2];
        _mapping.TryGetValue(type, out var data);
        if (data == null) return (type, data);
        var payload = new byte[buf[1] - 1];
        Array.Copy(buf, 3, payload, 0, payload.Length);
        data.Deserialize(type, support, payload);
        return (type, data);
    }
    
    /// <summary>
    /// Sets or verifies signature
    /// </summary>
    /// <param name="buf">Buffer</param>
    /// <param name="verify">Verify</param>
    public static void Sign(byte[] buf, bool verify = false) {
        ushort sum = 8217;
        for (var i = 0; i < buf.Length - 2; i++)
            sum += buf[i];
        var val1 = (sum >> 8) & 255;
        var val2 = sum & 255;
        if (verify) {
            if (buf[^1] != val2 || buf[^2] != val1)
                Log.Warning("Invalid signature, expected {0:X2}{1:X2} but found {2:X2}{3:X2} in {4}",
                    val1, val2, buf[^2], buf[^1], Convert.ToHexString(buf));
            return;
        }

        buf[^2] = (byte)val1;
        buf[^1] = (byte)val2;
    }
}