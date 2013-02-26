
namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Structure containing information about a token
    /// </summary>
    internal struct AssemblyToken
    {
        /// <summary>
        /// Type of token produced
        /// </summary>
        public readonly AssemblyTokenType Type;

        /// <summary>
        /// Line number the token occured on
        /// </summary>
        public readonly int LineNumber;

        /// <summary>
        /// Extra data for the token (only certain token types)
        /// </summary>
        public readonly string Data;

        /// <summary>
        /// Creates a new AssemblyToken
        /// </summary>
        /// <param name="type">type of the token</param>
        /// <param name="line">line number</param>
        /// <param name="data">any extra data</param>
        public AssemblyToken(AssemblyTokenType type, int line, string data = null)
        {
            this.Type = type;
            this.LineNumber = line;
            this.Data = data;
        }
    }
}
