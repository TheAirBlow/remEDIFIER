using ImGuiNET;
using Raylib_ImGui;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Widgets;

/// <summary>
/// Playback widget
/// </summary>
public class PlaybackWidget : IWidget {
    /// <summary>
    /// Features this widget supports
    /// </summary>
    public Feature[] Features => [ Feature.GetDeviceName ];

    /// <summary>
    /// AVCRP state
    /// </summary>
    private AVCRPState? _state;

    /// <summary>
    /// Song author name
    /// </summary>
    private string? _author;
    
    /// <summary>
    /// Song name
    /// </summary>
    private string? _song;

    /// <summary>
    /// Render widget with ImGui
    /// </summary>
    /// <param name="client">Edifier client</param>
    /// <param name="renderer">ImGui renderer</param>
    public void Render(EdifierClient client, ImGuiRenderer renderer) {
        ImGui.SeparatorText("Playback controls");
        ImGui.TextUnformatted(_state == AVCRPState.Playing
            ? _song != null 
                ? $"Playing {_author} - {_song}" 
                : "Song title is not available"
            : "Currently not playing anything");
        if (ImGui.Button("Previous")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.Previous }, wait: false);
        ImGui.SameLine();
        if (ImGui.Button("Next")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.Next }, wait: false);
        ImGui.SameLine();
        if (ImGui.Button("Play")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.Play }, wait: false);
        ImGui.SameLine();
        if (ImGui.Button("Pause")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.Pause }, wait: false);
        ImGui.SameLine();
        if (ImGui.Button("Vol -")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.VolumeDown }, wait: false);
        ImGui.SameLine();
        if (ImGui.Button("Vol +")) client.Send(PacketType.AVCRPCommand, 
            new AVCRPCommandData { Command = AVCRPCommand.VolumeUp }, wait: false);
    }

    /// <summary>
    /// Process a received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <returns>True if processed</returns>
    public bool PacketReceived(PacketType type, IPacketData? data) {
        switch (type) {
            case PacketType.AVCRPState:
                _state = ((AVCRPStateData)data!).State;
                if (_state == AVCRPState.Paused)
                    _author = _song = null;
                return true;
            case PacketType.AuthorName:
                _author = ((StringData)data!).Value;
                return true;
            case PacketType.SongName:
                _song = ((StringData)data!).Value;
                return true;
            default:
                return false;
        }
    }
}