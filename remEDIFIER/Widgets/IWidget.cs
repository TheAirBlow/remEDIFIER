using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Widgets;

/// <summary>
/// Device window widget
/// </summary>
public interface IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    public Feature[] Features { get; }

    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="client">Edifier client</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(EdifierClient client, ImGuiRenderer renderer);

    /// <summary>
    /// Process a received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <returns>True if processed</returns>
    public bool PacketReceived(PacketType type, IPacketData? data);
}