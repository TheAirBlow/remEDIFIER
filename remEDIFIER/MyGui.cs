using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;
using Raylib_ImGui;
using Serilog;

namespace remEDIFIER;

/// <summary>
/// ImGUI components and utilities
/// </summary>
public static class MyGui {
    /// <summary>
    /// Raylib ImGUI renderer
    /// </summary>
    public static ImGuiRenderer Renderer { get; private set; } = null!;

    /// <summary>
    /// Font sizes dictionary
    /// </summary>
    private static readonly Dictionary<int, ImFontPtr> _fontSizes = [];

    /// <summary>
    /// Pointer to the raw font data
    /// </summary>
    private static readonly IntPtr _fontData;

    /// <summary>
    /// Raw font data length
    /// </summary>
    private static readonly int _fontLength;
    
    /// <summary>
    /// Pointer to the raw font data
    /// </summary>
    private static readonly IntPtr _fallbackFontData;

    /// <summary>
    /// Raw font data length
    /// </summary>
    private static readonly int _fallbackFontLength;

    /// <summary>
    /// Content size stack
    /// </summary>
    private static readonly Stack<Vector2> _regionStack = new(); 
    
    /// <summary>
    /// Element padding vector (X: top, Y: left, Z: right, W: bottom)
    /// </summary>
    private static Vector4 _padding = Vector4.Zero;
    
    /// <summary>
    /// Centering ratio vector
    /// </summary>
    private static Vector2 _centerRatio = Vector2.Zero;

    /// <summary>
    /// Loads the embedded font
    /// </summary>
    static MyGui() {
        var font = Assembly.GetExecutingAssembly().GetEmbeddedResource("font.ttf");
        var fallback = Assembly.GetExecutingAssembly().GetEmbeddedResource("fallback.ttf");
        _fontData = GCHandle.Alloc(font, GCHandleType.Pinned).AddrOfPinnedObject();
        _fallbackFontData = GCHandle.Alloc(fallback, GCHandleType.Pinned).AddrOfPinnedObject();
        _fontLength = font.Length; _fallbackFontLength = fallback.Length;
    }
    
