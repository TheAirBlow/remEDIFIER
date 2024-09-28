using remEDIFIER.Protocol.Values;

namespace remEDIFIER.Protocol.Packets;

/// <summary>
/// Support packet data
/// </summary>
public class SupportData : IPacketData {
    /// <summary>
    /// Packet type to apply this data object to
    /// </summary>
    public PacketType[] Types => [PacketType.GetSupportedFeatures];

    /// <summary>
    /// Array of supported features
    /// </summary>
    public Feature[] Features { get; private set; } = [];
    
    /// <summary>
    /// Equalizer value
    /// </summary>
    public EqualizerValue? EqualizerValue { get; private set; }
    
    /// <summary>
    /// ANC value
    /// </summary>
    public ANCValue? AncValue { get; private set; }
    
    /// <summary>
    /// Maximum device name length
    /// </summary>
    public int? MaxDeviceName { get; private set; }

    /// <summary>
    /// Tap value
    /// </summary>
    public byte? TapValue { get; private set; }
    
    /// <summary>
    /// Deserializes packet from byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <param name="buf">Buffer</param>
    public void Deserialize(PacketType type, SupportData support, byte[] buf) {
        var features = new List<Feature>();
        if (buf.Length > 0) {
            AncValue = new ANCValue((byte)(buf[0] & 15));
            if (((buf[0] >> 5) & 1) == 1) features.Add(Feature.RightChannel);
            if (((buf[0] >> 6) & 1) == 1) features.Add(Feature.PeerHeadphones);
            if (((buf[0] >> 7) & 1) == 1) features.Add(Feature.TwsHeadphones);
        }

        if (buf.Length > 1 && buf[1] > 0) features.Add(Feature.ShowBattery);
        if (buf.Length > 2 && buf[2] > 0) features.Add(Feature.GetDeviceName);
        if (buf.Length > 3 && buf[3] > 0) {
            MaxDeviceName = buf[3] switch {
                1 => 24, 2 => 30, 3 => 29,
                4 => 35, _ => null
            };
            features.Add(Feature.SetDeviceName);
        }
        
        if (buf.Length > 4 && buf[4] > 0) features.Add(Feature.GetMacAddress);
        if (buf.Length > 5 && buf[5] > 0) features.Add(Feature.GetFirmwareVersion);
        if (buf.Length > 6 && buf[6] > 0) features.Add(Feature.Disconnect);
        if (buf.Length > 7 && buf[7] > 0) features.Add(Feature.RePair);
        if (buf.Length > 8 && buf[8] > 0) features.Add(Feature.NoAudioAutoShutdown);
        if (buf.Length > 9 && buf[9] > 0) features.Add(Feature.ShutdownTimer);
        if (buf.Length > 10 && buf[10] > 0) features.Add(Feature.ManualShutdown);
        if (buf.Length > 11 && buf[11] > 0) features.Add(Feature.ShowManual);
        if (buf.Length > 12) {
            if ((buf[12] & 1) == 1) features.Add(Feature.AndroidRfcommOta);
            if (((buf[12] >> 1) & 1) == 1) features.Add(Feature.AndroidBleOta);
            if (((buf[12] >> 2) & 1) == 1) features.Add(Feature.IosRfcommOta);
            if (((buf[12] >> 3) & 1) == 1) features.Add(Feature.IosBleOta);
            if (((buf[12] >> 4) & 1) == 1) features.Add(Feature.ShareMe);
            if (((buf[12] >> 5) & 1) == 1) features.Add(Feature.TapReset);
            if (((buf[12] >> 6) & 1) == 1) features.Add(Feature.StepCount);
            if (((buf[12] >> 7) & 1) == 1) features.Add(Feature.VoiceSwitch);
        }

        if (buf.Length > 13) EqualizerValue = new EqualizerValue(buf[13]);
        if (buf.Length > 14) {
            if ((buf[14] & 1) == 1) features.Add(Feature.OnShake);
            if (((buf[14] >> 1) & 1) == 1) features.Add(Feature.OffShake);
            if (((buf[14] >> 2) & 1) == 1) features.Add(Feature.CallShake);
            if (((buf[14] >> 3) & 1) == 1) features.Add(Feature.WxShake);
            if (((buf[14] >> 7) & 1) == 1) features.Add(Feature.Shake);
        }

        if (buf.Length > 15 && buf[15] > 0) {
            // TODO: implement TapValue based on https://gist.github.com/TheAirBlow/c9db9a26ce182419d0b8d90ceaf209c3
            features.Add(Feature.TapControls); TapValue = buf[15];
        }
        
        if (buf.Length > 16) {
            if ((buf[16] & 1) == 1) features.Add(Feature.LedSettings);
            if (((buf[16] >> 1) & 1) == 1) features.Add(Feature.BeepSwitchSettings);
            if (((buf[16] >> 2) & 1) == 1) features.Add(Feature.InEarDetectionSettings);
            if (((buf[16] >> 3) & 1) == 1) features.Add(Feature.TapSensitiveSettings);
            if (((buf[16] >> 4) & 1) == 1) features.Add(Feature.BeepVolumeSettings);
            if (((buf[16] >> 5) & 1) == 1) features.Add(Feature.DeviceResetSettings);
            if (((buf[16] >> 6) & 1) == 1) features.Add(Feature.GameMode);
            if (((buf[16] >> 7) & 1) == 1) features.Add(Feature.BoxBattery);
        }
        
        if (buf.Length > 17) {
            if ((buf[17] & 1) == 1) features.Add(Feature.WearingFitDetection);
            if (((buf[17] >> 1) & 1) == 1) features.Add(Feature.Lhdc);
            if (((buf[17] >> 2) & 1) == 1) features.Add(Feature.Ldac);
            if (((buf[17] >> 3) & 1) == 1) features.Add(Feature.EarmuffsSwitch);
            if (((buf[17] >> 4) & 1) == 1) features.Add(Feature.WindNoiseSettings);
            if (((buf[17] >> 5) & 1) == 1) features.Add(Feature.DeviceLeakDetection);
            if (((buf[17] >> 6) & 1) == 1) features.Add(Feature.ClearPairingRecord);
            if (((buf[17] >> 7) & 1) == 1) features.Add(Feature.LightColorSettings);
        }
        
        if (buf.Length > 18) {
            if ((buf[18] & 1) == 1) features.Add(Feature.InputSourceSettings);
            if (((buf[18] >> 1) & 1) == 1) features.Add(Feature.VolumeSettings);
            if (((buf[18] >> 2) & 1) == 1) features.Add(Feature.DragonSound);
            if (((buf[18] >> 3) & 1) == 1) features.Add(Feature.AmbientLightTimingOffSettings);
            if (((buf[18] >> 4) & 1) == 1) features.Add(Feature.MusicInfo);
            if (((buf[18] >> 5) & 1) == 1) features.Add(Feature.Pressure);
            if (((buf[18] >> 6) & 1) == 1) features.Add(Feature.Touch);
            if (((buf[18] >> 7) & 1) == 1) features.Add(Feature.AmbientLight);
        }
        
        if (buf.Length > 19) {
            if ((buf[19] & 1) == 1) features.Add(Feature.SoundSpace);
            if (((buf[19] >> 1) & 1) == 1) features.Add(Feature.PromptToneSettings);
            if (((buf[19] >> 2) & 1) == 1) features.Add(Feature.HiRes);
            if (((buf[19] >> 3) & 1) == 1) features.Add(Feature.Button);
            if (((buf[19] >> 4) & 1) == 1) features.Add(Feature.HearingProtection);
            if (((buf[19] >> 5) & 1) == 1) features.Add(Feature.TimeCalibration);
            if (((buf[19] >> 6) & 1) == 1) features.Add(Feature.Recovery);
            if (((buf[19] >> 7) & 1) == 1) features.Add(Feature.OnDragTwo);
        }
        
        if (buf.Length > 20) {
            if ((buf[20] & 1) == 1) features.Add(Feature.TimeCalibration);
            if (((buf[20] >> 1) & 1) == 1) features.Add(Feature.FastCharge);
            if (((buf[20] >> 2) & 1) == 1) features.Add(Feature.DenoiseMode);
            if (((buf[20] >> 3) & 1) == 1) features.Add(Feature.Study);
            if (((buf[20] >> 4) & 1) == 1) features.Add(Feature.SmartLight);
            if (((buf[20] >> 5) & 1) == 1) features.Add(Feature.BeepSet);
        }
        
        if (buf.Length > 21) {
            if ((buf[21] & 1) == 1) features.Add(Feature.HdAudioCodec);
            if (((buf[21] >> 7) & 1) == 1) features.Add(Feature.Allow192K);
        }
        
        if (buf.Length > 22) {
            if ((buf[22] & 1) == 1) features.Add(Feature.SavingMode);
            if (((buf[22] >> 1) & 1) == 1) features.Add(Feature.Microphone);
            if (((buf[22] >> 4) & 1) == 1) features.Add(Feature.LineHiRes);
            if (((buf[22] >> 6) & 1) == 1) features.Add(Feature.LanSwitch);
        }
        
        Features = features.ToArray();
    }

