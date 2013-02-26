
namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Types of token which can be generated
    /// </summary>
    internal enum AssemblyTokenType
    {
        /// <summary>Eof of file</summary>
        Eof,

        /// <summary>New line</summary>
        NewLine,

        /// <summary>Comma character ,</summary>
        Comma,

        /// <summary>Colon character :</summary>
        Colon,

        /// <summary>Open braket character (</summary>
        BraketOpen,

        /// <summary>Close braket character )</summary>
        BraketClose,

        /// <summary>A word / keyword / data (includes numbers)</summary>
        Word,
    }
}