    /// <summary>
    /// Initialization and stuff
    /// </summary>
    public static void Initialize() {
        Renderer = new ImGuiRenderer();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign,
            new Vector2(0.5f, 0.5f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 12);
        ImGui.GetIO().Fonts.Clear();
        LoadFontWithSize(24, false);

        unsafe {
            var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesJapanese());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesCyrillic());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesGreek());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesKorean());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesThai());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesVietnamese());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesChineseFull());
            builder.BuildRanges(out var ranges);
            
            var config = ImGuiNative.ImFontConfig_ImFontConfig();
            config->MergeMode = 1;
            
            ImGui.GetIO().Fonts.AddFontFromMemoryTTF(
                _fallbackFontData, _fallbackFontLength, 24, 
                config, ranges.Data);
        }
        
        Renderer.RecreateFontTexture();
    }

    /// <summary>
    /// Loads main font of size
    /// </summary>
    /// <param name="size">Font size</param>
    /// <param name="recreate">Recreate font texture</param>
    private static ImFontPtr LoadFontWithSize(int size, bool recreate = true) {
        unsafe {
            var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
            builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesDefault());
            builder.BuildRanges(out var ranges);
            
            _fontSizes[size] = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(
                _fontData, _fontLength, size, 
                ImGuiNative.ImFontConfig_ImFontConfig(), 
                ranges.Data);
        }
        
        if (recreate) Renderer.RecreateFontTexture();
        return _fontSizes[size];
    }

    /// <summary>
    /// Preloads additional font sizes
    /// </summary>
    /// <param name="sizes">Sizes</param>
    public static void PreloadFontSizes(params int[] sizes) {
        foreach (var size in sizes) LoadFontWithSize(size, false);
        Renderer.RecreateFontTexture();
    }

    /// <summary>
    /// Fetches font with size
    /// </summary>
    /// <param name="size">Size</param>
    /// <returns>Font pointer</returns>
    private static ImFontPtr GetFont(int size) {
        if (_fontSizes.TryGetValue(size, out var ptr)) return ptr;
        Log.Warning("Font of size {0} was not preloaded!", size);
        return LoadFontWithSize(size);
    }

    /// <summary>
    /// Draws text with specified options
    /// </summary>
    /// <param name="fmt">Content</param>
    /// <param name="size">Font size</param>
    /// <param name="tint">Color tint</param>
    public static void Text(string fmt, int? size = null, Color? tint = null) {
        if (size.HasValue)
            ImGui.PushFont(GetFont(size.Value));
        Wrapped(() => {
            if (!tint.HasValue) ImGui.Text(fmt);
            else ImGui.TextColored(tint.Value.ToVec4(), fmt);
        }, ImGui.CalcTextSize(fmt));
        if (size.HasValue)
            ImGui.PopFont();
    }
    
    /// <summary>
    /// Draws text with specified options
    /// </summary>
    /// <param name="fmt">Content</param>
    /// <param name="size">Font size</param>
    public static void TextWrapped(string fmt, int? size = null) {
        if (size.HasValue)
            ImGui.PushFont(GetFont(size.Value));
        Wrapped(() => ImGui.TextWrapped(fmt),
            ImGui.CalcTextSize(fmt, ImGui.GetContentRegionAvail().X));
        if (size.HasValue)
            ImGui.PopFont();
    }

    /// <summary>
    /// Draws an icon with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static void Icon(string name, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => Wrapped(() => ImGui.Image(Icons.Get(name), size, uv0 ?? Vector2.Zero, uv1 ?? Vector2.One, (tint ?? Color.White).ToVec4()), size);

    /// <summary>
    /// Draws an icon with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static bool IconButton(string name, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => Wrapped(() => {
            ImGui.Image(Icons.Get(name), size, uv0 ?? Vector2.Zero, uv1 ?? Vector2.One, (tint ?? Color.White).ToVec4());
            return ImGui.IsItemClicked();
        }, size);
    
    /// <summary>
    /// Pushes current content region size onto the stack.
    /// Useful for centering multiple elements on the same line.
    /// </summary>
    public static void PushContentRegion()
        => _regionStack.Push(ImGui.GetContentRegionAvail());
    
    /// <summary>
    /// Pops content region size from the stack
    /// </summary>
    public static void PopContentRegion()
        => _regionStack.Pop();
    
    /// <summary>
    /// Fetches size of the content region on the stack.
    /// Falls back to current content size if the stack is empty.
    /// </summary>
    /// <returns>Content region size</returns>
    public static Vector2 GetContentSize()
        => _regionStack.Count == 0 ? ImGui.GetContentRegionAvail() : _regionStack.Peek();

    /// <summary>
    /// Sets padding for the next element.
    /// Works seamlessly with all MyGui elements.
    /// </summary>
    /// <param name="top">Top padding</param>
    /// <param name="left">Left padding</param>
    /// <param name="right">Right padding</param>
    /// <param name="bottom">Bottom padding</param>
    public static void SetNextPadding(float top = 0, float left = 0, float right = 0, float bottom = 0)
        => _padding = new Vector4(top, left, right, bottom);
    
    /// <summary>
    /// Sets centering ratio for the next element.
    /// Works seamlessly with all MyGui elements.
    /// </summary>
    /// <param name="vertical"></param>
    /// <param name="horizontal"></param>
    public static void SetNextCentered(float horizontal = 0, float vertical = 0)
        => _centerRatio = new Vector2(horizontal, vertical);
    
    /// <summary>
    /// Applies both centering and padding to a plain ImGui element.
    /// Renders a copy of the element offscreen to calculate the size if not specified.
    /// </summary>
    /// <param name="method">Method</param>
    /// <param name="size">Content size</param>
    public static T Wrapped<T>(Func<T> method, Vector2? size = null) {
        T result = default!;
        Wrapped(() => {
            result = method();
        }, size);
        return result;
    }
    
    /// <summary>
    /// Applies both centering and padding to a plain ImGui element.
    /// Renders a copy of the element offscreen to calculate the size if not specified.
    /// </summary>
    /// <param name="method">Method</param>
    /// <param name="size">Content size</param>
    public static void Wrapped(Action method, Vector2? size = null) {
        var centerRatio = _centerRatio;
        var padding = _padding;
        _centerRatio = Vector2.Zero;
        _padding = Vector4.Zero;
        
        if (centerRatio != Vector2.Zero && size == null) {
            ImGui.SetNextWindowPos(new Vector2(-100000, -100000));
            ImGui.Begin("offscreen");
            ImGui.BeginGroup();
            method();
            ImGui.EndGroup();
            size = ImGui.GetItemRectSize();
            ImGui.End();
        }

        if (centerRatio.Y != 0) {
            var offset = (GetContentSize().Y - size!.Value.Y - padding.X - padding.W) * centerRatio.Y;
            var diff = Math.Max(0, GetContentSize().Y - ImGui.GetContentRegionAvail().Y);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + offset - diff);
        }

        if (centerRatio.X != 0) {
            var offset = (GetContentSize().X - size!.Value.X - padding.Y - padding.Z) * centerRatio.X;
            var diff = Math.Max(0, GetContentSize().X - ImGui.GetContentRegionAvail().X);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset - diff);
        }

        if (padding != Vector4.Zero) {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(padding.Y, 0));
            ImGui.SameLine();
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(0, padding.X));
        }
        
        method();
        
        if (padding != Vector4.Zero) {
            ImGui.Dummy(new Vector2(0, padding.W));
            ImGui.EndGroup();
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(padding.Z, 0));
            ImGui.PopStyleVar();
            ImGui.EndGroup();
        }
    }

    /// <summary>
    /// Converts color to a Vector4
    /// </summary>
    /// <param name="color">Color</param>
    public static Vector4 ToVec4(this Color color)
        => color.Pack() / new Vector4(255, 255, 255, 255);
}
