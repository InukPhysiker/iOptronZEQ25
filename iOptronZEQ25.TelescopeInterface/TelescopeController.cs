using NLog;
using System;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications;
using TA.Ascom.ReactiveCommunications.Transactions;

namespace iOptronZEQ25.TelescopeInterface
{
    public partial class TelescopeController : IDisposable
    {
        private readonly ITransactionProcessorFactory factory;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private bool disposed;
        private ITransactionProcessor transactionProcessor;
        public TelescopeController(ITransactionProcessorFactory factory)
            {
            this.factory = factory;
        }

        public bool IsOnline => transactionProcessor != null && (factory?.Channel?.IsOpen ?? false);

        public void Dispose()
            {
            Dispose(true);
            GC.SuppressFinalize(this);
            }

        public void TestTransactions()
        {

            #region Submit some transactions
            // Ready to go. We are going to use tasks to submit the transactions, just to demonstrate thread safety.
            var raTransaction = new TerminatedStringTransaction(":GR#", '#', ':') { Timeout = TimeSpan.FromSeconds(2) };
            // The terminator and initiator are optional parameters and default to values that work for Meade style protocols.
            var decTransaction = new TerminatedStringTransaction(":GD#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(raTransaction));
            Task.Run(() => transactionProcessor.CommitTransaction(decTransaction));
            #endregion Submit some transactions

            // Command: �:AH#�
            // Respond: �0� The telescope is not at �home� position,
            // �1� The telescope is at �home� position.
            // This command returns whether the telescope is at �home� position.
            var AtHomeTransaction = new ZEQ25BooleanTransaction(":AH#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(AtHomeTransaction));
            AtHomeTransaction.WaitForCompletionOrTimeout();

            #region Wait for the results
            // NOTE we are using the transactions in the reverse order that we committed them, just to prove a point.
            log.Info("Waiting for declination");
            decTransaction.WaitForCompletionOrTimeout();
            log.Info("Declination: {0}", decTransaction.Response);
            log.Info("Waiting for Right Ascensions");
            raTransaction.WaitForCompletionOrTimeout();
            log.Info("Right Ascension: {0}", raTransaction.Response);
            log.Info("Waiting for At Home");
            raTransaction.WaitForCompletionOrTimeout();
            log.Info("At Home: {0}", AtHomeTransaction.Response);
            #endregion Wait for the results
        }

        /// <summary>
        ///     Close the connection to the AWR system. This should never fail.
        /// </summary>
        public void Close()
            {
            log.Warn("Close requested");
            if (!IsOnline)
                {
                log.Warn("Ignoring Close request because already closed");
                return;
                }
            log.Info($"Closing device endpoint: {factory.Endpoint}");
            factory.DestroyTransactionProcessor();
            log.Info("====== Channel closed: the device is now disconnected ======");
            }

        protected virtual void Dispose(bool fromUserCode)
            {
            if (!disposed)
                if (fromUserCode)
                    Close();
            disposed = true;

            // ToDo: Call the base class's Dispose(Boolean) method, if available.
            // base.Dispose(fromUserCode);
            }

        // The IDisposable pattern, as described at
        // http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P

        /// <summary>
        ///     Finalizes this instance (called prior to garbage collection by the CLR)
        /// </summary>
        ~TelescopeController()
            {
            Dispose(false);
            }
        /// <summary>
        ///     Opens the transaction pipeline for sending and receiving and performs initial state synchronization with the drive
        ///     system.
        /// </summary>
        public void Open()
            {
            log.Info($"Opening device endpoint: {factory.Endpoint}");
            transactionProcessor = factory.CreateTransactionProcessor();
            log.Info("====== Initialization completed successfully : Device is now ready to accept commands ======");
            }

        public void PerformOnConnectTasks()
            {
            //TODO: perform any tasks that must occur as soon as the communication channel is connected.
            TestTransactions();
        }
    }
}
