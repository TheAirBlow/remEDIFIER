using remEDIFIER.Device;

namespace remEDIFIER.Windows;

/// <summary>
/// Debug toolbox device window
/// </summary>
public class DebugWindow : DeviceWindow {
    /// <summary>
    /// Icon to show in the top bar
    /// </summary>
    public override string Icon => "bug";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Debugging";
    
    /// <summary>
    /// Icon to show in the listing
    /// </summary>
    public override string ListIcon => "bug";
    
    /// <summary>
    /// Title to show in the listing
    /// </summary>
    public override string ListTitle => "Debugging toolbox";
    
    /// <summary>
    /// Connected device
    /// </summary>
    private EdifierDevice Device { get; }
    
    /// <summary>
    /// Creates a new device window
    /// </summary>
    /// <param name="device">Device</param>
    public DebugWindow(EdifierDevice device)
        => Device = device;

    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        
    }
}