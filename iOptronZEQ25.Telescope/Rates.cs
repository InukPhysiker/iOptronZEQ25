using ASCOM.DeviceInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ASCOM.iOptronZEQ25.Server
{
    #region Rate class
    //
    // The Rate class implements IRate, and is used to hold values
    // for AxisRates. You do not need to change this class.
    //
    // The Guid attribute sets the CLSID for ASCOM.iOptronZEQ25.Rate
    // The ClassInterface/None attribute prevents an empty interface called
    // _Rate from being created and used as the [default] interface
    //
    [Guid("10e1f673-c9cb-48ba-be11-19bed02c552e")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class Rate : ASCOM.DeviceInterface.IRate
    {
        private double maximum = 0;
        private double minimum = 0;

        //
        // Default constructor - Internal prevents public creation
        // of instances. These are values for AxisRates.
        //
        internal Rate(double minimum, double maximum)
        {
            this.maximum = maximum;
            this.minimum = minimum;
        }

        #region Implementation of IRate

        public void Dispose()
        {
            // TODO Add any required object clean-up here
        }

        public double Maximum
        {
            get { return this.maximum; }
            set { this.maximum = value; }
        }

        public double Minimum
        {
            get { return this.minimum; }
            set { this.minimum = value; }
        }

        #endregion
    }
    #endregion

    #region AxisRates
    //
    // AxisRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The IAxisRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.iOptronZEQ25.AxisRates
    // The ClassInterface/None attribute prevents an empty interface called
    // _AxisRates from being created and used as the [default] interface
    //
    [Guid("d06607e7-1835-485a-bc2d-b8f548fa77a4")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class AxisRates : IAxisRates, IEnumerable
    {
        private TelescopeAxes axis;
        private readonly Rate[] rates;
        private const double SiderealRateDPS = 0.004178; // degrees / second;

        //
        // Constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal AxisRates(TelescopeAxes axis)
        {
            this.axis = axis;
            //
            // This collection must hold zero or more Rate objects describing the 
            // rates of motion ranges for the Telescope.MoveAxis() method
            // that are supported by your driver. It is OK to leave this 
            // array empty, indicating that MoveAxis() is not supported.
            //
            // Note that we are constructing a rate array for the axis passed
            // to the constructor. Thus we switch() below, and each case should 
            // initialize the array for the rate for the selected axis.
            //
            switch (axis)
            {
                case TelescopeAxes.axisPrimary:
                    // TODO Initialize this array with any Primary axis rates that your driver may provide
                    // Example: m_Rates = new Rate[] { new Rate(10.5, 30.2), new Rate(54.0, 43.6) }
                    // this.rates = new Rate[0];
                    this.rates = new Rate[10]
                        {
                        new Rate(SiderealRateDPS *    0, SiderealRateDPS *    0), //Rate 0
                        new Rate(SiderealRateDPS *    1, SiderealRateDPS *    1), //Rate 1
                        new Rate(SiderealRateDPS *    2, SiderealRateDPS *    2), //Rate 2
                        new Rate(SiderealRateDPS *    8, SiderealRateDPS *    8), //Rate 3
                        new Rate(SiderealRateDPS *   16, SiderealRateDPS *   16), //Rate 4
                        new Rate(SiderealRateDPS *   64, SiderealRateDPS *   64), //Rate 5
                        new Rate(SiderealRateDPS *  128, SiderealRateDPS *  128), //Rate 6
                        new Rate(SiderealRateDPS *  256, SiderealRateDPS *  256), //Rate 7
                        new Rate(SiderealRateDPS *  512, SiderealRateDPS *  512), //Rate 8
                        new Rate(SiderealRateDPS * 1024, SiderealRateDPS * 1024)  //Rate 9
                        };
                    break;
                case TelescopeAxes.axisSecondary:
                    // TODO Initialize this array with any Secondary axis rates that your driver may provide
                    // this.rates = new Rate[0];
                    // Secondary axis rates that ZEQ25 driver may provide
                    this.rates = new Rate[10]
                        {
                        new Rate(SiderealRateDPS *    0, SiderealRateDPS *    0), //Rate 0
                        new Rate(SiderealRateDPS *    1, SiderealRateDPS *    1), //Rate 1
                        new Rate(SiderealRateDPS *    2, SiderealRateDPS *    2), //Rate 2
                        new Rate(SiderealRateDPS *    8, SiderealRateDPS *    8), //Rate 3
                        new Rate(SiderealRateDPS *   16, SiderealRateDPS *   16), //Rate 4
                        new Rate(SiderealRateDPS *   64, SiderealRateDPS *   64), //Rate 5
                        new Rate(SiderealRateDPS *  128, SiderealRateDPS *  128), //Rate 6
                        new Rate(SiderealRateDPS *  256, SiderealRateDPS *  256), //Rate 7
                        new Rate(SiderealRateDPS *  512, SiderealRateDPS *  512), //Rate 8
                        new Rate(SiderealRateDPS * 1024, SiderealRateDPS * 1024)  //Rate 9
                        };
                    break;
                case TelescopeAxes.axisTertiary:
                    // TODO Initialize this array with any Tertiary axis rates that your driver may provide
                    this.rates = new Rate[0];
                    break;
            }
        }

        #region IAxisRates Members

        public int Count
        {
            get { return this.rates.Length; }
        }

        public void Dispose()
        {
            // TODO Add any required object clean-up here
        }

        public IEnumerator GetEnumerator()
        {
            return rates.GetEnumerator();
        }

        public IRate this[int index]
        {
            get { return this.rates[index - 1]; }	// 1-based
        }

        #endregion
    }
    #endregion

    #region TrackingRates
    //
    // TrackingRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The ITrackingRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.iOptronZEQ25.TrackingRates
    // The ClassInterface/None attribute prevents an empty interface called
    // _TrackingRates from being created and used as the [default] interface
    //
    // This class is implemented in this way so that applications based on .NET 3.5
    // will work with this .NET 4.0 object.  Changes to this have proved to be challenging
    // and it is strongly suggested that it isn't changed.
    //
    [Guid("32f81205-1d70-4a70-939a-57c87cc22376")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class TrackingRates : ITrackingRates, IEnumerable, IEnumerator
    {
        private readonly DriveRates[] trackingRates;

        // this is used to make the index thread safe
        private readonly ThreadLocal<int> pos = new ThreadLocal<int>(() => { return -1; });
        private static readonly object lockObj = new object();

        //
        // Default constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal TrackingRates()
        {
            //
            // This array must hold ONE or more DriveRates values, indicating
            // the tracking rates supported by your telescope. The one value
            // (tracking rate) that MUST be supported is driveSidereal!
            //
            this.trackingRates = new[] { DriveRates.driveSidereal };
            // TODO Initialize this array with any additional tracking rates that your driver may provide
            // this.trackingRates = new[] { DriveRates.driveSidereal, DriveRates.driveLunar, DriveRates.driveSolar, DriveRates.driveKing };
        }

        #region ITrackingRates Members

        public int Count
        {
            get { return this.trackingRates.Length; }
        }

        public IEnumerator GetEnumerator()
        {
            pos.Value = -1;
            return this as IEnumerator;
        }

        public void Dispose()
        {
            // TODO Add any required object clean-up here
        }

        public DriveRates this[int index]
        {
            get { return this.trackingRates[index - 1]; }   // 1-based
        }

        #endregion

        #region IEnumerable members

        public object Current
        {
            get
            {
                lock (lockObj)
                {
                    if (pos.Value < 0 || pos.Value >= trackingRates.Length)
                    {
                        throw new System.InvalidOperationException();
                    }
                    return trackingRates[pos.Value];
                }
            }
        }

        public bool MoveNext()
        {
            lock (lockObj)
            {
                if (++pos.Value >= trackingRates.Length)
                {
                    return false;
                }
                return true;
            }
        }

        public void Reset()
        {
            pos.Value = -1;
        }
        #endregion
    }
    #endregion
}
