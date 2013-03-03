using System;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Exception thrown when there is an error during the import process
    /// </summary>
    [Serializable]
    public class ImportException : Exception
    {
        /// <summary>
        /// True if this exception is fatal
        /// </summary>
        public bool IsFatal { get; private set; }

        /// <summary>
        /// Creates a new ImportException
        /// </summary>
        /// <param name="msg">exception message</param>
        /// <param name="fatal">true if the exception is fatal</param>
        public ImportException(string msg, bool fatal = false)
            : base(msg)
        {
            this.IsFatal = fatal;
        }
    }
}
