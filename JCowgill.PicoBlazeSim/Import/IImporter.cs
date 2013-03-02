using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Interface which all importers implement
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Imports the program from the given TextReader
        /// </summary>
        /// <param name="input">the input stream to read from</param>
        /// <param name="errors">error list to store the list of errors in</param>
        Program Import(TextReader input, ImportErrorList errors);
    }
}
