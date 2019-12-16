using System.Collections;
using System.Collections.Generic;

using System.Net;
using System.Management;

namespace SPH3
{
    /// <summary>
    /// A collection of .NET helper methods. 
    /// 
    /// !!!Should not contain any MonoBehavior!!!
    /// </summary> 
    public static class vUtilDotNet
    {

        #region HARDWARE
        /// <summary>
        /// Get device battery level
        /// </summary>
        public static float GetAggregateBatteryRemaining()
        {
            // Create aggregate battery object
            #if ENABLE_WINMD_SUPPORT
                    var aggBattery = Windows.Devices.Power.Battery.AggregateBattery;
 
                    // Get report
                    var report = aggBattery.GetReport();
                    float amount = ( (float)report.RemainingCapacityInMilliwattHours / (float)report.FullChargeCapacityInMilliwattHours ) * 100;
                    return amount ;

                    // Update UI
                    //AddReportUI(BatteryReportPanel, report, aggBattery.DeviceId);
            #else
                        return 0;
            #endif
        }

        /// <summary>
        /// Check for internet conneciton
        /// </summary>
        /// <returns></returns>
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region NUMBERS
        /// <summary>
        /// Enforcing ranges
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int ForceRange(int start, int end, int val)
        {
            if (val < start)
                return start;
            else if (val > end)
                return end;
            else return val;
        }
        public static float ForceRange(int start, int end, float val)
        {
            if (val < start)
                return start;
            else if (val > end)
                return end;
            else return val;
        }

        /// <summary>
        /// Add a prefix to a number string
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="val"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        public static string PrefixDigits(string prefix, string val, int digits)
        {
            while (val.Length < digits)
                val = prefix + val;

            return val;
        }
        public static string PrefixDigits(string prefix, int val, int digits)
        {
            return PrefixDigits(prefix, val.ToString(), digits);
        }
        #endregion


    }
}