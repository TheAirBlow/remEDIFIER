using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace remEDIFIER;

/// <summary>
/// An edifier product JSON
/// </summary>
public class Product {
    /// <summary>
    /// Array of all Edifier products
    /// </summary>
    [JsonIgnore]
    public static Product[] Products { get; }

    /// <summary>
    /// Load products array
    /// </summary>
    static Product() {
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("products.json");
        Products = JsonSerializer.Deserialize(stream!, JsonContext.Default.ProductArray)!;
    }
    
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ProductName")]
    public string ProductName { get; set; } = "";

    [JsonPropertyName("ProductScheme")]
    public string ProductScheme { get; set; } = "";

    [JsonPropertyName("ProductModel")]
    public string ProductModel { get; set; } = "";

    [JsonPropertyName("ProductSearchUuid")]
    public string ProductSearchUuid { get; set; } = "";

    [JsonPropertyName("ProductServiceUuid")]
    public string ProductServiceUuid { get; set; } = "";

    [JsonPropertyName("ProductReadUuid")]
    public string ProductReadUuid { get; set; } = "";

    [JsonPropertyName("ProductWriteUuid")]
    public string ProductWriteUuid { get; set; } = "";

    [JsonPropertyName("ProductSPPUuid")]
    public string ProductSPPUuid { get; set; } = "";

    [JsonPropertyName("ProductAppName")]
    public string ProductAppName { get; set; } = "";

    [JsonPropertyName("ProductId")]
    public string ProductId { get; set; } = "";

    [JsonPropertyName("VendorId")]
    public string VendorId { get; set; } = "";

    [JsonPropertyName("CreateAt")]
    public string CreateAt { get; set; } = "";

    [JsonPropertyName("UpdateAt")]
    public string UpdateAt { get; set; } = ""; 

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("IsFirmwareUpdate")]
    public bool IsFirmwareUpdate { get; set; }

    [JsonPropertyName("IsTWS")]
    public bool IsTWS { get; set; }

    [JsonPropertyName("IsMFICertification")]
    public bool IsMFICertification { get; set; }

    [JsonPropertyName("IsPublish")]
    public bool IsPublish { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("ProductDesc")]
    public string ProductDesc { get; set; } = "";

    [JsonPropertyName("ProductImageLink")]
    public string ProductImageLink { get; set; } = "";

    [JsonPropertyName("ProductManualLink")]
    public string ProductManualLink { get; set; } = "";

    [JsonPropertyName("subjection")]
    public string Subjection { get; set; } = "";

    [JsonPropertyName("HeadsetType")]
    public int HeadsetType { get; set; }

    [JsonPropertyName("Note")]
    public string Note { get; set; } = "";

    [JsonPropertyName("MallLink")]
    public string MallLink { get; set; } = "";

    [JsonPropertyName("Series")]
    public List<int> Series { get; set; } = [];

    [JsonPropertyName("Weight")]
    public int Weight { get; set; }

    [JsonPropertyName("ShowArea")]
    public int ShowArea { get; set; }

    [JsonPropertyName("IsFind")]
    public int IsFind { get; set; }

    [JsonPropertyName("IsSupportDynamicIyrics")]
    public int IsSupportDynamicLyrics { get; set; }

    [JsonPropertyName("IsSupportSport")]
    public int IsSupportSport { get; set; }

    [JsonPropertyName("IsSupportHealth")]
    public int IsSupportHealth { get; set; }

    [JsonPropertyName("IsSupportHeartRate")]
    public int IsSupportHeartRate { get; set; }

    [JsonPropertyName("IsSupportBloodOxygen")]
    public int IsSupportBloodOxygen { get; set; }

    [JsonPropertyName("IsATT")]
    public int IsATT { get; set; }

    [JsonPropertyName("IsDongle")]
    public int IsDongle { get; set; }

    [JsonPropertyName("IsDrainage")]
    public int IsDrainage { get; set; }

    [JsonPropertyName("IsVoicePacketSwitching")]
    public int IsVoicePacketSwitching { get; set; }

    [JsonPropertyName("LastFirmwareVersion")]
    public string LastFirmwareVersion { get; set; } = "";

    [JsonPropertyName("TWSImages")]
    public List<TWSImage> TWSImages { get; set; } = [];

    [JsonPropertyName("otherImages")]
    public List<OtherImage> OtherImages { get; set; } = [];

    [JsonPropertyName("product_guide_types")]
    public List<int> ProductGuideTypes { get; set; } = [];

    [JsonPropertyName("product_drainage_audio")]
    public ProductDrainageAudio AudioDrainage { get; set; } = new();

    [JsonPropertyName("is_show")]
    public int IsShow { get; set; }
    
    /// <summary>
    /// TWS image
    /// </summary>
    public class TWSImage {
        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("Online")]
        public int Online { get; set; }

        [JsonPropertyName("LeftHeadPhoneImageLink")]
        public string LeftHeadPhoneImageLink { get; set; } = "";

        [JsonPropertyName("RightHeadPhoneImageLink")]
        public string RightHeadPhoneImageLink { get; set; } = "";

        [JsonPropertyName("ChargeBinImageLink")]
        public string ChargeBinImageLink { get; set; } = "";
    }
    
    /// <summary>
    /// Other image
    /// </summary>
    public class OtherImage {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("tit")] // sus
        public string Title { get; set; } = "";

        [JsonPropertyName("imgLink")]
        public string ImageLink { get; set; } = "";

        [JsonPropertyName("parent_id")]
        public int ParentId { get; set; }
    }

    /// <summary>
    /// Product audio drainage
    /// </summary>
    public class ProductDrainageAudio {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("audio_url")]
        public string AudioUrl { get; set; } = "";

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";
    }
}