namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Generic ushort packet data
/// </summary>
public class ShortData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.EnableShutdownTimer, PacketType.GetShutdownTimer];
    
    /// <summary>
    /// Current ushort value
    /// </summary>
    public ushort Value { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf)
        => Value = buf.Length < 2 ? buf[0] : BitConverter.ToUInt16(buf, 0);

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support)
        => Value < byte.MaxValue ? [(byte)Value] : BitConverter.GetBytes(Value);
}