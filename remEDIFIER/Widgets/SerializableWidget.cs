using static remEDIFIER.Configuration;
using System.Text.Json;
using remEDIFIER.Windows;

namespace remEDIFIER.Widgets;

/// <summary>
/// Serializable widget class
/// </summary>
public abstract class SerializableWidget {
    /* TODO: implement per-widget toggle
    /// <summary>
    /// Should settings be automatically restored
    /// </summary>
    public bool RestoreSettings { get; set; } = true;
    */
    
    /// <summary>
    /// Saves settings to the configuration
    /// </summary>
    protected void SaveSettings(DeviceWindow window) {
        var type = GetType();
        var info = JsonContext.Default.GetTypeInfo(type);
        if (info == null) throw new InvalidOperationException(
            $"{type.Name} is not marked as JSON serializable");
        var node = JsonSerializer.SerializeToNode(this, info)!;
        window.DeviceConfig!.Widgets[type.Name] = node.AsObject(); Config.Save();
    }
}