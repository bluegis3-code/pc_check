using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PCCheck;

internal static class Program
{
    private static Mutex? mutex;
    [STAThread]
    static void Main()
    {
        mutex = new Mutex(true, "PCCheck_SingleInstance_8F25A", out var first);
        if (!first) { MessageBox.Show("PC Check가 이미 실행 중입니다.", "PC Check"); return; }
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal static class Native
{
    [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("user32.dll")] internal static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    [StructLayout(LayoutKind.Sequential)] internal struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
    internal static uint IdleSeconds()
    {
        var i = new LASTINPUTINFO { cbSize=(uint)Marshal.SizeOf<LASTINPUTINFO>() };
        return GetLastInputInfo(ref i) ? ((uint)Environment.TickCount-i.dwTime)/1000 : 0;
    }
    internal static string ActiveProcess()
    {
        GetWindowThreadProcessId(GetForegroundWindow(), out var id);
        try { return Process.GetProcessById((int)id).ProcessName; } catch { return "알 수 없음"; }
    }
}
