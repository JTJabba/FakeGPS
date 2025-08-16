namespace FakeGPS.Common
{
    using System;
    using System.Device.Location;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// Static class to help with Geolocation operations.
    /// </summary>
    public static class GeolocationHelper
    {
        // http://stackoverflow.com/questions/3518504/regular-expression-for-matching-latitude-longitude-coordinates
        private static Regex regex = new Regex(@"^([-+]?\d{1,2}([.]\d+)?),\s*([-+]?\d{1,3}([.]\d+)?)$");

        /// <summary>
        /// Checks to see if the location string is valid.
        /// </summary>
        /// <param name="latLong">The location string.</param>
        /// <returns>
        /// A value indicating whether the location string was valid.
        /// </returns>
        public static bool IsValid(string latLong)
        {
            // protect IsMatch from null
            if (string.IsNullOrWhiteSpace(latLong))
            {
                return false;
            }

            // will return false if invalid
            return regex.IsMatch(latLong);
        }

        /// <summary>
        /// Convert a location string to a <see cref="LatLong"/>.
        /// </summary>
        /// <param name="latLong">The location string.</param>
        /// <returns>
        /// The <see cref="LatLong"/> instance.
        /// </returns>
        public static LatLong ToLatLong(string latLong)
        {
            if (string.IsNullOrWhiteSpace(latLong))
            {
                throw new ArgumentException("LatLong string cannot be null or empty.", nameof(latLong));
            }

            if (!IsValid(latLong))
            {
                throw new ArgumentException($"Invalid LatLong format: '{latLong}'. Expected format: 'latitude,longitude' (e.g., '51.5074,-0.1278')", nameof(latLong));
            }

            try
            {
                // ok we've got a well formated latLong string
                var splits = latLong.Split(',');
                
                if (splits.Length != 2)
                {
                    throw new ArgumentException($"Invalid LatLong format: '{latLong}'. Expected exactly one comma separator.", nameof(latLong));
                }

                return new LatLong()
                {
                    Latitude = Convert.ToDouble(splits[0].Trim(), CultureInfo.InvariantCulture),
                    Longitude = Convert.ToDouble(splits[1].Trim(), CultureInfo.InvariantCulture)
                };
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"Could not parse LatLong values from: '{latLong}'. Ensure both latitude and longitude are valid numbers.", nameof(latLong), ex);
            }
            catch (OverflowException ex)
            {
                throw new ArgumentException($"LatLong values are too large: '{latLong}'. Values must be within valid double range.", nameof(latLong), ex);
            }
        }

        /// <summary>
        /// Get the current location from the Windows location API.
        /// </summary>
        /// <returns>
        /// The <see cref="LatLong"/> instance.
        /// </returns>
        /// <remarks>
        /// This currently includes Thread.Sleep hack to ensure the device is ready.
        /// </remarks>
        public static LatLong Get()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();

            watcher.TryStart(true, TimeSpan.FromMilliseconds(1000));

            // TODO: hack hack hack
            Thread.Sleep(1000);

            if (watcher.Position == null)
            {
                throw new InvalidOperationException("Could not get position from GeoCoordinateWatcher. The Position property is null. This may indicate that location services are disabled or no GPS device is available.");
            }

            GeoCoordinate coord = watcher.Position.Location;
            
            if (coord == null)
            {
                throw new InvalidOperationException("Could not get location from GeoCoordinateWatcher. The Location property is null. This may indicate that location services are disabled or no GPS device is available.");
            }

            if (coord.IsUnknown)
            {
                throw new InvalidOperationException("Location is unknown. This may indicate that location services are disabled, no GPS device is available, or the location could not be determined.");
            }

            return new LatLong()
            {
                Latitude = coord.Latitude,
                Longitude = coord.Longitude
            };
        }
    }
}