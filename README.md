# MacCapsLock

Mac-style CapsLock language switching for Windows 10/11.

| Action | Result |
|---|---|
| Tap CapsLock (< 0.25s) | Switch to the next input language |
| Hold CapsLock | Toggle CAPS LOCK |
| Shift + CapsLock | Toggle CAPS LOCK |

- With more than two keyboard languages installed, each tap moves to the next one and wraps around (e.g. English â†’ Thai â†’ Japanese â†’ English).
- Switches instantly, without the Windows language pop-up. For apps that don't accept a direct switch (e.g. Store apps), it falls back to Win+Space.
- Single small `.exe` (~13 KB), no installer. Uses .NET Framework 4.x, which is built into Windows 10/11.
- Lives in the system tray. Right-click the tray icon to enable/disable, run at Windows startup, or exit.

## Usage

1. Download `MacCapsLock.exe` from [Releases](../../releases) (or build it yourself, see below).
2. Put it in a permanent folder and run it.
3. Right-click the tray icon â†’ **Run at Windows startup**.

> Close any other CapsLock remapper (e.g. AutoHotkey scripts) first so they don't conflict.
> It won't work inside apps running as Administrator unless MacCapsLock is also run as Administrator.

## Build

No SDK needed. Just run:

```bat
build.bat
```

The build uses the C# compiler that comes with Windows (`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`).
To change the hold time, edit `HoldMs` in `MacCapsLock.cs`.

## License

MIT
