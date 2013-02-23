using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Contains a program written for the picoblaze
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Gets the processor for this program
        /// </summary>
        public Processor Processor { get; private set; }

        /// <summary>
        /// Gets the list of instructions for this program (indexed by address)
        /// </summary>
        public IList<IInstruction> Instructions { get; private set; }

        /// <summary>
        /// Gets the dictionary of labels for this program
        /// </summary>
        public IDictionary<short, string> Labels { get; private set; }

        /// <summary>
        /// Creates a new Program
        /// </summary>
        /// <param name="processor">the processor for this program</param>
        /// <param name="instructions">the program instructions</param>
        /// <param name="labels">optional labels assigned to instructions</param>
        public Program(Processor processor, IList<IInstruction> instructions,
                        IDictionary<short, string> labels = null)
        {
            this.Processor = processor;

            // Check instructions size
            if (instructions.Count > processor.RomSize)
            {
                throw new ArgumentException("Number of instructions > size of instruction ROM");
            }

            // Create new readonly instructions array
            IInstruction[] instructionArray = new IInstruction[processor.RomSize];
            instructions.CopyTo(instructionArray, 0);
            this.Instructions = new ReadOnlyCollection<IInstruction>(instructionArray);

            // Copy labels dictionary
            if (labels == null)
                this.Labels = new ReadOnlyDictionary<short, string>();
            else
                this.Labels = new ReadOnlyDictionary<short, string>(
                                new Dictionary<short, string>(labels));
        }
    }
}
