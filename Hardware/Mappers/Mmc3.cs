namespace Hardware.Mappers;

using System.Diagnostics;

using Headers;

[MapperId(4)]
public class Mmc3 : AbstractMapper
{
    public Mmc3(Mirroring? mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
        : base(mirroring, prgBanks, chrBanks, prgRamBanks, chrRamBanks)
    {
    }

    // TODO: Needs to either be from the register or if not null from the header
    public override Mirroring Mirroring { get; }

    private byte bankSelectRegister;
    private byte[] bankRegisters = new byte[8];
    private byte mirroringRegister;
    private byte ramProtectRegister;
    private byte irqLatchRegister;
    private byte irqReloadRegister;
    private byte irqDisableRegister;
    private byte irqEnableRegister;

    public override bool IsCpuRead(ushort address)
    {
        return address >= 0x6000;
    }

    public override bool IsCpuWrite(ushort address)
    {
        return address >= 0x6000;
    }

    public override int? CpuRead(ushort address)
    {
        throw new System.NotImplementedException();
    }

    public override int? CpuWrite(ushort address, byte data)
    {
        if (address < 0x8000)
            return null;

        // even writes
        if ((address & 0x1) == 0)
        {
            switch (address)
            {
                case <= 0x9FFF:
                    bankSelectRegister = data;
                    break;
                case <= 0xBFFF:
                    mirroringRegister = data;
                    break;
                case <= 0xDFFF:
                    irqLatchRegister = data;
                    break;
                case <= 0xFFFF:
                    irqDisableRegister = data;
                    break;
            }

            return null;
        }

        // odd writes
        if ((address & 0x1) > 0)
        {
            switch (address)
            {
                case <= 0x9FFF:
                    SetBankData((byte)(bankSelectRegister & 0x7), data);
                    break;
                case <= 0xBFFF:
                    ramProtectRegister = data;
                    break;
                case <= 0xDFFF:
                    irqDisableRegister = data;
                    break;
                case <= 0xFFFF:
                    irqEnableRegister = data;
                    break;
            }
        }

        return null;
    }

    private void SetBankData(byte register, byte data)
    {
        bankRegisters[register] = register switch
        {
            0 or 1 => (byte)(data & 0xFE),
            2 or 3 or 4 or 5 => data,
            6 or 7 => (byte)(data & 0x3F),
            _ => throw new UnreachableException()
        };
    }

    public override bool PpuRead(ushort address, out ushort mappedAddress)
    {
        throw new System.NotImplementedException();
    }

    public override bool PpuWrite(ushort address, out ushort mappedAddress)
    {
        throw new System.NotImplementedException();
    }
}