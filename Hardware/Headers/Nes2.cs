using System.Runtime.InteropServices;

namespace Hardware.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct Nes2
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
    public string Identifier;

    private byte PrgRomSizeLSB;
    private byte ChrRomSizeLSB;
    public Flags6 Flags6;
    public Flags7 Flags7;
    public Mapper Mapper;
    private RomMsb RomMsb;
    private PrgRamNvRam PrgRamNvRam;
    private ChrRamNvRam ChrRamNvRam;
    public Timing Timing;
    private byte ConsoleType;
    public byte MiscRoms;
    public byte DefaultExpansionDevice;

    public ushort MapperId => (ushort) (Mapper.MapperSecondLowerNibble << 8 | Flags7.MapperFirstHigherNibble << 4 |
                                        Flags6.MapperFirstLowerNibble);

    public bool IsNes2 => Flags7.Identifier == 2;
    public bool IsINes => Flags7.Identifier != 2;
    
    public ushort PrgRomBanks => (ushort)(RomMsb.PrgRomSizeMsb << 8 | PrgRomSizeLSB);
    public ushort ChrRomBanks => (ushort)(RomMsb.ChrRomSizeMsb << 8 | ChrRomSizeLSB);
    public int PrgRomSize => PrgRomBanks * 16 * 1024;
    public int ChrRomSize => ChrRomBanks * 8 * 1024;
    public int PrgRamSize => PrgRamNvRam.PrgRamShift == 0 ? 0 : 64 << PrgRamNvRam.PrgRamShift;
    public ushort PrgRamBanks => (ushort)(PrgRamSize / (8 * 1024));
    public ushort PrgNvRamSize => (ushort) (PrgRamNvRam.PrgNvRamShift == 0 ? 0 : 64 << PrgRamNvRam.PrgNvRamShift);
    public ushort ChrRamSize => (ushort) (ChrRamNvRam.ChrRamShift == 0 ? 0 : 64 << ChrRamNvRam.ChrRamShift);
    public ushort ChrNvRamSize => (ushort) (ChrRamNvRam.ChrNvRamShift == 0 ? 0 : 64 << ChrRamNvRam.ChrNvRamShift);

    public ConsoleTypeConfig ConsoleTypeConfig
    {
        get
        {
            if (Flags7.ConsoleType == Headers.ConsoleType.NVS)
                return new ConsoleTypeConfig
                {
                    Type = Flags7.ConsoleType,
                    PPUType = (byte) (ConsoleType & 0xF),
                    HardwareType = (byte) (ConsoleType >> 4)
                };

            if (Flags7.ConsoleType == Headers.ConsoleType.ECT)
                return new ConsoleTypeConfig
                {
                    Type = Flags7.ConsoleType,
                    ExtendedType = (byte)(ConsoleType & 0xF)
                };
            
            return new ConsoleTypeConfig();
        }
    }
}