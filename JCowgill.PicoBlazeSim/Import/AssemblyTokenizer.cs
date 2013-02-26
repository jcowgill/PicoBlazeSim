using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Generates tokens for the assembly importer
    /// </summary>
    internal class AssemblyTokenizer
    {
        /// <summary>
        /// Input stream
        /// </summary>
        private readonly TextReader input;

        /// <summary>
        /// Gets the current token (last read)
        /// </summary>
        public AssemblyToken Current { get; private set; }

        /// <summary>
        /// Creates a new tokenizer
        /// </summary>
        /// <param name="input">input stream to read tokens from</param>
        public AssemblyTokenizer(TextReader input)
        {
            // Get the first token
            this.input = input;
            ConsumeToken();
        }

        /// <summary>
        /// Consumes a word and returns its data
        /// </summary>
        /// <param name="errStr">message of the exception to throw</param>
        /// <returns>data in the word token</returns>
        /// <exception cref="ImportException">Thrown if the token has the wrong type</exception>
        public string ConsumeWord(string errStr = "Syntax error")
        {
            // Get data and consume token
            string data = Current.Data;
            ConsumeToken(AssemblyTokenType.Word, errStr);
            return data;
        }

        /// <summary>
        /// Consumes a newline or an eof
        /// </summary>
        /// <param name="errStr">message of the exception to throw</param>
        /// <exception cref="ImportException">Thrown if the token has the wrong type</exception>
        public void ConsumeEndOfStmt(string errStr = "Syntax error")
        {
            // Test token type
            if (Current.Type != AssemblyTokenType.Eof &&
                Current.Type != AssemblyTokenType.NewLine)
            {
                throw new ImportException(errStr);
            }

            // Consume token
            ConsumeToken();
        }

        /// <summary>
        /// Consumes a token or throws an exception
        /// </summary>
        /// <param name="token">token which must be consumed</param>
        /// <param name="errStr">message of the exception to throw</param>
        /// <exception cref="ImportException">Thrown if the token has the wrong type</exception>
        public void ConsumeToken(AssemblyTokenType token, string errStr = "Syntax error")
        {
            // Test if it is actually that token
            if (Current.Type != token)
                throw new ImportException(errStr);

            // Consume token
            ConsumeToken();
        }

        /// <summary>
        /// Consumes the current token and updates <see cref="Current"/> with the next token
        /// </summary>
        /// <exception cref="ImportException">Thrown if a lexical error occured</exception>
        public void ConsumeToken()
        {
            int c = input.Read();

            // Skip leading whitespace
            while (c > 0 && c != '\n' && c != '\r' && char.IsWhiteSpace((char) c))
                c = input.Read();

            // Get and update current line number
            int line = Current.LineNumber;

            if (line == 0)
                line = 1;
            else if (Current.Type == AssemblyTokenType.NewLine)
                line++;

            // What type of token?
            AssemblyTokenType newTokType;
            string data = null;

            switch (c)
            {
                case ';':
                    // Comment - skip this line and convert to newline
                    input.ReadLine();
                    goto case '\n';

                case '\r':
                    // Skip \n if possible
                    if (input.Peek() == '\n')
                        input.Read();

                    goto case '\n';

                // Simple characters
                case -1:   newTokType = AssemblyTokenType.Eof;           break;
                case '\n': newTokType = AssemblyTokenType.NewLine;       break;
                case ',':  newTokType = AssemblyTokenType.Comma;         break;
                case ':':  newTokType = AssemblyTokenType.Colon;         break;
                case '(':  newTokType = AssemblyTokenType.BraketOpen;    break;
                case ')':  newTokType = AssemblyTokenType.BraketClose;   break;

                default:
                    // Identifier
                    StringBuilder builder = new StringBuilder();

                    // First char must be a letter
                    if (!IsIdentifierChar(c))
                        throw new ImportException("Lexical error (" + (char) c + ")");

                    // Get all the characters until we hit an invalid one
                    builder.Append((char) c);
                    c = input.Peek();

                    while (IsIdentifierChar(c))
                    {
                        builder.Append((char) c);
                        input.Read();
                        c = input.Peek();
                    }

                    // Store the data
                    data = builder.ToString();
                    newTokType = AssemblyTokenType.Word;
                    break;
            }

            // Create the new token
            Current = new AssemblyToken(newTokType, line, data);
        }

        /// <summary>
        /// Returns true if the given character is valid for an identifier
        /// </summary>
        /// <param name="c">character to test</param>
        /// <returns>true if the character is valid</returns>
        private static bool IsIdentifierChar(int c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') || c == '_';
        }
    }
}
