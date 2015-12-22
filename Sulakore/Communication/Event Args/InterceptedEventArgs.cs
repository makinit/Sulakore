using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sulakore.Protocol;

namespace Sulakore.Communication
{
    /// <summary>
    /// Represents an intercepted message that will be returned to the caller with blocking/replacing information.
    /// </summary>
    public class InterceptedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current count/step/order from which this data was intercepted.
        /// </summary>
        public int Step { get; }
        /// <summary>
        /// Gets the originally intercepted <see cref="HMessage"/>.
        /// </summary>
        public HMessage Packet { get; }
        /// <summary>
        /// Gets the replacement <see cref="HMessage"/> for the original data.
        /// </summary>
        public HMessage Replacement { get; set; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="InterceptedEventArgs"/> can be turned into a non-blocking operation by calling <see cref="ContinueRead"/>.
        /// </summary>
        public bool IsAsyncCapable { get; }
        /// <summary>
        /// Gets the <see cref="Func{TResult}"/> of type <see cref="Task"/> that will be invoked when <see cref="ContinueRead"/> is called.
        /// </summary>
        public Func<Task> Continuation { get; internal set; }
        /// <summary>
        /// Gets a value that determines whether <see cref="ContinueRead"/> was called by the receiver.
        /// </summary>
        public bool WasContinued { get; internal set; }
        /// <summary>
        /// Gets a value that determines whether the <see cref="Replacement"/> differs from the original.
        /// </summary>
        public bool WasReplaced
        {
            get { return !Packet.ToString().Equals(Replacement.ToString()); }
        }
        /// <summary>
        /// Gets or sets a value that determines whether the data should be blocked.
        /// </summary>
        public bool IsBlocked { get; set; }
        /// <summary>
        /// Gets a list of data that will be sent to the destination after this intercepted data has been processed first.
        /// </summary>
        public List<HMessage> Executions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptedEventArgs"/> class.
        /// </summary>
        /// <param name="continuation">The <see cref="Func{TResult}"/> of type <see cref="Task"/> that will be invoked when <see cref="ContinueRead"/> is called.</param>
        /// <param name="step">The current count/step/order from which this data was intercepted.</param>
        /// <param name="packet">The intercepted data to read/write from.</param>
        public InterceptedEventArgs(Func<Task> continuation, int step, HMessage packet)
        {
            Executions = new List<HMessage>();

            Continuation = continuation;
            IsAsyncCapable = (Continuation != null);

            Step = step;
            Packet = packet;

            Replacement = new HMessage(
                packet.ToBytes(), packet.Destination);
        }

        /// <summary>
        /// Invokes the <see cref="Func{TResult}"/> of type <see cref="Task"/> if <see cref="IsAsyncCapable"/> is true.
        /// </summary>
        public void ContinueRead()
        {
            if (!IsAsyncCapable || WasContinued) return;
            else WasContinued = true;

            Continuation();
        }
    }
}