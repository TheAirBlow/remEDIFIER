namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Version packet data
/// </summary>
public class VersionData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetFirmwareVersion];

    /// <summary>
    /// Firmware version
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData? support, byte[] buf)
        => Version = new Version(string.Join(".", buf));

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData? support)
        => throw new NotImplementedException();
}