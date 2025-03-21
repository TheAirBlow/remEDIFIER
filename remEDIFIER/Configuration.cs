using System.Text.Json;
using System.Text.Json.Nodes;
using remEDIFIER.Device;
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
    /// A list of devices hat user has connected to before
    /// </summary>
    public List<DeviceConfig> Devices { get; set; } = [];
    
    /// <summary>
    /// Save configuration changes
    /// </summary>
    public void Save() => File.WriteAllText("config.json", 
        JsonSerializer.Serialize(Config, JsonContext.Default.Configuration));
}

/// <summary>
/// Device configuration
/// </summary>
public class DeviceConfig {
    /// <summary>
    /// Bluetooth MAC address
    /// </summary>
    public string MacAddress { get; set; } = "";
        
    /// <summary>
    /// Protocol version
    /// </summary>
    public int ProtocolVersion { get; set; }
    
    /// <summary>
    /// Encryption type
    /// </summary>
    public EncryptionType EncryptionType { get; set; }
        
    /// <summary>
    /// Product identifier
    /// </summary>
    public int ProductId { get; set; }
}