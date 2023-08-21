using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Hardware;

public partial class Cpu
{
    public IBus Bus { get; }
    public byte A { get; private set; }
    public byte X { get; private set; }
    public byte Y { get; private set; }
    public ushort PC { get; private set; }
    public byte SP { get; private set; }
    public CpuFlags Status { get; private set; }

    private bool irq;
    private bool nmi;

    public void TriggerInterrupt()
    {
        irq = true;
    }

    public void TriggerNonMaskableInterrupt()
    {
        nmi = true;
    }

    public bool Carry
    {
        get => Status.HasFlag(CpuFlags.Carry);
        set => Status = Status.With(CpuFlags.Carry, value);
    }

    public bool Zero
    {
        get => Status.HasFlag(CpuFlags.Zero);
        set => Status = Status.With(CpuFlags.Zero, value);
    }

    public bool InterruptDisable
    {
        get => Status.HasFlag(CpuFlags.InterruptDisable);
        set => Status = Status.With(CpuFlags.InterruptDisable, value);
    }

    public bool DecimalMode
    {
        get => Status.HasFlag(CpuFlags.DecimalMode);
        set => Status = Status.With(CpuFlags.DecimalMode, value);
    }

    public bool BreakCommand
    {
        get => Status.HasFlag(CpuFlags.BreakCommand);
        set => Status = Status.With(CpuFlags.BreakCommand, value);
    }

    public bool Unused
    {
        get => Status.HasFlag(CpuFlags.Unused);
        set => Status = Status.With(CpuFlags.Unused, value);
    }

    public bool Overflow
    {
        get => Status.HasFlag(CpuFlags.Overflow);
        set => Status = Status.With(CpuFlags.Overflow, value);
    }

    public bool Negative
    {
        get => Status.HasFlag(CpuFlags.Negative);
        set => Status = Status.With(CpuFlags.Negative, value);
    }

    public byte Cycles { get; private set; }

    private Dictionary<byte, (string Name, AddressMode Mode, Func<byte> Func)> opcodeActions;
    private int cycleCount;

    public Cpu(IBus bus)
    {
        Bus = bus;
        LoadInstructions();
    }

