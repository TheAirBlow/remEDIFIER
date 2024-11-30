using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;

namespace remEDIFIER; 

/// <summary>
/// Global configuration file
/// </summary>
public class Configuration {
    /// <summary>
    /// Static object instance
    /// </summary>
    public static readonly Configuration Config;

    /// <summary>
    /// Load the configuration
    /// </summary>
    static Configuration() {
        if (File.Exists("config.json")) {
            Log.Information("Loading configuration file...");
            var content = File.ReadAllText("config.json");
            try {
                Config = JsonSerializer.Deserialize(content, JsonContext.Default.Configuration)!;
            } catch (Exception e) {
                Log.Fatal("Failed to load config: {0}", e);
                Environment.Exit(-1);
            }
            
            Config.Save();
            return;
        }

        Config = new Configuration();
        Config.Save();
    }

    /// <summary>
    /// Automatically connect to devices that you have connected to manually before over bluetooth low energy.
    /// Applies only in case there is no audio device connected over Classic.
    /// </summary>
    public bool AutoConnectOverLowEnergy { get; set; } = true;

    /// <summary>
    /// Automatically connect to devices that you have connected to manually before over bluetooth classic.
    /// Applies only if the connected device is an audio device.
    /// </summary>
    public bool AutoConnectOverClassic { get; set; } = true;

    /// <summary>
    /// A list of devices hat user has connected to before
    /// </summary>
    public List<Device> Devices { get; set; } = [];
    
    /// <summary>
    /// Save configuration changes
    /// </summary>
    public void Save() => File.WriteAllText("config.json", 
        JsonSerializer.Serialize(Config, JsonContext.Default.Configuration));

    /// <summary>
    /// Device configuration
    /// </summary>
    public class Device {
        /// <summary>
        /// Bluetooth MAC address
        /// </summary>
        public string MacAddress { get; set; } = "";
        
        /// <summary>
        /// Is automatic connection allowed for this device
        /// </summary>
        public bool AutoConnect { get; set; }
        
        /// <summary>
        /// Should settings be automatically restored
        /// </summary>
        public bool RestoreSettings { get; set; }

        /// <summary>
        /// List of serialized widgets
        /// </summary>
        public Dictionary<string, JsonObject> Widgets { get; set; } = new();
    }
}