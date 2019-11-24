using System;
using System.Runtime.InteropServices;

public static class TimeControl
{
    [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
    public extern static void Win32GetSystemTime(ref SystemTime sysTime);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern void SetSystemTime(ref SystemTime st);

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    public static void GetSystemTime(out DateTime dt)
    {
        var time = new SystemTime();
        Win32GetSystemTime(ref time);
        dt = time.ToDateTime();
    }

    public static void SetSystemTime(in DateTime dt)
    {
        var time = dt.ToSystemTime();
        SetSystemTime(ref time);
    }

    public static DateTime ToDateTime(this SystemTime st) =>
        new DateTime(st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);

    public static SystemTime ToSystemTime(this DateTime dt)
    {
        return new SystemTime()
        {
            wYear = (short) dt.Year,
            wMonth = (short) dt.Month,
            wDay = (short) dt.Day,
            wHour = (short) dt.Hour,
            wMinute = (short) dt.Minute,
            wSecond = (short) dt.Second,
            wMilliseconds = (short) dt.Millisecond,
        };
    }
}