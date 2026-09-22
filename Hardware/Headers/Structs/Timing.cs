namespace Hardware.Headers;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Timing
{
    private byte raw;

    public TimingMode TimingMode => (TimingMode) (raw & 0x3);
}