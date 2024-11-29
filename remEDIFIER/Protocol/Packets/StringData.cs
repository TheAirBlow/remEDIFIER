using System.Text;

namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Generic string packet data
/// </summary>
public class StringData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetDeviceName, PacketType.SetDeviceName, PacketType.SongName, PacketType.AuthorName];

    /// <summary>
    /// Current string value
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf)
        => Value = Encoding.UTF8.GetString(buf);

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support) {
        if (type == PacketType.SetDeviceName && Value.Length > support.MaxDeviceName)
            throw new InvalidDataException("Specified device name string is longer than maximum supported by current headset");
        return Encoding.UTF8.GetBytes(Value);
    }
}