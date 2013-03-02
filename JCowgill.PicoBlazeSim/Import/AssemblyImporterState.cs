using JCowgill.PicoBlazeSim.Instructions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace JCowgill.PicoBlazeSim.Import
{
    /// <summary>
    /// Imports instructions from assembly code
    /// </summary>
    internal class AssemblyImporterState
    {
        #region Public Importer

        /// <summary>
        /// Imports and validates an assembly program
        /// </summary>
        /// <param name="input">input reader where data is read from</param>
        /// <param name="errors">error list where errors are written to</param>
        /// <param name="processor">processor to use</param>
        /// <param name="keepDebugInfo">true to keep debug info in the final program</param>
        /// <returns>the imported program if there were no errors</returns>
        public static Program Import(TextReader input, ImportErrorList errors, Processor processor,
                                    bool keepDebugInfo = true)
        {
            // Create importer state
            AssemblyImporterState importer = new AssemblyImporterState(input, errors);

            // Do top level parsing
            importer.ParseTopLevel();

            // Check for errors
            if (errors.ErrorCount > 0)
                return null;

            // Create program
            Program program;

            try
            {
                program = importer.builder.CreateProgram(processor, keepDebugInfo);
            }
            catch (ImportException e)
            {
                // Add to error list
                errors.AddError(e.Message);
                return null;
            }

            // Validate program
            ProgramValidator.Validate(program, errors);
            return (errors.ErrorCount == 0) ? program : null;
        }

        #endregion

        #region Fields

        /// <summary>
        /// ProgramBuilder which the output is added to
        /// </summary>
        private readonly ProgramBuilder builder = new ProgramBuilder();

        /// <summary>
        /// The list of errors which occured during the import
        /// </summary>
        private readonly ImportErrorList errorList;

        /// <summary>
        /// Input stream
        /// </summary>
        private readonly AssemblyTokenizer tokenizer;

        /// <summary>
        /// Dictionary of constants and named registers (NOT labels)
        /// </summary>
        private Dictionary<string, Tuple<bool, short>> symbols =
            new Dictionary<string, Tuple<bool, short>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new AssemblyImporter state
        /// </summary>
        /// <param name="input">input stream</param>
        /// <param name="errors">error list</param>
        public AssemblyImporterState(TextReader input, ImportErrorList errors)
        {
            this.errorList = errors;
            this.tokenizer = new AssemblyTokenizer(input);
        }

        #endregion

        #region Main Parser

        /// <summary>
        /// Dictionary of keywords with the action which parses them
        /// </summary>
        private static readonly Dictionary<string, Action<AssemblyImporterState>> Keywords =
            new Dictionary<string, Action<AssemblyImporterState>>()
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
            // Start token loop
            while (tokenizer.Current.Type != AssemblyTokenType.Eof)
            {
                try
                {
                    if (tokenizer.Current.Type == AssemblyTokenType.NewLine)
                    {
                        tokenizer.ConsumeToken();
                    }
                    else if (tokenizer.Current.Type == AssemblyTokenType.Word)
                    {
                        Action<AssemblyImporterState> parseFunc;

                        // Get word data
                        string data = tokenizer.ConsumeWord();
                        string upper = data.ToUpperInvariant();

                        // Test for keywords
                        if (Keywords.TryGetValue(upper, out parseFunc))
                        {
                            parseFunc(this);
                            tokenizer.ConsumeEndOfStmt();
                        }
                        else
                        {
                            // Label

                            // Next token must be a colon
                            if (tokenizer.Current.Type != AssemblyTokenType.Colon)
                            {
                                throw new ImportException("Unknown instruction \"" + data + "\"");
                            }

                            // Mark the label
                            tokenizer.ConsumeToken();
                            builder.MarkLabel(data);
                        }
                    }
                    else
                    {
                        // Syntax error
                        throw new ImportException("Syntax error " + tokenizer.Current.Type);
                    }
                }
                catch (ImportException.Fatal e)
                {
                    // Convert exception into an error and add to the list
                    errorList.AddError(e.Message, tokenizer.Current.LineNumber);
                    return;
                }
                catch (ImportException e)
                {
                    // Convert exception into an error and add to the list
                    errorList.AddError(e.Message, tokenizer.Current.LineNumber);

                    // Skip all tokens until a new line
                    while (tokenizer.Current.Type != AssemblyTokenType.NewLine)
                    {
                        try
                        {
                            tokenizer.ConsumeToken();
                        }
                        catch (ImportException.Fatal)
                        {
                            return;
                        }
                        catch (ImportException)
                        {
                        }
                    }
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
            tokenizer.ConsumeToken(AssemblyTokenType.Comma);

            // Parse right side
            byte right;
            SymbolType type;

            if (indirectRight)
            {
                // Force register / constant depending on whether there are brakets
                if (tokenizer.Current.Type == AssemblyTokenType.BraketOpen)
                {
                    // Register
                    tokenizer.ConsumeToken();
                    type = ParseSmallSymbol(SymbolType.Register, out right);
                    tokenizer.ConsumeToken(AssemblyTokenType.BraketClose);
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
                AddInstruction(new BinaryConstant(op, left, right));
            else
                AddInstruction(new BinaryRegister(op, left, right));
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
            AddInstruction(new Shift(op, reg));
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
                tokenizer.ConsumeToken(AssemblyTokenType.Comma);

            // Get label / constant
            SymbolType symType = ParseSymbol(SymbolType.Constant | SymbolType.Label,
                                                out data, out labelName);

            // Add instruction
            if (symType == SymbolType.Label)
                AddInstruction(new JumpCall(isCall, cond), labelName);
            else
                AddInstruction(new JumpCall(isCall, data, cond));
        }

        /// <summary>
        /// Parses an indirect jump / call instruction
        /// </summary>
        /// <param name="isCall">true if this is a call instruction</param>
        private void ParseJumpCallIndirect(bool isCall)
        {
            // Get both registers
            byte reg1, reg2;

            tokenizer.ConsumeToken(AssemblyTokenType.BraketOpen);
            ParseSmallSymbol(SymbolType.Register, out reg1);
            tokenizer.ConsumeToken(AssemblyTokenType.Comma);
            ParseSmallSymbol(SymbolType.Register, out reg2);
            tokenizer.ConsumeToken(AssemblyTokenType.BraketClose);

            // Add instructions
            AddInstruction(new JumpCallIndirect(isCall, reg1, reg2));
        }

        /// <summary>
        /// Parses a return instruction
        /// </summary>
        private void ParseReturn()
        {
            // Create return based on conditional
            AddInstruction(new Return(ParseCondition()));
        }

        /// <summary>
        /// Parses a returni instruction
        /// </summary>
        private void ParseReturnInterrupt()
        {
            // Read disable word
            string enableDisable = tokenizer.ConsumeWord().ToUpperInvariant();

            if (enableDisable == "ENABLE")
                AddInstruction(new ReturnInterrupt(true));
            else if (enableDisable == "DISABLE")
                AddInstruction(new ReturnInterrupt(false));
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
            if (tokenizer.ConsumeWord().ToUpperInvariant() != "INTERRUPT")
                throw new ImportException("Syntax error");

            // Add flag setting statement
            AddInstruction(new SetInterruptFlag(enable));
        }

        /// <summary>
        /// Parses an REGBANK instruction
        /// </summary>
        private void ParseSetRegisterBank()
        {
            // Which bank to set to
            string bankStr = tokenizer.ConsumeWord().ToUpperInvariant();
            bool alternateBank;

            if (bankStr == "A")
                alternateBank = false;
            else if (bankStr == "B")
                alternateBank = true;
            else
                throw new ImportException("Invalid register bank: \"" + bankStr + "\"");

            // Add statement
            AddInstruction(new SetRegisterBank(alternateBank));
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
            AddInstruction(new HwBuild(reg));
        }

        /// <summary>
        /// Parses an OUTPUTK instruction
        /// </summary>
        private void ParseOutputConstant()
        {
            // Get both constants
            byte constant, port;

            ParseSmallSymbol(SymbolType.Constant, out constant);
            tokenizer.ConsumeToken(AssemblyTokenType.Comma);
            ParseSmallSymbol(SymbolType.Constant, out port);

            // Add instructions
            AddInstruction(new OutputConstant(constant, port));
        }

        /// <summary>
        /// Parses a condition
        /// </summary>
        /// <returns>the condition or ConditionType.Unconditional if there wasn't one</returns>
        private ConditionType ParseCondition()
        {
            ConditionType condType;

            // Test for currect type and string
            if (tokenizer.Current.Type != AssemblyTokenType.Word)
                return ConditionType.Unconditional;

            switch (tokenizer.Current.Data.ToUpperInvariant())
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
            tokenizer.ConsumeToken();
            return condType;
        }

        /// <summary>
        /// Parses a constant directive
        /// </summary>
        private void ParseConstantDirective()
        {
            // Get name and constant
            string name = tokenizer.ConsumeWord("Invalid constant name");
            tokenizer.ConsumeToken(AssemblyTokenType.Comma);

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
            string name = tokenizer.ConsumeWord("Invalid named register name");
            tokenizer.ConsumeToken(AssemblyTokenType.Comma);

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

        /// <summary>
        /// Adds an instruction to the builder using the current line number
        /// </summary>
        /// <param name="instruction">instruction to add</param>
        private void AddInstruction(IInstruction instruction)
        {
            builder.Add(instruction, tokenizer.Current.LineNumber);
        }

        /// <summary>
        /// Adds an instruction to the builder using the current line number
        /// </summary>
        /// <param name="instruction">instruction to add</param>
        /// <param name="label">label to jump to</param>
        private void AddInstruction(JumpCall instruction, string label)
        {
            builder.AddWithFixup(instruction, label, tokenizer.Current.LineNumber);
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
            string symbolStr = tokenizer.ConsumeWord("Invalid symbol");

            // Try builtin registers and constants
            if ((symbolStr[0] == 's' || symbolStr[0] == 'S') &&
                short.TryParse(symbolStr.Substring(1), NumberStyles.HexNumber, null, out data))
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
    }
}
