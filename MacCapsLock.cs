// MacCapsLock - Mac-style CapsLock language switcher for Windows
//  - กด CapsLock สั้น ๆ (< 0.25 วินาที)  : สลับภาษา (Win+Space)
//  - กดค้าง หรือ Shift+CapsLock          : เปิด/ปิด CAPS LOCK
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    [STAThread]
    static void Main()
    {
        bool created;
        using (var mutex = new Mutex(true, "MacCapsLock_SingleInstance", out created))
        {
            if (!created) return; // มีตัวที่รันอยู่แล้ว
            Application.EnableVisualStyles();
            Application.Run(new TrayApp());
        }
    }
}

class TrayApp : ApplicationContext
{
    const int HoldMs = 250;
    const string AppName = "MacCapsLock";
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    readonly NotifyIcon tray;
    readonly System.Windows.Forms.Timer holdTimer;
    readonly Native.LowLevelKeyboardProc proc;
    IntPtr hook = IntPtr.Zero;

    bool capsDown;   // CapsLock กำลังถูกกดอยู่
    bool handled;    // การกดครั้งนี้ถูกใช้ไปแล้ว (กดค้าง / Shift)
    bool enabled = true;

    public TrayApp()
    {
        proc = HookCallback;
        holdTimer = new System.Windows.Forms.Timer { Interval = HoldMs };
        holdTimer.Tick += delegate
        {
            holdTimer.Stop();
            if (capsDown && !handled) { handled = true; ToggleCapsLock(); }
        };

        var menu = new ContextMenuStrip();
        var enableItem = new ToolStripMenuItem("เปิดใช้งาน") { Checked = true, CheckOnClick = true };
        enableItem.CheckedChanged += delegate { enabled = enableItem.Checked; UpdateTooltip(); };
        var startupItem = new ToolStripMenuItem("เปิดพร้อม Windows") { Checked = IsStartupEnabled(), CheckOnClick = true };
        startupItem.CheckedChanged += delegate { SetStartup(startupItem.Checked); };
        menu.Items.Add(enableItem);
        menu.Items.Add(startupItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("ออก", null, delegate { ExitThread(); });

        tray = new NotifyIcon
        {
            Icon = MakeIcon(),
            ContextMenuStrip = menu,
            Visible = true
        };
        tray.DoubleClick += delegate { enableItem.Checked = !enableItem.Checked; };
        UpdateTooltip();

        using (var p = Process.GetCurrentProcess())
        using (var m = p.MainModule)
            hook = Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, proc, Native.GetModuleHandle(m.ModuleName), 0);
        if (hook == IntPtr.Zero)
            MessageBox.Show("ติดตั้ง keyboard hook ไม่สำเร็จ", AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    void UpdateTooltip()
    {
        tray.Text = enabled ? "MacCapsLock: CapsLock สลับภาษา" : "MacCapsLock: ปิดอยู่";
    }

    IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && enabled)
        {
            var k = (Native.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.KBDLLHOOKSTRUCT));
            bool ours = k.dwExtraInfo == Native.Marker;
            if (k.vkCode == Native.VK_CAPITAL && !ours)
            {
                int msg = wParam.ToInt32();
                bool down = msg == Native.WM_KEYDOWN || msg == Native.WM_SYSKEYDOWN;
                if (down)
                {
                    if (!capsDown) // ไม่นับ auto-repeat
                    {
                        capsDown = true;
                        handled = false;
                        if (ShiftHeld())
                        {
                            handled = true;
                            ToggleCapsLock();
                        }
                        else
                        {
                            holdTimer.Stop();
                            holdTimer.Start();
                        }
                    }
                }
                else
                {
                    holdTimer.Stop();
                    if (capsDown && !handled) SwitchLanguage();
                    capsDown = false;
                }
                return (IntPtr)1; // กลืนปุ่ม CapsLock จริงไว้
            }
        }
        return Native.CallNextHookEx(hook, nCode, wParam, lParam);
    }

    static bool ShiftHeld()
    {
        return (Native.GetAsyncKeyState(Native.VK_SHIFT) & 0x8000) != 0;
    }

    static void ToggleCapsLock()
    {
        Native.SendKeys(new[] {
            Native.Key(Native.VK_CAPITAL, false),
            Native.Key(Native.VK_CAPITAL, true)
        });
    }

    static void SwitchLanguage()
    {
        // Win+Space เหมือนสคริปต์ AHK เดิม ใช้ได้กับทุกแอปรวมถึงแอป UWP
        Native.SendKeys(new[] {
            Native.Key(Native.VK_LWIN, false),
            Native.Key(Native.VK_SPACE, false),
            Native.Key(Native.VK_SPACE, true),
            Native.Key(Native.VK_LWIN, true)
        });
    }

    static bool IsStartupEnabled()
    {
        using (var key = Registry.CurrentUser.OpenSubKey(RunKey))
            return key != null && key.GetValue(AppName) != null;
    }

    static void SetStartup(bool on)
    {
        using (var key = Registry.CurrentUser.CreateSubKey(RunKey))
        {
            if (on) key.SetValue(AppName, "\"" + Application.ExecutablePath + "\"");
            else key.DeleteValue(AppName, false);
        }
    }

    static Icon MakeIcon()
    {
        using (var bmp = new Bitmap(32, 32))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using (var path = new GraphicsPath())
            {
                path.AddArc(1, 1, 10, 10, 180, 90);
                path.AddArc(21, 1, 10, 10, 270, 90);
                path.AddArc(21, 21, 10, 10, 0, 90);
                path.AddArc(1, 21, 10, 10, 90, 90);
                path.CloseFigure();
                using (var b = new SolidBrush(Color.FromArgb(0, 120, 212))) g.FillPath(b, path);
            }
            using (var f = new Font("Leelawadee UI", 15, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString("กA", f, Brushes.White, new RectangleF(0, 0, 32, 32), sf);
            return Icon.FromHandle(bmp.GetHicon());
        }
    }

    protected override void ExitThreadCore()
    {
        if (hook != IntPtr.Zero) Native.UnhookWindowsHookEx(hook);
        tray.Visible = false;
        tray.Dispose();
        base.ExitThreadCore();
    }
}

static class Native
{
    public const int WH_KEYBOARD_LL = 13;
    public const int WM_KEYDOWN = 0x0100, WM_SYSKEYDOWN = 0x0104;
    public const int VK_CAPITAL = 0x14, VK_SHIFT = 0x10, VK_LWIN = 0x5B, VK_SPACE = 0x20;
    public static readonly IntPtr Marker = (IntPtr)0x4D43414C; // "MCAL" ใช้แยกปุ่มที่แอปนี้ส่งเอง
    const uint INPUT_KEYBOARD = 1, KEYEVENTF_KEYUP = 0x0002, KEYEVENTF_EXTENDEDKEY = 0x0001;

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT { public int vkCode, scanCode, flags, time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT { public ushort wVk, wScan; public uint dwFlags, time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT { public uint type; public InputUnion u; }

    public static INPUT Key(int vk, bool up)
    {
        var i = new INPUT { type = INPUT_KEYBOARD };
        i.u.ki.wVk = (ushort)vk;
        i.u.ki.dwFlags = (up ? KEYEVENTF_KEYUP : 0) | (vk == VK_LWIN ? KEYEVENTF_EXTENDEDKEY : 0);
        i.u.ki.dwExtraInfo = Marker;
        return i;
    }

    public static void SendKeys(INPUT[] inputs)
    {
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}
