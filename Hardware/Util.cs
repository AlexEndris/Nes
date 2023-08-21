using Microsoft.Xna.Framework;

namespace Hardware;

public static class Util
{
    public static uint ToPackedColor(this uint value)
    {
        byte r = (byte) (value >> 24);
        byte g = (byte) (value << 8 >> 24);
        byte b = (byte) (value << 16 >> 24);
        Color color = new Color(r, g, b);

        return color.PackedValue;
    }
}