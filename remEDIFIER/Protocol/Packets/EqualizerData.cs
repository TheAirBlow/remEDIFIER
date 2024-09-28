using remEDIFIER.Protocol.Values;

namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Equalizer packet data
/// </summary>
public class EqualizerData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetEqualizer, PacketType.SetEqualizer];
    
    /// <summary>
    /// Current equalizer preset
    /// </summary>
    public EqualizerPreset Preset { get; set; }

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf)
        => Preset = support.EqualizerValue!.Map(buf[0]);

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support)
        => [support.EqualizerValue!.Map(Preset)];
}