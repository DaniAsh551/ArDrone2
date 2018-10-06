using System;
using System.Device.Location.GeoCoordinate;
using System.Linq;
using ArDrone2.Client.Location;
using Ghostware.NMEAParser.Distance;
using Ghostware.NMEAParser.NMEAMessages;
using Ghostware.NMEAParser.NMEAMessages.Base;

namespace ArDrone2.Client.Location
{
    public static class LocationExtensions
    {

        #region Bearing
        
        public static Bearing BearingTo(
            this GeoCoordinate coordinateOne, GeoCoordinate coordinateTwo)
        {
            var lon1 = coordinateOne.Longitude;
            var lon2 = coordinateTwo.Longitude;
            var lat1 = coordinateOne.Latitude;
            var lat2 = coordinateTwo.Latitude;
            
            var dLon = ToRad(lon2-lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2)/2+Math.PI/4)/Math.Tan(ToRad(lat1)/2+Math.PI/4));
            if (Math.Abs(dLon) > Math.PI) 
                dLon = dLon > 0 ? -(2*Math.PI-dLon) : (2*Math.PI+dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static Bearing BearingTo(this GeoCoordinate coordinate, double lon2, double lat2)
        {
            var lon1 = coordinate.Longitude;
            var lat1 = coordinate.Latitude;
            
            var dLon = ToRad(lon2-lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2)/2+Math.PI/4)/Math.Tan(ToRad(lat1)/2+Math.PI/4));
            if (Math.Abs(dLon) > Math.PI) 
                dLon = dLon > 0 ? -(2*Math.PI-dLon) : (2*Math.PI+dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        private static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private static Bearing ToBearing(double radians) 
        {  
            // convert radians to degrees (as bearing: 0...360)
            var degrees = (ToDegrees(radians) + 360) % 360;
            return new Bearing(degrees, Bearing.BearingUnit.Degrees);
        }
        
        #endregion

        #region Position

        public static GeoPosition<GeoCoordinate> GetPosition(this NmeaMessage[] nmeas, GeoPosition<GeoCoordinate> lastPosition = null)
        {
            var lastGga = nmeas.Where(x => x is GpggaMessage)?.Cast<GpggaMessage>()?.OrderBy(x => x.FixTime)
                ?.LastOrDefault();

            var lastRmc = nmeas.Where(x => x is GprmcMessage)?.Cast<GprmcMessage>()?.OrderBy(x => x.FixTime)
                ?.LastOrDefault();

            var lastGsa = nmeas.Where(x => x is GpgsaMessage)?.Cast<GpgsaMessage>()?.LastOrDefault();

            if (lastGga == null)
                return null;

            var speed = double.NaN;
            GeoCoordinate coordinate = null;
            
            if (lastRmc == null)
            {
                var ggaFixTime = new DateTime(lastGga.FixTime.Ticks, DateTimeKind.Utc);
                    speed = 
                        (lastPosition != null
                        ? GetSpeed(lastGga.Latitude, lastGga.Longitude, ggaFixTime, lastPosition.Location.Latitude,
                            lastPosition.Location.Longitude, lastPosition.Timestamp.UtcDateTime) : double.NaN);
                    
                    var bearing = lastPosition?.Location?.BearingTo(lastGga.Longitude, lastGga.Latitude).Value ?? double
                        .NaN;
                    
                    coordinate = new GeoCoordinate(lastGga.Latitude, lastGga.Longitude, lastGga.Altitude,
                        lastGga.Hdop, double.NaN, speed, bearing);
                    return new GeoPosition<GeoCoordinate>(ggaFixTime, coordinate);
            }
            
            var fixTime = new DateTime((lastGga.FixTime.Ticks + lastRmc.FixTime.Ticks) / 2);
            var latitude = lastGga.Latitude.Average(lastRmc.Latitude);
            var longitude = lastGga.Longitude.Average(lastRmc.Longitude);
            var altitude = lastGga.AltitudeInUnits; altitude.ConvertTo(DistanceUnit.DistanceUnitName.Meters);
            var course = lastRmc.Course;
            speed = lastRmc.Speed;
            var hdop = lastGsa?.Hdop ?? lastGga.Hdop;
            var vdop = lastGsa?.Vdop ?? double.NaN;
            
            coordinate = new GeoCoordinate(latitude, longitude, altitude, hdop, vdop, speed, course);
            
            return new GeoPosition<GeoCoordinate>(fixTime, coordinate);
        }

        private static double GetSpeed(double currentLatitude, double currentLongitude, DateTime currentFixTime,
            double previousLatitude, double previousLongitude, DateTime previousFixTime)
        {
            var currentCoordinate = new GeoCoordinate(currentLatitude, currentLongitude);
            var previousCoordinate = new GeoCoordinate(previousLatitude, previousLongitude);

            var distance = currentCoordinate.GetDistanceTo(previousCoordinate);
            var time = currentFixTime.Subtract(previousFixTime).TotalSeconds;

            var speed = distance / time;
            return speed;
        }
        
        #endregion

        #region Status

        public static void SetGpsState(this NmeaMessage[] nmeas, ref GpsState state)
        {
            if (!nmeas?.Any() ?? true)
                return;

            var lastGga = nmeas.Where(x => x is GpggaMessage)?.Cast<GpggaMessage>()?.OrderBy(x => x.FixTime)
                ?.LastOrDefault();
            
            var lastGsa = nmeas.Where(x => x is GpgsaMessage)?.Cast<GpgsaMessage>()
                ?.LastOrDefault();

            if (lastGga == null && lastGsa == null)
                return;

            state = state ?? new GpsState();
            state.SatelliteFix = lastGsa?.SatelliteFix ?? state.SatelliteFix;
            state.Hdop = lastGga?.Hdop ?? lastGsa?.Hdop ?? state.Hdop;
            state.Vdop = lastGsa?.Vdop ?? state.Vdop;
            state.Pnrs = lastGsa?.Pnrs ?? state.Pnrs;
            state.FixQuality = lastGga?.FixQuality ?? state.FixQuality;
            state.NumberOfSatellites = lastGga?.NumberOfSatellites ?? state.NumberOfSatellites;
        }
        #endregion
    }
}