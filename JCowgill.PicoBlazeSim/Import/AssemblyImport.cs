using JCowgill.PicoBlazeSim.Instructions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Imports instructions from assembly code
    /// </summary>
    public class AssemblyImport
    {
        #region Fields

        /// <summary>
        /// ProgramBuilder which the output is added to
        /// </summary>
        private readonly ProgramBuilder builder = new ProgramBuilder();

        /// <summary>
        /// Input stream
        /// </summary>
        private readonly TextReader input;

        /// <summary>
        /// Dictionary of constants and named registers (NOT labels)
        /// </summary>
        private Dictionary<string, Tuple<bool, short>> symbols =
            new Dictionary<string, Tuple<bool, short>>();

        /// <summary>
        /// Current token
        /// </summary>
        private Token current;

        #endregion

        #region Public Constructors + Methods

        /// <summary>
        /// Creates a new AssemblyImport state
        /// </summary>
        /// <param name="input">input stream</param>
        public AssemblyImport(TextReader input)
        {
            this.input = input;
        }

        /// <summary>
        /// Imports a program from an input stream
        /// </summary>
        /// <param name="input">input stream</param>
        /// <param name="processor">processor used to create program from</param>
        /// <returns>the imported program object</returns>
        /// <exception cref="ImportException">an error occured during the import</exception>
        public static Program Import(TextReader input, Processor processor)
        {
            return new AssemblyImport(input).Import(processor);
        }

        /// <summary>
        /// Imports the program
        /// </summary>
        /// <returns>the imported program object</returns>
        /// <param name="processor">processor used to create program from</param>
        /// <exception cref="ImportException">an error occured during the import</exception>
        public Program Import(Processor processor)
        {
            // Do top-level parsing
            ParseTopLevel();

            // Return generated program
            return builder.CreateProgram(processor);
        }

        #endregion

        #region Main Parser

        /// <summary>
        /// Dictionary of keywords with the action which parses them
        /// </summary>
        private static readonly Dictionary<string, Action<AssemblyImport>> Keywords =
            new Dictionary<string, Action<AssemblyImport>>()
        {
            // Binary Instructions
            { "ADD",        x => x.ParseBinary(BinaryType.Add) },
            { "ADDCY",      x => x.ParseBinary(BinaryType.AddCarry) },
            { "AND",        x => x.ParseBinary(BinaryType.And) },
            { "COMPARE",    x => x.ParseBinary(BinaryType.Compare) },
            { "COMPARECY",  x => x.ParseBinary(BinaryType.CompareCarry) },
            { "FETCH",      x => x.ParseBinary(BinaryType.Fetch, true) },
            { "INPUT",      x => x.ParseBinary(BinaryType.Input, true) },
            { "LOAD",       x => x.ParseBinary(BinaryType.Load) },
            { "LOAD&RETURN",x => x.ParseBinary(BinaryType.LoadReturn) },
            { "LOADRET",    x => x.ParseBinary(BinaryType.LoadReturn) },
            { "OR",         x => x.ParseBinary(BinaryType.Or) },
            { "OUTPUT",     x => x.ParseBinary(BinaryType.Output, true) },
            { "STAR",       x => x.ParseBinary(BinaryType.Star) },
            { "STORE",      x => x.ParseBinary(BinaryType.Store, true) },
            { "SUB",        x => x.ParseBinary(BinaryType.Sub) },
            { "SUBCY",      x => x.ParseBinary(BinaryType.SubCarry) },
            { "TEST",       x => x.ParseBinary(BinaryType.Test) },
            { "TESTCY",     x => x.ParseBinary(BinaryType.TestCarry) },
            { "XOR",        x => x.ParseBinary(BinaryType.Xor) },

            // Shift Instructions
            { "RL",     	x => x.ParseShift(ShiftType.Rl)  },
            { "RR",     	x => x.ParseShift(ShiftType.Rr)  },
            { "SL0",    	x => x.ParseShift(ShiftType.Sl0) },
            { "SL1",    	x => x.ParseShift(ShiftType.Sl1) },
            { "SLA",    	x => x.ParseShift(ShiftType.Sla) },
            { "SLX",    	x => x.ParseShift(ShiftType.Slx) },
            { "SR0",    	x => x.ParseShift(ShiftType.Sr0) },
            { "SR1",    	x => x.ParseShift(ShiftType.Sr1) },
            { "SRA",    	x => x.ParseShift(ShiftType.Sra) },
            { "SRX",    	x => x.ParseShift(ShiftType.Srx) },

            // Other Instructions
            { "CALL",       x => x.ParseJumpCall(true) },
            { "JUMP",       x => x.ParseJumpCall(false) },
            { "CALL@",      x => x.ParseJumpCallIndirect(true) },
            { "JUMP@",      x => x.ParseJumpCallIndirect(false) },
            { "ENABLE",     x => x.ParseSetInterruptFlag(true) },
            { "DISABLE",    x => x.ParseSetInterruptFlag(false) },
            { "RETURN",     x => x.ParseReturn() },
            { "RETURNI",    x => x.ParseReturnInterrupt() },
            { "REGBANK",    x => x.ParseSetRegisterBank() },
            { "HWBUILD",    x => x.ParseHwBuild() },
            { "OUTPUTK",    x => x.ParseOutputConstant() },

            // Assebler Directives
            { "CONSTANT",   x => x.ParseConstantDirective() },
            { "NAMEREG",    x => x.ParseNameRegDirective() },
            { "ADDRESS",    x => x.ParseAddressDirective() },
        };

        /// <summary>
        /// Parses the top-level assembly file
        /// </summary>
        private void ParseTopLevel()
        {
            // Read first token
            ConsumeToken();

            // Start token loop
            while (current.Type != TokenType.Eof)
            {
                if (current.Type == TokenType.NewLine)
                {
                    ConsumeToken();
                }
                else if (current.Type == TokenType.Word)
                {
                    Action<AssemblyImport> parseFunc;

                    // Get word data
                    string data = ConsumeWord();
                    string upper = data.ToUpperInvariant();

                    // Test for keywords
                    if (Keywords.TryGetValue(upper, out parseFunc))
                    {
                        parseFunc(this);
                        ConsumeEndOfStmt();
                    }
                    else
                    {
                        // Label

                        // Next token must be a colon
                        if (current.Type != TokenType.Colon)
                        {
                            throw new ImportException("Label \"" + data +
                                "\" must be followed by a colon");
                        }

                        // Mark the label
                        ConsumeToken();
                        builder.MarkLabel(data);
                        break;
                    }
                }
                else
                {
                    // Syntax error
                    char c = (current.Type == TokenType.Comma) ? ',' : ':';
                    throw new ImportException("Syntax error " + c);
                }
            }
        }

        /// <summary>
        /// Parses a binary instruction (register and constant)
        /// </summary>
        /// <param name="op">The BinaryType for this instruction</param>
        /// <param name="indirectRight">True if the right argument is indirect (surrounded by brakets)</param>
        private void ParseBinary(BinaryType op, bool indirectRight = false)
        {
            // Parse left side
            byte left;
            ParseSmallSymbol(SymbolType.Register, out left);
            ConsumeToken(TokenType.Comma);

            // Parse right side
            byte right;
            SymbolType type;

            if (indirectRight)
            {
                // Force register / constant depending on whether there are brakets
                if (current.Type == TokenType.BraketOpen)
                {
                    // Register
                    ConsumeToken();
                    type = ParseSmallSymbol(SymbolType.Register, out right);
                    ConsumeToken(TokenType.BraketClose);
                }
                else
                {
                    //Constant
                    type = ParseSmallSymbol(SymbolType.Constant, out right);
                }
            }
            else
            {
                // Allow any token type
                type = ParseSmallSymbol(SymbolType.Constant | SymbolType.Register, out right);
            }

            // Add final statement
            if (type == SymbolType.Constant)
                builder.Add(new BinaryConstant(op, left, right));
            else
                builder.Add(new BinaryRegister(op, left, right));
        }

        /// <summary>
        /// Parses a shift instruction
        /// </summary>
        /// <param name="op">The ShiftType for this instruction</param>
        private void ParseShift(ShiftType op)
        {
            // Parse left side
            byte reg;
            ParseSmallSymbol(SymbolType.Register, out reg);

            // Create the shift statement based on register
            builder.Add(new Shift(op, reg));
        }

        /// <summary>
        /// Parses a jump / call instruction
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        private void ParseJumpCall(bool isCall)
        {
            short data;
            string labelName;

            // Get condition
            ConditionType cond = ParseCondition();
            if (cond != ConditionType.Unconditional)
                ConsumeToken(TokenType.Comma);

            // Get label / constant
            SymbolType symType = ParseSymbol(SymbolType.Constant | SymbolType.Label,
                                                out data, out labelName);

            // Add instruction
            if (symType == SymbolType.Label)
                builder.AddWithFixup(new JumpCall(isCall, cond), labelName);
            else
                builder.Add(new JumpCall(isCall, data, cond));
        }

        /// <summary>
        /// Parses an indirect jump / call instruction
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        private void ParseJumpCallIndirect(bool isCall)
        {
            // Get both registers
            byte reg1, reg2;

            ConsumeToken(TokenType.BraketOpen);
            ParseSmallSymbol(SymbolType.Register, out reg1);
            ConsumeToken(TokenType.Comma);
            ParseSmallSymbol(SymbolType.Register, out reg2);
            ConsumeToken(TokenType.BraketClose);

            // Add instructions
            builder.Add(new JumpCallIndirect(isCall, reg1, reg2));
        }

        /// <summary>
        /// Parses a return instruction
        /// </summary>
        private void ParseReturn()
        {
            // Create return based on conditional
            builder.Add(new Return(ParseCondition()));
        }

        /// <summary>
        /// Parses a returni instruction
        /// </summary>
        private void ParseReturnInterrupt()
        {
            // Read disable word
            string enableDisable = ConsumeWord().ToUpperInvariant();

            if (enableDisable == "ENABLE")
                builder.Add(new ReturnInterrupt(true));
            else if (enableDisable == "DISABLE")
                builder.Add(new ReturnInterrupt(false));
            else
                throw new ImportException("Syntax error: " + enableDisable);
        }

        /// <summary>
        /// Parses an interrupt enabling instruction
        /// </summary>
        /// <param name="enable">true if this is an enable interrupts instruction</param>
        private void ParseSetInterruptFlag(bool enable)
        {
            // Require the INTERRUPT keyword
            if (ConsumeWord().ToUpperInvariant() != "INTERRUPT")
                throw new ImportException("Syntax error");

            // Add flag setting statement
            builder.Add(new SetInterruptFlag(enable));
        }

        /// <summary>
        /// Parses an REGBANK instruction
        /// </summary>
        private void ParseSetRegisterBank()
        {
            // Which bank to set to
            string bankStr = ConsumeWord().ToUpperInvariant();
            bool alternateBank;

            if (bankStr == "A")
                alternateBank = false;
            else if (bankStr == "B")
                alternateBank = true;
            else
                throw new ImportException("Invalid register bank: \"" + bankStr + "\"");

            // Add statement
            builder.Add(new SetRegisterBank(alternateBank));
        }

        /// <summary>
        /// Parses an HWBUILD instruction
        /// </summary>
        private void ParseHwBuild()
        {
            // Parse register
            byte reg;
            ParseSmallSymbol(SymbolType.Register, out reg);

            // Add statement
            builder.Add(new HwBuild(reg));
        }

        /// <summary>
        /// Parses an OUTPUTK instruction
        /// </summary>
        private void ParseOutputConstant()
        {
            // Get both constants
            byte constant, port;

            ParseSmallSymbol(SymbolType.Constant, out constant);
            ConsumeToken(TokenType.Comma);
            ParseSmallSymbol(SymbolType.Constant, out port);

            // Add instructions
            builder.Add(new OutputConstant(constant, port));
        }

        /// <summary>
        /// Parses a condition
        /// </summary>
        /// <returns>the condition or ConditionType.Unconditional if there wasn't one</returns>
        private ConditionType ParseCondition()
        {
            ConditionType condType;

            // Test for currect type and string
            if (current.Type != TokenType.Word)
                return ConditionType.Unconditional;

            switch (current.Data.ToUpperInvariant())
            {
                case "C":
                    condType = ConditionType.Carry;
                    break;

                case "NC":
                    condType = ConditionType.NotCarry;
                    break;

                case "Z":
                    condType = ConditionType.Zero;
                    break;

                case "NZ":
                    condType = ConditionType.NotZero;
                    break;

                default:
                    // Not a condition
                    return ConditionType.Unconditional;
            }

            // Consume this token before returning
            ConsumeToken();
            return condType;
        }

        /// <summary>
        /// Parses a constant directive
        /// </summary>
        private void ParseConstantDirective()
        {
            // Get name and constant
            string name = ConsumeWord("Invalid constant name");
            ConsumeToken(TokenType.Comma);

            byte constant;
            ParseSmallSymbol(SymbolType.Constant, out constant);

            // Store in constants
            symbols[name] = new Tuple<bool, short>(false, constant);
        }

        /// <summary>
        /// Parses a namereg directive
        /// </summary>
        private void ParseNameRegDirective()
        {
            // Get name and register
            string name = ConsumeWord("Invalid named register name");
            ConsumeToken(TokenType.Comma);

            byte register;
            ParseSmallSymbol(SymbolType.Register, out register);

            // Store in named regs
            symbols[name] = new Tuple<bool,short>(true, register);
        }

        /// <summary>
        /// Parses an address directive
        /// </summary>
        private void ParseAddressDirective()
        {
            // Read address constant and set current address
            short addr;
            ParseSymbol(SymbolType.Constant, out addr);
            builder.Address = addr;
        }

        #endregion

        #region Symbol Parser

        /// <summary>
        /// Valid symbol types
        /// </summary>
        [Flags]
        private enum SymbolType
        {
            Constant    = 1,
            Register    = 2,
            Label       = 4,
        }

        /// <summary>
        /// Parses a byte symbol of the given type
        /// </summary>
        /// <param name="allowed">allowed types of symbol</param>
        /// <param name="data">symbol data</param>
        /// <returns>the symbol's type</returns>
        private SymbolType ParseSmallSymbol(SymbolType allowed, out byte data)
        {
            short result;

            // Do the main symbol parse
            SymbolType symType = ParseSymbol(allowed, out result);

            // Limit size
            if (result < byte.MinValue || result > byte.MaxValue)
                throw new ImportException("Constant too large \"" + result + "\"");

            // Return data
            data = (byte) result;
            return symType;
        }

        /// <summary>
        /// Parses a symbol
        /// </summary>
        /// <param name="allowed">allowed types of symbol</param>
        /// <param name="data">symbol data</param>
        /// <returns>the symbol</returns>
        /// <exception cref="ImportException">
        /// Thrown if the symbol was undefined and allowed does not include Label
        /// </exception>
        private SymbolType ParseSymbol(SymbolType allowed, out short data)
        {
            string dummy;
            return ParseSymbol(allowed, out data, out dummy);
        }

        /// <summary>
        /// Parses a symbol
        /// </summary>
        /// <param name="allowed">allowed types of symbol</param>
        /// <param name="data">symbol data</param>
        /// <param name="rawString">symbol raw string</param>
        /// <returns>the symbol</returns>
        /// <exception cref="ImportException">
        /// Thrown if the symbol was undefined and allowed does not include Label
        /// </exception>
        private SymbolType ParseSymbol(SymbolType allowed, out short data, out string rawString)
        {
            SymbolType symType = SymbolType.Label;
            Tuple<bool, short> symbol;

            // Get raw data
            string symbolStr = ConsumeWord("Invalid symbol");

            // Try builtin registers and constants
            if (symbolStr[0] == 's' && short.TryParse(symbolStr.Substring(1), out data))
            {
                symType = SymbolType.Register;
            }
            else if (short.TryParse(symbolStr, NumberStyles.HexNumber, null, out data))
            {
                symType = SymbolType.Constant;
            }
            else if (symbols.TryGetValue(symbolStr, out symbol))
            {
                symType = symbol.Item1 ? SymbolType.Register : SymbolType.Constant;
                data = symbol.Item2;
            }

            // Validate symbol
            if ((symType & allowed) == 0)
                throw new ImportException("Invalid symbol type for instruction: \"" +
                                            symbolStr + "\"");

            rawString = symbolStr;
            return symType;
        }

        #endregion

        #region Tokenizer

        /// <summary>
        /// Consumes the current token
        /// </summary>
        private void ConsumeToken()
        {
            current = NextToken();
        }

        /// <summary>
        /// Consumes a word and returns its data
        /// </summary>
        /// <returns>data in the word token</returns>
        private string ConsumeWord(string errStr = "Syntax error")
        {
            // Get data and consume token
            string data = current.Data;
            ConsumeToken(TokenType.Word, errStr);
            return data;
        }

        /// <summary>
        /// Consumes a token or throws an exception
        /// </summary>
        private void ConsumeToken(TokenType token, string errStr = "Syntax error")
        {
            // Test if it is actually that token
            if (current.Type != token)
                throw new ImportException(errStr);

            // Consume token
            ConsumeToken();
        }

        /// <summary>
        /// Consumes a newline or an eof
        /// </summary>
        private void ConsumeEndOfStmt(string errStr = "Syntax error")
        {
            // Test token type
            if (current.Type != TokenType.Eof && current.Type != TokenType.NewLine)
                throw new ImportException(errStr);

            // Consume token
            ConsumeToken();
        }

        /// <summary>
        /// Returns the next token in the input stream
        /// </summary>
        /// <returns>the next token</returns>
        /// <exception cref="ImportException">Thrown if a lexical error occured</exception>
        private Token NextToken()
        {
            int c = input.Read();

            // Skip leading whitespace
            while (c > 0 && c != '\n' && c != '\r' && char.IsWhiteSpace((char) c))
                c = input.Read();

            // What type of token?
            switch (c)
            {
                case -1:
                    // EOF
                    return new Token(TokenType.Eof);

                case ';':
                    // Comment - skip this line and convert to newline
                    input.ReadLine();
                    goto case '\n';

                case '\r':
                    // Skip \n if possible
                    if (input.Peek() == '\n')
                        input.Read();

                    goto case '\n';

                case '\n':
                    // Newline
                    return new Token(TokenType.NewLine);

                case ',':
                    // Comma
                    return new Token(TokenType.Comma);

                case ':':
                    // Colon
                    return new Token(TokenType.Colon);

                case '(':
                    // BraketOpen
                    return new Token(TokenType.BraketOpen);

                case ')':
                    // BraketClose
                    return new Token(TokenType.BraketClose);

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

                    // Return the token
                    return new Token(TokenType.Word, builder.ToString());
            }
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

        /// <summary>
        /// Structure containing information about a token
        /// </summary>
        private struct Token
        {
            public readonly TokenType Type;
            public readonly string Data;

            public Token(TokenType type, string data = null)
            {
                this.Type = type;
                this.Data = data;
            }
        }

        /// <summary>
        /// Types of token which can be generated
        /// </summary>
        private enum TokenType
        {
            Eof,
            NewLine,

            Comma,
            Colon,
            BraketOpen,
            BraketClose,

            Word,
        }

        #endregion
    }
}
