﻿using System.Reflection;
using ImGuiNET;
using Raylib_CsLo;
using Raylib_ImGui;
using remEDIFIER;
using remEDIFIER.Bluetooth;
using remEDIFIER.Windows;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
Log.Information("Starting remEDIFIER version {0} by TheAirBlow",
    Assembly.GetExecutingAssembly().GetName().Version);

BluetoothLoop.Start();
Log.Information("Started QCoreApplication loop");

Raylib.SetTraceLogLevel((int)TraceLogLevel.LOG_WARNING);
Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
Raylib.InitWindow(450, 700, "remEDIFIER");
Raylib.SetWindowMinSize(450, 700);
MyGui.Initialize();
MyGui.PreloadFontSizes(18, 20, 28, 32);

_ = Images.Get("edifier");
Raylib.SetWindowIcon(Assembly.GetExecutingAssembly()
    .GetEmbeddedResource("edifier.png").LoadAsImage(".png"));
var manager = new WindowManager();
MyGui.Renderer.OpenWindow(manager);
manager.OpenWindow(new DiscoveryWindow());

Log.Information("Running main game loop");
while (!Raylib.WindowShouldClose()) {
    MyGui.Renderer.Update();
    Raylib.BeginDrawing();
    ImGui.NewFrame();
    Raylib.ClearBackground(new Color(0, 0, 0, 255));
    ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(),
        ImGuiDockNodeFlags.PassthruCentralNode);
    
    MyGui.Renderer.DrawWindows();
    MyGui.Renderer.RenderImGui();
    Raylib.EndDrawing();
}

Log.Information("Exiting and closing window");
Raylib.CloseWindow();
BluetoothLoop.Stop();