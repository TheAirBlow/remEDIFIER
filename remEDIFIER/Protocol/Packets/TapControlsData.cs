using remEDIFIER.Protocol.Values;

// TODO: Support other types, not just 0x0F
namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Tap controls packet data.
/// </summary>
public class TapControlsData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.SetControlSetting, PacketType.GetControlSetting];

    /// <summary>
    /// Selected ANC modes
    /// </summary>
    public ANCMode[] Modes { get; set; } = [];

    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf)
        => Modes = support.AncValue!.Modes.Where((_, i) => ((buf[1] >> i) & 1) == 1).ToArray();

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support) {
        byte[] buf = [0x0A, 0x00];
        var all = support.AncValue!.Modes;
        for (var i = 0; i < all.Length; i++)
            if (Modes.Contains(Modes[i]))
                buf[1] |= (byte)(1 << i);
        return buf;
    }
}