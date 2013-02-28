using System.IO;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Interface which all exporters implement
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Exports the program to the given stream
        /// </summary>
        /// <param name="program">program to export</param>
        /// <param name="stream">stream to export to</param>
        void Export(Program program, Stream stream);
    }
}
