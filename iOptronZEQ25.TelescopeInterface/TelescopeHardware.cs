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

        //public bool AtHome
        //{
        //    get
        //    {
        //        // Command: “:AH#”
        //        // Respond: “0” The telescope is not at “home” position,
        //        // “1” The telescope is at “home” position.
        //        // This command returns whether the telescope is at “home” position.
        //        var AtHomeTransaction = new TerminatedStringTransaction(":AH#", '#', ':') { Timeout = TimeSpan.FromSeconds(2) };
        //        Task.Run(() => transactionProcessor.CommitTransaction(AtHomeTransaction));
        //        AtHomeTransaction.WaitForCompletionOrTimeout();
        //        return false;
        //    }
        //}

        //public double Azimuth
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
        //    }
        //}

        //public double Declination
        //{
        //    get
        //    {
        //        double declination = 0.0;
        //        return declination;
        //    }
        //}

        //public double DeclinationRate
        //{
        //    get
        //    {
        //        double declination = 0.0;
        //        return declination;
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
        //    }
        //}

        //public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        //{
        //    throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        //}


        //public void FindHome()
        //{
        //    throw new ASCOM.MethodNotImplementedException("FindHome");
        //}

        //public double GuideRateDeclination
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
        //    }
        //}

        //public double GuideRateRightAscension
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
        //    }
        //}

        //public bool IsPulseGuiding
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
        //    }
        //}

        //public void MoveAxis(TelescopeAxes Axis, double Rate)
        //{
        //    throw new ASCOM.MethodNotImplementedException("MoveAxis");
        //}

        //public void PulseGuide(GuideDirections Direction, int Duration)
        //{
        //    throw new ASCOM.MethodNotImplementedException("PulseGuide");
        //}

        //public double RightAscension
        //{
        //    get
        //    {
        //        double rightAscension = 0.0;
        //        return rightAscension;
        //    }
        //}

        //public double RightAscensionRate
        //{
        //    get
        //    {
        //        double rightAscensionRate = 0.0;
        //        return rightAscensionRate;
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
        //    }
        //}

        //public PierSide SideOfPier
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
        //    }
        //}

        //public double SiteLatitude
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SiteLatitude", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SiteLatitude", true);
        //    }
        //}

        //public double SiteLongitude
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SiteLongitude", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("SiteLongitude", true);
        //    }
        //}

        //public void SlewToAltAz(double Azimuth, double Altitude)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        //}

        //public void SlewToAltAzAsync(double Azimuth, double Altitude)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        //}

        //public void SlewToCoordinates(double RightAscension, double Declination)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToCoordinates");
        //}

        //public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToCoordinatesAsync");
        //}

        //public void SlewToTarget()
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToTarget");
        //}

        //public void SlewToTargetAsync()
        //{
        //    throw new ASCOM.MethodNotImplementedException("SlewToTargetAsync");
        //}

        //public bool Slewing
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("Slewing", false);
        //    }
        //}

        //public void SyncToAltAz(double Azimuth, double Altitude)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        //}

        //public void SyncToCoordinates(double RightAscension, double Declination)
        //{
        //    throw new ASCOM.MethodNotImplementedException("SyncToCoordinates");
        //}

        //public void SyncToTarget()
        //{
        //    throw new ASCOM.MethodNotImplementedException("SyncToTarget");
        //}

        //public double TargetDeclination
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("TargetDeclination", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("TargetDeclination", true);
        //    }
        //}

        //public double TargetRightAscension
        //{
        //    get
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", false);
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", true);
        //    }
        //}

        //public bool Tracking
        //{
        //    get
        //    {
        //        bool tracking = true;
        //        return tracking;
        //    }
        //    set
        //    {
        //        //        throw new ASCOM.PropertyNotImplementedException("Tracking", true);
        //    }
        //}

        #endregion

    }
}