    private void LoadInstructions()
    {
        opcodeActions = new()
        {
            #region Load 18

            {0xA9, ("LDA", AddressMode.IMM, LDAImm)},
            {0xA5, ("LDA", AddressMode.ZPG, LDAZpg)},
            {0xB5, ("LDA", AddressMode.ZPX, LDAZpgX)},
            {0xAD, ("LDA", AddressMode.ABS, LDAAbs)},
            {0xBD, ("LDA", AddressMode.ABX, LDAAbsX)},
            {0xB9, ("LDA", AddressMode.ABY, LDAAbsY)},
            {0xA1, ("LDA", AddressMode.IND, LDAIndX)},
            {0xB1, ("LDA", AddressMode.IND, LDAIndY)},
            {0xA2, ("LDX", AddressMode.IMM, LDXImm)},
            {0xA6, ("LDX", AddressMode.ZPG, LDXZpg)},
            {0xB6, ("LDX", AddressMode.ZPY, LDXZpgY)},
            {0xAE, ("LDX", AddressMode.ABS, LDXAbs)},
            {0xBE, ("LDX", AddressMode.ABY, LDXAbsY)},
            {0xA0, ("LDY", AddressMode.IMM, LDYImm)},
            {0xA4, ("LDY", AddressMode.ZPG, LDYZpg)},
            {0xB4, ("LDY", AddressMode.ZPX, LDYZpgX)},
            {0xAC, ("LDY", AddressMode.ABS, LDYAbs)},
            {0xBC, ("LDY", AddressMode.ABX, LDYAbsX)},

            #endregion

            #region Store 13

            {0x85, ("STA", AddressMode.ZPG, STAZpg)},
            {0x95, ("STA", AddressMode.ZPX, STAZpgX)},
            {0x8D, ("STA", AddressMode.ABS, STAAbs)},
            {0x9D, ("STA", AddressMode.ABX, STAAbsX)},
            {0x99, ("STA", AddressMode.ABY, STAAbsY)},
            {0x81, ("STA", AddressMode.IND, STAIndX)},
            {0x91, ("STA", AddressMode.IND, STAIndY)},
            {0x86, ("STX", AddressMode.ZPG, STXZpg)},
            {0x96, ("STX", AddressMode.ZPY, STXZpgY)},
            {0x8E, ("STX", AddressMode.ABS, STXAbs)},
            {0x84, ("STY", AddressMode.ZPG, STYZpg)},
            {0x94, ("STY", AddressMode.ZPX, STYZpgX)},
            {0x8C, ("STY", AddressMode.ABS, STYAbs)},

            #endregion

            #region Arithmetic 48

            {0x69, ("ADC", AddressMode.IMM, ADCImm)},
            {0x65, ("ADC", AddressMode.ZPG, ADCZpg)},
            {0x75, ("ADC", AddressMode.ZPX, ADCZpgX)},
            {0x6D, ("ADC", AddressMode.ABS, ADCAbs)},
            {0x7D, ("ADC", AddressMode.ABX, ADCAbsX)},
            {0x79, ("ADC", AddressMode.ABY, ADCAbsY)},
            {0x61, ("ADC", AddressMode.IND, ADCIndX)},
            {0x71, ("ADC", AddressMode.IND, ADCIndY)},

            {0xE9, ("SBC", AddressMode.IMM, SBCImm)},
            {0xEB, ("SBC*", AddressMode.IMM, SBCImm)},
            {0xE5, ("SBC", AddressMode.ZPG, SBCZpg)},
            {0xF5, ("SBC", AddressMode.ZPX, SBCZpgX)},
            {0xED, ("SBC", AddressMode.ABS, SBCAbs)},
            {0xFD, ("SBC", AddressMode.ABX, SBCAbsX)},
            {0xF9, ("SBC", AddressMode.ABY, SBCAbsY)},
            {0xE1, ("SBC", AddressMode.IND, SBCIndX)},
            {0xF1, ("SBC", AddressMode.IND, SBCIndY)},

            {0xE6, ("INC", AddressMode.ZPG, INCZpg)},
            {0xF6, ("INC", AddressMode.ZPX, INCZpgX)},
            {0xEE, ("INC", AddressMode.ABS, INCAbs)},
            {0xFE, ("INC", AddressMode.ABX, INCAbsX)},
            {0xE8, ("INX", AddressMode.IMP, INX)},
            {0xC8, ("INY", AddressMode.IMP, INY)},

            {0xC6, ("DEC", AddressMode.ZPG, DECZpg)},
            {0xD6, ("DEC", AddressMode.ZPX, DECZpgX)},
            {0xCE, ("DEC", AddressMode.ABS, DECAbs)},
            {0xDE, ("DEC", AddressMode.ABX, DECAbsX)},
            {0xCA, ("DEX", AddressMode.IMP, DEX)},
            {0x88, ("DEY", AddressMode.IMP, DEY)},

            {0x0A, ("ASL", AddressMode.ACC, ASLAcc)},
            {0x06, ("ASL", AddressMode.ZPG, ASLZpg)},
            {0x16, ("ASL", AddressMode.ZPX, ASLZpgX)},
            {0x0E, ("ASL", AddressMode.ABS, ASLAbs)},
            {0x1E, ("ASL", AddressMode.ABX, ASLAbsX)},

            {0x4A, ("LSR", AddressMode.ACC, LSRAcc)},
            {0x46, ("LSR", AddressMode.ZPG, LSRZpg)},
            {0x56, ("LSR", AddressMode.ZPX, LSRZpgX)},
            {0x4E, ("LSR", AddressMode.ABS, LSRAbs)},
            {0x5E, ("LSR", AddressMode.ABX, LSRAbsX)},

            {0x2A, ("ROL", AddressMode.ACC, ROLAcc)},
            {0x26, ("ROL", AddressMode.ZPG, ROLZpg)},
            {0x36, ("ROL", AddressMode.ZPX, ROLZpgX)},
            {0x2E, ("ROL", AddressMode.ABS, ROLAbs)},
            {0x3E, ("ROL", AddressMode.ABX, ROLAbsX)},

            {0x6A, ("ROR", AddressMode.ACC, RORAcc)},
            {0x66, ("ROR", AddressMode.ZPG, RORZpg)},
            {0x76, ("ROR", AddressMode.ZPX, RORZpgX)},
            {0x6E, ("ROR", AddressMode.ABS, RORAbs)},
            {0x7E, ("ROR", AddressMode.ABX, RORAbsX)},

            // Illegal
            {0x4B, ("ALR", AddressMode.IMM, ASR)},
            {0x6B, ("ARR", AddressMode.IMM, ARR)},

            #endregion

            #region Comparison 12

            {0xC9, ("CMP", AddressMode.IMM, CMPImm)},
            {0xC5, ("CMP", AddressMode.ZPG, CMPZpg)},
            {0xD5, ("CMP", AddressMode.ZPX, CMPZpgX)},
            {0xCD, ("CMP", AddressMode.ABS, CMPAbs)},
            {0xDD, ("CMP", AddressMode.ABX, CMPAbsX)},
            {0xD9, ("CMP", AddressMode.ABY, CMPAbsY)},
            {0xC1, ("CMP", AddressMode.IND, CMPIndX)},
            {0xD1, ("CMP", AddressMode.IND, CMPIndY)},

            {0xE0, ("CPX", AddressMode.IMM, CPXImm)},
            {0xE4, ("CPX", AddressMode.ZPG, CPXZpg)},
            {0xEC, ("CPX", AddressMode.ABS, CPXAbs)},

            {0xC0, ("CPY", AddressMode.IMM, CPYImm)},
            {0xC4, ("CPY", AddressMode.ZPG, CPYZpg)},
            {0xCC, ("CPY", AddressMode.ABS, CPYAbs)},

            #endregion

            #region Logic 28

            {0x29, ("AND", AddressMode.IMM, ANDImm)},
            {0x25, ("AND", AddressMode.ZPG, ANDZpg)},
            {0x35, ("AND", AddressMode.ZPX, ANDZpgX)},
            {0x2D, ("AND", AddressMode.ABS, ANDAbs)},
            {0x3D, ("AND", AddressMode.ABX, ANDAbsX)},
            {0x39, ("AND", AddressMode.ABY, ANDAbsY)},
            {0x21, ("AND", AddressMode.IND, ANDIndX)},
            {0x31, ("AND", AddressMode.IND, ANDIndY)},

            {0x09, ("ORA", AddressMode.IMM, ORAImm)},
            {0x05, ("ORA", AddressMode.ZPG, ORAZpg)},
            {0x15, ("ORA", AddressMode.ZPX, ORAZpgX)},
            {0x0D, ("ORA", AddressMode.ABS, ORAAbs)},
            {0x1D, ("ORA", AddressMode.ABX, ORAAbsX)},
            {0x19, ("ORA", AddressMode.ABY, ORAAbsY)},
            {0x01, ("ORA", AddressMode.IND, ORAIndX)},
            {0x11, ("ORA", AddressMode.IND, ORAIndY)},

            {0x49, ("EOR", AddressMode.IMM, EORImm)},
            {0x45, ("EOR", AddressMode.ZPG, EORZpg)},
            {0x55, ("EOR", AddressMode.ZPX, EORZpgX)},
            {0x4D, ("EOR", AddressMode.ABS, EORAbs)},
            {0x5D, ("EOR", AddressMode.ABX, EORAbsX)},
            {0x59, ("EOR", AddressMode.ABY, EORAbsY)},
            {0x41, ("EOR", AddressMode.IND, EORIndX)},
            {0x51, ("EOR", AddressMode.IND, EORIndY)},

            {0x24, ("BIT", AddressMode.ZPG, BITZpg)},
            {0x2C, ("BIT", AddressMode.ABS, BITAbs)},

            // Illegal
            {0x0B, ("AAC", AddressMode.IMM, AAC)},
            {0x2B, ("AAC", AddressMode.IMM, AAC)},
            
            #endregion

            #region Jump/Branching 11

            {0x4C, ("JMP", AddressMode.ABS, JMPAbs)},
            {0x6C, ("JMP", AddressMode.IND, JMPInd)},
            {0x20, ("JSR", AddressMode.ABS, JSR)},
            {0x60, ("RTS", AddressMode.IMP, RTS)},
            {0x90, ("BCC", AddressMode.REL, BCC)},
            {0xB0, ("BCS", AddressMode.REL, BCS)},
            {0xF0, ("BEQ", AddressMode.REL, BEQ)},
            {0xD0, ("BNE", AddressMode.REL, BNE)},
            {0x10, ("BPL", AddressMode.REL, BPL)},
            {0x30, ("BMI", AddressMode.REL, BMI)},
            {0x50, ("BVC", AddressMode.REL, BVC)},
            {0x70, ("BVS", AddressMode.REL, BVS)},

            #endregion

            #region Transfer 6

            {0xAA, ("TAX", AddressMode.IMP, TAX)},
            {0xA8, ("TAY", AddressMode.IMP, TAY)},
            {0xBA, ("TSX", AddressMode.IMP, TSX)},
            {0x8A, ("TXA", AddressMode.IMP, TXA)},
            {0x9A, ("TXS", AddressMode.IMP, TXS)},
            {0x98, ("TYA", AddressMode.IMP, TYA)},

            // Ilegal
            {0xAB, ("ATX", AddressMode.IMP, ATX)},
            {0xCB, ("AXS", AddressMode.IMP, AXS)},

            
            #endregion

            #region Stack Operations 4

            {0x48, ("PHA", AddressMode.IMP, PHA)},
            {0x08, ("PHP", AddressMode.IMP, PHP)},
            {0x68, ("PLA", AddressMode.IMP, PLA)},
            {0x28, ("PLP", AddressMode.IMP, PLP)},

            #endregion

            #region Set/Clear Flags 7

            {0x18, ("CLC", AddressMode.IMP, CLC)},
            {0xD8, ("CLD", AddressMode.IMP, CLD)},
            {0x58, ("CLI", AddressMode.IMP, CLI)},
            {0xB8, ("CLV", AddressMode.IMP, CLV)},
            {0x38, ("SEC", AddressMode.IMP, SEC)},
            {0xF8, ("SED", AddressMode.IMP, SED)},
            {0x78, ("SEI", AddressMode.IMP, SEI)},

            #endregion

            #region Interrupt Handling 2

            {0x00, ("BRK", AddressMode.IMP, BRK)},
            {0x40, ("RTI", AddressMode.IMP, RTI)},

            #endregion

            #region No Operation 1

            {0xEA, ("NOP", AddressMode.IMP, () => 2)},
            
            // Illegal
            {0x1A, ("NOP*", AddressMode.IMP, () => 2)},
            {0x3A, ("NOP*", AddressMode.IMP, () => 2)},
            {0x5A, ("NOP*", AddressMode.IMP, () => 2)},
            {0x7A, ("NOP*", AddressMode.IMP, () => 2)},
            {0xDA, ("NOP*", AddressMode.IMP, () => 2)},
            {0xFA, ("NOP*", AddressMode.IMP, () => 2)},
            {0x80, ("DOP*", AddressMode.IMM, DOP)},
            {0x82, ("DOP*", AddressMode.IMM, DOP)},
            {0x89, ("DOP*", AddressMode.IMM, DOP)},
            {0xC2, ("DOP*", AddressMode.IMM, DOP)},
            {0xE2, ("DOP*", AddressMode.IMM, DOP)},

            #endregion
        };
    }

