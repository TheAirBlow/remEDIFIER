using System.Numerics;
using ImGuiNET;
using remEDIFIER.Device;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Windows;

/// <summary>
/// Debug toolbox device window
/// </summary>
public class DebugWindow : ManagedWindow {
    /// <summary>
    /// Icon to show in the top bar
    /// </summary>
    public override string Icon => "bug";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Debugging";
    
    /// <summary>
    /// Connected device
    /// </summary>
    private EdifierDevice Device { get; }
    
    private int _encoding;
    private string _input = "";
    private string _output = "";
    private string _payload = "";
    private string _result = "Waiting for input...";
    private PacketType _type;

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DebugWindow(EdifierDevice device) {
        device.Client.PacketReceived += PacketReceived;
        device.Client.PacketTimedOut += PacketTimedOut;
        Device = device;
    }

    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        ImGui.SeparatorText("Encoding");
        ImGui.SetNextItemWidth(100);
        ImGui.Combo("##encoding", ref _encoding, Enum.GetNames<Encoding>(), 3);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##input", ref _input, 256);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##output", ref _output, 256);
        if (_input.Length != 0)
            switch ((Encoding)_encoding) {
                case Encoding.Utf8: {
                    _output = Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(_input));
                    break;
                }
                case Encoding.Uint8: {
                    if (!byte.TryParse(_input, out var val)) {
                        _output = "value out of range";
                        break;
                    }

                    _output = $"{val:X2}";
                    break;
                }
                case Encoding.Uint16: {
                    if (!ushort.TryParse(_input, out var val)) {
                        _output = "value out of range";
                        break;
                    }

                    _output = $"{val >> 8:X2}{val & 0xFF:X2}";
                    break;
                }
            }
        ImGui.SeparatorText("Sending packets");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##payload", ref _payload, 256);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##result", ref _result, uint.MaxValue);
        ImGui.BeginDisabled(_result == null);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.Button("Send packet"))
            try {
                var data = Convert.FromHexString(_payload.Replace(" ", ""));
                if (data.Length < 1) throw new Exception("Missing packet type");
                _type = (PacketType)data[0];
                var buf = Packet.Serialize(_type, Device.Client.Support, data[1..]);
                Device.Client.Send(_type, buf, notify: true, wantResponse: false);
                _result = "Waiting for response...";
            } catch (Exception e) {
                _result = e.Message;
            }
        ImGui.EndDisabled();
    }
    
    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    /// <param name="payload">Payload</param>
    private void PacketReceived(PacketType type, IPacketData? data, byte[] payload) {
        if (type != _type) return;
        _result = Convert.ToHexString(payload);
    }
    
    /// <summary>
    /// Handles timed out packet
    /// </summary>
    /// <param name="type">Type</param>
    private void PacketTimedOut(PacketType type) {
        if (type != _type) return;
        _result = "Timed out!";
    }

    /// <summary>
    /// Unregisters event handlers
    /// </summary>
    public override void OnClosed() {
        Device.Client.PacketReceived -= PacketReceived;
        Device.Client.PacketTimedOut -= PacketTimedOut;
    }

    /// <summary>
    /// Encoding type
    /// </summary>
    private enum Encoding {
        Utf8 = 0,
        Uint8 = 1,
        Uint16 = 2
    }
}