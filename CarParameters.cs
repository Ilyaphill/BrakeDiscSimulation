using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrakeDiscSimulation
{
    internal class Car
    {
        public double Mass { get; set; } 
        public BrakeDisc BrakeDisc { get; set; }
    }

    internal class BrakeDisc
    {
        public double COP { get; set; } // Coefficient of produktivity
        public double Diameter { get; set; } 
        public double Width { get; set; }
        public double Thickness { get; set; } 
        public double Density { get; set; } 
        public double padHardness { get; set; }
        public double HeatCapacity { get; set; }
        public double WearCoefficient { get; set; } 
        public double ContactArea => Math.PI * Diameter * Width;
        public double HeatedVolume => ContactArea * Thickness; 
        public double HeatedMass => Density * HeatedVolume; 
        public double EffectiveMass => 4 * HeatedMass; 

        public BrakeDisc(double diameter, double width, double thickness, double density, double heatCapacity, double wearCoefficient, double cop, double hardness)
        {
            Diameter = diameter;
            Width = width;
            Thickness = thickness;
            Density = density;
            HeatCapacity = heatCapacity;
            WearCoefficient = wearCoefficient;
            COP = cop;
            padHardness = hardness;
        }

    }
}
