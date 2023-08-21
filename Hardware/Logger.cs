using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Logging;
using SmartFormat;

namespace Hardware;

public struct LogLine
{
    public ushort PC;
    public string Instruction;
    public ushort? Operands;
    public byte P;
    public byte A;
    public byte X;
    public byte Y;
    public ushort SP;
    public AddressMode Mode;
    public int Cycle;
}

public static class Logger
{
    private static LogLine logLine;
    private static bool enabled;

    public static void Enable() => enabled = true;
    
    public static void Start(string fileName)
    {
        if (!enabled)
            return;

        _writer = File.CreateText(fileName);
    }

    public static void StartLine(int cycle)
    {
        if (!enabled)
            return;
        
        logLine = new LogLine
        {
            Cycle = cycle
        };
    }

    public static void State(Cpu cpu)
    {
        if (!enabled)
            return;
        
        logLine.PC = cpu.PC;
        logLine.A = cpu.A;
        logLine.X = cpu.X;
        logLine.Y = cpu.Y;
        logLine.SP = cpu.SP;
        logLine.P = (byte) cpu.Status;
    }

    public static void Op(string instruction, AddressMode addressMode)
    {
        if (!enabled)
            return;
        
        logLine.Instruction = instruction;
        logLine.Mode = addressMode;
    }

    public static void Data(ushort data)
    {
        if (!enabled)
            return;
        
        logLine.Operands = data;
    }

    private static readonly string InstructionTemplate = 
            "{LogLine:{Instruction} {Mode:choose(IMP|ACC|IMM):||#$|$}}{Operands}";
    
    private static readonly string Template =
        "{LogLine.PC}: {Instruction} {LogLine:A:{A} X:{X} Y:{Y} P:{Status} SP:{SP} Cycle:{Cycle}}";
    
    private static readonly string Pattern =
        "{0:X4}:\t{1:3} ${2:X2}{3:X2}\tA:{4:X2} X:{5:X2} Y:{6:X2} P:{7:X2} SP:{8:X2}";
    private static readonly string Pattern2 =
        "{0:X4}:\t{1:3}      \tA:{4:X2} X:{5:X2} Y:{6:X2} P:{7:X2} SP:{8:X2}";

    private static StreamWriter _writer;

    // public static void EndLine()
    // {
    //     if (!enabled)
    //         return;
    //     
    //     if (logLine.Operands.HasValue)
    //     {
    //         var highByte = (byte) (logLine.Operands >> 8);
    //         var lowByte = (byte) (logLine.Operands);
    //
    //         _writer.WriteLine(Pattern, logLine.PC, logLine.Instruction, highByte > 0 ? highByte : lowByte,
    //             highByte > 0 ? lowByte : "  ", logLine.A, logLine.X, logLine.Y, logLine.P, logLine.SP);
    //     }
    //     else
    //         _writer.WriteLine(Pattern2, logLine.PC, logLine.Instruction, "  ", "  ", logLine.A, logLine.X, logLine.Y,
    //             logLine.P, logLine.SP);
    //     
    //     _writer.Flush();
    // }    
    
    public static void EndLine()
    {
        if (!enabled)
            return;

        var operands = FormatOperands();
        object model = new
        {
            LogLine = logLine,
            Operands = operands
        };
        var instruction = Smart.Format(InstructionTemplate, model);
        model = new
        {
            LogLine = logLine,
            Instruction = instruction
        };
        var line = Smart.Format(
            "{LogLine.PC:X4}: {Instruction,-12} A:{LogLine.A:X2} X:{LogLine.X:X2} Y:{LogLine.Y:X2} P:{LogLine.P:X2} SP:{LogLine.SP:X2} Cycle:{LogLine.Cycle}", model);

        _writer.WriteLine(line);
        
        if (logLine.Cycle % 1000 == 0)
            _writer.Flush();
    }

    private static string FormatOperands()
    {
        var operands = logLine.Operands;
        var mode = logLine.Mode;
        return operands switch
        {
            null when mode != AddressMode.ACC => string.Empty,
            null => "A",
            > 0xFF => $"{operands:X4}",
            _ => $"{operands:X2}"
        };
    }
}