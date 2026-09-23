using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Hardware.Headers;


namespace Hardware;

public class Loader
{
    public static Cartridge LoadFromFile(string fileName)
    {
        using (var reader = new BinaryReader(File.OpenRead(fileName), Encoding.Default, false))
        {
            var headerBytes = reader.ReadBytes(16);
            var handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
            var nes2 = Marshal.PtrToStructure<Nes2>(handle.AddrOfPinnedObject());
            handle.Free();

            if (nes2.Flags6.Trainer)
                reader.ReadBytes(512);
            
            if (nes2.IsNes2)
            {
                return LoadNes2(reader, nes2);
            }
            
            handle = GCHandle.Alloc(headerBytes, GCHandleType.Pinned);
            var ines = Marshal.PtrToStructure<INes>(handle.AddrOfPinnedObject());
            handle.Free();
            
            return LoadINes(reader, ines);
        }
    }

    private static Cartridge LoadINes(BinaryReader reader, INes header)
    {
        var prgMem = reader.ReadBytes(header.PrgRomSize);
        var chrMem = header.ChrRomBanks == 0 ? new byte[8*1024] : reader.ReadBytes(header.ChrRomSize);
        var prgRam = new byte[header.PrgRamSize];
        var chrRamBanks = (ushort)(header.ChrRomBanks == 0 ? 1 : 0);
        
        var mapper = CreateMapper(header.MapperId, header.Flags6.Mirroring, header.PrgRomBanks, header.ChrRomBanks, header.PrgRamBanks, chrRamBanks);
            
        return new Cartridge(mapper, prgMem, chrMem, prgRam);
    }

    private static Cartridge LoadNes2(BinaryReader reader, Nes2 header)
    {
        var prgMem = reader.ReadBytes(header.PrgRomSize);
        var chrMem = header.ChrRomBanks == 0 ? new byte[header.ChrRamSize] : reader.ReadBytes(header.ChrRomSize);
        var prgRam = new byte[header.PrgRamSize];
        var chrRamBanks = (ushort)(header.ChrRomBanks == 0 ? header.ChrRamSize / 0x2000 : 0);

        var mapper = CreateMapper(header.MapperId, header.Flags6.Mirroring, header.PrgRomBanks, header.ChrRomBanks, header.PrgRamBanks, chrRamBanks);
            
        return new Cartridge(mapper, prgMem, chrMem, prgRam);
    }

    private static IMapper CreateMapper(ushort id, Mirroring mirroring, ushort prgBanks, ushort chrBanks, ushort prgRamBanks, ushort chrRamBanks)
    {
        var mappers = typeof(IMapper).Assembly.GetTypes()
            .Where(t => typeof(IMapper).IsAssignableFrom(t) && !t.IsInterface);

        var mapper = mappers.Single(t => t.GetCustomAttribute<MapperIdAttribute>().MapperId == id);

        return (IMapper) Activator.CreateInstance(mapper, mirroring, prgBanks, chrBanks, prgRamBanks, chrRamBanks);
    }
}