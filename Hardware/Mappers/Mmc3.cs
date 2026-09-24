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

    // PrgBanks counts in 16kb Size but MMC3 is 8kb
    public byte PrgBanks8k => (byte)(PrgBanks * 2);
    
    // TODO: If bit 6 of Flags 6 in mapper is set this should be four screen mirroring
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
        if (address < 0x8000) 
            return null;

        // Last 8kb are always fixed
        if (address >= 0xE000)
            return (address & 0x1FFF) | ((PrgBanks - 1) << 13);

        if (((bankSelectRegister >> 6) & 0x1) == 0)
        {
            switch (address)
            {
                case >=0x8000 and <0xA000:
                    return (address & 0x1FFF) | ((bankRegisters[6]) << 13);
                case >=0xA000 and <0xC000:
                    return (address & 0x1FFF) | ((bankRegisters[7]) << 13);
                case >=0xC000:
                    return (address & 0x1FFF) | ((PrgBanks8k - 2) << 13);
            }
        }
        else
        {
            switch (address)
            {
                case >=0x8000 and <0xA000:
                    return (address & 0x1FFF) | ((PrgBanks8k - 2) << 13);
                case >=0xA000 and <0xC000:
                    return (address & 0x1FFF) | ((bankRegisters[7]) << 13);
                case >=0xC000:
                    return (address & 0x1FFF) | ((bankRegisters[6]) << 13);
            }
        }

        throw new UnreachableException();
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

    public override bool PpuRead(ushort address, out int mappedAddress)
    {
        mappedAddress = 0;
        if (address >= 0x2000)
            return false;

        if (((bankSelectRegister >> 7) & 0x1) == 0)
        {
            switch (address)
            {
                case <0x0800:
                    mappedAddress = ((address & 0x7FF) | (bankRegisters[0] << 10));
                    break;
                case <0x1000:
                    mappedAddress = ((address & 0x7FF)| (bankRegisters[1] << 10));
                    break;
                case <0x1400:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[2] << 10));
                    break;
                case <0x1800:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[3] << 10));
                    break;
                case <0x1C00:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[4] << 10));
                    break;
                case <0x2000:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[5] << 10));
                    break;
            }
        }
        else
        {
            switch (address)
            {
                case <0x0400:
                    mappedAddress = ((address & 0x3FF) | (bankRegisters[2] << 10));
                    break;
                case <0x0800:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[3] << 10));
                    break;
                case <0x0C00:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[4] << 10));
                    break;
                case <0x1000:
                    mappedAddress = ((address & 0x3FF)| (bankRegisters[5] << 10));
                    break;
                case <0x1800:
                    mappedAddress = ((address & 0x7FF)| (bankRegisters[0] << 10));
                    break;
                case <0x2000:
                    mappedAddress = ((address & 0x7FF)| (bankRegisters[1] << 10));
                    break;
            }
        }
        
        return true;
    }

    public override bool PpuWrite(ushort address, out ushort mappedAddress)
    {
        if (address <= 0x1FFF && ChrRamBanks > 0)
        {
            mappedAddress = address;
            return true;
        }

        mappedAddress = 0;
        return false;
    }
}