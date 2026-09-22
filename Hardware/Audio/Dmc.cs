namespace Hardware.Audio;

using System;

public class Dmc
{
    private ushort[] rateLookup =
    {
        428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106, 84, 72, 54
    };

    public void Clock(Func<ushort, byte> reader)
    {
        
    }
}