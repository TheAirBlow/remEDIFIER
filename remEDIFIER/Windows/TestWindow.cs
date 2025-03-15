using ImGuiNET;
namespace remEDIFIER.Windows;

/// <summary>
/// Test window
/// </summary>
public class TestWindow(int index) : ManagedWindow {
    /// <summary>
    /// con to show in the top bar
    /// </summary>
    public override string Icon => "bluetooth";
    
    /// <summary>
    /// Title to show in the top bar
    /// </summary>
    public override string Title => "Testing";

    /// <summary>
    /// Draws window GUI
    /// </summary>
    public override void Draw() {
        MyGui.PushContentRegion();
        MyGui.SetNextPadding(bottom: 48f);
        MyGui.SetNextCentered(0.5f, 0.5f);
        MyGui.Text(index.ToString(), 84);
        MyGui.SetNextCentered(0.5f, 0.5f);
        MyGui.SetNextPadding(top: 48f);
        if (MyGui.Wrapped(() => ImGui.Button("Next")))
            Manager.OpenWindow(new TestWindow(index + 1));
        MyGui.PopContentRegion();
    }
}