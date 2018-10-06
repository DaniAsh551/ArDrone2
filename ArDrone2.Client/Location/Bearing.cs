using System;

namespace ArDrone2.Client.Location
{
    public struct Bearing
    {
        public enum BearingUnit
        {
            Degrees = 0,
            Radians = 1,
        }

        public Bearing(double value, BearingUnit unit)
        {
            Value = value;
            Unit = unit;
        }
        
        public double Value { get; set; }
        public BearingUnit Unit { get; set; }

        public void ConvertTo(BearingUnit unit)
        {
            if (unit == this.Unit)
                return;

            if (unit == BearingUnit.Degrees)
                this.Value = this.Value * 180 / Math.PI;
            else
                this.Value = this.Value * (Math.PI / 180);
        }
        
        public override string ToString()
            => $"{Value:N5} {Unit.ToString()}";

        public override bool Equals(object obj)
        {
            if (!(obj is Bearing bearing)) return false;
            
            if (bearing.Value.IsTolerable(this.Value, 0.00001) && bearing.Unit == this.Unit)
                return true;
                
            var dummyBearing = new Bearing(bearing.Value, bearing.Unit);
            dummyBearing.ConvertTo(this.Unit);

            return dummyBearing.Value.IsTolerable(this.Value, 0.00001);
        }
    }
}