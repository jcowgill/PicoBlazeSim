using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Manages the simulation of a program using run, break, step etc methods
    /// </summary>
    /// <remarks>
    /// <para>The program simulation is run on a separate background thread.
    /// This means that all the events in this class and your IInputOutputManager must
    /// work from a different thread.</para>
    /// </remarks>
    public class SimulationManager : IDisposable
    {
        private readonly BlockingCollection<ThreadCommand> commandQueue =
            new BlockingCollection<ThreadCommand>();

        private readonly ConcurrentDictionary<short, bool> breakpoints =
            new ConcurrentDictionary<short, bool>();

        private bool disposed;

        /// <summary>
        /// Gets the simulator this class manages
        /// </summary>
        /// <remarks>
        /// Since the ProgramSimulator class is not thread-safe, do not modify
        /// anything if the simulation is running.
        /// </remarks>
        public ProgramSimulator Simulator { get; private set; }

        /// <summary>
        /// Event raised when a break event occurs
        /// </summary>
        public event EventHandler<BreakEventArgs> BreakEvent;

        /// <summary>
        /// Gets the list of breakpoints
        /// </summary>
        /// <remarks>
        /// The dictionary maps addresses -> breakpoint enabled
        /// </remarks>
        public ConcurrentDictionary<short, bool> Breakpoints { get { return breakpoints; } }

        /// <summary>
        /// Creates a new SimulationManager to manage the given simulator
        /// </summary>
        /// <param name="simulator">simulator to manage</param>
        /// <remarks>
        /// The simulator is started but breaks before executing the first instruction.
        /// Call <see cref="Continue"/> to run the simulation.
        /// </remarks>
        public SimulationManager(ProgramSimulator simulator)
        {
            this.Simulator = simulator;

            // Create simulation thread
            var thread = new Thread(SimulationThreadProc);
            thread.IsBackground = true;
            thread.Name = "SimulationManager Thread";
            thread.Start();
        }

        /// <summary>
        /// Signals the simulation thread to close
        /// </summary>
        /// <remarks>
        /// This method does not wait for the thread to close. The thread will not close until
        /// the current instruction and any break events produced have completed.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed",
            MessageId = "commandQueue")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                commandQueue.Add(ThreadCommand.Destroy);
            }
        }

        /// <summary>
        /// Main method run on the simulation thread
        /// </summary>
        private void SimulationThreadProc()
        {
            try
            {
                ThreadCommand state = ThreadCommand.Break;

                // Break when returning to this number of stack frames
                //  Used by StepOver and StepReturn
                int? stepFrameCount = null;

                // Starting command processing loop
                for (; ; )
                {
                    ThreadCommand cmd = ThreadCommand.Nop;

                    // If we're doing nothing, block for the next command
                    if (state == ThreadCommand.Break)
                        cmd = commandQueue.Take();

                    // Process any commands
                    do
                    {
                        switch (cmd)
                        {
                            case ThreadCommand.Nop:
                                // Ignore
                                break;

                            case ThreadCommand.Destroy:
                                // Destroy thread
                                return;

                            case ThreadCommand.Break:
                                // Break if we're not already
                                if (state != ThreadCommand.Break)
                                {
                                    state = ThreadCommand.Break;
                                    OnBreakEvent(new BreakEventArgs(BreakEventReason.BreakMethod));
                                }

                                break;

                            default:
                                // Cause something to run unless we are doing something already
                                if (state == ThreadCommand.Break)
                                    state = cmd;

                                break;
                        }
                    }
                    while (commandQueue.TryTake(out cmd));

                    // Destroyed?
                    if (state == ThreadCommand.Destroy)
                        break;

                    // Run one step if we need to
                    if (state != ThreadCommand.Break)
                    {
                        try
                        {
                            switch (state)
                            {
                                case ThreadCommand.StepInto:
                                    // Execute one step and break
                                    Simulator.StepInstruction();
                                    state = ThreadCommand.Break;
                                    OnBreakEvent(new BreakEventArgs(BreakEventReason.Step));
                                    break;

                                case ThreadCommand.StepOver:
                                    // Set return frame count
                                    stepFrameCount = Simulator.CallStack.Count;

                                    // Convert to continue
                                    state = ThreadCommand.Continue;
                                    goto case ThreadCommand.Continue;

                                case ThreadCommand.StepReturn:
                                    // Set return frame count
                                    stepFrameCount = Simulator.CallStack.Count - 1;

                                    // Convert to continue
                                    state = ThreadCommand.Continue;
                                    goto case ThreadCommand.Continue;

                                case ThreadCommand.Continue:
                                    // Execute one step
                                    Simulator.StepInstruction();

                                    // Check for breakpoints
                                    BreakEventArgs breakArgs = null;

                                    if (Simulator.CallStack.Count <= stepFrameCount)
                                        breakArgs = new BreakEventArgs(BreakEventReason.Step);
                                    else if (IsBreakpointSet(Simulator.ProgramCounter))
                                        breakArgs = new BreakEventArgs(BreakEventReason.Breakpoint);

                                    // Raise break event
                                    if (breakArgs != null)
                                    {
                                        stepFrameCount = null;

                                        state = ThreadCommand.Break;
                                        OnBreakEvent(breakArgs);
                                    }

                                    break;
                            }
                        }
                        catch (SimulationException e)
                        {
                            // Break on exception
                            state = ThreadCommand.Break;
                            OnBreakEvent(new BreakEventArgs(e));
                        }
                    }
                }
            }
            finally
            {
                // Destroy queue
                commandQueue.Dispose();
            }
        }

        /// <summary>
        /// Returns true if a breakpoint is set for the given address
        /// </summary>
        /// <param name="address">address to check</param>
        /// <returns>true if a breakpoint is set</returns>
        public bool IsBreakpointSet(short address)
        {
            bool value;

            if (Breakpoints.TryGetValue(address, out value))
                return value;

            return false;
        }

        /// <summary>
        /// Called when a break even occurs
        /// </summary>
        /// <param name="e">event arguments</param>
        protected virtual void OnBreakEvent(BreakEventArgs e)
        {
            if (BreakEvent != null)
                BreakEvent(this, e);
        }

        #region Command Methods

        public void Continue()
        {
            commandQueue.Add(ThreadCommand.Continue);
        }

        public void Break()
        {
            commandQueue.Add(ThreadCommand.Break);
        }

        public void StepInto()
        {
            commandQueue.Add(ThreadCommand.StepInto);
        }

        public void StepOver()
        {
            commandQueue.Add(ThreadCommand.StepOver);
        }

        public void StepReturn()
        {
            commandQueue.Add(ThreadCommand.StepReturn);
        }

        #endregion

        /// <summary>
        /// Enumeration of the commands the thread can process
        /// </summary>
        private enum ThreadCommand
        {
            Nop,

            Destroy,
            Break,

            Continue,
            StepInto,
            StepOver,
            StepReturn,
        }
    }
}
