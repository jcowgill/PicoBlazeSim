using System.Collections.Generic;
using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Exporter which outputs in the internal assembly format
    /// </summary>
    /// <remarks>
    /// Will include labels and line numbers if given in the program
    /// </remarks>
    public class DebugExporter : TextExporter
    {
        /// <summary>
        /// Creates an empty ProgramDebugInfo object if the given debug info is null
        /// </summary>
        /// <param name="debugInfo">debug info to use if available</param>
        /// <returns>a valid ProgramDebugInfo object</returns>
        private static ProgramDebugInfo GetNotNull(ProgramDebugInfo debugInfo)
        {
            // Invalid?
            if (debugInfo == null)
            {
                debugInfo = new ProgramDebugInfo(new Dictionary<short, int>(),
                                                 new Dictionary<short, string>());
            }

            return debugInfo;
        }

        /// <summary>
        /// Exports the program to the given text writer
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="writer">text writer to export to</param>
        public override void Export(Program program, TextWriter writer)
        {
            // Get debug information
            ProgramDebugInfo debugInfo = GetNotNull(program.DebugInfo);

            // Process each instruction
            bool hasGap = false;

            for (short i = 0; i < program.Instructions.Count; i++)
            {
                var instruction = program.Instructions[i];

                // Valid instruction?
                if (instruction != null)
                {
                    // Add blank line if there was a gap
                    if (hasGap)
                        writer.WriteLine();

                    // Labels here?
                    string label;

                    if (debugInfo.Labels.TryGetValue(i, out label))
                        writer.WriteLine(label + ":");

                    // Line number?
                    int lineNumber;

                    if (debugInfo.LineNumbers.TryGetValue(i, out lineNumber))
                        writer.WriteLine("  ({2,3}) {0:X4} {1}", i, instruction, lineNumber);
                    else
                        writer.WriteLine("          {0:X4} {1}", i, instruction);
                }
                else
                {
                    // Ignore and mark gap
                    hasGap = true;
                }
            }
        }
    }
}
