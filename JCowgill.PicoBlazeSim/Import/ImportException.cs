using System;
using System.Runtime.Serialization;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Exception thrown when there is an error during the import process
    /// </summary>
    [Serializable]
    public class ImportException : Exception
    {
        [NonSerialized]
        private ExceptionState state;

        /// <summary>
        /// True if this exception is fatal
        /// </summary>
        public bool IsFatal
        {
            get { return state.isFatal; }
        }

        /// <summary>
        /// Creates a new ImportException
        /// </summary>
        /// <param name="msg">exception message</param>
        /// <param name="fatal">true if the exception is fatal</param>
        public ImportException(string msg, bool fatal = false)
            : base(msg)
        {
            // Create exception state
            this.state = new ExceptionState(fatal);

            // Add serialization method
            this.SerializeObjectState +=
                (ex, args) => args.AddSerializedState(state);
        }

        /// <summary>
        /// Struct holding the exception data (so that serialization works)
        /// </summary>
        [Serializable]
        private struct ExceptionState : ISafeSerializationData
        {
            public readonly bool isFatal;

            public ExceptionState(bool isFatal)
            {
                this.isFatal = isFatal;
            }

            public void CompleteDeserialization(object deserialized)
            {
                // Deserialize given object using my data
                ((ImportException) deserialized).state = this;
            }
        }
    }
}
