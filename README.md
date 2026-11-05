# MacCapsLock

Mac-style CapsLock language switching for Windows 10/11.
สลับภาษาด้วยปุ่ม Caps Lock แบบ Mac บน Windows

| Action | Result |
|---|---|
| Tap CapsLock (< 0.25s) / กดสั้น | Switch input language (Win+Space) / สลับภาษา |
| Hold CapsLock / กดค้าง | Toggle CAPS LOCK / เปิด-ปิด Caps Lock |
| Shift + CapsLock | Toggle CAPS LOCK |

- Single small `.exe` (~11 KB), no installer. Uses .NET Framework 4.x, which is built into Windows 10/11.
- Lives in the system tray. Right-click the tray icon to enable/disable, run at Windows startup, or exit.

## Usage / วิธีใช้

1. Download `MacCapsLock.exe` from [Releases](../../releases) (or build it yourself, see below).
2. Put it in a permanent folder and run it.
3. Right-click the tray icon → **เปิดพร้อม Windows** (Run at startup).

> Close any other CapsLock remapper (e.g. AutoHotkey scripts) first so they don't conflict.
> It won't work inside apps running as Administrator unless MacCapsLock is also run as Administrator.

## Build / คอมไพล์เอง

No SDK needed. Just run:

```bat
build.bat
```

The build uses the C# compiler that comes with Windows (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`).
To change the hold time, edit `HoldMs` in `MacCapsLock.cs`.

## License

MIT
