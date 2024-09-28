namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// AVCRP state packet data
/// </summary>
public class AVCRPStateData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.AVCRPState];
    
    /// <summary>
    /// Current AVCRP state
    /// </summary>
    public AVCRPState State { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf)
        => State = (AVCRPState)buf[0];

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support)
        => throw new NotImplementedException();
}

/// <summary>
/// AVCRP state enum
/// </summary>
public enum AVCRPState : byte {
    Paused = 0x03,
    Playing = 0x0D
}