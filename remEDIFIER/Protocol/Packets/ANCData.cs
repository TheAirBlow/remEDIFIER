using remEDIFIER.Protocol.Values;

namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// ANC packet data
/// </summary>
public class ANCData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetANCMode, PacketType.SetANCMode];
    
    /// <summary>
    /// Current ANC mode
    /// </summary>
    public ANCMode Mode { get; set; }
    
    /// <summary>
    /// First extra value
    /// </summary>
    public byte? Extra1 { get; set; }
    
    /// <summary>
    /// Second extra value
    /// </summary>
    public byte? Extra2 { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf) {
        Mode = support.AncValue!.Map(buf[0]);
        if (buf.Length > 1) Extra1 = buf[1];
        if (buf.Length > 2) Extra2 = buf[2];
    }

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support) {
        var index = support.AncValue!.Map(Mode);
        if (Extra2 != null)
            return [index, Extra1!.Value, Extra2!.Value];
        if (Extra1 != null)
            return [index, Extra1!.Value];
        return [index];
    }
}