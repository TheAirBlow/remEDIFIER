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
    /// Element margin vector (X: top, Y: left, Z: right, W: bottom)
    /// </summary>
    private static Vector4 _margin = Vector4.Zero;
    
    /// <summary>
    /// Centering ratio vector
    /// </summary>
    private static Vector2 _centerRatio = Vector2.Zero;

    /// <summary>
    /// Loads the embedded font
    /// </summary>
    static MyGui() {
        var font = Assembly.GetExecutingAssembly().GetEmbeddedResource("main.ttf");
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
    /// <param name="text">Content</param>
    /// <param name="size">Font size</param>
    /// <param name="tint">Color tint</param>
    public static void Text(string text, int? size = null, Color? tint = null) {
        if (size.HasValue)
            ImGui.PushFont(GetFont(size.Value));
        Wrapped(() => {
            if (!tint.HasValue) ImGui.TextUnformatted(text);
            else ImGui.TextColored(tint.Value.ToVec4(), text);
        }, ImGui.CalcTextSize(text));
        if (size.HasValue)
            ImGui.PopFont();
    }
    
    /// <summary>
    /// Draws text with specified options
    /// </summary>
    /// <param name="text">Content</param>
    /// <param name="size">Font size</param>
    public static void TextWrapped(string text, int? size = null) {
        if (size.HasValue)
            ImGui.PushFont(GetFont(size.Value));
        Wrapped(() => ImGui.TextWrapped(text),
            ImGui.CalcTextSize(text, ImGui.GetContentRegionAvail().X));
        if (size.HasValue)
            ImGui.PopFont();
    }

    /// <summary>
    /// Shows a loading spinner if value is null
    /// </summary>
    /// <param name="text">Content</param>
    /// <param name="value">Value</param>
    /// <param name="size">Font size</param>
    public static void LoadingText(string text, object? value, int? size = null) {
        if (value != null) {
            Text(text.Replace("%s", value.ToString()), size);
            return;
        }
        
        var split = text.Split("%s");
        ImGui.BeginGroup();
        Text(split[0], size);
        ImGui.SameLine();
        Spinner(size ?? 24);
        ImGui.SameLine();
        Text(split[1], size);
        ImGui.EndGroup();
    }

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static void Image(string name, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => Image(Images.Get(name), size, uv0, uv1, tint);

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="scaler">Scaler</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static void Image(string name, Scaler scaler, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => Image(Images.Get(name), scaler, uv0, uv1, tint);
    
    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="scaler">Scaler</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static void Image(Image image, Scaler scaler, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null) {
        var size = scaler.GetSize(image.Size);
        _margin += scaler.GetPadding(image.Size);
        Image(image, size, uv0, uv1, tint);
    }

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static void Image(Image image, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null) {
        if (image.Loading) {
            var min = Math.Min(size.X, size.Y);
            var padX = (size.X - min) / 2f;
            var padY = (size.Y - min) / 2f;
            ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(padX, padY));
            Spinner(min);
            return;
        }
        
        Wrapped(() => ImGui.Image(image.Binding, size, uv0 ?? Vector2.Zero, uv1 ?? Vector2.One, (tint ?? Color.White).ToVec4()), size);
    }

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static bool ImageButton(string name, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => ImageButton(Images.Get(name), size, uv0, uv1, tint);

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="name">Icon name</param>
    /// <param name="scaler">Scaler</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static bool ImageButton(string name, Scaler scaler, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => ImageButton(Images.Get(name), scaler, uv0, uv1, tint);

    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="scaler">Scaler</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static bool ImageButton(Image image, Scaler scaler, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null) {
        var size = scaler.GetSize(image.Size);
        _margin += scaler.GetPadding(image.Size);
        return ImageButton(image, size, uv0, uv1, tint);
    }
    
    /// <summary>
    /// Draws an image with specified options
    /// </summary>
    /// <param name="image">Image</param>
    /// <param name="size">Icon size</param>
    /// <param name="uv0">UV0</param>
    /// <param name="uv1">UV1</param>
    /// <param name="tint">Color tint</param>
    public static bool ImageButton(Image image, Vector2 size, Vector2? uv0 = null, Vector2? uv1 = null, Color? tint = null)
        => Wrapped(() => {
            ImGui.Image(image.Binding, size, uv0 ?? Vector2.Zero, uv1 ?? Vector2.One, (tint ?? Color.White).ToVec4());
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
    /// Sets margin for the next element.
    /// Works seamlessly with all MyGui elements.
    /// </summary>
    /// <param name="top">Top margin</param>
    /// <param name="left">Left margin</param>
    /// <param name="right">Right margin</param>
    /// <param name="bottom">Bottom margin</param>
    public static void SetNextMargin(float top = 0, float left = 0, float right = 0, float bottom = 0)
        => _margin = new Vector4(top, left, right, bottom);
    
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
        var padding = _margin;
        _centerRatio = Vector2.Zero;
        _margin = Vector4.Zero;
        
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

    /// <summary>
    /// Draws a spinner
    /// </summary>
    /// <param name="size">Size</param>
    /// <param name="speed">Speed</param>
    public static void Spinner(float size, float speed = 1f)
        => Wrapped(() => {
            var drawList = ImGui.GetWindowDrawList();
            var vector = new Vector2(size, size);
            var pos = ImGui.GetCursorScreenPos();
            var center = pos + vector / 2;
            var time = (float)ImGui.GetTime();
            var angle = time * speed * MathF.PI * 2;
            var corners = new Vector2[4];
            var halfSize = vector / 2;

            var offsets = new Vector2[] {
                new(-halfSize.X, -halfSize.Y),
                new(halfSize.X, -halfSize.Y),
                new(halfSize.X, halfSize.Y),
                new(-halfSize.X, halfSize.Y)
            };

            for (var i = 0; i < 4; i++) {
                var x = offsets[i].X;
                var y = offsets[i].Y;
                var rotatedX = x * MathF.Cos(angle) - y * MathF.Sin(angle);
                var rotatedY = x * MathF.Sin(angle) + y * MathF.Cos(angle);
                corners[i] = center + new Vector2(rotatedX, rotatedY);
            }

            drawList.AddImageQuad(
                Images.Get("loading").Binding,
                corners[0], corners[1], corners[2], corners[3],
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                0xFFFFFFFF
            );

            ImGui.Dummy(vector);
        }, new Vector2(size, size));
}

/// <summary>
/// Image scaler
/// </summary>
public class Scaler {
    /// <summary>
    /// Scaler operation
    /// </summary>
    public ScalerOperation Operation { get; set; }
    
    /// <summary>
    /// Scaler constraint
    /// </summary>
    public Vector2 Constraint { get; set; }
    
    /// <summary>
    /// Scaler constraint
    /// </summary>
    public Vector4 Padding { get; set; }

    /// <summary>
    /// Creates a new scaler
    /// </summary>
    /// <param name="operation">Operation</param>
    /// <param name="constraint">Constraint</param>
    /// <param name="padding">Extra padding</param>
    public Scaler(ScalerOperation operation, Vector2 constraint, Vector4? padding = null) {
        Operation = operation; Constraint = constraint; Padding = padding ?? Vector4.Zero;
    }

    /// <summary>
    /// Creates a scale constraint
    /// </summary>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    /// <param name="padding">Extra padding</param>
    /// <returns>Scaler</returns>
    public static Scaler Scale(float width = -1, float height = -1, Vector4? padding = null) {
        if (width == -1 && height == -1)
            throw new ArgumentException("Tt least the width or height are required");
        return new Scaler(ScalerOperation.Scale, new Vector2(width, height), padding);
    }
    
    /// <summary>
    /// Creates a scale constraint
    /// </summary>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    /// <param name="padding">Extra padding</param>
    /// <returns>Scaler</returns>
    public static Scaler Fit(float width, float height, Vector4? padding = null)
        => new(ScalerOperation.Fit, new Vector2(width, height), padding);

    /// <summary>
    /// Calculates padding for vector
    /// </summary>
    /// <param name="size">Vector</param>
    /// <returns>Padding</returns>
    public Vector4 GetPadding(Vector2 size) {
        switch (Operation) {
            case ScalerOperation.Scale:
                return Padding;
            case ScalerOperation.Fit:
                size = GetSize(size);
                var padY = (Constraint.Y - size.Y) / 2f;
                var padX = (Constraint.X - size.X) / 2f;
                return new Vector4(padY, padX, padX, padY);
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Calculates new size for vector
    /// </summary>
    /// <param name="size">Vector</param>
    /// <returns>Size</returns>
    public Vector2 GetSize(Vector2 size) {
        var padding = new Vector2(Padding.Y + Padding.Z, Padding.X + Padding.W);
        switch (Operation) {
            case ScalerOperation.Scale:
                return new Vector2(
                    Constraint.X == -1 ? Constraint.Y * (size.X / size.Y) : Constraint.X, 
                    Constraint.Y == -1 ? Constraint.X * (size.Y / size.X) : Constraint.Y
                ) - padding;
            case ScalerOperation.Fit:
                if (size.X == size.Y)
                    return Constraint - padding;
                if (size.X > size.Y)
                    return new Vector2(
                        Constraint.X, 
                        Constraint.Y * (size.Y / size.X)
                    ) - padding;
                if (size.X < size.Y)
                    return new Vector2(
                        Constraint.X * (size.X / size.Y), 
                        Constraint.Y
                    ) - padding;
                return Vector2.Zero; // should never happen
            default:
                throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Scaler operation
/// </summary>
public enum ScalerOperation {
    /// <summary>
    /// Stretches image to specified constraint
    /// </summary>
    Scale,
        
    /// <summary>
    /// Scales image to it into specified constraint
    /// </summary>
    Fit,
}