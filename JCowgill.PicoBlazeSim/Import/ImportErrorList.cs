using System.Collections;
using System.Collections.Generic;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Contains a list of errors or warnings which occured during the import
    /// </summary>
    public class ImportErrorList : IEnumerable<ImportError>
    {
        /// <summary>
        /// The list of errors
        /// </summary>
        private readonly List<ImportError> errors = new List<ImportError>();

        /// <summary>
        /// Gets the number of warnings in the list
        /// </summary>
        public int WarningCount { get; private set; }

        /// <summary>
        /// Gets the number of errors in the list
        /// </summary>
        public int ErrorCount
        {
            get { return errors.Count - WarningCount; }
        }

        /// <summary>
        /// Adds a new error / warning to the list
        /// </summary>
        /// <param name="error">error to add</param>
        public void Add(ImportError error)
        {
            // Add and update counts
            errors.Add(error);

            if (error.IsWarning)
                WarningCount++;
        }

        /// <summary>
        /// Adds a new error to the list
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="line">optional line number</param>
        public void AddError(string message, int? line = null)
        {
            Add(new ImportError(message, line));
        }

        /// <summary>
        /// Adds a new warning to the list
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="line">optional line number</param>
        public void AddWarning(string message, int? line = null)
        {
            Add(new ImportError(message, line, true));
        }

        public IEnumerator<ImportError> GetEnumerator()
        {
            return errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
