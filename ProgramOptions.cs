using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCowgill.PicoBlazeSim
{
    /// <summary>
    /// Contains optiona affecting the environment the program is run in
    /// </summary>
    public class ProgramOptions
    {
        /// <summary>
        /// Gets the size of the instruction ROM
        /// </summary>
        public int RomSize
        {
            get;
            private set;
        }
    }
}
