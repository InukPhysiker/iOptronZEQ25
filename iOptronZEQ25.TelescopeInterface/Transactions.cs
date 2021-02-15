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

	/// <summary>
	///     Receives a response consisting of 8408 if the mount is an iOptron ZEQ25
	/// </summary>
	public class ZEQ25MountInfoTransaction : DeviceTransaction
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
		/// </summary>
		/// <param name="command">The command string to send to the device.</param>
		public ZEQ25MountInfoTransaction(string command) : base(command)
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
			//var _messageDelimiter = '#';

			var buffer = new List<char>();

			source.Subscribe(b =>
			{
				buffer.Add(b);
				if (buffer.Count == 4)
				{
					var Response = new string(buffer.ToArray());
					OnNext(Response);
					OnCompleted();
					buffer.Clear();
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
					Value = Response.Single().Equals("8408");
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

    public class ZEQ25NoReplyTransaction : DeviceTransaction
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceTransaction" /> class.
        /// </summary>
        /// <param name="command">The command to be sent to the communications channel.</param>
        public ZEQ25NoReplyTransaction(string command) : base(command)
        {
            Contract.Requires(!string.IsNullOrEmpty(command));
            Timeout = TimeSpan.FromMilliseconds(500); // No reply expected but allow time to process
        }

        /// <summary>
        ///     Ignores the source sequence and does not wait for any received data. Completes the transaction immediately.
        /// </summary>
        /// <param name="source">The source sequence of received characters (ignored).</param>
        public override void ObserveResponse(IObservable<char> source)
        {
            Contract.Ensures(Response != null);
            Response = new Maybe<string>(string.Empty); // string.Empty is a value, not the absence of a value.

            // iOptron ZEQ25 requires a fraction of a second to process a NoReplyTransaction
            source.Buffer(TimeSpan.FromMilliseconds(250)).Take(1).Subscribe(x =>
            {
                if (x.Count > 0) // Unexpected reply!
                {
                    string Unexpected = new string(x.ToArray());
                    OnNext(Unexpected);
                }
                else
                {
                    OnNext(string.Empty); // Expected no reply
                }
            }, OnError, OnCompleted);
        }

        /// <summary>
        ///     Called when the response sequence completes. This indicates a successful transaction.
        /// </summary>
        /// <remarks>
        ///     If there has been an unexpected response (<c>Response.Any() == true</c>) then false is placed in
        ///     the <see cref="Value" /> property.
        /// </remarks>
        protected override void OnCompleted()
        {
            try
            {
                if (Response.Any())
                    Value = false; // Unexpected response
                else
                    Value = true; // Expected with no reply tranacation
            }
            catch (FormatException)
            {
                Value = false;
            }
            base.OnCompleted();
        }

        /// <summary>
        ///     Gets the final value of the transaction's response, as a boolean.
        /// </summary>
        /// <value><c>true</c> if value; otherwise, <c>false</c>.</value>
        public bool Value { get; private set; }
    }
}