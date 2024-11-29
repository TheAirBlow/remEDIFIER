using System.Reflection;
using Raylib_CsLo;
using Raylib_ImGui;

namespace remEDIFIER;

/// <summary>
/// Images bindings handler for ImGui
/// </summary>
public static class Images {
    /// <summary>
    /// Array of icon filenames to load
    /// </summary>
    private static readonly string[] _icons = [ "audio", "bluetooth", "edifier" ];

    /// <summary>
    /// Dictionary of icon handles
    /// </summary>
    private static readonly Dictionary<string, IntPtr> _handles = [];

    /// <summary>
    /// Initialises handles dictionary
    /// </summary>
    static Images() {
        var ass = Assembly.GetExecutingAssembly();
        foreach (var name in _icons) {
            var ptr = ass.GetEmbeddedResource($"{name}.png")
                .LoadAsTexture(".png").CreateBinding();
            _handles.Add(name, ptr);
        }
    }

    /// <summary>
    /// Returns image handle by name
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>Handle</returns>
    public static IntPtr Get(string name) {
        if (!_handles.TryGetValue(name, out var image))
            throw new ArgumentOutOfRangeException(nameof(name),
                "Image with this name does not exist");
        return image;
    }
}