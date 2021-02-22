//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Telescope driver for iOptronZEQ25
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam
//				erat, sed diam voluptua. At vero eos et accusam et justo duo
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Telescope interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//

// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
#define Telescope

using ASCOM.Astrometry.AstroUtils;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using iOptronZEQ25.TelescopeInterface;
using NLog;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using TA.PostSharp.Aspects;

namespace ASCOM.iOptronZEQ25.Server
{
    //
    // Your driver's DeviceID is ASCOM.iOptronZEQ25.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.iOptronZEQ25.Telescope
    // The ClassInterface/None attribute prevents an empty interface called
    // _iOptronZEQ25 from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for iOptronZEQ25.
    /// </summary>
    [Guid("b1ae37be-f8fd-44a5-8c0c-9959c8f45bc3")]
    //[ProgId("ASCOM.iOptronZEQ25.Telescope")]
    [ProgId(SharedResources.TelescopeDriverId)]
    //[ServedClassName("iOptron ZEQ25 Telescope")]
    [ServedClassName(SharedResources.TelescopeDriverName)]
    [ClassInterface(ClassInterfaceType.None)]
    //[NLogTraceWithArguments]
    public class Telescope : ReferenceCountedObjectBase, ITelescopeV3, IDisposable, IAscomDriver
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        //internal static string driverID = "ASCOM.iOptronZEQ25.Telescope";
        internal static string driverID;

        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        //private static string driverDescription = "ASCOM Telescope Driver for iOptronZEQ25.";
        private static string driverDescription;

        // internal static string comPortProfileName = "COM Port"; // Constants used for Profile persistence
        // internal static string comPortDefault = "COM1";
        internal static string traceStateProfileName = "Trace Level";

        internal static string traceStateDefault = "false";

        internal static string comPort; // Variables to hold the current device configuration

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        //private bool connectedState;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        // Reactive stuff
        private readonly Guid clientId;

        private readonly ILogger log = LogManager.GetCurrentClassLogger();
        private TelescopeController telescope;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="iOptronZEQ25"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
            driverID = Marshal.GenerateProgIdForType(this.GetType());
            driverDescription = GetDriverDescription();

            tl = new TraceLogger("", "iOptronZEQ25");
            //ReadProfile(); // Read device configuration from the ASCOM Profile store
            //Properties.Settings.Default.Reload();
            comPort = iOptronZEQ25.Properties.Settings.Default.COMPort;

            tl.LogMessage("Telescope", "Starting initialisation");

            //connectedState = false; // Initialise connected to false
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro-utilities object
            //TODO: Implement your additional construction here

            clientId = SharedResources.ConnectionManager.RegisterClient(SharedResources.TelescopeDriverId);

