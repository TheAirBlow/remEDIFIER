namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Mac address packet data
/// </summary>
public class MacAddressData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetMacAddress];

    /// <summary>
    /// Mac address in string form
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf)
        => Value = BitConverter.ToString(buf).Replace("-", ":");

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support)
        => throw new NotImplementedException();
}