    private static bool IsOverflow(byte original, ushort value, ushort sum)
    {
        return ((original ^ value) & 0x80) == 0
               && ((original ^ sum) & 0x80) > 0;
    }

    public void Reset()
    {
        PC = Bus.Read16Bit(0xFFFC);
        Logger.Enable();
        PC = 0xC000;
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFD;
        Status = CpuFlags.Unused | CpuFlags.InterruptDisable;
        Cycles = 8;
        cycleCount = 0;
        Logger.Start("..\\..\\..\\test.log");
    }

    public void Cycle()
    {
        cycleCount++;
        if (Cycles != 0)
        {
            Cycles--;
            return;
        }

        HandleInterrupt();

        //Unused = true;
        Logger.StartLine(cycleCount);
        Logger.State(this);
        var opcode = ReadNextProgramByteNoLog();
        Cycles = Execute(opcode);
        Logger.EndLine();
    }

    private void HandleInterrupt()
    {
        if (nmi)
        {
            HandleNmi();
        }
        else if (!InterruptDisable && irq)
        {
            HandleIrq();
        }
    }

    private void HandleNmi()
    {
        PushToStack(PC);

        BreakCommand = false;
        Unused = true;
        PushToStack((byte)Status);
        InterruptDisable = true;

        PC = Bus.Read16Bit(0xFFFA);
        Cycles = 8;
        nmi = false;
    }

