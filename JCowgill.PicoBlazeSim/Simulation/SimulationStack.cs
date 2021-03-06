﻿using System.Collections;
using System.Collections.Generic;

namespace JCowgill.PicoBlazeSim.Simulation
{
    /// <summary>
    /// Class which stores the simulation call stack
    /// </summary>
    /// <remarks>
    /// <para>This is similar to a normal stack but places a limit on its size</para>
    /// </remarks>
    public class SimulationStack : IEnumerable<SimulationStack.Frame>
    {
        private Frame[] stack;
        private int stackPtr;

        /// <summary>
        /// Creates a new simulation stack of the given size
        /// </summary>
        /// <param name="size">size of the simulation stack</param>
        public SimulationStack(int size)
        {
            this.stack = new Frame[size];
        }

        /// <summary>
        /// Gets the maximum size of the stack
        /// </summary>
        public int Capacity
        {
            get { return stack.Length; }
        }

        /// <summary>
        /// Returns the number of items on the stack
        /// </summary>
        public int Count
        {
            get { return stackPtr; }
        }

        /// <summary>
        /// Returns an enumerator for the stack frames in REVERSE ORDER
        /// </summary>
        /// <returns>the enumerator</returns>
        public IEnumerator<Frame> GetEnumerator()
        {
            // Iterate over everything in the stack
            for (int i = 0; i < stackPtr; i++)
            {
                yield return stack[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Clears the stack
        /// </summary>
        public void Clear()
        {
            this.stackPtr = 0;
        }

        /// <summary>
        /// Pushes a frame onto the stack
        /// </summary>
        /// <param name="frame">frame to push</param>
        /// <exception cref="SimulationException">thrown if stack overflow occurs</exception>
        public void Push(Frame frame)
        {
            // Check size
            if (stackPtr + 1 >= stack.Length)
                throw new SimulationException("Call stack overflow");

            // Store in stack (post-increment)
            this.stack[stackPtr++] = frame;
        }

        /// <summary>
        /// Gets the item at the top of the stack without popping it off
        /// </summary>
        /// <returns>item at the top of the stack</returns>
        /// <exception cref="SimulationException">thrown if stack underflow occurs</exception>
        public Frame Peek()
        {
            // Check size
            if (stackPtr == 0)
                throw new SimulationException("Call stack underflow");

            return this.stack[stackPtr - 1];
        }

        /// <summary>
        /// Pops the item at the top off
        /// </summary>
        /// <returns>item at the top of the stack</returns>
        /// <exception cref="SimulationException">thrown if stack underflow occurs</exception>
        public Frame Pop()
        {
            // Check size
            if (stackPtr == 0)
                throw new SimulationException("Call stack underflow");

            stackPtr--;
            return this.stack[stackPtr];
        }

        /// <summary>
        /// Represents a stack frame containing the address to return to
        /// </summary>
        public struct Frame
        {
            /// <summary>
            /// The saved address to return to
            /// </summary>
            /// <remarks>
            /// For normal calls, this actually stores the address of the call instruction and NOT
            /// the address to return to (unlike most processors).
            /// </remarks>
            public short Address { get; private set; }

            /// <summary>
            /// True if this call was generated by an interrupt
            /// </summary>
            /// <remarks>
            /// This property is for debug purposes only - it isn't used by the simulator
            /// </remarks>
            public bool IsInterrupt { get; private set; }

            /// <summary>
            /// Creates a new stack frame
            /// </summary>
            /// <param name="address">address to store in the frame</param>
            /// <param name="isInterrupt">true if the call was generated by an interrupt</param>
            public Frame(short address, bool isInterrupt = false) : this()
            {
                this.Address = address;
                this.IsInterrupt = isInterrupt;
            }
        }
    }
}
