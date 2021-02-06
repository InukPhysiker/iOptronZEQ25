using ASCOM.DeviceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA.Ascom.ReactiveCommunications.Transactions;

namespace iOptronZEQ25.TelescopeInterface
{
    public partial class TelescopeController : IDisposable
    {
        #region ITelescope Implementation

        public void AbortSlew()
        {
            var AbortSlewTransaction = new NoReplyTransaction(":q#");
            //    throw new ASCOM.MethodNotImplementedException("AbortSlew");
        }

        #endregion

    }
}
