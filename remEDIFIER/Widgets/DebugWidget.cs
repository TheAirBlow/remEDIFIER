using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Windows;

namespace remEDIFIER.Widgets;

/// <summary>
/// Debug widget
/// </summary>
public class DebugWidget : IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    public Feature[] Features => [];
    
    /// <summary>
    /// Packet as hex string
    /// </summary>
    private string _packet = "";

    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="window">Device window</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(DeviceWindow window, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Protocol debugging");
        ImGui.InputText("##hex", ref _packet, 255);
        ImGui.SameLine();
        if (ImGui.Button("Send"))
            try {
                var bytes = Convert.FromHexString(_packet);
                var buf = new byte[bytes.Length + 4];
                Array.Copy(bytes, 0, buf, 2, bytes.Length);
                buf[0] = 0xAA;
                buf[1] = (byte)bytes.Length;
                var signSize = window.Client.Support!.ProtocolVersion <= 1 ? 2 : 1;
                Packet.Hash(buf, signSize);
                window.Client.Send(buf);
            } catch (Exception e) {
                renderer.OpenWindow(new PopupWindow("Failed to send packet", e.ToString()));
            }
    }

    /// <summary>
    /// Process a received packet
    /// </summary>
    /// <param name="window">Window</param>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <returns>True if processed</returns>
    public bool PacketReceived(DeviceWindow window, PacketType type, IPacketData? data) => false;
    
    /// <summary>
    /// Sends all the packets necessary
    /// </summary>
    /// <param name="window">Window</param>
    public void ReadSettings(DeviceWindow window) { }
}