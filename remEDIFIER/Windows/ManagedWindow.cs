namespace remEDIFIER.Windows;

/// <summary>
/// Window managed by <see cref="WindowManager"/>
/// </summary>
public abstract class ManagedWindow {
    /// <summary>
    /// Window's unique identifier
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Is this window closed
    /// </summary>
    public bool Closed { get; set; }
    
    /// <summary>
    /// Is this window hidden
    /// </summary>
    public bool Hidden { get; set; }
    
    /// <summary>
    /// Is this window processing something important (disables back button)
    /// </summary>
    public bool Processing { get; set; }

    /// <summary>
    /// Icon to show in the top bar
    /// </summary>
    public virtual string Icon => "unknown";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public virtual string Title => "Unknown";

    /// <summary>
    /// Parent window manager
    /// </summary>
    public WindowManager Manager { get; set; }
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    public virtual void Draw() { }
    
    /// <summary>
    /// Handles window getting closed
    /// </summary>
    public virtual void OnClosed() { }
    
    /// <summary>
    /// Handles window getting hidden
    /// </summary>
    public virtual void OnHidden() { }
    
    /// <summary>
    /// Handles window getting hidden
    /// </summary>
    public virtual void OnShown() { }
}