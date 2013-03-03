using System;
using System.Runtime.Serialization;

namespace JCowgill.PicoBlazeSim.Export
{
    /// <summary>
    /// Exception thrown when there is an error during the export process
    /// </summary>
    [Serializable]
    public class ExportException : Exception
    {
        public ExportException(string msg)
            : base(msg)
        {
        }

        protected ExportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
