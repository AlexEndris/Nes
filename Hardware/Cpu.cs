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

    private Dictionary<byte, (string Name, AddressMode Mode, byte Cycles, Func<Func<ushort>, ushort, byte> Func)> opcodeActions;
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

            {0xA9, ("LDA", AddressMode.IMM, 2, LDA)},
            {0xA5, ("LDA", AddressMode.ZPG, 3, LDA)},
            {0xB5, ("LDA", AddressMode.ZPX, 4, LDA)},
            {0xAD, ("LDA", AddressMode.ABS, 4, LDA)},
            {0xBD, ("LDA", AddressMode.ABX, 4, LDA)},
            {0xB9, ("LDA", AddressMode.ABY, 4, LDA)},
            {0xA1, ("LDA", AddressMode.INX, 6, LDA)},
            {0xB1, ("LDA", AddressMode.INY, 4, LDA)},
            {0xA2, ("LDX", AddressMode.IMM, 2, LDX)},
            {0xA6, ("LDX", AddressMode.ZPG, 3, LDX)},
            {0xB6, ("LDX", AddressMode.ZPY, 4, LDX)},
            {0xAE, ("LDX", AddressMode.ABS, 4, LDX)},
            {0xBE, ("LDX", AddressMode.ABY, 4, LDX)},
            {0xA0, ("LDY", AddressMode.IMM, 2, LDY)},
            {0xA4, ("LDY", AddressMode.ZPG, 3, LDY)},
            {0xB4, ("LDY", AddressMode.ZPX, 4, LDY)},
            {0xAC, ("LDY", AddressMode.ABS, 4, LDY)},
            {0xBC, ("LDY", AddressMode.ABX, 4, LDY)},

            #endregion

            #region Store 13

            {0x85, ("STA", AddressMode.ZPG, 3, STA)},
            {0x95, ("STA", AddressMode.ZPX, 4, STA)},
            {0x8D, ("STA", AddressMode.ABS, 4, STA)},
            {0x9D, ("STA", AddressMode.ABX, 5, STA)},
            {0x99, ("STA", AddressMode.ABY, 5, STA)},
            {0x81, ("STA", AddressMode.INX, 6, STA)},
            {0x91, ("STA", AddressMode.INY, 6, STA)},
            {0x86, ("STX", AddressMode.ZPG, 3, STX)},
            {0x96, ("STX", AddressMode.ZPY, 4, STX)},
            {0x8E, ("STX", AddressMode.ABS, 4, STX)},
            {0x84, ("STY", AddressMode.ZPG, 3, STY)},
            {0x94, ("STY", AddressMode.ZPX, 4, STY)},
            {0x8C, ("STY", AddressMode.ABS, 4, STY)},

            #endregion

            #region Arithmetic 48

            {0x69, ("ADC", AddressMode.IMM, 2, ADC)},
            {0x65, ("ADC", AddressMode.ZPG, 3, ADC)},
            {0x75, ("ADC", AddressMode.ZPX, 4, ADC)},
            {0x6D, ("ADC", AddressMode.ABS, 4, ADC)},
            {0x7D, ("ADC", AddressMode.ABX, 4, ADC)},
            {0x79, ("ADC", AddressMode.ABY, 4, ADC)},
            {0x61, ("ADC", AddressMode.INX, 6, ADC)},
            {0x71, ("ADC", AddressMode.INY, 5, ADC)},

            {0xE9, ("SBC", AddressMode.IMM, 2, SBC)},
            {0xEB, ("SBC*", AddressMode.IMM, 2, SBC)},
            {0xE5, ("SBC", AddressMode.ZPG, 3, SBC)},
            {0xF5, ("SBC", AddressMode.ZPX, 4, SBC)},
            {0xED, ("SBC", AddressMode.ABS, 4, SBC)},
            {0xFD, ("SBC", AddressMode.ABX, 4, SBC)},
            {0xF9, ("SBC", AddressMode.ABY, 4, SBC)},
            {0xE1, ("SBC", AddressMode.INX, 6, SBC)},
            {0xF1, ("SBC", AddressMode.INY, 5, SBC)},

            {0xE6, ("INC", AddressMode.ZPG, 5, INC)},
            {0xF6, ("INC", AddressMode.ZPX, 6, INC)},
            {0xEE, ("INC", AddressMode.ABS, 6, INC)},
            {0xFE, ("INC", AddressMode.ABX, 7, INC)},
            {0xE8, ("INX", AddressMode.IMP, 2, INX)},
            {0xC8, ("INY", AddressMode.IMP, 2, INY)},

            {0xC6, ("DEC", AddressMode.ZPG, 5, DEC)},
            {0xD6, ("DEC", AddressMode.ZPX, 6, DEC)},
            {0xCE, ("DEC", AddressMode.ABS, 6, DEC)},
            {0xDE, ("DEC", AddressMode.ABX, 7, DEC)},
            {0xCA, ("DEX", AddressMode.IMP, 2, DEX)},
            {0x88, ("DEY", AddressMode.IMP, 2, DEY)},

            {0x0A, ("ASL", AddressMode.ACC, 2, ASLA)},
            {0x06, ("ASL", AddressMode.ZPG, 5, ASL)},
            {0x16, ("ASL", AddressMode.ZPX, 6, ASL)},
            {0x0E, ("ASL", AddressMode.ABS, 6, ASL)},
            {0x1E, ("ASL", AddressMode.ABX, 7, ASL)},

            {0x4A, ("LSR", AddressMode.ACC, 2, LSRA)},
            {0x46, ("LSR", AddressMode.ZPG, 5, LSR)},
            {0x56, ("LSR", AddressMode.ZPX, 6, LSR)},
            {0x4E, ("LSR", AddressMode.ABS, 6, LSR)},
            {0x5E, ("LSR", AddressMode.ABX, 7, LSR)},

            {0x2A, ("ROL", AddressMode.ACC, 2, ROLA)},
            {0x26, ("ROL", AddressMode.ZPG, 5, ROL)},
            {0x36, ("ROL", AddressMode.ZPX, 6, ROL)},
            {0x2E, ("ROL", AddressMode.ABS, 6, ROL)},
            {0x3E, ("ROL", AddressMode.ABX, 7, ROL)},

            {0x6A, ("ROR", AddressMode.ACC, 2, RORA)},
            {0x66, ("ROR", AddressMode.ZPG, 5, ROR)},
            {0x76, ("ROR", AddressMode.ZPX, 6, ROR)},
            {0x6E, ("ROR", AddressMode.ABS, 6, ROR)},
            {0x7E, ("ROR", AddressMode.ABX, 7, ROR)},

            // Illegal
            {0x4B, ("ALR", AddressMode.IMM, 2, ASR)},
            {0x6B, ("ARR", AddressMode.IMM, 2, ARR)},

            #endregion

            #region Comparison 12

            {0xC9, ("CMP", AddressMode.IMM, 2, CMP)},
            {0xC5, ("CMP", AddressMode.ZPG, 3, CMP)},
            {0xD5, ("CMP", AddressMode.ZPX, 4, CMP)},
            {0xCD, ("CMP", AddressMode.ABS, 4, CMP)},
            {0xDD, ("CMP", AddressMode.ABX, 4, CMP)},
            {0xD9, ("CMP", AddressMode.ABY, 4, CMP)},
            {0xC1, ("CMP", AddressMode.INX, 6, CMP)},
            {0xD1, ("CMP", AddressMode.INY, 5, CMP)},

            {0xE0, ("CPX", AddressMode.IMM, 2, CPX)},
            {0xE4, ("CPX", AddressMode.ZPG, 3, CPX)},
            {0xEC, ("CPX", AddressMode.ABS, 4, CPX)},

            {0xC0, ("CPY", AddressMode.IMM, 2, CPY)},
            {0xC4, ("CPY", AddressMode.ZPG, 3, CPY)},
            {0xCC, ("CPY", AddressMode.ABS, 4, CPY)},

            #endregion

            #region Logic 28

            {0x29, ("AND", AddressMode.IMM, 2, AND)},
            {0x25, ("AND", AddressMode.ZPG, 3, AND)},
            {0x35, ("AND", AddressMode.ZPX, 4, AND)},
            {0x2D, ("AND", AddressMode.ABS, 4, AND)},
            {0x3D, ("AND", AddressMode.ABX, 4, AND)},
            {0x39, ("AND", AddressMode.ABY, 4, AND)},
            {0x21, ("AND", AddressMode.INX, 6, AND)},
            {0x31, ("AND", AddressMode.INY, 5, AND)},

            {0x09, ("ORA", AddressMode.IMM, 2, ORA)},
            {0x05, ("ORA", AddressMode.ZPG, 3, ORA)},
            {0x15, ("ORA", AddressMode.ZPX, 4, ORA)},
            {0x0D, ("ORA", AddressMode.ABS, 4, ORA)},
            {0x1D, ("ORA", AddressMode.ABX, 4, ORA)},
            {0x19, ("ORA", AddressMode.ABY, 4, ORA)},
            {0x01, ("ORA", AddressMode.INX, 6, ORA)},
            {0x11, ("ORA", AddressMode.INY, 5, ORA)},

            {0x49, ("EOR", AddressMode.IMM, 2, EOR)},
            {0x45, ("EOR", AddressMode.ZPG, 3, EOR)},
            {0x55, ("EOR", AddressMode.ZPX, 4, EOR)},
            {0x4D, ("EOR", AddressMode.ABS, 4, EOR)},
            {0x5D, ("EOR", AddressMode.ABX, 4, EOR)},
            {0x59, ("EOR", AddressMode.ABY, 4, EOR)},
            {0x41, ("EOR", AddressMode.INX, 6, EOR)},
            {0x51, ("EOR", AddressMode.INY, 5, EOR)},

            {0x24, ("BIT", AddressMode.ZPG, 3, BIT)},
            {0x2C, ("BIT", AddressMode.ABS, 4, BIT)},

            // Illegal
            {0x0B, ("AAC", AddressMode.IMM, 2, AAC)},
            {0x2B, ("AAC", AddressMode.IMM, 2, AAC)},
            
            #endregion

            #region Jump/Branching 11

            {0x4C, ("JMP", AddressMode.ABS, 3, JMPAbs)},
            {0x6C, ("JMP", AddressMode.IND, 5, JMPInd)},
            {0x20, ("JSR", AddressMode.ABS, 6, JSR)},
            {0x60, ("RTS", AddressMode.IMP, 6, RTS)},
            {0x90, ("BCC", AddressMode.REL, 2, BCC)},
            {0xB0, ("BCS", AddressMode.REL, 2, BCS)},
            {0xF0, ("BEQ", AddressMode.REL, 2, BEQ)},
            {0xD0, ("BNE", AddressMode.REL, 2, BNE)},
            {0x10, ("BPL", AddressMode.REL, 2, BPL)},
            {0x30, ("BMI", AddressMode.REL, 2, BMI)},
            {0x50, ("BVC", AddressMode.REL, 2, BVC)},
            {0x70, ("BVS", AddressMode.REL, 2, BVS)},

            #endregion

            #region Transfer 6

            {0xAA, ("TAX", AddressMode.IMP, 2, TAX)},
            {0xA8, ("TAY", AddressMode.IMP, 2, TAY)},
            {0xBA, ("TSX", AddressMode.IMP, 2, TSX)},
            {0x8A, ("TXA", AddressMode.IMP, 2, TXA)},
            {0x9A, ("TXS", AddressMode.IMP, 2, TXS)},
            {0x98, ("TYA", AddressMode.IMP, 2, TYA)},
                                               
            // Ilegal
            {0xAB, ("ATX", AddressMode.IMP, 2, ATX)},
            {0xCB, ("AXS", AddressMode.IMP, 2, AXS)},

            
            #endregion

            #region Stack Operations 4

            {0x48, ("PHA", AddressMode.IMP, 3, PHA)},
            {0x08, ("PHP", AddressMode.IMP, 3, PHP)},
            {0x68, ("PLA", AddressMode.IMP, 4, PLA)},
            {0x28, ("PLP", AddressMode.IMP, 4, PLP)},

            #endregion

            #region Set/Clear Flags 7

            {0x18, ("CLC", AddressMode.IMP, 2, CLC)},
            {0xD8, ("CLD", AddressMode.IMP, 2, CLD)},
            {0x58, ("CLI", AddressMode.IMP, 2, CLI)},
            {0xB8, ("CLV", AddressMode.IMP, 2, CLV)},
            {0x38, ("SEC", AddressMode.IMP, 2, SEC)},
            {0xF8, ("SED", AddressMode.IMP, 2, SED)},
            {0x78, ("SEI", AddressMode.IMP, 2, SEI)},

            #endregion

            #region Interrupt Handling 2

            {0x00, ("BRK", AddressMode.IMP, 7, BRK)},
            {0x40, ("RTI", AddressMode.IMP, 6, RTI)},

            #endregion

            #region No Operation 1

            {0xEA, ("NOP", AddressMode.IMP, 2, (_,_) => 0)},
            
            // Illegal
            {0x1A, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0x3A, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0x5A, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0x7A, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0xDA, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0xFA, ("NOP*", AddressMode.IMP, 2, (_,_) => 0)},
            {0x80, ("DOP*", AddressMode.IMM, 3, DOP)},
            {0x82, ("DOP*", AddressMode.IMM, 3, DOP)},
            {0x89, ("DOP*", AddressMode.IMM, 3, DOP)},
            {0xC2, ("DOP*", AddressMode.IMM, 3, DOP)},
            {0xE2, ("DOP*", AddressMode.IMM, 3, DOP)},

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
        //PC = 0xC000;
        A = 0;
        X = 0;
        Y = 0;
        SP = 0xFD;
        Status = CpuFlags.Unused | CpuFlags.InterruptDisable;
        Cycles = 8;
        
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
        var cyclesToAdd = Execute();
        Cycles += cyclesToAdd;
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

    private byte Execute()
    {
        var opcode = ReadNextProgramByteNoLog();

        if (!opcodeActions.TryGetValue(opcode, out var entry))
        {
            throw new Exception("Unhandled opcode: 0x" + opcode.ToString("X2"));
        }

        Logger.Op(entry.Name, entry.Mode);

        (Func<ushort> fetch, ushort address, byte additionalCycles) = FetchData(entry.Mode);
        
        byte cycles = entry.Cycles;
        var additionalCycles2 = entry.Func.Invoke(fetch, address);
        cycles += (byte)(additionalCycles & additionalCycles2);

        return cycles;
    }

    private (Func<ushort> fetch, ushort address, byte cycles) FetchData(AddressMode mode)
    {
        byte zeroPageAddress = 0;
        ushort baseAddress = 0;
        ushort actualAddress = 0;
        ushort value = 0;
        switch (mode)
        {
            case AddressMode.IMM:
                value = ReadNextProgramByte();
                return (() => value, 0, 0);
            case AddressMode.ZPG:
                zeroPageAddress = ReadNextProgramByte();
                return (() => Read(zeroPageAddress), zeroPageAddress, 0);
            case AddressMode.ZPX:
                zeroPageAddress = (byte) (ReadNextProgramByte() + X);
                return (() => Read(zeroPageAddress), zeroPageAddress, 0);
            case AddressMode.ZPY:
                zeroPageAddress = (byte) (ReadNextProgramByte() + Y);
                return (() => Read(zeroPageAddress), zeroPageAddress, 0);
            case AddressMode.ACC:
            case AddressMode.IMP:
                return (() => 0,0,0);
            case AddressMode.REL:
                value = ReadNextProgramByte();
                return (() => value, 0, 0); 
            case AddressMode.ABS:
                actualAddress = ReadNext16BitProgram();
                return (() => Read(actualAddress), actualAddress, 0);
            case AddressMode.ABX:
                baseAddress = ReadNext16BitProgram();
                actualAddress = (ushort) (baseAddress + X);
                return (() => Read(actualAddress), actualAddress, Memory.CrossesPageBoundary(baseAddress, actualAddress)
                    ? (byte)1
                    : (byte)0);
            case AddressMode.ABY:
                baseAddress = ReadNext16BitProgram();
                actualAddress = (ushort) (baseAddress + Y);
                return (() => Read(actualAddress), actualAddress, Memory.CrossesPageBoundary(baseAddress, actualAddress)
                    ? (byte)1
                    : (byte)0);
            case AddressMode.IND:
                ushort address = ReadNext16BitProgram();
                return (() => Read16Bit(address), address, 0);
            case AddressMode.INX:
                zeroPageAddress = (byte) (ReadNextProgramByte() + X);
                actualAddress = Read16Bit(zeroPageAddress);
                return (() => Read(actualAddress), actualAddress, 0);
            case AddressMode.INY:
                zeroPageAddress = ReadNextProgramByte();
                baseAddress = Read16Bit(zeroPageAddress);
                actualAddress = (ushort) (baseAddress + Y);
                return (() => Read(actualAddress), actualAddress, Memory.CrossesPageBoundary(baseAddress, actualAddress)
                    ? (byte)1
                    : (byte)0);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private byte Read(ushort address) => Bus.Read(address);

    private ushort Read16Bit(ushort address)
    {
        byte lowByte = Bus.Read(address);
        byte highByte = Bus.Read((ushort) ((address & 0xFF00) | ((address+1) & 0xFF)));

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