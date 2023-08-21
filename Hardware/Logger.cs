using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Logging;

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
}

public static class Logger
{
    private static LogLine logLine;
    private static string fileName;

    private static bool enabled;

    public static void Enable() => enabled = true;
    
    public static void Start(string fileName)
    {
        if (!enabled)
            return;
        
        Logger.fileName = fileName;

        _writer = File.CreateText(fileName);
    }

    public static void StartLine()
    {
        if (!enabled)
            return;
        logLine = new LogLine();
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

    public static void Op(string instruction)
    {
        if (!enabled)
            return;
        logLine.Instruction = instruction;
    }

    public static void Data(ushort data)
    {
        if (!enabled)
            return;
        logLine.Operands = data;
    }

    private static readonly string Pattern =
        "{0:X4}:\t{1:3} ${2:X2}{3:X2}\tA:{4:X2} X:{5:X2} Y:{6:X2} P:{7:X2} SP:{8:X2}";
    private static readonly string Pattern2 =
        "{0:X4}:\t{1:3}      \tA:{4:X2} X:{5:X2} Y:{6:X2} P:{7:X2} SP:{8:X2}";

    private static StreamWriter _writer;

    public static void EndLine()
    {
        if (!enabled)
            return;
        
        if (logLine.Operands.HasValue)
        {
            var highByte = (byte) (logLine.Operands >> 8);
            var lowByte = (byte) (logLine.Operands);

            _writer.WriteLine(Pattern, logLine.PC, logLine.Instruction, highByte > 0 ? highByte : lowByte,
                highByte > 0 ? lowByte : "  ", logLine.A, logLine.X, logLine.Y, logLine.P, logLine.SP);
        }
        else
            _writer.WriteLine(Pattern2, logLine.PC, logLine.Instruction, "  ", "  ", logLine.A, logLine.X, logLine.Y,
                logLine.P, logLine.SP);
        
        _writer.Flush();
    }
}