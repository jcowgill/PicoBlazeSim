using System;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Class which controls the simulation of programs
    /// </summary>
    public class ProgramSimulator
    {
        #region Private Fields

        /// <summary>
        /// Backing variable for ProgramCounter
        /// </summary>
        private short pcBacking;

        /// <summary>
        /// Visitor class which handles instruction simulation
        /// </summary>
        private readonly Visitor myVisitor;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the program the simulator is simulating
        /// </summary>
        public Program Program { get; private set; }

        /// <summary>
        /// Gets the Io Manager the simulator is using
        /// </summary>
        public IInputOutputManager IoManager { get; private set; }

        /// <summary>
        /// Gets or sets the current program counter
        /// </summary>
        public short ProgramCounter
        {
            get { return pcBacking; }

            set
            {
                // Validate value
                if (value < 0 || value >= Program.Instructions.Count)
                    throw new ArgumentOutOfRangeException("value");

                pcBacking = value;
            }
        }

        /// <summary>
        /// Gets the array of registers this simulator can use
        /// </summary>
        public byte[] Registers { get; private set; }

        /// <summary>
        /// Gets the array of scratchpad ram data this simulator can use
        /// </summary>
        public byte[] ScratchpadRam { get; private set; }

        /// <summary>
        /// Gets the call stack
        /// </summary>
        public SimulationStack CallStack { get; private set; }

        /// <summary>
        /// The zero flag
        /// </summary>
        public bool Zero { get; set; }

        /// <summary>
        /// The carry flag
        /// </summary>
        public bool Carry { get; set; }

        /// <summary>
        /// The value of the zero flag preserved across interrupts
        /// </summary>
        public bool PreservedZero { get; set; }

        /// <summary>
        /// The value of the carry flag preserved across interrupts
        /// </summary>
        public bool PreservedCarry { get; set; }

        /// <summary>
        /// If true, allows interrupts to be handled
        /// </summary>
        public bool InterruptEnable { get; set; }

        /// <summary>
        /// If true, an interrupt is pending
        /// </summary>
        public bool InterruptPending { get; set; }

        #endregion

        #region Public Constructors and Methods

        /// <summary>
        /// Creates a new simulator for a given program
        /// </summary>
        /// <param name="program">program to simulate</param>
        /// <param name="ioManager">io manager to handle io requests</param>
        /// <exception cref="SimulationException">A program with no instructions is passed</exception>
        public ProgramSimulator(Program program, IInputOutputManager ioManager)
        {
            // Program must have some instructions
            if (program.Instructions.Count == 0)
                throw new SimulationException("Program must have some instructions");

            // Setup default values for properties
            this.Program = program;
            this.IoManager = ioManager;
            this.Registers = new byte[program.Processor.RegisterCount];
            this.ScratchpadRam = new byte[program.Processor.ScratchpadSize];
            this.CallStack = new SimulationStack(program.Processor.StackSize);

            // Setup simulation visitor
            this.myVisitor = new Visitor(this);
        }

        /// <summary>
        /// Simulates a RESET event
        /// </summary>
        /// <remarks>
        /// This does what the PicoBlaze does on a reset - it DOES NOT clear ram or the registers
        /// </remarks>
        public void Reset()
        {
            this.Zero = false;
            this.Carry = false;
            this.PreservedZero = false;
            this.PreservedCarry = false;
            this.InterruptEnable = false;
            this.InterruptPending = false;
            this.ProgramCounter = 0;
            this.CallStack.Clear();
        }

        /// <summary>
        /// Executes one instruction OR handles one interrupt
        /// </summary>
        public void StepInstruction()
        {
            // Check for interrupts
            if (InterruptPending && InterruptEnable)
            {
                // Call the interrupt vector
                CallStack.Push(new SimulationStack.Frame(ProgramCounter, true));
                ProgramCounter = Program.Processor.InterruptVector;

                // Preserve and clear flags
                PreservedZero = Zero;
                PreservedCarry = Carry;
                InterruptEnable = false;
                InterruptPending = false;
            }
            else
            {
                // Get instruction and increment program counter
                IInstruction instr = Program.Instructions[ProgramCounter];
                ProgramCounter = FixPcWraparound(ProgramCounter + 1);

                // Ignore null instructions
                if (instr != null)
                {
                    // Execute the instuction
                    instr.Accept(myVisitor);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a wrapped-around version of the given program counter if needed
        /// </summary>
        /// <param name="pc">program counter value to check</param>
        /// <returns>the wrapped around pc value</returns>
        private short FixPcWraparound(int pc)
        {
            if (pc < 0)
                return (short) (Program.Instructions.Count - 1);
            else if (pc >= Program.Instructions.Count)
                return 0;
            else
                return (short) pc;
        }

        #endregion

        #region Simulation Visitor

        /// <summary>
        /// Visitor class which simulates individual instructions
        /// </summary>
        private class Visitor : IInstructionVisitor
        {
            /// <summary>
            /// Parent ProgramSimulator
            /// </summary>
            private readonly ProgramSimulator parent;

            /// <summary>
            /// Creates a new simulation visitor from the given parent
            /// </summary>
            /// <param name="parent">parent simulator</param>
            public Visitor(ProgramSimulator parent)
            {
                this.parent = parent;
            }

            /// <summary>
            /// Ensures that a processor flag is set
            /// </summary>
            /// <param name="flag">flag to check</param>
            private void EnsureProcessorFlag(ProcessorFlags flag)
            {
                if ((parent.Program.Processor.Flags & flag) == 0)
                    throw new SimulationException("Invalid instruction for this processor (" + flag + ")");
            }

            /// <summary>
            /// Validates and reads a register
            /// </summary>
            /// <param name="reg">register to read</param>
            /// <returns>the value of that register</returns>
            private byte ReadRegister(byte reg)
            {
                if (reg >= parent.Registers.Length)
                    throw new SimulationException("Invalid register number: " + reg);

                return parent.Registers[reg];
            }

            /// <summary>
            /// Returns true if the given value has odd parity (odd number of 1s)
            /// </summary>
            /// <param name="val">value to test</param>
            /// <returns>true if the value has odd parity</returns>
            private static bool HasOddParity(byte val)
            {
                // http://graphics.stanford.edu/~seander/bithacks.html#ParityParallel
                val ^= (byte) (val >> 4);
                val &= 0xF;
                return ((0x6996 >> val) & 1) != 0;
            }

            /// <summary>
            /// Executes a binary instruction with a value on the right side
            /// </summary>
            /// <param name="instruction">instruction to execute</param>
            /// <param name="rightVal">value of whatever was on the right side</param>
            private void BinaryHelper(Instructions.Binary instruction, byte rightVal)
            {
                byte[] ram = parent.ScratchpadRam;

                // Read left register value
                byte result = ReadRegister(instruction.Left);

                // Do the operation
                switch (instruction.Type)
                {
                    case Instructions.BinaryType.Load:
                        // Copy right into left
                        result = rightVal;
                        break;

                    case Instructions.BinaryType.And:
                        // And right with left
                        result &= rightVal;
                        parent.Carry = false;
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.Or:
                        // Or right with left
                        result |= rightVal;
                        parent.Carry = false;
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.Xor:
                        // Xor right with left
                        result ^= rightVal;
                        parent.Carry = false;
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.Add:
                        // Add right with left
                        result += rightVal;
                        parent.Carry = (result < rightVal);
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.AddCarry:
                        // And right with left and carry
                        result += (byte) (rightVal + (parent.Carry ? 1U : 0U));
                        parent.Carry = (result < rightVal);
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.Sub:
                        // Subtract right from left
                        result -= rightVal;
                        parent.Carry = (result > rightVal);
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.SubCarry:
                        // Subtract right from left and carry
                        result -= (byte) (rightVal + (parent.Carry ? 1U : 0U));
                        parent.Carry = (result > rightVal);
                        parent.Zero = (result == 0);
                        break;

                    case Instructions.BinaryType.Test:
                        // Updates flags based on results of AND and XOR
                        EnsureProcessorFlag(ProcessorFlags.HasTestCompare);
                        parent.Zero = ((result & rightVal) == 0);
                        parent.Carry = HasOddParity((byte) (result & rightVal));
                        break;

                    case Instructions.BinaryType.Compare:
                        // Do comparisons
                        EnsureProcessorFlag(ProcessorFlags.HasTestCompare);
                        parent.Zero = (result == rightVal);
                        parent.Carry = (result < rightVal);
                        break;

                    case Instructions.BinaryType.Input:
                        // Forward to io manager
                        result = parent.IoManager.Input(rightVal);
                        break;

                    case Instructions.BinaryType.Output:
                        // Forward to io manager
                        parent.IoManager.Output(rightVal, result);
                        break;

                    case Instructions.BinaryType.Fetch:
                        // Fetch from scratchpad ram
                        //  The actual picoblaze ignores the high bits so we will mask them out
                        EnsureProcessorFlag(ProcessorFlags.HasStoreFetch);
                        result = ram[rightVal & (ram.Length - 1)];
                        break;

                    case Instructions.BinaryType.Store:
                        // Write to scratchpad ram
                        //  The actual picoblaze ignores the high bits so we will mask them out
                        EnsureProcessorFlag(ProcessorFlags.HasStoreFetch);
                        ram[rightVal & (ram.Length - 1)] = result;
                        break;

                    default:
                        throw new SimulationException("Invalid value for BinaryType: " +
                            instruction.Type);
                }

                // Write result back
                parent.Registers[instruction.Left] = result;
            }

            public void Visit(Instructions.BinaryConstant instruction)
            {
                BinaryHelper(instruction, instruction.Right);
            }

            public void Visit(Instructions.BinaryRegister instruction)
            {
                BinaryHelper(instruction, ReadRegister(instruction.Right));
            }

            public void Visit(Instructions.Shift instruction)
            {
                // Read register value
                uint result = ReadRegister(instruction.Register);
                bool newCarry;

                // Calculate new carry (only based on left / rightness)
                if (instruction.Type >= Instructions.ShiftType.Sra)
                    newCarry = (result & 0x01) != 0;
                else
                    newCarry = (result & 0x80) != 0;

                // Do operation
                switch (instruction.Type)
                {
                    case Instructions.ShiftType.Sla:
                        result = (result << 1) | (parent.Carry ? 1U : 0U);
                        break;

                    case Instructions.ShiftType.Rl:
                        result = (result << 1) | (result >> 7);
                        break;

                    case Instructions.ShiftType.Slx:
                        result = (result << 1) | (result & 1);
                        break;

                    case Instructions.ShiftType.Sl0:
                        result = (result << 1);
                        break;

                    case Instructions.ShiftType.Sl1:
                        result = (result << 1) | 0x01;
                        break;

                    case Instructions.ShiftType.Sra:
                        result = (result >> 1) | (parent.Carry ? 0x80U : 0U);
                        break;

                    case Instructions.ShiftType.Srx:
                        result = (result >> 1) | (result & 0x80);
                        break;

                    case Instructions.ShiftType.Rr:
                        result = (result >> 1) | (result << 7);
                        break;

                    case Instructions.ShiftType.Sr0:
                        result = (result >> 1);
                        break;

                    case Instructions.ShiftType.Sr1:
                        result = (result >> 1) | 0x80;
                        break;

                    default:
                        throw new SimulationException("Invalid value for ShiftType: " +
                            instruction.Type);
                }

                // Set flags
                parent.Carry = newCarry;
                parent.Zero = ((byte) result == 0);

                // Write result back
                parent.Registers[instruction.Register] = (byte) result;
            }

            public void Visit(Instructions.Return instruction)
            {
                // Check condition
                if (instruction.EvaluateCondition(parent.Zero, parent.Carry))
                {
                    // Return to previous address
                    parent.ProgramCounter =
                        parent.FixPcWraparound(parent.CallStack.Pop().Address + 1);
                }
            }

            public void Visit(Instructions.ReturnInterrupt instruction)
            {
                // Get previous address from stack
                parent.ProgramCounter = parent.CallStack.Pop().Address;

                // Restore and set flags
                parent.Zero = parent.PreservedZero;
                parent.Carry = parent.PreservedCarry;
                parent.InterruptEnable = instruction.EnableInterrupts;
            }

            public void Visit(Instructions.SetInterruptFlag instruction)
            {
                parent.InterruptEnable = instruction.EnableInterrupts;
            }

            public void Visit(Instructions.JumpCall instruction)
            {
                // Check condition
                if (instruction.EvaluateCondition(parent.Zero, parent.Carry))
                {
                    // Push address onto stack
                    if (instruction.IsCall)
                    {
                        // Get previous address
                        short prevAddress = parent.FixPcWraparound(parent.ProgramCounter - 1);

                        // Push onto stack
                        parent.CallStack.Push(new SimulationStack.Frame(prevAddress));
                    }

                    // Set new address
                    parent.ProgramCounter = instruction.Destination;
                }
            }
        }

        #endregion
    }
}
