using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Abstract class for textual exporters
    /// </summary>
    public abstract class TextExporter : IExporter
    {
        /// <summary>
        /// Exports the program to the given text writer
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="writer">text writer to export to</param>
        public abstract void Export(Program program, TextWriter writer);

        /// <summary>
        /// Exports the program to the given stream
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="stream">stream to export to</param>
        public void Export(Program program, Stream stream)
        {
            Export(program, new StreamWriter(stream));
        }
    }
}
