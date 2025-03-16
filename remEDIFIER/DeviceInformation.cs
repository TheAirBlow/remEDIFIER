using remEDIFIER.Protocol;
using remEDIFIER.Protocol.Packets;

namespace remEDIFIER;

/// <summary>
/// Edifier device information
/// </summary>
public class DeviceInformation {
    public EdifierClient Client { get; }
    public string? SongAuthor { get; set; }
    public string? SongName { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? DeviceName { get; set; }
    public string? MacAddress { get; set; }
    public int? Battery { get; set; }
    public bool Playing { get; set; }
    
    /// <summary>
    /// Creates a new device information instance
    /// </summary>
    /// <param name="client">Edifier client</param>
    public DeviceInformation(EdifierClient client)
        => Client = client;

    /// <summary>
    /// Send information requests
    /// </summary>
    public void Request()
        => new Thread(RequestInternal).Start();
    
    /// <summary>
    /// Internal request implementation
    /// </summary>
    private void RequestInternal() {
        Client.Send(PacketType.GetPromptVolume, notify: true);
        Client.Send(PacketType.GetAudioDecoding, notify: true);
        Client.Send(PacketType.AVCRPState, notify: true);
        Client.Send(PacketType.GetFirmwareVersion, notify: true);
        Client.Send(PacketType.GetMacAddress, notify: true);
        if (Client.Supports(Feature.GameMode))
            Client.Send(PacketType.GetGameMode, notify: true);
        if (Client.Supports(Feature.Ldac))
            Client.Send(PacketType.GetLDAC, notify: true);
        if (Client.Supports(Feature.GetDeviceName))
            Client.Send(PacketType.GetDeviceName, notify: true);
        if (Client.Supports(Feature.ActiveNoiseCancellation))
            Client.Send(PacketType.GetANCMode, notify: true);
        if (Client.Supports(Feature.ShowBattery))
            Client.Send(PacketType.GetBattery, notify: true);
        if (Client.Supports(Feature.ShutdownTimer))
            Client.Send(PacketType.GetShutdownTimer, notify: true);
        if (Client.Supports(Feature.Equalizer))
            Client.Send(PacketType.GetEqualizer, notify: true);
        if (Client.Supports(Feature.NoAudioAutoShutdown))
            Client.Send(PacketType.GetShutdownWithNoAudio, notify: true);
        if (Client.Supports(Feature.TapControls))
            Client.Send(PacketType.GetControlSetting, notify: true);
    }

    /// <summary>
    /// Updates device information
    /// </summary>
    /// <param name="type">Packet type</param>
    /// <param name="data">Packet data</param>
    public void Update(PacketType type, IPacketData? data) {
        switch (type) {
            case PacketType.SongName:
                SongName = ((StringData)data!).Value;
                break;
            case PacketType.AuthorName:
                SongAuthor = ((StringData)data!).Value;
                break;
            case PacketType.GetPromptVolume:
                break;
            case PacketType.SetPromptVolume:
                break;
            case PacketType.GetGameMode:
                break;
            case PacketType.SetGameMode:
                break;
            case PacketType.SetLDAC:
                break;
            case PacketType.GetLDAC:
                break;
            case PacketType.PlayInfo:
                var info = (PlayData)data!;
                Playing = info.Playing;
                if (!Playing) {
                    SongAuthor = SongName = null;
                    break;
                }

                SongAuthor = info.Author;
                SongName = info.Song;
                break;
            case PacketType.GetAudioDecoding:
                break;
            case PacketType.SetANCMode:
                break;
            case PacketType.AVCRPState:
                Playing = ((AVCRPStateData)data!).State == AVCRPState.Playing;
                if (!Playing) SongAuthor = SongName = null;
                break;
            case PacketType.SetEqualizer:
                break;
            case PacketType.GetFirmwareVersion:
                FirmwareVersion = ((VersionData)data!).Version.ToString();
                break;
            case PacketType.GetMacAddress:
                MacAddress = ((MacAddressData)data!).Value;
                break;
            case PacketType.GetDeviceName:
                DeviceName = ((StringData)data!).Value;
                break;
            case PacketType.SetDeviceName:
                break;
            case PacketType.GetANCMode:
                break;
            case PacketType.GetBattery:
                Battery = ((ByteData)data!).Value;
                break;
            case PacketType.EnableShutdownTimer:
                break;
            case PacketType.DisableShutdownTimer:
                break;
            case PacketType.GetShutdownTimer:
                break;
            case PacketType.GetEqualizer:
                break;
            case PacketType.SetShutdownWithNoAudio:
                break;
            case PacketType.GetShutdownWithNoAudio:
                break;
            case PacketType.GetControlSetting:
                break;
            case PacketType.SetControlSetting:
                break;
        }
    }
}