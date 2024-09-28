namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// AVCRP command packet data
/// </summary>
public class AVCRPCommandData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.AVCRPCommand];
    
    /// <summary>
    /// AVCRP command
    /// </summary>
    public AVCRPCommand Command { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf)
        => Command = (AVCRPCommand)buf[0];

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support)
        => [(byte)Command];
}

/// <summary>
/// AVCRP command enum
/// </summary>
public enum AVCRPCommand : byte {
    Play, Pause, VolumeUp, VolumeDown, Next, Previous
}