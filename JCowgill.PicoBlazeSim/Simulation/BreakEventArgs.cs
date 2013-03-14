using System;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// EventArgs class for break events in the simulation manager
    /// </summary>
    public class BreakEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason this break event was raised
        /// </summary>
        public BreakEventReason Reason { get; private set; }

        /// <summary>
        /// Gets the exception thrown (if any)
        /// </summary>
        public SimulationException Exception { get; private set; }

        /// <summary>
        /// Creates a new BreakEventArgs class
        /// </summary>
        /// <param name="reason">reason this break event occured</param>
        public BreakEventArgs(BreakEventReason reason)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// Creates a new BreakEventArgs class from a SimulationException
        /// </summary>
        /// <param name="exception">the exception raised</param>
        public BreakEventArgs(SimulationException exception)
            : this(BreakEventReason.SimulationException)
        {
            this.Exception = exception;
        }
    }
}