    /// <summary>
    /// Serializes packet to byte buffer
    /// </summary>
    /// <param name="type">Packet Type</param>
    /// <param name="support">Support</param>
    /// <returns>Buffer</returns>
    public byte[] Serialize(PacketType type, SupportData support)
        => throw new NotImplementedException();
}

/// <summary>
/// Features enum
/// </summary>
public enum Feature {
    RightChannel,
    PeerHeadphones,
    TwsHeadphones,
    ShowBattery,
    GetDeviceName,
    SetDeviceName,
    GetMacAddress,
    GetFirmwareVersion,
    Disconnect,
    RePair,
    NoAudioAutoShutdown,
    ShutdownTimer,
    ManualShutdown,
    ShowManual,
    AndroidRfcommOta,
    AndroidBleOta,
    IosRfcommOta,
    IosBleOta,
    ShareMe,
    TapReset,
    StepCount,
    VoiceSwitch,
    OnShake,
    OffShake,
    CallShake,
    WxShake,
    Shake,
    TapControls,
    LedSettings,
    BeepSwitchSettings,
    InEarDetectionSettings,
    TapSensitiveSettings,
    BeepVolumeSettings,
    DeviceResetSettings,
    GameMode,
    BoxBattery,
    WearingFitDetection,
    Lhdc,
    Ldac,
    EarmuffsSwitch,
    WindNoiseSettings,
    DeviceLeakDetection,
    ClearPairingRecord,
    LightColorSettings,
    InputSourceSettings,
    VolumeSettings,
    DragonSound,
    AmbientLightTimingOffSettings,
    MusicInfo,
    Pressure,
    Touch,
    AmbientLight,
    SoundSpace,
    PromptToneSettings,
    HiRes,
    Button,
    HearingProtection,
    TimeCalibration,
    Recovery,
    OnDragTwo,
    FastCharge,
    DenoiseMode,
    Study,
    SmartLight,
    BeepSet,
    HdAudioCodec,
    Allow192K,
    SavingMode,
    Microphone,
    LineHiRes,
    LanSwitch
}