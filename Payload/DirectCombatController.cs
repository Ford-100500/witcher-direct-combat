// Witcher Direct Combat beta controller bridge. Compile with the Windows .NET Framework C# compiler.
// Original mod by Nick Farrow (GitHub: VeggieVampire): https://github.com/VeggieVampire/witcher-direct-combat
// Copyright (c) 2026 Witcher Direct Combat contributors. MIT license; see LICENSE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal static class DirectCombatController
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Gamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short LeftX;
        public short LeftY;
        public short RightX;
        public short RightY;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct XInputState
    {
        public uint Packet;
        public Gamepad Pad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyInput
    {
        public ushort Vk;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public UIntPtr Extra;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint Data;
        public uint Flags;
        public uint Time;
        public UIntPtr Extra;
    }

    // This build targets 64-bit Windows. SendInput's union begins at byte 8 there.
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    private struct Input
    {
        [FieldOffset(0)] public uint Type;
        [FieldOffset(8)] public KeyInput Key;
        [FieldOffset(8)] public MouseInput Mouse;
    }

    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(uint slot, out XInputState state);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, Input[] inputs, int size);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int max);

    private const ushort A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000;
    private const ushort Menu = 0x0010;
    private const ushort DUp = 0x0001, DDown = 0x0002, DLeft = 0x0004, DRight = 0x0008;
    private const ushort LB = 0x0100, RB = 0x0200;
    private const ushort Back = 0x0020, LeftThumb = 0x0040, RightThumb = 0x0080;
    private const uint ScanCode = 0x0008, KeyUp = 0x0002;
    private const uint MouseMove = 0x0001, LeftDown = 0x0002, LeftUp = 0x0004;
    private const uint RightDown = 0x0008, RightUp = 0x0010;
    private const int StickEngage = 12000, StickRelease = 8500, CameraDeadzone = 7000;
    private const int TriggerThreshold = 96;
    private const ushort OverShoulderCamera = 0x3D; // F3
    private const ushort PauseGame = 0x39; // Space
    private const ushort Escape = 0x01; // Esc
    private const ushort Tab = 0x0F; // Tab
    private const ushort Inventory = 0x17; // I
    private const ushort Map = 0x32; // M
    private const ushort LeftAlt = 0x38; // Alt
    private static readonly ushort[] Styles = { 0x2D, 0x2C, 0x2E }; // X fast, Z strong, C group
    private static readonly ushort[] Signs = { 0x02, 0x03, 0x04, 0x05, 0x06 }; // 1 Aard, 2 Quen, 3 Yrden, 4 Igni, 5 Axii
    private static readonly ushort[] Weapons = { 0x10, 0x12, 0x13 }; // Q steel, E silver, R extra weapon 1
    private static readonly ushort[] QuickSlots = { 0x07, 0x08, 0x09 }; // 6, 7, 8: potions and bombs
    private static readonly Dictionary<char, ushort> MovementScans = new Dictionary<char, ushort>
    {
        { 'W', 0x11 }, { 'A', 0x1E }, { 'S', 0x1F }, { 'D', 0x20 }
    };

    private static void Send(Input input)
    {
        if (SendInput(1, new Input[] { input }, Marshal.SizeOf(typeof(Input))) != 1)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }

    private static void Key(ushort scan, bool down)
    {
        Input input = new Input();
        input.Type = 1;
        input.Key = new KeyInput { Scan = scan, Flags = ScanCode | (down ? 0u : KeyUp) };
        Send(input);
    }

    private static void Tap(ushort scan)
    {
        Key(scan, true);
        try { Thread.Sleep(50); }
        finally { Key(scan, false); }
    }

    private static void Mouse(uint flags, int dx, int dy)
    {
        Input input = new Input();
        input.Type = 0;
        input.Mouse = new MouseInput { Dx = dx, Dy = dy, Flags = flags };
        Send(input);
    }

    private static void Click()
    {
        Mouse(LeftDown, 0, 0);
        Mouse(LeftUp, 0, 0);
    }

    private static string ForegroundTitle()
    {
        StringBuilder text = new StringBuilder(256);
        GetWindowText(GetForegroundWindow(), text, text.Capacity);
        return text.ToString();
    }

    // Match the game window by owning process so localized titles ("Ведьмак", "The Witcher")
    // and title changes during loading both work.
    private static bool IsWitcherActive(Process witcherProcess)
    {
        if (witcherProcess == null) return false;
        uint foregroundProcess;
        GetWindowThreadProcessId(GetForegroundWindow(), out foregroundProcess);
        if (foregroundProcess == (uint)witcherProcess.Id) return true;
        return ForegroundTitle().StartsWith("The Witcher (", StringComparison.Ordinal);
    }

    private static bool Down(ushort buttons, ushort mask) { return (buttons & mask) != 0; }

    private static double CameraAxis(short value)
    {
        int magnitude = Math.Abs((int)value);
        if (magnitude <= CameraDeadzone) return 0;
        double fraction = (double)(magnitude - CameraDeadzone) / (32767 - CameraDeadzone);
        return Math.Sign(value) * fraction * fraction;
    }

    private static HashSet<char> WantedMovement(Gamepad pad, HashSet<char> held)
    {
        HashSet<char> wanted = new HashSet<char>();
        if (pad.LeftY >= (held.Contains('W') ? StickRelease : StickEngage)) wanted.Add('W');
        else if (pad.LeftY <= -(held.Contains('S') ? StickRelease : StickEngage)) wanted.Add('S');
        if (pad.LeftX >= (held.Contains('D') ? StickRelease : StickEngage)) wanted.Add('D');
        else if (pad.LeftX <= -(held.Contains('A') ? StickRelease : StickEngage)) wanted.Add('A');
        return wanted;
    }

    private static char DodgeDirection(Gamepad pad)
    {
        if (Math.Max(Math.Abs((int)pad.LeftX), Math.Abs((int)pad.LeftY)) < StickRelease) return 'S';
        if (Math.Abs((int)pad.LeftX) > Math.Abs((int)pad.LeftY)) return pad.LeftX > 0 ? 'D' : 'A';
        return pad.LeftY > 0 ? 'W' : 'S';
    }

    private static void Dodge(char direction)
    {
        ushort scan = MovementScans[direction];
        Key(scan, true); Thread.Sleep(60); Key(scan, false);
        Thread.Sleep(60);
        Key(scan, true);
        try { Thread.Sleep(60); }
        finally { Key(scan, false); }
    }

    private static void Attack(ushort styleScan)
    {
        Tap(styleScan);
        Thread.Sleep(50);
        Click();
    }

    private static void SelectSign(ushort scan)
    {
        Tap(scan);
    }

    // Windows owns Alt+Tab, Alt+Esc and Alt+Space, so release Alt around those keys.
    private static void TapClearingAlt(ushort scan, bool altDown)
    {
        if (!altDown)
        {
            Tap(scan);
            return;
        }
        Key(LeftAlt, false);
        Tap(scan);
        Key(LeftAlt, true);
    }

    private static void ReleaseHeld(HashSet<char> held, ref bool signMouseDown)
    {
        foreach (char direction in held) Key(MovementScans[direction], false);
        held.Clear();
        if (signMouseDown) { Mouse(RightUp, 0, 0); signMouseDown = false; }
    }

    private static Process FindWitcherProcess()
    {
        Process[] games = Process.GetProcessesByName("witcher");
        if (games.Length == 0) return null;
        for (int i = 1; i < games.Length; i++) games[i].Dispose();
        return games[0];
    }

    private static int Run(string[] args)
    {
        if (IntPtr.Size != 8)
        {
            Console.Error.WriteLine("This beta requires 64-bit Windows.");
            return 1;
        }
        int slot = 0;
        int duration = 86400;
        int startupTimeout = 1800;
        for (int i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == "--slot") int.TryParse(args[i + 1], out slot);
            if (args[i] == "--duration") int.TryParse(args[i + 1], out duration);
            if (args[i] == "--startup-timeout") int.TryParse(args[i + 1], out startupTimeout);
        }
        startupTimeout = Math.Max(1, startupTimeout);
        Console.WriteLine("Witcher Direct Combat: controller slot {0}; press Ctrl+C to stop.", slot);
        Stopwatch clock = Stopwatch.StartNew();
        double deadline = duration;
        double previousTick = clock.Elapsed.TotalSeconds;
        double remainderX = 0, remainderY = 0;
        bool previousX = false, previousY = false, previousA = false, previousB = false;
        bool previousMenu = false, previousLB = false;
        bool previousSignCast = false;
        bool f3Selected = false;
        bool previousBack = false, previousLeftThumb = false, previousRightThumb = false;
        bool previousUp = false, previousDown = false, previousLeft = false, previousRight = false;
        bool altLatch = false, altDown = false;
        int styleIndex = 1, signIndex = Signs.Length - 1, weaponIndex = Weapons.Length - 1;
        int quickTaps = 0;
        double quickTapDeadline = 0;
        bool signMouseDown = false;
        Process witcherProcess = null;
        double nextStartupCheck = 0, lastControllerInput = 0, nextIdleCheck = 30;
        bool wasIdle = false;
        HashSet<char> held = new HashSet<char>();
        try
        {
            while (clock.Elapsed.TotalSeconds < deadline)
            {
                double now = clock.Elapsed.TotalSeconds;
                if (witcherProcess == null && now >= nextStartupCheck)
                {
                    witcherProcess = FindWitcherProcess();
                    nextStartupCheck = now + 1.0;
                }
                if (witcherProcess == null && now >= startupTimeout)
                {
                    Console.WriteLine("Witcher did not start within the timeout; stopping controller bridge.");
                    break;
                }
                double dt = Math.Min(now - previousTick, 0.05);
                previousTick = now;
                XInputState state;
                bool connected = XInputGetState((uint)slot, out state) == 0;
                Gamepad pad = connected ? state.Pad : new Gamepad();
                bool controllerInput = connected && (pad.Buttons != 0 ||
                    pad.LeftTrigger >= 10 || pad.RightTrigger >= 10 ||
                    Math.Abs((int)pad.LeftX) >= StickRelease || Math.Abs((int)pad.LeftY) >= StickRelease ||
                    Math.Abs((int)pad.RightX) > CameraDeadzone || Math.Abs((int)pad.RightY) > CameraDeadzone);
                if (controllerInput)
                {
                    if (wasIdle && witcherProcess != null && witcherProcess.HasExited)
                    {
                        Console.WriteLine("Witcher closed; stopping controller bridge.");
                        break;
                    }
                    lastControllerInput = now;
                    wasIdle = false;
                }
                else if (witcherProcess != null && now - lastControllerInput >= 30 && now >= nextIdleCheck)
                {
                    if (witcherProcess.HasExited)
                    {
                        Console.WriteLine("Witcher closed; stopping controller bridge.");
                        break;
                    }
                    nextIdleCheck = now + 30;
                    wasIdle = true;
                }
                bool active = connected && IsWitcherActive(witcherProcess);
                HashSet<char> wanted = active ? WantedMovement(pad, held) : new HashSet<char>();
                foreach (char key in held) if (!wanted.Contains(key)) Key(MovementScans[key], false);
                foreach (char key in wanted) if (!held.Contains(key)) Key(MovementScans[key], true);
                held = wanted;

                double cameraX = active ? CameraAxis(pad.RightX) : 0;
                double cameraY = active ? -CameraAxis(pad.RightY) : 0;
                bool cameraStick = cameraX != 0 || cameraY != 0;
                if (active && !f3Selected)
                {
                    Tap(OverShoulderCamera);
                    Console.WriteLine("Selected F3 over-the-shoulder camera.");
                    f3Selected = true;
                }
                if (cameraStick)
                {
                    double rawX = cameraX * 900 * dt + remainderX;
                    double rawY = cameraY * 900 * dt + remainderY;
                    int dx = (int)rawX, dy = (int)rawY;
                    remainderX = rawX - dx; remainderY = rawY - dy;
                    if (dx != 0 || dy != 0) Mouse(MouseMove, dx, dy);
                }
                else { remainderX = 0; remainderY = 0; }

                bool x = connected && Down(pad.Buttons, X);
                bool y = connected && Down(pad.Buttons, Y);
                bool a = connected && Down(pad.Buttons, A);
                bool b = connected && Down(pad.Buttons, B);
                bool menu = connected && Down(pad.Buttons, Menu);
                bool back = connected && Down(pad.Buttons, Back);
                bool lb = connected && Down(pad.Buttons, LB);
                bool rb = connected && Down(pad.Buttons, RB);
                bool leftThumb = connected && Down(pad.Buttons, LeftThumb);
                bool rightThumb = connected && Down(pad.Buttons, RightThumb);
                bool up = connected && Down(pad.Buttons, DUp);
                bool down = connected && Down(pad.Buttons, DDown);
                bool left = connected && Down(pad.Buttons, DLeft);
                bool right = connected && Down(pad.Buttons, DRight);

                // RS click latches Alt until pressed again; RB holds Alt while held.
                if (active && rightThumb && !previousRightThumb) altLatch = !altLatch;
                previousRightThumb = rightThumb;
                if (!active) altLatch = false;
                bool altWanted = active && (rb || altLatch);
                if (altWanted != altDown)
                {
                    Key(LeftAlt, altWanted);
                    altDown = altWanted;
                }

                if (active && menu && !previousMenu) TapClearingAlt(Escape, altDown);
                previousMenu = menu;

                if (active && back && !previousBack) TapClearingAlt(PauseGame, altDown);
                previousBack = back;

                // LB sheathes or draws the sword (Tab).
                if (active && lb && !previousLB) TapClearingAlt(Tab, altDown);
                previousLB = lb;

                // Left stick click opens the map.
                if (active && leftThumb && !previousLeftThumb) Tap(Map);
                previousLeftThumb = leftThumb;

                // D-pad up cycles the combat style: fast, strong, group.
                if (active && up && !previousUp)
                {
                    styleIndex = (styleIndex + 1) % Styles.Length;
                    Tap(Styles[styleIndex]);
                }
                previousUp = up;

                // D-pad left cycles the weapon in hand: steel, silver, extra.
                if (active && left && !previousLeft)
                {
                    weaponIndex = (weaponIndex + 1) % Weapons.Length;
                    Tap(Weapons[weaponIndex]);
                }
                previousLeft = left;

                // D-pad right cycles the selected Sign.
                if (active && right && !previousRight)
                {
                    signIndex = (signIndex + 1) % Signs.Length;
                    Tap(Signs[signIndex]);
                }
                previousRight = right;

                // D-pad down opens the inventory.
                if (active && down && !previousDown) Tap(Inventory);
                previousDown = down;

                if (active && x && !previousX) Attack(Styles[styleIndex]);
                if (active && y && !previousY)
                {
                    if (quickTaps < QuickSlots.Length) quickTaps++;
                    quickTapDeadline = now + 0.4;
                }
                if (active && a && !previousA) Click(); // Talk, pick up, use
                previousX = x; previousY = y; previousA = a;

                // 1 tap uses slot 6, 2 quick taps slot 7, 3 quick taps slot 8.
                if (quickTaps > 0 && now >= quickTapDeadline)
                {
                    if (active) Tap(QuickSlots[quickTaps - 1]);
                    quickTaps = 0;
                }

                bool signCast = connected && pad.RightTrigger >= TriggerThreshold;
                if (active && signCast && !previousSignCast && !signMouseDown)
                {
                    Mouse(RightDown, 0, 0);
                    signMouseDown = true;
                }
                if (signMouseDown && (!active || !signCast))
                {
                    Mouse(RightUp, 0, 0);
                    signMouseDown = false;
                }
                previousSignCast = signCast;

                if (active && b && !previousB)
                {
                    char direction = DodgeDirection(pad);
                    foreach (char key in held) Key(MovementScans[key], false);
                    held.Clear();
                    Dodge(direction);
                }
                previousB = b;
                Thread.Sleep(20);
            }
        }
        finally
        {
            if (altDown)
            {
                try { Key(LeftAlt, false); } catch { }
                altDown = false;
            }
            ReleaseHeld(held, ref signMouseDown);
            if (witcherProcess != null) witcherProcess.Dispose();
        }
        Console.WriteLine("Controller bridge stopped; held input released.");
        return 0;
    }

    private static int Main(string[] args)
    {
        bool firstInstance;
        Mutex oneBridge = new Mutex(true, "Local\\WitcherDirectCombatController", out firstInstance);
        if (!firstInstance)
        {
            Console.WriteLine("Witcher Direct Combat is already running.");
            oneBridge.Dispose();
            return 0;
        }
        try { return Run(args); }
        finally
        {
            oneBridge.ReleaseMutex();
            oneBridge.Dispose();
        }
    }
}
