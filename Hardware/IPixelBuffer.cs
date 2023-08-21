using Microsoft.Xna.Framework;

namespace Hardware;

public interface IPixelBuffer
{
    public uint[] Pixels { get; }
    public void SetPixel(ushort x, ushort y, uint colour);
}