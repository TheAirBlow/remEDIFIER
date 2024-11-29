using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Raylib_CsLo;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using remEDIFIER;
using remEDIFIER.Bluetooth;
using remEDIFIER.Windows;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting remEDIFIER version {0} by TheAirBlow",
    Assembly.GetExecutingAssembly().GetName().Version);

BluetoothLoop.Start();
Log.Information("Started QCoreApplication loop");

var renderer = new ImGuiRenderer();
Raylib.SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
Raylib.InitWindow(1280, 720, "remEDIFIER");
ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign,
    new Vector2(0.5f, 0.5f));
ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10);
ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6);
ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 12);
ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
unsafe {
    ImGui.GetIO().Fonts.Clear();
    var font = Assembly.GetExecutingAssembly()
        .GetEmbeddedResource("font.ttf");
    var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());
    builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesDefault());
    builder.AddRanges(ImGui.GetIO().Fonts.GetGlyphRangesJapanese());
    builder.BuildRanges(out var ranges);
    fixed (byte* p = font) ImGui.GetIO().Fonts.AddFontFromMemoryTTF(
        (IntPtr)p, font.Length, 24,
        ImGuiNative.ImFontConfig_ImFontConfig(), 
        ranges.Data);
    renderer.RecreateFontTexture();
}

_ = Images.Get("edifier");
Raylib.SetWindowIcon(Assembly.GetExecutingAssembly()
    .GetEmbeddedResource("edifier.png").LoadAsImage(".png"));
var logo = Assembly.GetExecutingAssembly()
    .GetEmbeddedResource("logo.png").LoadAsTexture(".png");
renderer.OpenWindow(new DiscoveryWindow(renderer));

Log.Information("Running main game loop");
while (!Raylib.WindowShouldClose()) {
    renderer.Update();
    Raylib.BeginDrawing();
    ImGui.NewFrame();
    Raylib.ClearBackground(new Color(0, 0, 0, 255));
    Raylib.DrawTexture(logo, Raylib.GetRenderWidth() / 2 - 185,
        Raylib.GetRenderHeight() / 2 - 64, new Color(255, 255, 255, 255));
    ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(),
        ImGuiDockNodeFlags.PassthruCentralNode);
    if (ImGui.BeginMainMenuBar()) {
        if (ImGui.BeginMenu("Open")) {
            if (ImGui.MenuItem("ImGui Demo"))
                renderer.OpenWindow(new DemoWindow());
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Help")) {
            if (ImGui.MenuItem("About Raylib-ImGui"))
                renderer.OpenWindow(new AboutWindow());
            ImGui.EndMenu();
        }
        
        ImGui.EndMainMenuBar();
    }
    
    renderer.DrawWindows();
    renderer.RenderImGui();
    Raylib.EndDrawing();
}

Log.Information("Exiting and closing window");
Raylib.CloseWindow();
BluetoothLoop.Stop();