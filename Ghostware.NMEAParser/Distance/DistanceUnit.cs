using System;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ghostware.NMEAParser.Distance
{
    public struct DistanceUnit
    {
        public enum DistanceUnitName
        {
            Nanometers = 0,
            Micrometers = 1,
            Millimeters = 2,
            Centimeters = 3,
            Meters = 4,
            Kilometers = 5
        }
        
        private static Tuple<string,DistanceUnitName,double>[] _units = new Tuple<string, DistanceUnitName, double>[]
        {
            new Tuple<string, DistanceUnitName, double>("nm", DistanceUnitName.Nanometers, 1e-9d), 
            new Tuple<string, DistanceUnitName, double>("μm", DistanceUnitName.Micrometers, 1e-6d),
            new Tuple<string, DistanceUnitName, double>("mm", DistanceUnitName.Millimeters, 0.001d),
            new Tuple<string, DistanceUnitName, double>("cm", DistanceUnitName.Centimeters, 0.01d),
            new Tuple<string, DistanceUnitName, double>("m",  DistanceUnitName.Meters, 1.0d),
            new Tuple<string, DistanceUnitName, double>("km", DistanceUnitName.Kilometers, 1000.0d),
        };
        
        public DistanceUnit(double magnitude, string symbolOrName)
        {
            if(string.IsNullOrWhiteSpace(symbolOrName))
                throw new ArgumentException("symbolOrName cannot be empty or null.", nameof(symbolOrName));
            
            var isSymbol = symbolOrName.Length < 3;
            symbolOrName = isSymbol 
                ? symbolOrName.ToLower() 
                : char.ToUpper(symbolOrName[0]) + symbolOrName.Substring(1, symbolOrName.Length - 1);

            var unit = isSymbol
                ? _units.FirstOrDefault(x => x.Item1 == symbolOrName)
                : _units.FirstOrDefault(x => x.Item2.ToString() == symbolOrName);

            this._unit = unit ?? throw new ArgumentException("symbolOrName is invalid.", nameof(symbolOrName));
            Magnitude = magnitude;
        }

        public DistanceUnit(double magnitude, double weight)
        {
            var unit = _units.FirstOrDefault(x => x.Item3 == weight);
            this._unit = unit ?? throw new ArgumentException("weight is invalid.", nameof(weight));
            Magnitude = magnitude;
        }

        public DistanceUnit(double magnitude, DistanceUnitName name)
        {
            var unit = _units.FirstOrDefault(x => x.Item2 == name);
            this._unit = unit ?? throw new ArgumentException("name is invalid.", nameof(name));
            Magnitude = magnitude;
        }
        
        public string Symbol => _unit.Item1;
        public DistanceUnitName Name => _unit.Item2;
        public double Weight => _unit.Item3;
        public double Magnitude { get; private set; }

        private Tuple<string, DistanceUnitName, double> _unit;


        #region Implicit Conversions

        public static implicit operator DistanceUnit(double magnitudeInMeters)
            => new DistanceUnit(magnitudeInMeters, DistanceUnitName.Meters);

        public static implicit operator double(DistanceUnit unit)
        {
            if (unit.Name == DistanceUnitName.Meters)
                return unit.Magnitude;
            
            unit.ConvertTo(DistanceUnitName.Meters);
            return unit.Magnitude;
        }

        public static implicit operator DistanceUnit(string magnitudeAndNameOrSymbol)
        {
            if(string.IsNullOrWhiteSpace(magnitudeAndNameOrSymbol) || !magnitudeAndNameOrSymbol.Any(x => char.IsDigit(x)) || !(magnitudeAndNameOrSymbol.Any(x => char.IsLetter(x)) || magnitudeAndNameOrSymbol.Any(x => x == 'μ')) || !magnitudeAndNameOrSymbol.All(x => char.IsDigit(x) || char.IsLetter(x) || x == 'μ') )
                throw new ArgumentException($"{nameof(magnitudeAndNameOrSymbol)} is invalid.", nameof(magnitudeAndNameOrSymbol));

            if (magnitudeAndNameOrSymbol.Contains(' '))
                magnitudeAndNameOrSymbol = magnitudeAndNameOrSymbol.Replace(" ", string.Empty);
            
            var lastMagIndex = magnitudeAndNameOrSymbol.LastIndexOfAny("1234567890".ToCharArray());
            var magnitudeString = magnitudeAndNameOrSymbol.Substring(0, lastMagIndex + 1);
            var nameOrSymbol = magnitudeString.Substring(lastMagIndex + 1, magnitudeString.Length - (lastMagIndex + 1));
            
            if(!double.TryParse(magnitudeString, out var magnitude))
                throw new ArgumentException($"{nameof(magnitudeAndNameOrSymbol)} is invalid.", nameof(magnitudeAndNameOrSymbol));
            
            return new DistanceUnit(magnitude, nameOrSymbol);
        }

        public static implicit operator string(DistanceUnit unit)
            => unit.ToString();
        
        #endregion
        
        
        
        public void ConvertTo(DistanceUnitName name)
        {
            var unit = _units.FirstOrDefault(x => x.Item2 == name) ?? throw new ArgumentException("name is invalid.", nameof(name));
            var currentWeight = Weight;
            var newWeight = unit.Item3;

            Magnitude = (currentWeight / newWeight) * Magnitude;
            _unit = unit;
        }
        
        public override string ToString()
            => $"{Magnitude:N5}{Symbol}";

        public override bool Equals(object obj)
        {
            if (!(obj is DistanceUnit unit))
                return false;

            if (unit.Name == Name && unit.Magnitude == Magnitude)
                return true;
            
            var dummyUnit = new DistanceUnit(unit.Magnitude, unit.Name);
            dummyUnit.ConvertTo(this.Name);

            return Magnitude == dummyUnit.Magnitude;
        }
    }
}