            tl.LogMessage("Telescope", "Completed initialisation");
        }

        internal bool IsOnline => telescope?.IsOnline ?? false;

        private string GetDriverDescription()
        {
            string descr;
            if (this.GetType().GetCustomAttributes(typeof(ServedClassNameAttribute), true).FirstOrDefault() is ServedClassNameAttribute attr)
            {
                descr = attr.DisplayName;
            }
            else
            {
                descr = this.GetType().Assembly.FullName;
            }
            return descr;
        }

        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsOnline)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    //WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                    //Properties.Settings.Default.Save();
                    //SharedResources.UpdateTransactionProcessFactory();
                }
            }
        }

        //public void SetupDialog()
        //{
        //    SharedResources.DoSetupDialog(clientId);
        //}

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            LogMessage("", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        [MustBeConnected]
        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        [MustBeConnected]
        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            // string retString = CommandString(command, raw); // Send the command and wait for the response
            // bool retBool = XXXXXXXXXXXXX; // Parse the returned string and create a boolean True / False value
            // return retBool; // Return the boolean value to the client

            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        [MustBeConnected]
        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the trace logger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsOnline;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    Connect();
                    //connectedState = true;
                    LogMessage("Connected Set", "Connecting to port {0}", comPort);
                    // TODO connect to the device
                }
                else
                {
                    Disconnect();
                    //connectedState = false;
                    LogMessage("Connected Set", "Disconnecting from port {0}", comPort);
                    // TODO disconnect from the device
                }
            }
        }

        public string Description
        {
            // TODO customise this device description
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description
                string driverInfo = "Information about the driver itself. Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "Short driver name - please customise";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion Common properties and methods.

        #region ITelescope Implementation

        public void AbortSlew()
        {
            //tl.LogMessage("AbortSlew", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("AbortSlew");
            telescope.AbortSlew();
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                //tl.LogMessage("AlignmentMode Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("AlignmentMode", false);
                return AlignmentModes.algGermanPolar;
            }
        }

        public double Altitude
        {
            get
            {
                //tl.LogMessage("Altitude", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("Altitude", false);
                return telescope.Altitude;
            }
        }

        public double ApertureArea
        {
            get
            {
                tl.LogMessage("ApertureArea Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureArea", false);
            }
        }

        public double ApertureDiameter
        {
            get
            {
                tl.LogMessage("ApertureDiameter Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("ApertureDiameter", false);
            }
        }

        public bool AtHome
        {
            get
            {
                //tl.LogMessage("AtHome", "Get - " + false.ToString());
                //return false;
                return telescope.AtHome;
            }
        }

        public bool AtPark
        {
            get
            {
                tl.LogMessage("AtPark", "Get - " + false.ToString());
                return false;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            tl.LogMessage("AxisRates", "Get - " + Axis.ToString());
            return new AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                //tl.LogMessage("Azimuth Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("Azimuth", false);
                return telescope.Azimuth;
            }
        }

        public bool CanFindHome
        {
            get
            {
                tl.LogMessage("CanFindHome", "Get - " + true.ToString());
                return true;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            tl.LogMessage("CanMoveAxis", "Get - " + Axis.ToString());
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary: return true;
                case TelescopeAxes.axisSecondary: return true;
                case TelescopeAxes.axisTertiary: return false;
                default: throw new InvalidValueException("CanMoveAxis", Axis.ToString(), "0 to 2");
            }
        }

        public bool CanPark
        {
            get
            {
                tl.LogMessage("CanPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                tl.LogMessage("CanPulseGuide", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                tl.LogMessage("CanSetDeclinationRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                tl.LogMessage("CanSetGuideRates", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSetPark
        {
            get
            {
                tl.LogMessage("CanSetPark", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                tl.LogMessage("CanSetPierSide", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                tl.LogMessage("CanSetRightAscensionRate", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                tl.LogMessage("CanSetTracking", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSlew
        {
            get
            {
                tl.LogMessage("CanSlew", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                tl.LogMessage("CanSlewAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                tl.LogMessage("CanSlewAltAzAsync", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                tl.LogMessage("CanSlewAsync", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSync
        {
            get
            {
                tl.LogMessage("CanSync", "Get - " + false.ToString());
                return true;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                tl.LogMessage("CanSyncAltAz", "Get - " + false.ToString());
                return false;
            }
        }

        public bool CanUnpark
        {
            get
            {
                tl.LogMessage("CanUnpark", "Get - " + false.ToString());
                return false;
            }
        }

        public double Declination
        {
            get
            {
                //double declination = 0.0;
                //tl.LogMessage("Declination", "Get - " + utilities.DegreesToDMS(declination, ":", ":"));
                //return declination;
                return telescope.Declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                double declination = 0.0;
                tl.LogMessage("DeclinationRate", "Get - " + declination.ToString());
                return declination;
            }
            set
            {
                tl.LogMessage("DeclinationRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DeclinationRate", true);
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            tl.LogMessage("DestinationSideOfPier Get", "Not implemented");
            throw new ASCOM.PropertyNotImplementedException("DestinationSideOfPier", false);
        }

        public bool DoesRefraction
        {
            get
            {
                tl.LogMessage("DoesRefraction Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", false);
            }
            set
            {
                tl.LogMessage("DoesRefraction Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("DoesRefraction", true);
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equTopocentric;
                tl.LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            //tl.LogMessage("FindHome", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("FindHome");
            telescope.FindHome();
        }

        public double FocalLength
        {
            get
            {
                tl.LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                //tl.LogMessage("GuideRateDeclination Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
                return telescope.GuideRateDeclination;
            }
            set
            {
                double SiderealRateDPS = 0.004178; // degrees / second;
                //tl.LogMessage("GuideRateDeclination Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
                if (value != 0)
                {
                CheckRange(value, 0.2 * SiderealRateDPS, 1.00 * SiderealRateDPS, "GuideRateDeclination", "Rate");
                }
                telescope.GuideRateDeclination = value;
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                //tl.LogMessage("GuideRateRightAscension Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
                return telescope.GuideRateRightAscension;
            }
            set
            {
                double SiderealRateDPS = 0.004178; // degrees / second;
                //tl.LogMessage("GuideRateRightAscension Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
                if (value != 0)
                {
                CheckRange(value, 0.2 * SiderealRateDPS, 1.00 * SiderealRateDPS, "GuideRateDeclination", "Rate");
                }
                telescope.GuideRateDeclination = value;
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                //tl.LogMessage("IsPulseGuiding Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
                return telescope.IsPulseGuiding;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            //tl.LogMessage("MoveAxis", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("MoveAxis");
            if (!CanMoveAxis(Axis))
            {
                throw new MethodNotImplementedException("CanMoveAxis " + Enum.GetName(typeof(TelescopeAxes), Axis));
            }
            CheckRate(Axis, Rate);
            telescope.MoveAxis(Axis, Rate);
        }

        public void Park()
        {
            tl.LogMessage("Park", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Park");
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            //tl.LogMessage("PulseGuide", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("PulseGuide");
            CheckRange(Duration, 0, 32767, "PulseGuide", "Duration");
            telescope.PulseGuide(Direction, Duration);
        }

        public double RightAscension
        {
            get
            {
                //double rightAscension = 0.0;
                //tl.LogMessage("RightAscension", "Get - " + utilities.HoursToHMS(rightAscension));
                //return rightAscension;
                return telescope.RightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                double rightAscensionRate = 0.0;
                tl.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                tl.LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            tl.LogMessage("SetPark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SetPark");
        }

        public PierSide SideOfPier
        {
            get
            {
                //tl.LogMessage("SideOfPier Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
                return telescope.SideOfPier;
            }
            set
            {
                tl.LogMessage("SideOfPier Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
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

                tl.LogMessage("SiderealTime", "Get - " + siderealTime.ToString());
                return siderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                tl.LogMessage("SiteElevation Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", false);
            }
            set
            {
                tl.LogMessage("SiteElevation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }

        public double SiteLatitude
        {
            get
            {
                //tl.LogMessage("SiteLatitude Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("SiteLatitude", false);
                return telescope.SiteLatitude;
            }
            set
            {
                tl.LogMessage("SiteLatitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", true);
            }
        }

        public double SiteLongitude
        {
            get
            {
                //tl.LogMessage("SiteLongitude Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("SiteLongitude", false);
                return telescope.SiteLongitude;
            }
            set
            {
                tl.LogMessage("SiteLongitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", true);
            }
        }

        public short SlewSettleTime
        {
            get
            {
                tl.LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                tl.LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            tl.LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            //tl.LogMessage("SlewToCoordinates", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SlewToCoordinates");
            CheckRange(RightAscension, 0, 24, "SlewToCoordinates", "RightAscension");
            CheckRange(Declination, -90, 90, "SlewToCoordinates", "Declination");
            //CheckParked("SlewToCoordinates");
            //CheckTracking(true, "SlewToCoordinates");
            _TargetRightAscension = RightAscension; // Set the Target RA and Dec prior to the Slew attempt per the ASCOM Telescope specification
            _TargetDeclination = Declination;
            telescope.SlewToCoordinates(RightAscension, Declination);
            // Block until the slew completes
            while (telescope.Slewing)
            {
                Thread.Sleep(1000);// Allow time for main timer loop to update the axis state
            }
            // Refine the slew
            telescope.SlewToCoordinates(RightAscension, Declination);
            //Block until the slew completes
            while (telescope.Slewing)
            {
                Thread.Sleep(1000);  // Allow time for main timer loop to update the axis state
            }
            Thread.Sleep(1000); // Allow the telescope positions to update
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            //tl.LogMessage("SlewToCoordinatesAsync", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SlewToCoordinatesAsync");
            CheckRange(RightAscension, 0, 24, "SlewToCoordinates", "RightAscension");
            CheckRange(Declination, -90, 90, "SlewToCoordinates", "Declination");
            //CheckParked("SlewToCoordinates");
            //CheckTracking(true, "SlewToCoordinates");
            _TargetRightAscension = RightAscension; // Set the Target RA and Dec prior to the Slew attempt per the ASCOM Telescope specification
            _TargetDeclination = Declination;
            telescope.SlewToCoordinatesAsync(RightAscension, Declination);
            // return immediately
        }

        public void SlewToTarget()
        {
            //tl.LogMessage("SlewToTarget", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SlewToTarget");
            CheckRange(telescope.TargetRightAscension, 0, 24, "SlewToTarget", "TargetRightAscension");
            CheckRange(telescope.TargetDeclination, -90, 90, "SlewToTarget", "TargetDeclination"); ;
            //CheckParked("SlewToTarget");
            //CheckTracking(true, "SlewToTarget");
            telescope.TargetRightAscension = TargetRightAscension;
            telescope.TargetDeclination = TargetDeclination;
            telescope.SlewToTarget();
            // Block until the slew completes
            while (telescope.Slewing)
            {
                Thread.Sleep(1000);// Allow time for main timer loop to update the axis state
            }
            // Refine the slew
            telescope.SlewToTarget();
            //Block until the slew completes
            while (telescope.Slewing)
            {
                Thread.Sleep(1000);  // Allow time for main timer loop to update the axis state
            }
            Thread.Sleep(1000); // Allow the telescope positions to update
        }

        public void SlewToTargetAsync()
        {
            //tl.LogMessage("SlewToTargetAsync", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SlewToTargetAsync");
            CheckRange(telescope.TargetRightAscension, 0, 24, "SlewToTargetAsync", "TargetRightAscension");
            CheckRange(telescope.TargetDeclination, -90, 90, "SlewToTargetAsync", "TargetDeclination");
            telescope.TargetRightAscension = TargetRightAscension;
            telescope.TargetDeclination = TargetDeclination;
            telescope.SlewToTargetAsync();
        }

        public bool Slewing
        {
            get
            {
                //tl.LogMessage("Slewing Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("Slewing", false);
                return telescope.Slewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            tl.LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            //tl.LogMessage("SyncToCoordinates", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SyncToCoordinates");
            CheckRange(RightAscension, 0, 24, "SyncToCoordinates", "RightAscension");
            CheckRange(Declination, -90, 90, "SyncToCoordinates", "Declination");
            //CheckParked("SyncToCoordinates");
            //CheckTracking(true, "SyncToCoordinates");
            _TargetRightAscension = RightAscension;
            _TargetDeclination = Declination;
            telescope.SyncToCoordinates(RightAscension, Declination);
        }

        public void SyncToTarget()
        {
            //tl.LogMessage("SyncToTarget", "Not implemented");
            //throw new ASCOM.MethodNotImplementedException("SyncToTarget");
            telescope.SyncToTarget();
        }

        private double? _TargetDeclination;

        public double TargetDeclination
        {
            get
            {
                //tl.LogMessage("TargetDeclination Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TargetDeclination", false);
                if (!_TargetDeclination.HasValue)
                {
                    throw new ASCOM.InvalidOperationException("Target declination has not been set.");
                }
                _TargetDeclination = telescope.TargetDeclination;
                return _TargetDeclination.Value;
            }
            set
            {
                //tl.LogMessage("TargetDeclination Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TargetDeclination", true);
                CheckRange(value, -90, 90, "TargetDeclination");
                _TargetDeclination = value;
                telescope.TargetDeclination = value;
            }
        }

        private double? _TargetRightAscension;

        public double TargetRightAscension
        {
            get
            {
                //tl.LogMessage("TargetRightAscension Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", false);
                if (!_TargetRightAscension.HasValue)
                {
                    throw new ASCOM.InvalidOperationException("Target right ascention has not been set.");
                }
                //_TargetRightAscension = telescope.TargetRightAscension;
                return _TargetRightAscension.Value;
            }
            set
            {
                //tl.LogMessage("TargetRightAscension Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", true);
                CheckRange(value, 0, 24, "TargetRightAscension");
                _TargetRightAscension = value;
                telescope.TargetRightAscension = value;
            }
        }

        public bool Tracking
        {
            get
            {
                //bool tracking = true;
                //tl.LogMessage("Tracking", "Get - " + tracking.ToString());
                //return tracking;
                return telescope.Tracking;
            }
            set
            {
                //tl.LogMessage("Tracking Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("Tracking", true);
                telescope.Tracking = value;
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                //tl.LogMessage("TrackingRate Get", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TrackingRate", false);
                return telescope.TrackingRate;
            }
            set
            {
                //tl.LogMessage("TrackingRate Set", "Not implemented");
                //throw new ASCOM.PropertyNotImplementedException("TrackingRate", true);
                //if ((value < DriveRates.driveSidereal) || (value > DriveRates.driveKing))
                if ((value != DriveRates.driveSidereal))
                {
                    //throw new InvalidValueException("TrackingRate", value.ToString(), "0 (driveSidereal) to 3 (driveKing)");
                    throw new InvalidValueException("TrackingRate", value.ToString(), "0 (driveSidereal)");
                }
                telescope.TrackingRate = value;
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                ITrackingRates trackingRates = new TrackingRates();
                tl.LogMessage("TrackingRates", "Get - ");
                foreach (DriveRates driveRate in trackingRates)
                {
                    tl.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                DateTime utcDate = DateTime.UtcNow;
                tl.LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                tl.LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            tl.LogMessage("Unpark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Unpark");
        }

        #endregion ITelescope Implementation

        #region Private properties and methods

        // here are some useful properties and methods that can be used as required
        // to help with driver development

        //#region ASCOM Registration

        //// Register or unregister driver for ASCOM. This is harmless if already
        //// registered or unregistered.
        ////
        ///// <summary>
        ///// Register or unregister the driver with the ASCOM Platform.
        ///// This is harmless if the driver is already registered/unregistered.
        ///// </summary>
        ///// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        //private static void RegUnregASCOM(bool bRegister)
        //{
        //    using (var P = new ASCOM.Utilities.Profile())
        //    {
        //        P.DeviceType = "Telescope";
        //        if (bRegister)
        //        {
        //            P.Register(driverID, driverDescription);
        //        }
        //        else
        //        {
        //            P.Unregister(driverID);
        //        }
        //    }
        //}

        ///// <summary>
        ///// This function registers the driver with the ASCOM Chooser and
        ///// is called automatically whenever this class is registered for COM Interop.
        ///// </summary>
        ///// <param name="t">Type of the class being registered, not used.</param>
        ///// <remarks>
        ///// This method typically runs in two distinct situations:
        ///// <list type="numbered">
        ///// <item>
        ///// In Visual Studio, when the project is successfully built.
        ///// For this to work correctly, the option <c>Register for COM Interop</c>
        ///// must be enabled in the project settings.
        ///// </item>
        ///// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        ///// </list>
        ///// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        ///// </remarks>
        //[ComRegisterFunction]
        //public static void RegisterASCOM(Type t)
        //{
        //    RegUnregASCOM(true);
        //}

        ///// <summary>
        ///// This function unregisters the driver from the ASCOM Chooser and
        ///// is called automatically whenever this class is unregistered from COM Interop.
        ///// </summary>
        ///// <param name="t">Type of the class being registered, not used.</param>
        ///// <remarks>
        ///// This method typically runs in two distinct situations:
        ///// <list type="numbered">
        ///// <item>
        ///// In Visual Studio, when the project is cleaned or prior to rebuilding.
        ///// For this to work correctly, the option <c>Register for COM Interop</c>
        ///// must be enabled in the project settings.
        ///// </item>
        ///// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        ///// </list>
        ///// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        ///// </remarks>
        //[ComUnregisterFunction]
        //public static void UnregisterASCOM(Type t)
        //{
        //    RegUnregASCOM(false);
        //}

        //#endregion

        private void CheckRate(TelescopeAxes axis, double rate)
        {
            IAxisRates rates = AxisRates(axis);
            string ratesStr = string.Empty;
            foreach (Rate item in rates)
            {
                if (Math.Abs(rate) >= item.Minimum && Math.Abs(rate) <= item.Maximum)
                {
                    return;
                }
                ratesStr = string.Format("{0}, {1} to {2}", ratesStr, item.Minimum, item.Maximum);
            }
            throw new InvalidValueException("MoveAxis", rate.ToString(CultureInfo.InvariantCulture), ratesStr);
        }

        private static void CheckRange(double value, double min, double max, string propertyOrMethod, string valueName)
        {
            if (double.IsNaN(value))
            {
                //SharedResources.TrafficEnd(string.Format(CultureInfo.CurrentCulture, "{0}:{1} value has not been set", propertyOrMethod, valueName));
                throw new ValueNotSetException(propertyOrMethod + ":" + valueName);
            }
            if (value < min || value > max)
            {
                //SharedResources.TrafficEnd(string.Format(CultureInfo.CurrentCulture, "{0}:{4} {1} out of range {2} to {3}", propertyOrMethod, value, min, max, valueName));
                throw new InvalidValueException(propertyOrMethod, value.ToString(CultureInfo.CurrentCulture), string.Format(CultureInfo.CurrentCulture, "{0}, {1} to {2}", valueName, min, max));
            }
        }

        private static void CheckRange(double value, double min, double max, string propertyOrMethod)
        {
            if (double.IsNaN(value))
            {
                //SharedResources.TrafficEnd(string.Format(CultureInfo.CurrentCulture, "{0} value has not been set", propertyOrMethod));
                throw new ValueNotSetException(propertyOrMethod);
            }
            if (value < min || value > max)
            {
                //SharedResources.TrafficEnd(string.Format(CultureInfo.CurrentCulture, "{0} {1} out of range {2} to {3}", propertyOrMethod, value, min, max));
                throw new InvalidValueException(propertyOrMethod, value.ToString(CultureInfo.CurrentCulture), string.Format(CultureInfo.CurrentCulture, "{0} to {1}", min, max));
            }
        }

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return IsOnline;
                //return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        ///     Connects to the device.
        /// </summary>
        /// <exception cref="ASCOM.DriverException">
        ///     Failed to connect. Open apparently succeeded but then the device reported that
        ///     is was offline.
        /// </exception>
        private void Connect()
        {
            //connectedState = true;
            telescope = SharedResources.ConnectionManager.GoOnline(clientId);
            if (!telescope.IsOnline)
            {
                log.Error("Connect failed - device reported offline");
                throw new DriverException(
                    "Failed to connect. Open apparently succeeded but then the device reported that is was offline.");
            }
            telescope.PerformOnConnectTasks();
        }

        /// <summary>
        ///     Disconnects from the device.
        /// </summary>
        private void Disconnect()
        {
            //connectedState = false;
            SharedResources.ConnectionManager.GoOffline(clientId);
            telescope = null; //[Sentinel]
        }

        protected virtual void Dispose(bool fromUserCode)
        {
            if (!disposed)
            {
                if (fromUserCode)
                {
                    SharedResources.ConnectionManager.UnregisterClient(clientId);
                }
            }
            disposed = true;
        }

        /// <summary>
        ///     Finalizes this instance (called prior to garbage collection by the CLR)
        /// </summary>
        ~Telescope()
        {
            Dispose(false);
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        //internal void ReadProfile()
        //{
        //    using (Profile driverProfile = new Profile())
        //    {
        //        driverProfile.DeviceType = "Telescope";
        //        tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
        //        comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
        //    }
        //}

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        //internal void WriteProfile()
        //{
        //    using (Profile driverProfile = new Profile())
        //    {
        //        driverProfile.DeviceType = "Telescope";
        //        driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
        //        driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
        //    }
        //}

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }

        #endregion Private properties and methods
    }
}