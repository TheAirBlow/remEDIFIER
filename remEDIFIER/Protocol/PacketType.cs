namespace remEDIFIER.Protocol;

/// <summary>
/// Packet type enum
/// </summary>
public enum PacketType : byte {
    SongName = 0x01,
    AuthorName = 0x02,
    GetPromptVolume = 0x05,
    SetPromptVolume = 0x06,
    FactoryReset = 0x07,
    GetGameMode = 0x08,
    SetGameMode = 0x09,
    SetLDAC = 0x49,
    GetLDAC = 0x48,
    GetAudioDecoding = 0x68,
    SetANCMode = 0xC1,
    AVCRPCommand = 0xC2,
    AVCRPState = 0xC3,
    SetEqualizer = 0xC4,
    GetFirmwareVersion = 0xC6,
    GetMacAddress = 0xC8,
    GetDeviceName = 0xC9,
    SetDeviceName = 0xCA,
    GetANCMode = 0xCC,
    Disconnect = 0xCD,
    Shutdown = 0xCE,
    RePair = 0xCF,
    GetBattery = 0xD0,
    EnableShutdownTimer = 0xD1,
    DisableShutdownTimer = 0xD2,
    GetShutdownTimer = 0xD3,
    GetEqualizer = 0xD5,
    SetShutdownWithNoAudio = 0xD6,
    GetShutdownWithNoAudio = 0xD7,
    GetSupportedFeatures = 0xD8,
    GetControlSetting = 0xF0,
    SetControlSetting = 0xF1
}