using System.Globalization;
using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Imports instructions from a hex dump (one per line)
    /// </summary>
    public class HexImporter : AbstractImporter
    {
        /// <summary>
        /// Creates a new HexImporter
        /// </summary>
        /// <param name="processor">processor this importer uses</param>
        /// <param name="keepDebugInfo">true to keep debug information</param>
        public HexImporter(Processor processor, bool keepDebugInfo = true)
            : base(processor, keepDebugInfo)
        {
        }

        protected override ProgramBuilder ImportBuilder(TextReader input, ImportErrorList errors)
        {
            // Create disassembler and builder
            InstructionDisassembler disasm = InstructionDisassembler.Create(Processor);
            ProgramBuilder builder = new ProgramBuilder();

            // Disassemble each instruction and add to the builder
            for (int lineNumber = 1; ; lineNumber++)
            {
                string line = input.ReadLine();

                // Eof?
                if (line == null)
                    break;

                // Parse as hex
                int instructionInt;
                if (!int.TryParse(line, NumberStyles.HexNumber, null, out instructionInt))
                {
                    // Fail fast here
                    errors.AddError("Line is not a valid hex string", lineNumber);
                    return null;
                }

                // Try to disassemble and add instruction
                try
                {
                    builder.Add(disasm.Disassemble(instructionInt));
                }
                catch (ImportException e)
                {
                    // Report error
                    errors.AddError(e.Message, lineNumber);
                }
            }

            return builder;
        }
    }
}
