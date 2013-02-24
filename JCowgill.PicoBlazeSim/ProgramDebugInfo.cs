using System.Collections.Generic;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Class containing debug information about a program
    /// </summary>
    public class ProgramDebugInfo
    {
        /// <summary>
        /// Gets the dictionary of address -> line numbers
        /// </summary>
        public IDictionary<short, int> LineNumbers { get; private set; }

        /// <summary>
        /// Gets the dictionary of labels for this program
        /// </summary>
        public IDictionary<short, string> Labels { get; private set; }

        /// <summary>
        /// Creates a new ProgramDebugInfo class
        /// </summary>
        /// <param name="lines">dictionary of line numbers for the program</param>
        /// <param name="labels">dictionary of labels for the program</param>
        public ProgramDebugInfo(IDictionary<short, int> lines, IDictionary<short, string> labels)
        {
            this.LineNumbers = new ReadOnlyDictionary<short, int>(
                                new Dictionary<short, int>(lines));
            this.Labels = new ReadOnlyDictionary<short, string>(
                                new Dictionary<short, string>(labels));
        }
    }
}
