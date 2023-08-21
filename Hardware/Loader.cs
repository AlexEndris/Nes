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
            var header = Marshal.PtrToStructure<Nes2>(handle.AddrOfPinnedObject());
            handle.Free();

            var prgMem = reader.ReadBytes(header.PrgRomSize * 16384);
            var chrMem = reader.ReadBytes(header.ChrRomSize * 8192);

            var mapper = CreateMapper(header.MapperId, header.PrgRomSize, header.ChrRomSize);
            
            return new Cartridge(header.Flags6.Mirroring, mapper, header.PrgRomSize, prgMem, header.ChrRomSize, chrMem);
        }
    }

    private static IMapper CreateMapper(ushort id, ushort prgBanks, ushort chrBanks)
    {
        var mappers = typeof(IMapper).Assembly.GetTypes()
            .Where(t => typeof(IMapper).IsAssignableFrom(t) && !t.IsInterface);

        var mapper = mappers.Single(t => t.GetCustomAttribute<MapperIdAttribute>().MapperId == id);

        return (IMapper) Activator.CreateInstance(mapper, prgBanks, chrBanks);
    }
}