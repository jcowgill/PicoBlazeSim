using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Interface which all exporters implement
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Exports the program to the given TextWriter
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="writer">writer to export to</param>
        /// <exception cref="ExportException">thrown if there's an error during the export</exception>
        void Export(Program program, TextWriter writer);
    }
}
