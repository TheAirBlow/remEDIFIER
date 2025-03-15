using System.Reflection;
using Raylib_CsLo;
using Raylib_ImGui;

namespace remEDIFIER;

/// <summary>
/// Images bindings handler for ImGui
/// </summary>
public static class Icons {
    /// <summary>
    /// Dictionary of icon handles
    /// </summary>
    private static readonly Dictionary<string, IntPtr> _handles = [];

    /// <summary>
    /// Initialises handles dictionary
    /// </summary>
    static unsafe Icons() {
        var ass = Assembly.GetExecutingAssembly();
        foreach (var name in ass.GetManifestResourceNames()) {
            if (!name.EndsWith(".png")) continue;
            var buffer = ass.GetEmbeddedResource(name);
            fixed (byte* fileData = buffer) {
                var image = Raylib.LoadImageFromMemory(".png", fileData, buffer.Length);
                var texture = Raylib.LoadTextureFromImage(image);
                Raylib.UnloadImage(image);
                var ptr = texture.CreateBinding();
                Raylib.GenTextureMipmaps((Texture*)ptr);
                RlGl.rlTextureParameters(texture.id, RlGl.RL_TEXTURE_MAG_FILTER, RlGl.RL_TEXTURE_FILTER_LINEAR_MIP_NEAREST);
                RlGl.rlTextureParameters(texture.id, RlGl.RL_TEXTURE_MIN_FILTER, RlGl.RL_TEXTURE_FILTER_LINEAR_MIP_NEAREST);
                _handles.Add(Path.GetFileNameWithoutExtension(name), ptr);
            }
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