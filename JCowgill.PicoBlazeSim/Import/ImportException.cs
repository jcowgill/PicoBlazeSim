using System;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Exception thrown when there is an error during the import process
    /// </summary>
    [Serializable]
    public class ImportException : Exception
    {
        public ImportException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Fatal version of ImportException
        /// </summary>
        [Serializable]
        public class Fatal : ImportException
        {
            public Fatal(string msg)
                : base(msg)
            {
            }
        }
    }
}
