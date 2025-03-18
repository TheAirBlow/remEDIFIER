## Documentation
> [!WARNING]
> This is no longer up to date. You should [read the source code](https://github.com/TheAirBlow/remEDIFIER/tree/main/remEDIFIER/Protocol) instead.

This isn't really supposed to be readable by a non-autistic human being. \
If you have issues deciphering this, make yourself a favor and click off xD

## Decrypting products_release.json
Use [this CyberChef recipe](https://gchq.github.io/CyberChef/#recipe=From_Base64('A-Za-z0-9%2B/%3D',true,false)AES_Decrypt(%7B'option':'UTF8','string':'HiO9d5jV0Ln7VNE5M%2BlM%3DE%26E%5EMobt_TA'%7D,%7B'option':'UTF8','string':'3-eM$%2BG6-UZBisv$'%7D,'CBC','Raw','Raw',%7B'option':'Hex','string':''%7D,%7B'option':'Hex','string':''%7D)) and input the file's contents.

## Format
Header (AA/BB), Length byte, Payload, CRC32. \
BLE above version 1 use encryption which I cba to document and implement. \
Probably for newer models, internally encryption is enabled when BLE version is above 1.
```java
public final byte[] addCRC(byte[] dataArray) {
    int i = 8217;
    for (byte b : dataArray) {
        i += b & UByte.MAX_VALUE;
    }
    int i2 = (i >> 8) & 255;
    int i3 = i & 255;
    return ArraysKt.plus(ArraysKt.plus(dataArray, (byte) i2), (byte) i3);
}
```

## Outbound (AA)
- `C1xxyy` - ANC mode, values are specified based on D8 bea (yy usually used as AN volume)
- `C2xx` - 00: play, 01: pause, 02: vol up, 03: vol down, 04: next, 05: prev
- `C4xx` - set equalizer, values are specified based on D8 bean
- `F1xxyy` - button modes, values are specified based on D8 bean (bit flags)
- `49xx` - 00: LDAC off, 01: LDAC 48k, 02: LDAC 96k, 03: LDAC 192K
- `09xx` - 00: game mode off, 01: game mode on
- `06xx` - prompt volume (no limits, stock is 0-15)
- `CAxx...` - set device name (limit specified by D8 bean)
- `D2` - disable shutdown timer
- `D1xxxx` - enable shutdown timer, time is in minutes
- `D6xx` - enable automatic shutdown without music in 20 mins
- `CE` - shutdown
- `CD` - disconnect
- `CF` - repair
- `07` - factory reset

## Inbound (BB)
- `D5xx` - current equaliser
- `08xx` - game mode
- `D0xx` - battery percentage
- `48xx` - LDAC mode
- `05xx` - prompt volume
- `D3xx` - shutdown timer
- `D7xx` - automatic shutdown without music in 20 mins
- `C8xxxxxxxxxxxx` - mac address
- `C6xx...` - firmware version, usually just 3 octets
- `CCxxyy` - current mode + ambient noise volume
- `C9xx` - device name
- `F0xxyy` - button modes
- `D3xxxx` - shutdown timeout (two byte integer)
- `C3xx` - 03: paused, 0D: unpaused
- `01xx...` - song name (UTF-8 encoded, max length appears to be 35)
- `02xx...` - author name (UTF-8 encoded, max length appears to be 35)
- `D8xx` - documented below

## D8 inbound aka. PAIN 😭
Some of the last ones might be missing. Feature flags just get stacked on top of one another with time. \
`!!` means that feature's protocol was not documented. \
`??` means that the flag was not used in the frontend.
1) ANC value (4 bits | 06: normal, high NC, medium NC, AS | 0C: normal, NC, AS | 13: high NC, medium NC, low NC, AS, wind reduction, normal | 17: NC, AS, Normal | 1A: high NC, low NC, wind reduction, AS, normal | 1F: high NC, medium NC, low NC, AS, wind reduction, normal | 10: high NC, medium NC, wind reduction, AS, normal | 11: adaptive NC, AS, normal | 1C: NC, AS, normal | 1D: adaptive NC, high NC, medium NC, AS > adaptive transparency + highly human voice + balanced + background sound, wind reduction, normal), ?? RChannel, ?? Peer, ?? TWS
2) supports show battery
3) supports query name
4) supports set name (1 = 24 max, 2 = 30 max, 3 = 29 max, 4 = 35 max)
5) supports query mac
6) supports query version
7) supports disconnect
8) supports repair
9) supports auto shutdown without audio input for 20 mins
10) supports shutdown timer (01: never>10>20>30>60>90, 02: never>5>15>30>60>180 in frontend, but there is no hard limit)
11) supports manual shutdown
12) supports show manual
13) !! AndroidSppOTA, !! AndroidBleOTA, !! IOSSppOTA, !! IOSBleOTA, !! ShareMe, !! TapReset, !! StepCount, !! VoiceSwitch (bit flags, << N from zero in order)
14) support equalizer (00: no | 01: classic, pop, classical, rock | 02: classic, pop, classical, rock, rock?, rock? | 03: classic, pop, rock | 04: classic, pop, classical, rock, customized | 06: classic, surround, game | 07: classic, dynamic, customized | 08: classic, dynamic, surround, customized | 09: classic, hifi, stax | 0a: classic, monitor, dynamic, vocal, customized | 0b: classic, game, classic?, dynamic, customized | 0c: music, game, movie, customized | 0d: music, game, customized | 0e: music, game, movie | 0f: music, game, movie, customized | 10: classic, monitor, dynamic, vocal, customized | 11: classic, monitor, game, vocal, customized | 12: music, monitor, game, movie, customized | 16: original, dynamic, monitor, customized | 17: original, dynamic, electrostatic, customized | 18: class, dynamic, customized | 19: classic, bassy, vocal, customized | 1a: classic, popular, classical, rock, movie | 1b: class, classical, bassy, rock, customized | 1d: class, DYZJ, vocal, GYZQ | 1e: monitor, music, customized | 20: class, dynamic, vocal, customized | 23: class, popular, classical, rock, theatre, customized) *gasp*... HOLY SHIT THIS WAS PAINFUL TO DOCUMENT
15) ?? skin (whatever that means)
16) !! OnShake, !! OffShake, !! CallShake, !! WXShake, !! Shake but shift by 7 for some reason FUCK THIS out of order motherfucker (bit flags, << N from zero in order)
17) supports tap or button controls (only 0F will be implemented, other values and their official implementation [can be seen here](https://gist.github.com/TheAirBlow/c9db9a26ce182419d0b8d90ceaf209c3) for later documentation)
18) !! LEDSettings, !! BeepSwitchSettings, !! InEarDetectionSettings, !! TapSensitiveSettings, !! BeepVolumeSettings, !! DeviceResetSettings, GameMode, !! ShowBoxBattery (bit flags, << N from zero in order)
19) !! WearingFitDetection, !! LHDC, LDAC, !! EarmuffsSwitch, !! WindNoiseSettings, !! DeviceLeakDetection, !! ClearHeadphonePairingRecord, !! LightColorSettings
20) !! InputSourceSettings, !! VolumeSettings, !! DragonSound, !! AmbientLightTimingOffSettings, ShowMusicInfo, !! Pressure, !! Touch, !! AmbientLight
21) !! SoundSpace, !! PromptToneSettings, !! HiRes, !! Btn, !! HearingProtection, !! TimeCalibration, !! Recovery, !! OnDragTwo
22) !! TimeCalibration, !! FastCharge, !! DenoiseMode, !! Study, !! SmartLight, !! BeepSet
23) !! HdAudioCodec e.g. LDAC/LHDC, !! additional 192K option but shift by 7 (bit flags, << N from zero in order)
24) !! SavingMode, ?? Mic, !! LineHiRes but shift by 4, !! LanSwitch but shift by 6 (bit flags, << N from zero in order)
