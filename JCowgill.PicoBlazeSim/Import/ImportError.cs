
namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// An error or warning which occured during an import
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// True if this is a warning and not an error
        /// </summary>
        public bool IsWarning { get; private set; }

        /// <summary>
        /// Gets the line number this error occured on (or null if it doesn't have a line)
        /// </summary>
        public int? LineNumber { get; private set; }

        /// <summary>
        /// Gets the message associated with this warning
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Creates a new ImportError
        /// </summary>
        /// <param name="msg">error message</param>
        /// <param name="line">optional line number</param>
        /// <param name="warning">true to create a warning and not an error</param>
        public ImportError(string msg, int? line = null, bool warning = false)
        {
            this.Message = msg;
            this.LineNumber = line;
            this.IsWarning = warning;
        }

        /// <summary>
        /// Gets the string representing this error or warning
        /// </summary>
        /// <returns>the string representation of this error</returns>
        public override string ToString()
        {
            // Get error prefix
            string prefix = IsWarning ? "Warning" : "Error";

            // Using line number?
            if (LineNumber.HasValue)
                return prefix + "(" + LineNumber.Value + "): " + Message;
            else
                return prefix + ": " + Message;
        }
    }
}
