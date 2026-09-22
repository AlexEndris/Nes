// namespace Hardware.Mappers;
//
// [MapperId(1)]
// public class Mmc1 : IMapper
// {
//     public ushort PrgBanks { get; }
//
//     public ushort ChrBanks { get; }
//
//     private byte ControlRegister { get; set; }
//     private byte ChrBank0Register { get; set; }
//     private byte ChrBank1Register { get; set; }
//     private byte PrgBankRegister { get; set; }
//     private byte ShiftRegister { get; set; }
//
//     public Mmc1(ushort prgBanks, ushort chrBanks)
//     {
//         PrgBanks = prgBanks;
//         ChrBanks = chrBanks;
//         ControlRegister = 0xC;
//         ResetShiftRegister();
//     }
//
//     public bool IsCpuRead(ushort address)
//     {
//         return address >= 0x6000;
//     }
//
//     public bool IsCpuWrite(ushort address)
//     {
//         return address >= 0x8000;
//     }
//
//     public int? CpuRead(ushort address)
//     {
//         
//     }
//
//     public int? CpuWrite(ushort address, byte data)
//     {
//         if ((data & 0x80) > 0)
//         {
//             ResetShiftRegister();
//             ControlRegister |= 0xC; 
//             return null;
//         }
//
//         if ((ShiftRegister & 0x1) > 0)
//         {
//             AddToShiftRegister(data);
//
//             switch (address)
//             {
//                 case >=0xE000:
//                     PrgBankRegister = ShiftRegister;
//                     break;
//                 case >=0xC000:
//                     ChrBank1Register = ShiftRegister;
//                     break;
//                 case >=0xA000:
//                     ChrBank0Register = ShiftRegister;
//                     break;
//                 case >=0x8000:
//                     ControlRegister = ShiftRegister;
//                     break;
//             }
//             ResetShiftRegister();
//             return null;
//         }
//
//         AddToShiftRegister(data);
//         
//         return null;
//     }
//
//     private void AddToShiftRegister(byte data)
//     {
//         ShiftRegister >>= 1;
//         ShiftRegister |= (byte)((data & 0x1) << 4);
//     }
//
//     private void ResetShiftRegister()
//     {
//         ShiftRegister = 0x10;
//     }
//
//     public bool PpuRead(ushort address, out ushort mappedAddress)
//     {
//         
//     }
//
//     public bool PpuWrite(ushort address, out ushort mappedAddress)
//     {
//         
//     }
// }
