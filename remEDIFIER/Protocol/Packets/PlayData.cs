using System.Text;

namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Play info data
/// </summary>
public class PlayData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.PlayInfo];

    /// <summary>
    /// Is currently playing a song
    /// </summary>
    public bool Playing { get; set; }
    
    /// <summary>
    /// Song author
    /// </summary>
    public string Author { get; set; } = "";
    
    /// <summary>
    /// Song name
    /// </summary>
    public string Song { get; set; } = "";

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf) {
        Playing = buf[0] == 0x01;
        Song = Encoding.UTF8.GetString(buf, 3, buf[1]).Replace("\ufffd", "");
        Author = Encoding.UTF8.GetString(buf, buf[1] + 3, buf[2]).Replace("\ufffd", "");
        if (Author.Contains("unknow", StringComparison.OrdinalIgnoreCase) // this is intentional!
            || Author.Contains("Not Provided", StringComparison.OrdinalIgnoreCase))
            Author = "<Unknown>";
        if (Song.Contains("unknow", StringComparison.OrdinalIgnoreCase) // this is intentional!
            || Song.Contains("Not Provided", StringComparison.OrdinalIgnoreCase))
            Song = "<Unknown>";
    }

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support)
        => throw new NotImplementedException();
}