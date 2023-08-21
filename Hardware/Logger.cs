using System.IO;
using System.Text;
using SmartFormat;

namespace Hardware;


// ReSharper disable NotAccessedField.Global
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
// ReSharper enable NotAccessedField.Global

public static class Logger
{
    private static LogLine logLine;
    private static bool enabled;

    public static void Enable() => enabled = true;
    
    public static void Start(string fileName)
    {
        if (!enabled)
            return;

        _stream = File.Create(fileName);
        //_writer = new StreamWriter(_stream, Encoding.UTF8, 65536);
        _writer = new StreamWriter(_stream, Encoding.UTF8, 4096);
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

    private static StreamWriter _writer;
    private static FileStream _stream;

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