using System;
using System.Runtime.CompilerServices;
using Hardware.Headers;

namespace Hardware;

public class PpuBus : IBus
{
    public Cartridge Cartridge { get; private set; }
    private byte[] palette = new byte[0x20];
    private byte[] nameTables = new byte[0x800];

    public byte[] NameTables => nameTables;
    
    public void Insert(Cartridge cartridge)
    {
        Cartridge = cartridge;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public byte Read(ushort address)
    {
        address &= 0x3FFF;
        
        if (Cartridge.PpuRead(address, out byte value))
            return value;

        if (address is >= 0x2000 and < 0x3EFF)
        {
            address &= 0xFFF;
            switch (Cartridge.Mirroring)
            {
                case Mirroring.Vertical:
                    switch (address)
                    {
                        case <= 0x03FF:
                            return nameTables[address & 0x3FF];
                        case <= 0x07FF:
                            return nameTables[(address & 0x3FF) | 0x400];
                        case <= 0x0BFF:
                            return nameTables[address & 0x3FF];
                        case <= 0x0FFF:
                            return nameTables[(address & 0x3FF) | 0x400];
                    }

                    break;
                case Mirroring.Horizontal:
                    switch (address)
                    {
                        case <= 0x03FF:
                            return nameTables[address & 0x3FF];
                        case <= 0x07FF:
                            return nameTables[(address & 0x3FF)];
                        case <= 0x0BFF:
                            return nameTables[(address & 0x3FF) | 0x400];
                        case <= 0x0FFF:
                            return nameTables[(address & 0x3FF) | 0x400];
                    }

                    break;
            }
        }
        
        if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x1F;
            if (address is 0x10 or 0x14 or 0x18 or 0x1C)
                address &= 0xF;
            
            return palette[address];
        }        
        return 0;
    }

    public ushort Read16Bit(ushort address)
    {
        // Not Gonna Happen
        throw new NotImplementedException();
    }

    public void Write(ushort address, byte value)
    {
        address &= 0x3FFF;
        
        if (Cartridge.PpuWrite(address, value))
        {}
        else if (address is >= 0x2000 and < 0x3EFF)
        {
            address &= 0xFFF;
            switch (Cartridge.Mirroring)
            {
                case Mirroring.Vertical:
                    switch (address)
                    {
                        case <= 0x03FF:
                            nameTables[address & 0x3FF] = value;
                            break;
                        case <= 0x07FF:
                            nameTables[(address & 0x3FF) | 0x400] = value;
                            break;
                        case <= 0x0BFF:
                            nameTables[address & 0x3FF] = value;
                            break;
                        case <= 0x0FFF:
                            nameTables[(address & 0x3FF) | 0x400] = value;
                            break;
                    }

                    break;
                case Mirroring.Horizontal:
                    switch (address)
                    {
                        case <= 0x03FF:
                            nameTables[address & 0x3FF] = value;
                            break;
                        case <= 0x07FF:
                            nameTables[(address & 0x3FF) ] = value;
                            break;
                        case <= 0x0BFF:
                            nameTables[(address & 0x3FF) | 0x400] = value;
                            break;
                        case <= 0x0FFF:
                            nameTables[(address & 0x3FF) | 0x400] = value;
                            break;
                    }

                    break;
            }
        }            
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x1F;
            if (address is 0x10 or 0x14 or 0x18 or 0x1C)
                address &= 0b01111;
            
            palette[address] = value;
        }
    }
}