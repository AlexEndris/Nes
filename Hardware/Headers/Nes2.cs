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
    private byte PrgRomSizeMSB;
    private byte ChrRomSizeMSB;
    private byte PrgRamShift;
    private byte PrgNvRamShift;
    private byte ChrRamShift;
    private byte ChrNvRamShift;
    public Timing Timing;
    private byte ConsoleType;
    public byte MiscRoms;
    public byte DefaultExpansionDevice;

    public ushort MapperId => (ushort) (Mapper.MapperSecondLowerNibble << 8 | Flags7.MapperFirstHigherNibble << 4 |
                                        Flags6.MapperFirstLowerNibble);

    public bool IsNES2 => Flags7.Identifier == 0x10;
    public bool IsINes => Flags7.Identifier != 0x10;
    
    public ushort PrgRomSize => (ushort) (PrgRomSizeMSB << 8 | PrgRomSizeLSB);
    public ushort ChrRomSize => (ushort) (ChrRomSizeMSB << 8 | ChrRomSizeLSB);

    public ushort PrgRamSize => (ushort) (PrgRamShift == 0 ? 0 : 64 << PrgRamShift);
    public ushort PrgNvRamSize => (ushort) (PrgNvRamShift == 0 ? 0 : 64 << PrgNvRamShift);
    public ushort ChrRamSize => (ushort) (ChrRamShift == 0 ? 0 : 64 << ChrRamShift);
    public ushort ChrNvRamSize => (ushort) (ChrNvRamShift == 0 ? 0 : 64 << ChrNvRamShift);

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

[StructLayout(LayoutKind.Sequential)]
public struct Flags6
{
    private byte raw;
     
    public Mirroring Mirroring => (Mirroring)(raw & 0x1);
    public bool Battery => (raw >> 1 & 0x1) == 1;
    public bool Trainer => (raw >> 2 & 0x1) == 1;
    public bool FourScreen => (raw >> 3 & 0x1) == 1;
    public byte MapperFirstLowerNibble => (byte)(raw >> 4 & 0xF);
}

[StructLayout(LayoutKind.Sequential)]
public struct Flags7
{
    private byte raw;
     
    public ConsoleType ConsoleType => (ConsoleType)(raw & 0x3);
    public byte Identifier => (byte) (raw >> 2 & 0x3);
    public byte MapperFirstHigherNibble => (byte)(raw >> 4 & 0xF);
}

[StructLayout(LayoutKind.Sequential)]
public struct Mapper
{
    private byte raw;

    public byte MapperSecondLowerNibble => (byte) (raw & 0xF);
    public byte Submapper => (byte) (raw >> 4 & 0xF);
}

[StructLayout(LayoutKind.Sequential)]
public struct Timing
{
    private byte raw;

    public TimingMode TimingMode => (TimingMode) (raw & 0x3);
}

public struct ConsoleTypeConfig
{
    public ConsoleType Type;
    public byte PPUType;
    public byte HardwareType;
    public byte ExtendedType;
}

public enum Mirroring
{
    Horizontal = 0,
    Vertical = 1
}

public enum ConsoleType
{
    NES = 0, // Nintendo Entertainment System / Family Computer
    NVS = 1, // Nintendo Vs. System
    NPC = 2, // Nintendo Playchoice 10
    ECT = 3 // Extended Console Type
}

public enum TimingMode
{
    RP2C02 = 0, // NTSC NES
    RP2C07 = 1, // PAL NES
    MultiRegion = 2,
    UA6538 = 3 // Dendy
}