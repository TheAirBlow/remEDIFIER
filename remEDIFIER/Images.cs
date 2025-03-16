using System.Numerics;
using System.Reflection;
using Raylib_CsLo;
using Raylib_ImGui;
using Serilog;

namespace remEDIFIER;

/// <summary>
/// Images bindings handler for ImGui
/// </summary>
public static class Images {
    /// <summary>
    /// Dictionary of icon handles
    /// </summary>
    private static readonly Dictionary<string, Image> _handles = [];

    /// <summary>
    /// Initialises handles dictionary
    /// </summary>
    static Images() {
        var ass = Assembly.GetExecutingAssembly();
        foreach (var name in ass.GetManifestResourceNames()) {
            if (!name.EndsWith(".png")) continue;
            var buffer = ass.GetEmbeddedResource(name);
            var image = LoadFromBuffer(buffer, name);
            _handles.Add(Path.GetFileNameWithoutExtension(name), image);
        }
    }

    /// <summary>
    /// Returns image by name (supports URLs)
    /// </summary>
    /// <param name="name">Name</param>
    /// <returns>Image</returns>
    public static Image Get(string name) {
        if (!_handles.TryGetValue(name, out var image)) {
            if (Uri.TryCreate(name, UriKind.Absolute, out var uri)) {
                var loading = Get("loading");
                image = new Image(loading.Binding, loading.Size, true);
                _handles.Add(uri.ToString(), image);
                _ = Task.Run(() => LoadFromUrl(uri));
                return image;
            }

            return Get("unknown");
        }
        
        if (image.ToLoad != null) {
            var newImage = LoadFromImage(image.ToLoad.Value);
            image.ToLoad = null;
            _handles[name] = newImage;
            return newImage;
        }
        
        return image;
    }

    /// <summary>
    /// Loads an image by URL
    /// </summary>
    /// <param name="uri">URL</param>
    private static async Task LoadFromUrl(Uri uri) {
        var name = uri.ToString();
        try {
            using var client = new HttpClient();
            var resp = await client.GetAsync(uri);
            var data = await resp.Content.ReadAsByteArrayAsync();
            unsafe {
                fixed (byte* fileData = data) {
                    _handles[name].ToLoad = Raylib.LoadImageFromMemory(Path.GetExtension(name), fileData, data.Length);
                }
            }
        } catch (Exception e) {
            Log.Warning("Failed to load image from {0}: {1}", uri.ToString(), e);
            _handles[name] = Get("unknown");
        }
    }

    /// <summary>
    /// Loads an image from buffer
    /// </summary>
    /// <param name="buf">Buffer</param>
    /// <param name="name">Filename</param>
    private static unsafe Image LoadFromBuffer(byte[] buf, string name) {
        fixed (byte* fileData = buf) {
            return LoadFromImage(Raylib.LoadImageFromMemory(Path.GetExtension(name), fileData, buf.Length));
        }
    }

    /// <summary>
    /// Loads an image from buffer
    /// </summary>
    /// <param name="image">Image</param>
    private static unsafe Image LoadFromImage(Raylib_CsLo.Image image) {
        var texture = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        var ptr = texture.CreateBinding();
        Raylib.GenTextureMipmaps((Texture*)ptr);
        RlGl.rlTextureParameters(texture.id, RlGl.RL_TEXTURE_MAG_FILTER, RlGl.RL_TEXTURE_FILTER_MIP_LINEAR);
        RlGl.rlTextureParameters(texture.id, RlGl.RL_TEXTURE_MIN_FILTER, RlGl.RL_TEXTURE_FILTER_MIP_LINEAR);
        return new Image(ptr, new Vector2(image.width, image.height));
    }
}

/// <summary>
/// Loaded image
/// </summary>
public class Image {
    /// <summary>
    /// ImGui binding
    /// </summary>
    public IntPtr Binding { get; set; }
    
    /// <summary>
    /// Image size
    /// </summary>
    public Vector2 Size { get; set; }
    
    /// <summary>
    /// Underlying image to load as a texture
    /// </summary>
    public Raylib_CsLo.Image? ToLoad { get; set; }
    
    /// <summary>
    /// Is this image still loading
    /// </summary>
    public bool Loading { get; set; }

    /// <summary>
    /// Creates a new image
    /// </summary>
    /// <param name="binding">Binding</param>
    /// <param name="size">Image size</param>
    /// <param name="loading">Is loading</param>
    public Image(IntPtr binding, Vector2 size, bool loading = false) {
        Binding = binding; Size = size; Loading = loading;
    }
}