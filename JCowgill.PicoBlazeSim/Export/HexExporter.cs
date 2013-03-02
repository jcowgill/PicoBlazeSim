using System.Collections.Generic;
using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Exporter which outputs each instruction in hex - one per line
    /// </summary>
    public class HexExporter : IExporter
    {
        public void Export(Program program, TextWriter writer)
        {
            // Create new compiler
            InstructionAssembler compiler = new InstructionAssembler(program.Processor);
            string hexPattern = compiler.WideInstructions ? "{0:X5}" : "{0:X4}";

            // Compile each instruction and write as hex
            for (int i = 0; i < program.Instructions.Count; i++)
            {
                int result;

                // Compile instruction
                if (program.Instructions[i] == null)
                    result = 0;
                else
                    result = compiler.Assemble(program.Instructions[i]);

                // Write result
                writer.WriteLine(hexPattern, result);
            }
        }
    }
}