    private void HandleIrq()
    {
        PushToStack(PC);
 
        BreakCommand = false;
        Unused = true;
        PushToStack((byte)Status);
        InterruptDisable = true;

        PC = Bus.Read16Bit(0xFFFE);
        Cycles = 7;
        irq = false;
    }


    private byte Execute(byte opcode)
    {
        if (!opcodeActions.TryGetValue(opcode, out var entry))
        {
            throw new Exception("Unhandled opcode: 0x" + opcode.ToString("X2"));
        }
        
        Logger.Op(entry.Name, entry.Mode);
        
        return entry.Func.Invoke();
    }

    private byte Read(ushort address) => Bus.Read(address);

    private ushort Read16Bit(ushort address)
    {
        byte lowByte = Bus.Read(address);
        byte highByte = Bus.Read((ushort) ((address & 0xFF00) | ((address+1) & 0xFF)));

        return ByteUtil.To16Bit(highByte, lowByte);
    }    
    
    private ushort Read16BitZpg(ushort address)
    {
        byte lowByte = Bus.Read((ushort)(address&0xFF));
        byte highByte = Bus.Read((ushort) ((address + 1)&0xff));

        return ByteUtil.To16Bit(highByte, lowByte);
    }

    private void Write(ushort address, byte value) => Bus.Write(address, value);

