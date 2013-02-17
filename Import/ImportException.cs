using System;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Exception thrown when there is an error during the import process
    /// </summary>
    public class ImportException : Exception
    {
        public ImportException(string msg)
            : base(msg)
        {
        }

        public ImportException(string msg, Exception inner)
            : base(msg, inner)
        {
        }
    }
}
