﻿using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace iOptronZEQ25.TelescopeInterface
{
    public partial class TelescopeController : IDisposable
    {
        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private static Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        //
        // Vectors are used for pairs of angles that represent the various positions and rates
        //
        // X is the primary axis, Hour angle, Right Ascension or azimuth and Y is the secondary axis,
        // declination or altitude.
        //
        // Ra and hour angle are in hours and the mount positions, Declination, azimuth and altitude are in degrees.
        //

        /// <summary>
        /// Current azimuth (X) and altitude (Y )in degrees, derived from the mountAxes Vector
        /// </summary>
        private static Vector altAzm;

        /// <summary>
        /// Park axis positions, X primary, Y secondary in Alt/Az degrees
        /// </summary>
        //private static Vector parkPosition;

        /// <summary>
        /// current Ra (X, hrs) and Dec (Y, deg), derived from the mount axes
        /// </summary>
        private static Vector currentRaDec;

        /// <summary>
        /// Target right ascension (X, hrs) and declination (Y, deg)
        /// </summary>
        private static Vector targetRaDec;

        private bool isTracking = false;
        private bool slewingState;
        private double? _SiteLatitude;
        private static bool isAtHome;
        private static bool pulseGuiding;
        private static double? _SiteLongitude;
        private bool isMoving = false;
        private PierSide _SideOfPier = PierSide.pierEast;
        private bool UpdatingDeclination;
        private bool UpdatingRightAscension;
        private bool UpdatingSlewing;
        private bool UpdatingTracking;
        private static readonly double SiderealRateDPS = 0.004178; // degrees / second;
        private bool UpdatingSideOfPier;

        #region ITelescope Implementation

        public void AbortSlew()
        {
            // tl.LogMessage("AbortSlew", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("AbortSlew");
            var AbortSlewTransaction = new ZEQ25BooleanTransaction(":Q#") { Timeout = TimeSpan.FromSeconds(2) };

            int Retry = 3;
            for (int i = 0; i < Retry; i++)
            {
                Task.Run(() => transactionProcessor.CommitTransaction(AbortSlewTransaction));
                AbortSlewTransaction.WaitForCompletionOrTimeout();
                if (!AbortSlewTransaction.Failed)
                {
                    break;
                }
                else
                {
                    log.Info("AbortSlew: Failed after 3 retries!");
                }
            }
        }

        //public AlignmentModes AlignmentMode
        //{
        //    get
        //    {
        //        tl.LogMessage("AlignmentMode Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("AlignmentMode", false);
        //    }
        //}

        public double Altitude
        {
            get { return altAzm.Y; }
            set { altAzm.Y = value; }
        }

        //public double ApertureArea
        //{
        //    get
        //    {
        //        tl.LogMessage("ApertureArea Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
        //    }
        //}

        //public double ApertureDiameter
        //{
        //    get
        //    {
        //        tl.LogMessage("ApertureDiameter Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
        //    }
        //}

        private void UpdateAtHome()
        {
            // Command: “:AH#”
            // Respond: “0” The telescope is not at “home” position,
            // “1” The telescope is at “home” position.
            // This command returns whether the telescope is at “home” position.
            var AtHomeTransaction = new ZEQ25BooleanTransaction(":AH#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(AtHomeTransaction));
            AtHomeTransaction.WaitForCompletionOrTimeout();
            String response = AtHomeTransaction.Response.ToString();
            isAtHome = AtHomeTransaction.Value;
            log.Info("At Home: {0}", AtHomeTransaction.Response);
            log.Info("At Home: {0}", AtHomeTransaction.Value);
        }

        public bool AtHome
        {
            // Command: “:AH#”
            // Respond: “0” The telescope is not at “home” position,
            // “1” The telescope is at “home” position.
            // This command returns whether the telescope is at “home” position.
            get
            {
                UpdateAtHome();
                return isAtHome;
            }
        }

        //public bool AtPark
        //{
        //    get
        //    {
        //        tl.LogMessage("AtPark", "Get - " + false.ToString());
        //        return false;
        //    }
        //}

        //public IAxisRates AxisRates(TelescopeAxes Axis)
        //{
        //    tl.LogMessage("AxisRates", "Get - " + Axis.ToString());
        //    return new AxisRates(Axis);
        //}

        public double Azimuth
        {
            get { return altAzm.X; }
            set { altAzm.X = value; }
        }

        internal void UpdateDeclination()
        {
            //Command: “:GD#”
            //Response: “sDD*MM:SS#”
            UpdatingDeclination = true;
            var DeclinationTransaction = new ZEQ25Transaction(":GD#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(DeclinationTransaction));
            DeclinationTransaction.WaitForCompletionOrTimeout();
            String response = DeclinationTransaction.Response.ToString();
            if (!DeclinationTransaction.Failed)
            {
                response = response.Replace("#", "");
                response = response.Replace("*", ":");
                currentRaDec.Y = utilities.DMSToDegrees(response);
            }
            log.Info("Declination (Response): {0}", DeclinationTransaction.Response);
            UpdatingDeclination = false;
        }

        public double Declination
        {
            get
            {
                if (!UpdatingDeclination)
                {
                    UpdatingDeclination = true;
                    UpdateDeclination();
                }
                while (UpdatingDeclination)
                {
                    Thread.Sleep(100);
                }
                return currentRaDec.Y;
            }
            set { currentRaDec.Y = value; }
        }

        //public double DeclinationRate
        //{
        //    get
        //    {
        //        double declination = 0.0;
        //        tl.LogMessage("DeclinationRate", "Get - " + declination.ToString());
        //        return declination;
        //    }
        //    set
        //    {
        //        tl.LogMessage("DeclinationRate Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
        //    }
        //}

        //public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        //{
        //    tl.LogMessage("DestinationSideOfPier Get", "Not implemented");
        //    throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        //}

        //public bool DoesRefraction
        //{
        //    get
        //    {
        //        // tl.LogMessage("DoesRefraction Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
        //    }
        //    set
        //    {
        //        // tl.LogMessage("DoesRefraction Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
        //    }
        //}

        //public EquatorialCoordinateType EquatorialSystem
        //{
        //    get
        //    {
        //        EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equTopocentric;
        //        tl.LogMessage("EquatorialSystem", "Get - " + equatorialSystem.ToString());
        //        return equatorialSystem;
        //    }
        //}

        public void FindHome()
        {
            //Tracking = false;
            //Command: “:MH#”
            //Respond: “1”
            //This command will slew to the “home” position immediately.
            var FindHomeTransaction = new ZEQ25BooleanTransaction(":MH#") { Timeout = TimeSpan.FromSeconds(2) };

            int Retry = 1;
            for (int i = 0; i < Retry; i++)
            {
                Task.Run(() => transactionProcessor.CommitTransaction(FindHomeTransaction));
                FindHomeTransaction.WaitForCompletionOrTimeout();
                if (!FindHomeTransaction.Failed)
                {
                    break;
                }
                else
                {
                    log.Info("FindHome: Failed");
                }
            }
        }

        //public double FocalLength
        //{
        //    get
        //    {
        //        tl.LogMessage("FocalLength Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
        //    }
        //}

        public double GuideRateDeclination
        {
            //Command: “:AG#”
            //Response: “n.nn#”
            //This command returns the guide rate.
            //The current Declination movement rate offset for telescope guiding (degrees/sec)
            get
            {
                var GuideRateDeclinationeTransaction = new ZEQ25BooleanTransaction(":AG#") { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(GuideRateDeclinationeTransaction));
                GuideRateDeclinationeTransaction.WaitForCompletionOrTimeout();
                String response = GuideRateDeclinationeTransaction.Response.ToString();
                response = response.Replace("#", "");
                return SiderealRateDPS * Convert.ToDouble(response);
            }
            //Command: “:RGnnn#”
            //Response: “1”
            //Selects guide rate nnn*0.01x sidereal rate. nnn is in the range of 10 to 90, and 100.
            //The current Declination movement rate offset for telescope guiding (degrees/sec)
            set
            {
                int guiderate = (int)((value / SiderealRateDPS) * 100);
                String command = ":RG" + guiderate.ToString().PadLeft(3, '0') + "#";
                var GuideRateDeclinationeTransaction = new ZEQ25BooleanTransaction(command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(GuideRateDeclinationeTransaction));
            }
        }

        public double GuideRateRightAscension
        {
            //Command: “:AG#”
            //Response: “n.nn#”
            //This command returns the guide rate.
            //The current Declination movement rate offset for telescope guiding (degrees/sec)
            get
            {
                var GuideRateRightAscensionTransaction = new ZEQ25BooleanTransaction(":AG#") { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(GuideRateRightAscensionTransaction));
                GuideRateRightAscensionTransaction.WaitForCompletionOrTimeout();
                String response = GuideRateRightAscensionTransaction.Response.ToString();
                response = response.Replace("#", "");
                return SiderealRateDPS * Convert.ToDouble(response);
            }
            //Command: “:RGnnn#”
            //Response: “1”
            //Selects guide rate nnn*0.01x sidereal rate. nnn is in the range of 10 to 90, and 100.
            //The current Declination movement rate offset for telescope guiding (degrees/sec)
            set
            {
                int guiderate = (int)((value / SiderealRateDPS) * 100);
                String command = ":RG" + guiderate.ToString().PadLeft(3, '0') + "#";
                var GuideRateRightAscensionTransaction = new ZEQ25BooleanTransaction(command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(GuideRateRightAscensionTransaction));
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                return pulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            // When the motion is stopped by setting the rate to zero the scope will be set to the previous TrackingRate
            // or to no movement, depending on the state of the Tracking property.
            // iOptron ZEQ25 mount takes care of this.

            // StopMovingTransaction should execute before setting isMoving to false
            isMoving = (Rate != 0) || isMoving;
            //Command: “:SRn#”
            //Response: “1”
            //Sets the moving rate used for the N-S-E-W buttons.
            // For n, specify an integer from 1 to 9.
            // 1 stands for 1x sidereal tracking rate, 2 stands for 2x,
            // 3 stands for 8x, 4 stands for 16x, 5 stands for 64x,
            // 6 stands for 128x, 7 stands for 256x, 8 stands for 512x,
            // 9 stands for maximum speed(larger than 512x).
            //Command: “:mn#” “:me#” “:ms#” “:mw#”
            //Response: (none)
            log.Info("MoveAxis Axis: {0}", Axis);
            log.Info("MoveAxis Rate: {0}", Rate);
            String SetRateCommand;
            String MoveCommand;
            bool response = false;
            int direction = Math.Sign(Rate);

            int speed = (int)(Math.Abs(Rate) / SiderealRateDPS);
            int speed_int = speed;
            // Round up to nearest power of 2 using bitwise method
            speed--;
            speed |= speed >> 1;  // divided by 2
            speed |= speed >> 2;  // divided by 4
            speed |= speed >> 4;  // divided by 16
            speed |= speed >> 8;  // divided by 256
            speed |= speed >> 16; // divided by 65536
            speed++; // next power of 2
                     // Note: 4x and 32x are not supported so will need to be ignored or substituted

            int x = speed >> 1; // previous power of 2

            // next power of 2 - requested speed        (proximity to next power of 2)
            // requested speed - previous power of 2    (proximity to previous power of 2)
            // set speed_power to nearest power of 2
            int speed_power = (speed - speed_int) > (speed_int - x) ? x : speed;

            log.Info("MoveAxis, Speed: {0} x SiderealRateDPS", speed);
            switch (speed_power)
            {
                case (0):
                    var StopMovingTransaction = new ZEQ25NoReplyTransaction(":q#");
                    Task.Run(() => transactionProcessor.CommitTransaction(StopMovingTransaction));
                    StopMovingTransaction.WaitForCompletionOrTimeout();
                    isMoving = false;
                    return;

                case (1):
                    SetRateCommand = ":SR1#";
                    break;

                case (2):
                    SetRateCommand = ":SR2#";
                    break;

                case (4):
                    SetRateCommand = ":SR2#"; // 4x not supported set to 2x instead
                    break;

                case (8):
                    SetRateCommand = ":SR3#";
                    break;

                case (16):
                    SetRateCommand = ":SR4#";
                    break;

                case (32):
                    SetRateCommand = ":SR4#"; // 32x not supported set to 16x instead
                    break;

                case (64):
                    SetRateCommand = ":SR5#";
                    break;

                case (128):
                    SetRateCommand = ":SR6#";
                    break;

                case (256):
                    SetRateCommand = ":SR7#";
                    break;

                case (512):
                    SetRateCommand = ":SR8#";
                    break;

                case (1024):
                    SetRateCommand = ":SR9#";
                    break;

                default:
                    SetRateCommand = ":SR1#";
                    break;
            }

            var SetRateTransaction = new ZEQ25BooleanTransaction(SetRateCommand) { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SetRateTransaction));
            log.Info("Waiting for set rate command completion");
            SetRateTransaction.WaitForCompletionOrTimeout();
            response = SetRateTransaction.Value;

            if (response == false)
            {
                log.Info("MoveAxis", "Error setting speed!");
                return;
            }
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    // East is positive
                    MoveCommand = (direction == 1) ? ":me#" : ":mw#";
                    break;

                case TelescopeAxes.axisSecondary:
                    // North is positive if pierSide is pierEast
                    MoveCommand = (direction == 1) ? ":mn#" : ":ms#";
                    // Not inverting axis intentionally to avoid problems near pole
                    break;

                default:
                    // Not expected!
                    return;
            }
            var MoveTransaction = new ZEQ25NoReplyTransaction(MoveCommand);
            Task.Run(() => transactionProcessor.CommitTransaction(MoveTransaction));
            log.Info("Waiting for move command completion");
            MoveTransaction.WaitForCompletionOrTimeout();
            if (MoveTransaction.Failed)
            {
                log.Info("MoveTransaction Failed!");
            }
        }

        //public void Park()
        //{
        //    tl.LogMessage("Park", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("Park");
        //}

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            //Command: “:MnXXXXX#” “:MsXXXXX#” “:MeXXXXX#” “:MwXXXXX#”
            //Response: (none)
            //Command motion for XXXXX milliseconds in the direction specified at the currently selected guide
            //rate. If XXXXX has a value of zero, motion is continuous and requires a “:Mx00000#” command
            //to terminate.x is the same direction of the previous command.
            // XXXXX is in the range of 0 to 32767.
            pulseGuiding = true;
            String MoveDurationCommand;
            String XXXXX = Duration.ToString().PadLeft(5, '0');
            switch (Direction)
            {
                case ASCOM.DeviceInterface.GuideDirections.guideEast:
                    MoveDurationCommand = ":Me" + XXXXX + "#";
                    break;

                case ASCOM.DeviceInterface.GuideDirections.guideNorth:
                    MoveDurationCommand = ":Mn" + XXXXX + "#";
                    break;

                case ASCOM.DeviceInterface.GuideDirections.guideSouth:
                    MoveDurationCommand = ":Ms" + XXXXX + "#";
                    break;

                case ASCOM.DeviceInterface.GuideDirections.guideWest:
                    MoveDurationCommand = ":Mw" + XXXXX + "#";
                    break;

                default:
                    MoveDurationCommand = "";
                    break;
            }
            if (MoveDurationCommand != "")
            {
                var MoveDurationTransaction = new ZEQ25NoReplyTransaction(MoveDurationCommand);
                Task.Run(() => transactionProcessor.CommitTransaction(MoveDurationTransaction));
                log.Info("Waiting for move command completion");
                MoveDurationTransaction.WaitForCompletionOrTimeout();
                Thread.Sleep(Duration);
            }
            pulseGuiding = false;
        }

        public void UpdateRightAscension()
        {
            UpdatingRightAscension = true;
            //Command: “:GR#”
            //Response: “HH:MM:SS#”
            //Gets the current Right Ascension.
            var RightAscensionTransaction = new ZEQ25Transaction(":GR#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(RightAscensionTransaction));
            RightAscensionTransaction.WaitForCompletionOrTimeout();
            String response = RightAscensionTransaction.Response.ToString();
            if (!RightAscensionTransaction.Failed)
            {
                response = response.Replace("#", "");
                currentRaDec.X = utilities.HMSToHours(response);
            }
            log.Info("Update Right Ascension (Response): {0}", RightAscensionTransaction.Response);
            UpdatingRightAscension = false;
        }

        public double RightAscension
        {
            get
            {
                if (!UpdatingRightAscension)
                {
                    UpdatingRightAscension = true;
                    UpdateRightAscension();
                }
                while (UpdatingRightAscension)
                {
                    Thread.Sleep(100);
                }
                return currentRaDec.X;
            }
            set { currentRaDec.X = value; }
        }

        //public double RightAscensionRate
        //{
        //    get
        //    {
        //        double rightAscensionRate = 0.0;
        //        tl.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
        //        return rightAscensionRate;
        //    }
        //    set
        //    {
        //        tl.LogMessage("RightAscensionRate Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
        //    }
        //}

        //public void SetPark()
        //{
        //    tl.LogMessage("SetPark", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("SetPark");
        //}

        private void UpdateSideOfPier()
        {
            if (Slewing || AtHome)
            {
                UpdatingSideOfPier = false;
                return;
            }

            double HourAngle = astroUtilities.ConditionHA(SiderealTime - RightAscension);

            bool NearMeridian = (HourAngle < -11 || HourAngle > 11) || (HourAngle > -1 && HourAngle < 1);

            if (!NearMeridian) // SideOfPier is constrained by mount design (no need to send :pS# command)
            {
                _SideOfPier = (HourAngle > 0) ? PierSide.pierEast : PierSide.pierWest;
                UpdatingSideOfPier = false;
                return;
            }

            //Command: “:pS#”
            //Response: “0” East, “1” West.
            var SideOfPierTransaction = new ZEQ25BooleanTransaction(":pS#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SideOfPierTransaction));
            SideOfPierTransaction.WaitForCompletionOrTimeout();
            String response = SideOfPierTransaction.Response.ToString();
            log.Info("Update SideOfPier (Response): {0}", SideOfPierTransaction.Response);

            // pierEast is returned when the mount is observing at an hour angle between -12.0 and  -6.0
            // pierWest is returned when the mount is observing at an hour angle between  -6.0 and   0.0
            // pierEast is returned when the mount is observing at an hour angle between   0.0 and  +6.0
            // pierWest is returned when the mount is observing at an hour angle between  +6.0 and +12.0

            // "Through the pole"
            if (HourAngle < -6 || HourAngle > 6) // between -12.0 and -6.0 or between + 6.0 and + 12.0
            {
                _SideOfPier = SideOfPierTransaction.Value ? PierSide.pierEast : PierSide.pierWest;
            }
            else // between -6.0 and 0.0 or between 0.0 and + 6.0 (Normal pointing state) - 1 = West, 0 = East
            {
                _SideOfPier = SideOfPierTransaction.Value ? PierSide.pierWest : PierSide.pierEast;
            }
            UpdatingSideOfPier = false;
        }

        //public PierSide SideOfPier { get; set; }

        public PierSide SideOfPier
        {
            get
            {
                if (!UpdatingSideOfPier)
                {
                    UpdatingSideOfPier = true;
                    UpdateSideOfPier();
                }
                while (UpdatingSideOfPier)
                {
                    Thread.Sleep(100);
                }
                return _SideOfPier;
            }
            set
            {
                //tl.LogMessage("SideOfPier Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                // Now using NOVAS 3.1
                double siderealTime = 0.0;
                using (var novas = new ASCOM.Astrometry.NOVAS.NOVAS31())
                {
                    var jd = utilities.DateUTCToJulian(DateTime.UtcNow);
                    novas.SiderealTime(jd, 0, novas.DeltaT(jd),
                        ASCOM.Astrometry.GstType.GreenwichApparentSiderealTime,
                        ASCOM.Astrometry.Method.EquinoxBased,
                        ASCOM.Astrometry.Accuracy.Reduced, ref siderealTime);
                }

                // Allow for the longitude
                siderealTime += SiteLongitude / 360.0 * 24.0;

                // Reduce to the range 0 to 24 hours
                siderealTime = astroUtilities.ConditionRA(siderealTime);

                // tl.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        //public double SiteElevation
        //{
        //    get
        //    {
        //        tl.LogMessage("SiteElevation Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("SiteElevation", false);
        //    }
        //    set
        //    {
        //        tl.LogMessage("SiteElevation Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
        //    }
        //}

        private void UpdateSiteLatitude()
        {
            var SiteLatitudeTransaction = new ZEQ25Transaction(":Gt#") { Timeout = TimeSpan.FromSeconds(2) };
            int Retry = 1;
            for (int i = 0; i < Retry; i++)
            {
                Task.Run(() => transactionProcessor.CommitTransaction(SiteLatitudeTransaction));
                log.Info("Waiting for SiteLatitude");
                SiteLatitudeTransaction.WaitForCompletionOrTimeout();
                if (!SiteLatitudeTransaction.Failed)
                {
                    log.Info("SiteLatitude (Response): {0}", SiteLatitudeTransaction.Response);
                    String response = SiteLatitudeTransaction.Response.ToString();
                    response = response.Replace("#", "");
                    response = response.Replace("*", ":");
                    _SiteLatitude = utilities.DMSToDegrees(response);
                    break; // jump out of loop
                }
                else
                {
                    log.Info("UpdateSiteLatitude: Failed");
                }
            }
        }

        public double SiteLatitude
        {
            //Command: “:Gt#”
            //Response: “sDD*MM:SS#”
            //Gets the current latitude. Note the return value will be in signed format, North is positive.
            get
            {
                while (!_SiteLatitude.HasValue)
                {
                    UpdateSiteLatitude();
                    Thread.Sleep(1000);
                }
                return _SiteLatitude.Value;
            }
            //Command: “:St sDD*MM:SS#”
            //Response: “1”
            //Sets the current latitude. Data entered with this command will be “remembered” through a power
            //cycle and automatically re-applied on the next power up.
            //The latitude can only be entered in the range of - 90 to 90, north is positive.
            set
            {
                _SiteLatitude = value;
                String DDMMSS = utilities.DegreesToDMS(value, "*", ":", "");
                String sign = (value < 0) ? "" : "+";
                String Command = ":St " + sign + DDMMSS + "#";
                var SiteLatitudeTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(SiteLatitudeTransaction));
                SiteLatitudeTransaction.WaitForCompletionOrTimeout();
            }
        }

        private void UpdateSiteLongitude()
        {
            var SiteLongitudeTransaction = new ZEQ25Transaction(":Gg#") { Timeout = TimeSpan.FromSeconds(2) };
            int Retry = 1;
            for (int i = 0; i < Retry; i++)
            {
                Task.Run(() => transactionProcessor.CommitTransaction(SiteLongitudeTransaction));
                log.Info("Waiting for SiteLongitude");
                SiteLongitudeTransaction.WaitForCompletionOrTimeout();
                if (!SiteLongitudeTransaction.Failed)
                {
                    log.Info("SiteLongitude (Response): {0}", SiteLongitudeTransaction.Response);
                    String response = SiteLongitudeTransaction.Response.ToString();
                    response = response.Replace("#", "");
                    response = response.Replace("*", ":");
                    _SiteLongitude = utilities.DMSToDegrees(response);
                    break; // jump out of loop
                }
                else
                {
                    log.Info("UpdateSiteLongitude: Failed");
                }
            }
        }

        public double SiteLongitude
        {
            //Command: “:Gg#”
            //Response: “sDDD*MM:SS#”
            //Gets the current longitude. Note the return value will be in signed format, East is positive.
            get
            {
                while (!_SiteLongitude.HasValue)
                {
                    UpdateSiteLongitude();
                    Thread.Sleep(1000);
                }
                return _SiteLongitude.Value;
            }
            //Command: “:Sg sDDD*MM:SS#”
            //Response: “1”
            //Sets the current longitude. Data entered with this command will be “remembered” through a power
            //cycle and automatically re-applied on the next power up. The longitude can only be entered in the
            //range of -180 to 180, east is positive.
            set
            {
                _SiteLongitude = value;
                String DDMMSS = utilities.DegreesToDMS(value, "*", ":", "");
                String sign = (value < 0) ? "" : "+";
                String zero = (Math.Abs(value) <= 100) ? "0" : "";
                String Command = ":Sg " + sign + zero + DDMMSS + "#";
                var SiteLongitudeTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(SiteLongitudeTransaction));
                SiteLongitudeTransaction.WaitForCompletionOrTimeout();
            }
        }

        //public short SlewSettleTime
        //{
        //    get
        //    {
        //        tl.LogMessage("SlewSettleTime Get", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
        //    }
        //    set
        //    {
        //        tl.LogMessage("SlewSettleTime Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
        //    }
        //}

        //public void SlewToAltAz(double Azimuth, double Altitude)
        //{
        //    tl.LogMessage("SlewToAltAz", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        //}

        //public void SlewToAltAzAsync(double Azimuth, double Altitude)
        //{
        //    tl.LogMessage("SlewToAltAzAsync", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        //}

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            slewingState = true;
            //tl.LogMessage("SlewToCoordinates", "RightAscension " + RightAscension);
            //tl.LogMessage("SlewToCoordinates", "Declincation " + Declination);
            TargetRightAscension = RightAscension;
            TargetDeclination = Declination;
            SlewToTarget();
            // Wait for Slew to Finish and return
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            slewingState = true;
            //tl.LogMessage("SlewToCoordinatesAsync", "RightAscension " + RightAscension);
            //tl.LogMessage("SlewToCoordinatesAsync", "Declincation " + Declination);
            TargetRightAscension = RightAscension;
            TargetDeclination = Declination;
            SlewToTargetAsync();
        }

        public void SlewToTarget()
        {
            slewingState = true;
            //Server.s_MainForm.labelSlew.ForeColor = Color.Red;
            isAtHome = false;
            String Command = ":MS#";
            var SlewToTargetTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SlewToTargetTransaction));
            SlewToTargetTransaction.WaitForCompletionOrTimeout();
        }

        public void SlewToTargetAsync()
        {
            slewingState = true;
            //Server.s_MainForm.labelSlew.ForeColor = Color.Red;
            isAtHome = false;
            String Command = ":MS#";
            var SlewToTargetAsyncTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SlewToTargetAsyncTransaction));
            SlewToTargetAsyncTransaction.WaitForCompletionOrTimeout();
        }

        private void UpdateSlewing()
        {
            UpdatingSlewing = true;
            //Command: “:SE?#”
            //Response: “0” not in slewing, “1” in slewing.
            //This command get the slewing status.
            String Command = ":SE?#";
            var SlewingStateTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SlewingStateTransaction));
            SlewingStateTransaction.WaitForCompletionOrTimeout();
            if (!SlewingStateTransaction.Failed)
            {
                slewingState = SlewingStateTransaction.Value;
            }
            log.Info("Update Slewing (Response): {0}", SlewingStateTransaction.Response);
            UpdatingSlewing = false;
        }

        public bool Slewing
        {
            get
            {
                if (isMoving)
                {
                    return true;
                }
                if (!UpdatingSlewing)
                {
                    UpdatingSlewing = true;
                    UpdateSlewing();
                }
                while (UpdatingSlewing)
                {
                    Thread.Sleep(250);
                }
                return slewingState;
            }
        }

        //public void SyncToAltAz(double Azimuth, double Altitude)
        //{
        //    tl.LogMessage("SyncToAltAz", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        //}

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            //tl.LogMessage("SyncToCoordinates", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SyncToCoordinates");
            TargetRightAscension = RightAscension;
            TargetDeclination = Declination;
            SyncToTarget();
        }

        public void SyncToTarget()
        {
            //tl.LogMessage("SyncToTarget", "Sending Command :CM#");
            String Command = ":CM#";
            var SyncToTargetTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(SyncToTargetTransaction));
            SyncToTargetTransaction.WaitForCompletionOrTimeout();
        }

        public double TargetDeclination
        {
            get
            {
                // Note: ":GD#" gets the current declination! Not commanded declination.
                return targetRaDec.Y;
            }
            set
            {
                targetRaDec.Y = value;
                String DDMMSS = utilities.DegreesToDMS(value, "*", ":", "");
                String sign = (value < 0) ? "" : "+";
                String Command = ":Sd " + sign + DDMMSS + "#";
                var TargetDeclinationTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(TargetDeclinationTransaction));
                TargetDeclinationTransaction.WaitForCompletionOrTimeout();
                log.Info("TargetDeclination (Response): {0}", TargetDeclinationTransaction.Response);
            }
        }

        public double TargetRightAscension
        {
            get
            {
                return targetRaDec.X;
            }
            set
            {
                targetRaDec.X = value;
                String HHMMSS = utilities.HoursToHMS(value);
                String Command = ":Sr " + HHMMSS + "#";
                var TargetRightAscensionTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                Task.Run(() => transactionProcessor.CommitTransaction(TargetRightAscensionTransaction));
                TargetRightAscensionTransaction.WaitForCompletionOrTimeout();
                log.Info("TargetRightAscension (Response): {0}", TargetRightAscensionTransaction.Response);
            }
        }

        private void UpdateTracking()
        {
            UpdatingTracking = true;
            var TrackingTransaction = new ZEQ25BooleanTransaction(":AT#") { Timeout = TimeSpan.FromSeconds(2) };
            Task.Run(() => transactionProcessor.CommitTransaction(TrackingTransaction));
            TrackingTransaction.WaitForCompletionOrTimeout();
            isTracking = TrackingTransaction.Value;
            UpdatingTracking = false;
        }

        public bool Tracking
        {
            get
            {
                if (!UpdatingTracking)
                {
                    UpdatingTracking = true;
                    UpdateTracking();
                }
                while (UpdatingTracking)
                {
                    Thread.Sleep(250);
                }
                return isTracking;
            }
            set
            {
                if (value)
                {
                    UpdatingTracking = true;
                    isTracking = true;
                    String Command = ":ST1#";
                    var TrackingTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                    Task.Run(() => transactionProcessor.CommitTransaction(TrackingTransaction));
                    TrackingTransaction.WaitForCompletionOrTimeout();
                    UpdatingTracking = false;
                }
                else
                {
                    UpdatingTracking = true;
                    isTracking = false;
                    String Command = ":ST0#";
                    var TrackingTransaction = new ZEQ25BooleanTransaction(Command) { Timeout = TimeSpan.FromSeconds(2) };
                    Task.Run(() => transactionProcessor.CommitTransaction(TrackingTransaction));
                    TrackingTransaction.WaitForCompletionOrTimeout();
                    UpdatingTracking = false;
                }
            }
        }

        public DriveRates TrackingRate
        {
            //Command: “:QT#”
            //Response: “0” Sidereal rate, “1” Lunar rate, “2” Solar rate, “3” King rate, “4” Custom rate
            //This command gets the tracking rate.
            get
            {
                return DriveRates.driveSidereal;

                //if (IsConnected)
                //{
                //    String response;
                //    response = CommandStringInt(":QT#", false);
                //    switch (response)
                //    {
                //        case "0":
                //            return DriveRates.driveSidereal;
                //        case "1":
                //            return DriveRates.driveLunar;
                //        case "2":
                //            return DriveRates.driveSolar;
                //        case "3":
                //            return DriveRates.driveKing;
                //        default:
                //            throw new ASCOM.DriverException("TrackingRate");
                //    }
                //}
                //else
                //    return DriveRates.driveSidereal;
            }
            //Command: “:RT0#” “:RT1#” “:RT2#” “:RT3#” “:RT4#”
            //Response: “1”
            //This command selects the tracking rate. It selects sidereal (:RT0#), lunar (:RT1#), solar (:RT2#),
            //King (:RT3#), or custom (“:RT4#”). The sidereal rate is assumed as a default by the next power up.
            //This command has no effect on the use of the N-S-E-W buttons.
            set
            {
                //if (IsConnected)
                //{
                //    switch (value)
                //    {
                //        case DriveRates.driveSidereal:
                //            CommandBool(":RT0#", false);
                //            break;
                //        case DriveRates.driveLunar:
                //            CommandBool(":RT1#", false);
                //            break;
                //        case DriveRates.driveSolar:
                //            CommandBool(":RT2#", false);
                //            break;
                //        case DriveRates.driveKing:
                //            CommandBool(":RT3#", false);
                //            break;
                //        default:
                //            break;
                //    }
                //}
            }
        }

        //public ITrackingRates TrackingRates
        //{
        //    get
        //    {
        //        ITrackingRates trackingRates = new TrackingRates();
        //        tl.LogMessage("TrackingRates", "Get - ");
        //        foreach (DriveRates driveRate in trackingRates)
        //        {
        //            tl.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
        //        }
        //        return trackingRates;
        //    }
        //}

        //public DateTime UTCDate
        //{
        //    get
        //    {
        //        DateTime utcDate = DateTime.UtcNow;
        //        tl.LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
        //        return utcDate;
        //    }
        //    set
        //    {
        //        tl.LogMessage("UTCDate Set", "Not implemented");
        //        throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
        //    }
        //}

        //public void Unpark()
        //{
        //    tl.LogMessage("Unpark", "Not implemented");
        //    throw new ASCOM.MethodNotImplementedException("Unpark");
        //}

        #endregion ITelescope Implementation
    }
}