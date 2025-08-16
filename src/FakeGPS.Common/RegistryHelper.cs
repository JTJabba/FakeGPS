namespace FakeGPS.Common
{
    using System;
    using System.Diagnostics;
    using Microsoft.Win32;

    /// <summary>
    /// Static class to help with Registry operations.
    /// </summary>
    public static class RegistryHelper
    {
        // TODO: Fix hard coded to the first device instance.
        private const string Path = @"SYSTEM\CurrentControlSet\Enum\ROOT\UNKNOWN\0000\Device Parameters\FakeGPS";
        private const string LatitudeProperty = @"SENSOR_PROPERTY_LATITUDE";
        private const string LongitudeProperty = @"SENSOR_PROPERTY_LONGITUDE";

        /// <summary>
        /// Set the Latitude and Longitude to the Registry.
        /// </summary>
        /// <param name="latLong">The <see cref="LatLong"/>.</param>
        public static void SetLatLong(LatLong latLong)
        {
            if (latLong == null)
            {
                throw new ArgumentNullException(nameof(latLong));
            }

            try
            {
                var key = Registry.LocalMachine.OpenSubKey(Path, true);
                
                if (key == null)
                {
                    throw new InvalidOperationException($"Could not open registry key: HKLM\\{Path}. The registry key may not exist, or you may not have sufficient permissions to access it. Try running as Administrator.");
                }

                using (key)
                {
                    key.SetValue(LatitudeProperty, latLong.Latitude);
                    key.SetValue(LongitudeProperty, latLong.Longitude);
                }
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                // Add more context to the exception
                throw new InvalidOperationException($"Failed to set GPS coordinates in registry. Path: HKLM\\{Path}", ex);
            }
        }
    }
}