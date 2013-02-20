using System.Collections.Concurrent;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Simple implementation of a serial port UART
    /// </summary>
    /// <remarks>
    /// <para>This class does not simulate any delay whatsoever and there is never any data loss</para>
    /// <para>This class handles 2 ports:</para>
    /// <para>0 = Input / Output port where data is sent and received on</para>
    /// <para>1 = Status register</para>
    /// </remarks>
    public abstract class SerialIoManager : IInputOutputManager
    {
        /// <summary>
        /// Queue of bytes to be sent to the processor
        /// </summary>
        private ConcurrentQueue<byte> txQueue = new ConcurrentQueue<byte>();

        /// <summary>
        /// Enqueues a byte of data to be sent
        /// </summary>
        /// <param name="data">data to send</param>
        /// <remarks>
        /// This method is thread safe
        /// </remarks>
        public void Enqueue(byte data)
        {
            txQueue.Enqueue(data);
        }

        public byte Input(byte port)
        {
            // Which port?
            if (port == 0)
            {
                byte result;

                // Return 0 or the next byte on the queue
                if (txQueue.TryDequeue(out result))
                    return result;
            }
            else if (port == 1)
            {
                // Status port
                //  LSB is set when data is available
                return (byte) (txQueue.IsEmpty ? 6 : 7);
            }

            return 0;
        }

        public void Output(byte port, byte data)
        {
            // If sent to correct port, raise the DataSent event
            if (port == 0)
                DataReceived(data);
        }

        /// <summary>
        /// Called when data is received by the serial port
        /// </summary>
        /// <param name="data">the received data</param>
        protected abstract void DataReceived(byte data);
    }
}
