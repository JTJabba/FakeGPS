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
        private const string LatitudeProperty = @"SENSOR_PROPERTY_LATITUDE";
        private const string LongitudeProperty = @"SENSOR_PROPERTY_LONGITUDE";

        /// <summary>
        /// Gets the FakeGPS device registry path for debugging purposes.
        /// </summary>
        /// <returns>The registry path if found, null otherwise.</returns>
        public static string GetFakeGpsRegistryPath()
        {
            return FindFakeGpsRegistryPath();
        }
        
        /// <summary>
        /// Dynamically finds the FakeGPS device registry path by searching common locations.
        /// </summary>
        /// <returns>The registry path if found, null otherwise.</returns>
        private static string FindFakeGpsRegistryPath()
        {
            // Common device class names and instance IDs to search
            string[] deviceClasses = { "SENSOR", "UNKNOWN" };
            string[] instanceIds = { "0000", "0001", "0002", "0003" };
            
            foreach (var deviceClass in deviceClasses)
            {
                foreach (var instanceId in instanceIds)
                {
                    var testPath = $@"SYSTEM\CurrentControlSet\Enum\ROOT\{deviceClass}\{instanceId}\Device Parameters\FakeGPS";
                    
                    try
                    {
                        using (var key = Registry.LocalMachine.OpenSubKey(testPath, false))
                        {
                            if (key != null)
                            {
                                return testPath;
                            }
                        }
                    }
                    catch
                    {
                        // Continue searching if this path fails
                        continue;
                    }
                }
            }
            
            return null;
        }

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
                // Dynamically find the correct registry path
                var registryPath = FindFakeGpsRegistryPath();
                
                if (registryPath == null)
                {
                    throw new InvalidOperationException(
                        "Could not find FakeGPS device registry path. Searched common locations:\n" +
                        "- HKLM\\SYSTEM\\CurrentControlSet\\Enum\\ROOT\\SENSOR\\0000\\Device Parameters\\FakeGPS\n" +
                        "- HKLM\\SYSTEM\\CurrentControlSet\\Enum\\ROOT\\UNKNOWN\\0000\\Device Parameters\\FakeGPS\n" +
                        "- And other variations...\n\n" +
                        "Make sure the FakeGPS driver is installed, or try running as Administrator.");
                }

                var key = Registry.LocalMachine.OpenSubKey(registryPath, true);
                
                if (key == null)
                {
                    throw new InvalidOperationException($"Could not open registry key: HKLM\\{registryPath}. You may not have sufficient permissions to access it. Try running as Administrator.");
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
                throw new InvalidOperationException($"Failed to set GPS coordinates in registry.", ex);
            }
        }
    }
}