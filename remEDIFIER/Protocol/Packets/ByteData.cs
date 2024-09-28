namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Generic byte packet data
/// </summary>
public class ByteData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.SetPromptVolume, PacketType.GetPromptVolume];
    
    /// <summary>
    /// Current byte value
    /// </summary>
    public byte Value { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf)
        => Value = buf[0];

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support)
        => [Value];
}