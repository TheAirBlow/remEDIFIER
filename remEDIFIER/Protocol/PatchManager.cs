using remEDIFIER.Protocol.Packets;

namespace remEDIFIER.Protocol;

/// <summary>
/// Various patches because Edifier is incompetent
/// </summary>
public static class PatchManager {
    /// <summary>
    /// Patches the ANC value
    /// </summary>
    /// <param name="ancValue">ANC value</param>
    /// <param name="data">Support data</param>
    /// <returns>ANC value</returns>
    public static int PatchAnc(this int ancValue, SupportData? data)
        => data?.Extra?.Product.ProductSearchUuid switch {
            "00009200-0000-1000-8000-00805f9b34fb" => 0x1A, // WH950NB
            "0000cb00-0000-1000-8000-00805f9b34fb" => 0x1A, // TWS1_PRO_2
            // 0xF6 is an arbitrary number that later resolves to the correct ANC list
            "00005300-0000-1000-8000-00805f9b34fb" => 0xF6, // NEOBUDS_PRO_OUTSIDE
            "00003300-0000-1000-8000-00805f9b34fb" => 0xF6, // NEOBUDS_PRO
            _ => ancValue
        };

    /// <summary>
    /// Overrides feature support value if necessary
    /// </summary>
    /// <param name="value">Original value</param>
    /// <param name="feature">Feature</param>
    /// <param name="data">Support data</param>
    /// <returns>True if supports</returns>
    public static bool Override(bool value, Feature feature, SupportData? data)
        => feature switch {
            Feature.ClearPairingRecord => 
                data?.Extra?.Product.ProductSearchUuid switch { 
                    "00009b00-0000-1000-8000-00805f9b34fb" => true, // LOLLI3_PRO
                    _ => value 
                },
            Feature.RePair => 
                data?.Extra?.Product.ProductSearchUuid switch { 
                    "00005500-0000-1000-8000-00805f9b34fb" => true, // W220T
                    "00007000-0000-1000-8000-00805f9b34fb" => true, // LOLLI3
                    _ => value 
                },
            Feature.ShowBattery => 
                data?.Extra?.Product.ProductSearchUuid switch { 
                    "00006C00-0000-1000-8000-00805f9b34fb" => false, // S3000
                    "00007f00-0000-1000-8000-00805f9b34fb" => false, // S3000_OUTSIDE
                    _ => value 
                },
            Feature.SmartLight => 
                data?.Extra?.Product.ProductModel switch { 
                    "D32" => false,
                    _ => value 
                },
            _ => value
        };

    /// <summary>
    /// Should custom equalizer be shown
    /// </summary>
    /// <param name="data">Support data</param>
    /// <returns>True or false</returns>
    public static bool ShowCustomEq(SupportData? data)
        => data?.Extra?.Product.ProductSearchUuid switch {
            "00008e00-0000-1000-8000-00805f9b34fb" => false, // M25
            "0000bf00-0000-1000-8000-00805f9b34fb" => false, // ZX3
            "00002a00-0000-1000-8000-00805f9b34fb" => false, // MG250
            _ => true
        };
}