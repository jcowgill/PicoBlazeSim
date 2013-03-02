using System.Globalization;
using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Imports instructions from a hex dump (one per line)
    /// </summary>
    public static class HexImporter
    {
        /// <summary>
        /// Imports and validates a program stored as hex program
        /// </summary>
        /// <param name="input">input reader where data is read from</param>
        /// <param name="errors">error list where errors are written to</param>
        /// <param name="processor">processor to use</param>
        /// <returns>the imported program if there were no errors</returns>
        public static Program Import(TextReader input, ImportErrorList errors, Processor processor)
        {
            // Create disassembler and builder
            InstructionDisassembler disasm = InstructionDisassembler.Create(processor);
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

            // Exit if there were errors
            if (errors.ErrorCount > 0)
                return null;

            // Finish building
            Program program;

            try
            {
                program = builder.CreateProgram(processor, false);
            }
            catch (ImportException e)
            {
                // Report and exit
                errors.AddError(e.Message);
                return null;
            }

            // Validate
            ProgramValidator.Validate(program, errors);
            return (errors.ErrorCount == 0) ? program : null;
        }
    }
}
