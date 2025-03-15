namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// LDAC packet data
/// </summary>
public class LdacData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetLDAC, PacketType.SetLDAC];
    
    /// <summary>
    /// Current LDAC state
    /// </summary>
    public LDACState Value { get; set; }
    
    /// <summary>
    /// Creates packet data with default values
    /// </summary>
    public LdacData() { }
    
    /// <summary>
    /// Creates packet data with specified value
    /// </summary>
    /// <param name="value">Value</param>
    public LdacData(LDACState value)
        => Value = value;

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf)
        => Value = (LDACState)buf[0];

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support) {
        if (Value == LDACState.On192K && !support!.Features.Contains(Feature.Allow192K)) 
            throw new InvalidDataException("192k bitrate is not supported by the current headset");
        return [(byte)Value];
    }
}

/// <summary>
/// LDAC state enum
/// </summary>
public enum LDACState : byte {
    Off, On48K, On96K, On192K 
}