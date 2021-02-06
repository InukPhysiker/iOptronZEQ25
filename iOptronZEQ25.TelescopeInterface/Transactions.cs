using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications;

namespace iOptronZEQ25.TelescopeInterface
{
    /// <summary>
    ///     Receives a response consisting of <c>0</c> or <c>1</c> and interprets it as a boolean.
    /// </summary>
    public class ZEQ25BooleanTransaction : DeviceTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
        /// </summary>
        /// <param name="command">The command string to send to the device.</param>
        public ZEQ25BooleanTransaction(string command) : base(command)
        {
            Contract.Requires(!string.IsNullOrEmpty(command));
        }

        /// <summary>
        ///     Observes the character sequence from the communications channel
        ///     until a satisfactory response has been received.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        public override void ObserveResponse(IObservable<char> source)
        {
            source.Take(1).Subscribe(x => OnNext(x.ToString()), OnError, OnCompleted);
        }

        protected override void OnNext(string value)
        {
            base.OnNext(value);
        }

        /// <summary>
        ///     Called when the response sequence completes. This indicates a successful transaction.
        /// </summary>
        /// <remarks>
        ///     If there has been a valid response (<c>Response.Any() == true</c>) then it is converted to a boolean and placed in
        ///     the <see cref="Value" /> property.
        /// </remarks>
        protected override void OnCompleted()
        {
            try
            {
                if (Response.Any())
                    Value = Response.Single().StartsWith("1");
            }
            catch (FormatException)
            {
                Value = false;
            }
            base.OnCompleted();
        }

        /// <summary>
        ///     Gets the final value of teh transaction's response, as a boolean.
        /// </summary>
        /// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
        public bool Value { get; private set; }
    }
    
    /// <summary>
    ///     Receives a response consisting of <c>0#</c> or <c>1#</c> and interprets it as a boolean.
    /// </summary>
    public class ZEQ25Transaction : DeviceTransaction
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
        /// </summary>
        /// <param name="command">The command string to send to the device.</param>
        public ZEQ25Transaction(string command) : base(command)
        {
            Contract.Requires(!string.IsNullOrEmpty(command));
        }

        /// <summary>
        ///     Observes the character sequence from the communications channel
        ///     until a satisfactory response has been received.
        /// </summary>
        /// <param name="source">The source sequence.</param>
        public override void ObserveResponse(IObservable<char> source)
        {
            var _messageDelimiter = '#';

            var buffer = new List<char>();

            source.Subscribe(b =>
            {
                if (b == _messageDelimiter)
                {
                    var Response = new string(buffer.ToArray());
                    OnNext(Response);
                    OnCompleted();
                    buffer.Clear();
                }
                else
                {
                    buffer.Add(b);
                }
            },
            OnError, OnCompleted);
        }

        protected override void OnNext(string value)
        {
            base.OnNext(value);
        }

        /// <summary>
        ///     Called when the response sequence completes. This indicates a successful transaction.
        /// </summary>
        /// <remarks>
        ///     If there has been a valid response (<c>Response.Any() == true</c>) then it is converted to a boolean and placed in
        ///     the <see cref="Value" /> property.
        /// </remarks>
        protected override void OnCompleted()
        {
            try
            {
                if (Response.Any())
                    Value = Response.Single().StartsWith("1");
            }
            catch (FormatException)
            {
                Value = false;
            }
            base.OnCompleted();
        }

        /// <summary>
        ///     Gets the final value of teh transaction's response, as a boolean.
        /// </summary>
        /// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
        public bool Value { get; private set; }
    }

}