// Copyright © 2016-2017 Tigra Astronomy, all rights reserved.
// Licensed under the MIT license, see http://tigra.mit-license.org/

// based on the TA.ArduinoPowerController project
// File: ReactiveTransactionProcessorFactory.cs modified to use iOptronZEQ25.TelescopeInterace

using System;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications;

namespace iOptronZEQ25.TelescopeInterface
{
    /// <summary>
    ///     Class ReactiveTransactionProcessorFactory. Used to set up and tear down the communications stack
    ///     as the device is connected and disconnected.
    ///     Implements <see cref="iOptronZEQ25.TelescopeInterface.ITransactionProcessorFactory" />
    /// </summary>
    /// <seealso cref="iOptronZEQ25.TelescopeInterface.ITransactionProcessorFactory" />
    public class ReactiveTransactionProcessorFactory : ITransactionProcessorFactory
    {

        private TransactionObserver observer;
        private ReactiveTransactionProcessor processor;

        public ReactiveTransactionProcessorFactory(string connectionString)
            {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public ICommunicationChannel Channel { get; private set; }

        /// <summary>
        ///     Creates the transaction processor ready for use. Also creates and initialises the
        ///     device endpoint and the communications channel and opens the channel.
        /// </summary>
        /// <returns>ITransactionProcessor.</returns>
        public ITransactionProcessor CreateTransactionProcessor()
            {
            Channel = new ChannelFactory().FromConnectionString(ConnectionString);
            observer = new TransactionObserver(Channel);
            processor = new ReactiveTransactionProcessor();
            processor.SubscribeTransactionObserver(observer, TimeSpan.FromMilliseconds(100));
            Channel.Open();
            // iOptron ZEQ25 may need a few seconds to initialize after starting the connection
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            return processor;
            }

        /// <summary>
        ///     Destroys the transaction processor and its dependencies. Ensures that the
        ///     <see cref="Channel" /> is closed. Once this method has been called, the
        ///     <see cref="Channel" /> and <see cref="Endpoint" /> properties will be null. A new
        ///     connection to the same endpoint can be created by calling
        ///     <see cref="CreateTransactionProcessor" /> again.
        /// </summary>
        public void DestroyTransactionProcessor()
            {
            processor?.Dispose();
            processor = null; // [Sentinel]
            observer = null;
            if (Channel?.IsOpen ?? false)
                Channel.Close();
            Channel?.Dispose();
            Channel = null; // [Sentinel]
            GC.Collect(3, GCCollectionMode.Forced, blocking: true);
            }

        public DeviceEndpoint Endpoint => Channel?.Endpoint ?? new InvalidEndpoint();
    }
}
