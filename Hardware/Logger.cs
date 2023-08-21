using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;
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

    private static BlockingCollection<LogLine> _lines = new BlockingCollection<LogLine>();

    public static void Enable() => enabled = true;
    
    public static void Start(string fileName)
    {
        if (!enabled)
            return;

        Task.Factory.StartNew(() => StartWriting(fileName));
    }

    private static async Task StartWriting(string fileName)
    {
        await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using var writer = new StreamWriter(stream, Encoding.Default, 65535);

        while (true)
        {
            var line = await Task.Run(() => _lines.Take());
            var text = GetLine(line);

            await writer.WriteLineAsync(text);
        }
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

    private static readonly string OperandsDefault =
        "{Mode:choose(IMP|ACC|IMM):||#$|$}{Operands}{Mode:choose(ZPX|ZPY|ABX|ABY):,X|,Y|,X|,Y|}";

    private static readonly string OperandsInx =
        "(${Operands},X)";
    
    private static readonly string OperandsIny =
        "(${Operands}),Y";
    
    private static readonly string OperandsInd =
        "(${Operands})";
    
    private static readonly string Instruction = 
            "{LogLine:{Instruction}} {Operands}";
    
    private static readonly string Template =
        "{LogLine.PC}: {Instruction} {LogLine:A:{A} X:{X} Y:{Y} P:{Status} SP:{SP} Cycle:{Cycle}}";

    private static string GetLine(LogLine line)
    {
        var operands = FormatOperands();
        object model = new
        {
            LogLine = line,
            Operands = operands
        };
        var instruction = Smart.Format(Instruction, model);
        model = new
        {
            LogLine = line,
            Instruction = instruction
        };
        
        return Smart.Format(
            "{LogLine.PC:X4}: {Instruction,-12} A:{LogLine.A:X2} X:{LogLine.X:X2} Y:{LogLine.Y:X2} P:{LogLine.P:X2} SP:{LogLine.SP:X2} Cycle:{LogLine.Cycle}", model);
    }

    public static void EndLine()
    {
        if (!enabled)
            return;
        
        _lines.Add(logLine);
    }
    
    private static string FormatOperands()
    {
        var operands = logLine.Operands;
        var mode = logLine.Mode;
        var bytes = operands switch
        {
            null when mode != AddressMode.ACC => string.Empty,
            null => "A",
            > 0xFF => $"{operands:X4}",
            _ => $"{operands:X2}"
        };

        var model = new
        {
            Mode = mode,
            Operands = bytes
        };

        return mode switch
        {
            AddressMode.INX => Smart.Format(OperandsInx, model),
            AddressMode.INY => Smart.Format(OperandsIny, model),
            AddressMode.IND => Smart.Format(OperandsInd, model),
            _ => Smart.Format(OperandsDefault, model)
        };
    }
}