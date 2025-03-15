using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;

namespace remEDIFIER.Windows;

/// <summary>
/// Sleek window manager
/// </summary>
public class WindowManager : GuiWindow {
    /// <summary>
    /// List of managed windows
    /// </summary>
    private readonly List<ManagedWindow> _windows = [];

    /// <summary>
    /// Sliding animation progress
    /// </summary>
    private float _progress = 1;

    /// <summary>
    /// Should animate to right side
    /// </summary>
    private bool _animRight = true;

    /// <summary>
    /// Progress value lock
    /// </summary>
    private Lock _lock = new();
    
    /// <summary>
    /// Draws window GUI
    /// </summary>
    /// <param name="renderer">Renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        var size = ImGui.GetIO().DisplaySize;
        ImGui.SetNextWindowPos(new Vector2(-1, -1));
        ImGui.SetNextWindowSize(new Vector2(size.X + 2, 55));
        if (ImGui.Begin("Title Bar", ImGuiWindowFlags.NoDecoration)) {
            MyGui.PushContentRegion();
            if (_windows.Count > 1) {
                MyGui.SetNextPadding(left: 5);
                MyGui.SetNextCentered(0f, 0.5f);
                if (MyGui.IconButton("arrow-left", new Vector2(28, 28)) && _progress == 1)
                    _animRight = false;
                ImGui.SameLine();
            }
            MyGui.SetNextCentered(0.5f, 0.5f);
            MyGui.Text(_windows[^1].Title, 28);
            ImGui.SameLine();
            MyGui.SetNextPadding(right: 5);
            MyGui.SetNextCentered(1f, 0.5f);
            MyGui.Icon(_windows[^1].Icon, new Vector2(28, 28));
            MyGui.PopContentRegion();
            ImGui.End();
        }
        
        _lock.Enter();
        if (_progress != 1 && _animRight)
            _progress = Math.Min(1, _progress + ImGui.GetIO().DeltaTime * 5f);
        if (_progress != 0 && !_animRight) {
            _progress = Math.Max(0, _progress - ImGui.GetIO().DeltaTime * 5f);
            if (_progress == 0) {
                _windows[^1].Closed = true;
                _windows[^1].OnHidden();
                _windows[^1].OnClosed();
                _windows.RemoveAt(_windows.Count - 1);
                if (_windows.Count > 0) {
                    _windows[^1].Hidden = false;
                    _windows[^1].OnShown();
                }
                _animRight = true; _progress = 1;
            }
        }
        _lock.Exit();
        
        for (var i = 0; i < _windows.Count; i++) {
            var window = _windows[i];
            var idx = _windows.Count - i - 1;
            if (idx == 0 && window.Closed) _animRight = false;
            ImGui.SetNextWindowPos(new Vector2(-size.X * idx + size.X * (1 - _progress), 55));
            ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize - new Vector2(0, 55));
            if (ImGui.Begin(window.Id, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground)) {
                window.Draw();
                ImGui.End();
            }
        }
    }

    /// <summary>
    /// Opens a new window
    /// </summary>
    /// <param name="window">Window</param>
    public void OpenWindow(ManagedWindow window) {
        if (_windows.Count != 0) {
            _lock.Enter();
            _animRight = true;
            _progress = 0;
            _lock.Exit();
        }
        
        window.Manager = this;
        _windows.Add(window);
        if (_windows.Count > 1) {
            _windows[^2].Hidden = true;
            _windows[^2].OnHidden();
        }
    }
}