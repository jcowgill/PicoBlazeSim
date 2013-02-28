using System;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// An IO Manager which acts as a block of RAM
    /// </summary>
    /// <remarks>
    /// Any requested port that doesn't exist acts like a
    /// blackhole (write does nothing, read returns 0)
    /// </remarks>
    public class RamIoManager : IInputOutputManager
    {
        /// <summary>
        /// Gets the array of stored data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Creates a new RamIOManager
        /// </summary>
        /// <param name="size">size of available memory</param>
        public RamIoManager(int size = 256)
        {
            // Validate size
            if (size < 0 || size > 256)
                throw new ArgumentOutOfRangeException("size");

            this.Data = new byte[size];
        }

        public byte Input(byte port)
        {
            if (port < Data.Length)
                return Data[port];
            else
                return 0;
        }

        public void Output(byte port, byte data)
        {
            if (port < Data.Length)
                Data[port] = data;
        }
    }
}
