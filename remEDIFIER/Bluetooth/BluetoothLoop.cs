using System.Runtime.InteropServices;
using Serilog;

namespace remEDIFIER.Bluetooth;

/// <summary>
/// Bluetooth communication library loop
/// </summary>
public partial class BluetoothLoop {
    /// <summary>
    /// Starts QCoreApplication loop
    /// </summary>
    public static void StartLoop() {
        new Thread(() => {
            Log.Information("QCoreApplication loop exited with code {0}", RunApplication());
        }).Start();
        
        Console.CancelKeyPress += (_, _) => {
            ExitApplication(0);
        };
        
        AppDomain.CurrentDomain.ProcessExit += (_, _) => {
            ExitApplication(0);
        };
    }
    
    [LibraryImport("comhelper")]
    private static partial int RunApplication();
    
    [LibraryImport("comhelper")]
    private static partial void ExitApplication(int code);
}