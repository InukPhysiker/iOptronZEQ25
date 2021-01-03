// Copyright © 2016-2017 Tigra Astronomy, all rights reserved.
// Licensed under the MIT license, see http://tigra.mit-license.org/

// based on the TA.ArduinoPowerController project
// File: ReactiveTransactionProcessorFactory.cs modified to use iOptronZEQ25.TelescopeInterace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications;

namespace iOptronZEQ25.TelescopeInterface
{
    public class ReactiveTransactionProcessorFactory : ITransactionProcessorFactory
    {
        public string ConnectionString { get; }

        private TransactionObserver observer;
        private ReactiveTransactionProcessor processor;

        public ReactiveTransactionProcessorFactory(string connectionString)
            {
            this.ConnectionString = connectionString;
            // Endpoint will be InvalidEndpoint if the connection string is invalid.
            Endpoint = DeviceEndpoint.FromConnectionString(connectionString);
        }

        public ICommunicationChannel Channel { get; private set; }

        /// <summary>
        ///     Creates the transaction processor ready for use. Also creates and initialises the
        ///     device endpoint and the communications channel and opens the channel.
        /// </summary>
        /// <returns>ITransactionProcessor.</returns>
        public ITransactionProcessor CreateTransactionProcessor()
            {
            Endpoint = DeviceEndpoint.FromConnectionString(ConnectionString);
            Channel = CommunicationsStackBuilder.BuildChannel(Endpoint);
            observer = new TransactionObserver(Channel);
            processor = new ReactiveTransactionProcessor();
            processor.SubscribeTransactionObserver(observer, TimeSpan.FromMilliseconds(100));
            Channel.Open();
            //Task.Delay(TimeSpan.FromSeconds(2)).Wait(); // Arduino needs 2 seconds to initialize
            Thread.Sleep(TimeSpan.FromSeconds(3));
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

        public DeviceEndpoint Endpoint { get; set; }
    }
}
