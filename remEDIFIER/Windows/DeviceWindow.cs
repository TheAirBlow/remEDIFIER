using System.Reflection;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER.Bluetooth;
using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;
using remEDIFIER.Widgets;
using Serilog;

namespace remEDIFIER.Windows;

/// <summary>
/// Device management window
/// </summary>
public class DeviceWindow : GuiWindow {
    /// <summary>
    /// Widgets to render
    /// </summary>
    private List<IWidget>? _widgets;

    /// <summary>
    /// Bluetooth connection
    /// </summary>
    private readonly EdifierClient _client;

    /// <summary>
    /// Display name
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="renderer">Renderer</param>
    /// <param name="info">Device Info</param>
    /// <param name="name">Display Name</param>
    public DeviceWindow(ImGuiRenderer renderer, DeviceInfo info, string name) {
        _name = name;
        _client = new EdifierClient();
        _client.DeviceConnected += () => {
            var data = _client.Send(PacketType.GetSupportedFeatures);
            if (data is not SupportData support) {
                renderer.OpenWindow(new PopupWindow("An error occured", "Failed to receive support data"));
                _client.Disconnect();
                return;
            }

            _widgets = Assembly.GetCallingAssembly().GetTypes()
                .Where(x => typeof(IWidget).IsAssignableFrom(x) && x != typeof(IWidget))
                .Select(x => (IWidget)Activator.CreateInstance(x)!)
                .Where(x => _client.Support!.Features.Any(y => x.Features.Contains(y)))
                .ToList();
            _client.Send(PacketType.GetDeviceName, notify: true);
            _client.Send(PacketType.GetFirmwareVersion, notify: true);
            _client.Send(PacketType.GetMacAddress, notify: true);
            _client.Send(PacketType.AVCRPState, notify: true);
            if (support.Supports(Feature.ShowBattery))
                _client.Send(PacketType.GetBattery, notify: true);
            if (support.Supports(Feature.HiRes) || support.Supports(Feature.LineHiRes))
                _client.Send(PacketType.GetAudioDecoding, notify: true);
        };
        _client.DeviceDisconnected += () => {
            IsOpen = false;
        };
        _client.ErrorOccured += (err, code) => {
            renderer.OpenWindow(new PopupWindow("An error occured", $"{err} ({code})"));
            IsOpen = false;
        };
        _client.PacketReceived += PacketReceived;
        _client.Connect(info);
    }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    /// <param name="renderer">Renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        if (ImGui.Begin($"{_name} ##{ID}", ImGuiWindowFlags.AlwaysAutoResize)) {
            if (_client is { Connected: true, Support: null }) {
                ImGui.Text("Waiting for support data...");
                ImGui.End();
                return;
            }
                
            if (_client.Connected) {
                if (_widgets == null) {
                    ImGui.Text("Waiting for widgets...");
                    ImGui.End();
                    return;
                }
                
                foreach (var widget in _widgets)
                    widget.Render(_client, renderer);
                
                var supported = _widgets.SelectMany(x => x.Features);
                var notSupported = _client.Support!.Features.Where(x => !supported.Contains(x));
                if (notSupported.Any()) {
                    ImGui.SeparatorText("Missing features");
                    ImGui.TextWrapped(string.Join(", ", notSupported));
                }
                ImGui.End();
                return;
            } 
            
            ImGui.Text("Connecting, please wait...");
            ImGui.End();
        }
    }
    
    /// <summary>
    /// Handles received packet
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="data">Data</param>
    private void PacketReceived(PacketType type, IPacketData? data) {
        if (_widgets == null) return;
        foreach (var widget in _widgets)
            if (widget.PacketReceived(type, data)) return;
    }
}