using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Windows;

/// <summary>
/// Device managed window
/// </summary>
public class DeviceWindow : ManagedWindow {
    /// <summary>
    /// Icon to show in the listing
    /// </summary>
    public virtual string ListIcon => "bug";
    
    /// <summary>
    /// Title to show in the listing
    /// </summary>
    public virtual string ListTitle => "Debugging toolbox";
    
    /// <summary>
    /// Features this device window supports
    /// </summary>
    public Feature[] Features { get; }
}