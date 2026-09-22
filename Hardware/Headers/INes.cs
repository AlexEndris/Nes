using System.Runtime.InteropServices;

namespace Hardware.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct INes
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string Identifier;

    public byte PrgRomBanks;
    public byte ChrRomBanks;
    public Flags6 Flags6;
    public Flags7 Flags7;
    private byte InternalPrgRamBanks;
    private byte Unused9;
    private byte Unused10;
    private byte Unused11;
    private byte Unused12;
    private byte Unused13;
    private byte Unused14;
    private byte Unused15;

    public ushort MapperId => (ushort) (Flags7.MapperFirstHigherNibble << 4 | Flags6.MapperFirstLowerNibble);
    public int PrgRomSize => PrgRomBanks * 16 * 1024;
    public int ChrRomSize => ChrRomBanks * 8 * 1024;
    public int PrgRamSize => (PrgRamBanks == 0 ? 1 : PrgRamBanks) * 8 * 1024;
    public ushort PrgRamBanks => (ushort)(InternalPrgRamBanks == 0 ? 1 : InternalPrgRamBanks);
}