    private byte ReadNextProgramByte()
    {
        byte data = Bus.Read(PC++);
        Logger.Data(data);
        return data;
    }
    
    private byte ReadNextProgramByteNoLog()
    {
        byte data = Bus.Read(PC++);
        return data;
    }

    private ushort ReadNext16BitProgram()
    {
        byte lowByte = ReadNextProgramByte();
        byte highByte = ReadNextProgramByte();
        ushort data = ByteUtil.To16Bit(highByte, lowByte);
        Logger.Data(data);

        return data;
    }

    private void PushToStack(byte value)
    {
        ushort address = ByteUtil.To16Bit(0x01, SP);
        Bus.Write(address, value);
        SP--;
    }

    private void PushToStack(ushort value)
    {
        PushToStack((byte) (value >> 8));
        PushToStack((byte) (value & 0xFF));
    }

    private byte PopFromStack()
    {
        SP++;
        ushort address = ByteUtil.To16Bit(0x01, SP);
        return Bus.Read(address);
    }

    private ushort PopFromStack16Bit()
    {
        byte lowByte = PopFromStack();
        byte highByte = PopFromStack();
        return ByteUtil.To16Bit(highByte, lowByte);
    }

    public Dictionary<ushort, string> Disassemble(ushort start = 0x0, ushort end = 0xFFFF)
    {
        var dict = new Dictionary<ushort, string>();

        for (ushort address = start; address <= end;)
        {
            var lineAddress = address;
            byte opcode = Bus.Read(address++);

            var op = opcodeActions[opcode];
            string instruction = op.Name;

            string firstHalf = $"${lineAddress:x4}: {instruction} ";
            string secondHalf;
            byte value;
            byte high;
            byte low;
            switch (op.Mode)
            {
                case AddressMode.IMM:
                    value = Bus.Read(address++);
                    secondHalf = $"${value:x2} {{IMM}}";
                    break;
                case AddressMode.ZPG:
                    value = Bus.Read(address++);
                    secondHalf = $"${value:x2} {{ZPG}}";
                    break;
                case AddressMode.ZPX:
                    value = Bus.Read(address++);
                    secondHalf = $"${value:x2}, X {{ZPX}}";
                    break;
                case AddressMode.ZPY:
                    value = Bus.Read(address++);
                    secondHalf = $"${value:x2}, Y {{ZPY}}";
                    break;
                case AddressMode.IMP:
                    secondHalf = $"{{IMP}}";
                    break;
                case AddressMode.REL:
                    value = Bus.Read(address++);
                    secondHalf = $"${value:x2}, [${address + (sbyte) value:x4}] {{REL}}";
                    break;
                case AddressMode.ABS:
                    low = Bus.Read(address++);
                    high = Bus.Read(address++);
                    secondHalf = $"${high << 8 | low:x4} {{ABS}}";
                    break;
                case AddressMode.ABX:
                    low = Bus.Read(address++);
                    high = Bus.Read(address++);
                    secondHalf = $"${high << 8 | low:x4}, X {{ABX}}";
                    break;
                case AddressMode.ABY:
                    low = Bus.Read(address++);
                    high = Bus.Read(address++);
                    secondHalf = $"${high << 8 | low:x4}, Y {{ABY}}";
                    break;
                case AddressMode.IND:
                    low = Bus.Read(address++);
                    high = Bus.Read(address++);
                    secondHalf = $"(${high << 8 | low:x4}) {{IND}}";
                    break;
                case AddressMode.INX:
                    value = Bus.Read(address++);
                    secondHalf = $"(${value:x2}, X) {{INX}}";
                    break;
                case AddressMode.INY:
                    value = Bus.Read(address++);
                    secondHalf = $"(${value:x2}), Y {{INY}}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            dict[lineAddress] = firstHalf + secondHalf;
        }

        return dict;
    }
}