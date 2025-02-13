﻿namespace BrakeDiscSimulation
{
    internal class Calculations
    {
        private DiscParameter Disc;
        public Calculations()
        {
            InitParameters();
        }
        private void InitParameters()
        {
            Disc = new DiscParameter();
            
            Disc.COP = 0.85; // Coefficent of produktivity - shows, how much of the energy while braking goes into the disc
            Disc.Rho = 7800;
            Disc.D = 0.3;
            Disc.W = 0.05;
            Disc.Tlayer = 0.006;
            Disc.Acontact = Math.PI * Disc.D * Disc.W; 
            Disc.Vlayer = Disc.Acontact * Disc.Tlayer; // Volume of the disc's layer
            Disc.Mlayer = Disc.Rho * Disc.Vlayer; // Mass of this layer
            Disc.Meff = 4 * Disc.Mlayer; // Effective mass of discs, which participate in braking 

        }

        private Tuple<List<double>, List<double>> Calculate_deltaE_speed(double t, double dt, double v0, double a)
        {
            List<double> speed = new List<double>();
            List<double> energyLog = new List<double>();

            for (double currentTime = 0; currentTime <= t; currentTime += dt)
            {
                if (v0 > 0.0)
                {
                    double vNext = v0 - a * currentTime;
                    if (vNext < 0)
                    {
                        vNext = 0; 
                    }

                    double deltaE = 0.5 * Disc.M * (Math.Pow(v0, 2) - Math.Pow(vNext, 2));
                    energyLog.Add(deltaE);
                    speed.Add(vNext);
                }

            }

            return Tuple.Create(energyLog, speed);

        }
        private List<double> Calculate_Distance(double t, double dt, double v0, double a)
        {
            List<double> brakingDistance = new List<double>();
            double s0 = 0;
            for (double i = 0; i <= t; i += dt)
            {

                double s = 0.5 * a * Math.Pow(dt,2) + v0 * dt + s0;
                s0 = s;

                if (s >= Math.Pow(v0, 2) / (2 * a))
                {
                    brakingDistance.Add(Math.Pow(v0, 2) / (2 * a));
                }
                else
                {
                    brakingDistance.Add(s);
                }
            }

            return brakingDistance;
        }

        private List<double> Make_dt_List(double dt, double t)
        {
            List<double> dt_List = new List<double>();
            for (double i = 0; i <= t; i += dt)
            {
                dt_List.Add(i);
            }
            return dt_List;
        }

        public Tuple<List<double>, List<double>, List<double>, List<double>, List<double>> CalculateHeating_ReturnEverything(
        double dt, double t, double speed, double deceleration, double car_mass, double heat_capacity)
        {
            Disc.M = car_mass;
            Disc.Cmetal = heat_capacity; // thermal heat capacity

            var deltaE_speed = Calculate_deltaE_speed(t, dt, speed / 3.6, deceleration);
            List<double> deltaE = deltaE_speed.Item1;
            List<double> speed_perPeriods = deltaE_speed.Item2;
            List<double> brakingDistance = Calculate_Distance(t, dt, speed / 3.6, deceleration);
            List<double> dt_List = Make_dt_List(dt, t);

            List<double> temperatureChanges_implicit = new List<double>();
            List<double> temperatureChanges_analytical = new List<double>();

            List<double> totalEnergy = new List<double>();
            List<double> Q_in = new List<double>();

            double Tcurrent_implicit = 0; 
            double Tcurrent_analytical = 0;

            for (int i = 0; i < dt_List.Count; i++)
            {
                Q_in.Add(Disc.COP * deltaE[i]);
            }


            for (int i = 0; i < dt_List.Count; i++)
            {
                Tcurrent_analytical = (Q_in[i] / (Disc.Meff * Disc.Cmetal));



                if (i == 0)
                {
                    Tcurrent_implicit = Tcurrent_analytical;
                }
                else if (i < dt_List.Count - 1 && i != 0)
                {
                    Tcurrent_implicit = (Tcurrent_implicit + dt * (Q_in[i + 1] / (Disc.Meff * Disc.Cmetal))) / (0.9995 + dt); // very strongly smoothed
                }
                else
                {
                    Tcurrent_implicit = Tcurrent_analytical;
                }

                temperatureChanges_implicit.Add(Tcurrent_implicit);
                temperatureChanges_analytical.Add(Tcurrent_analytical);
                totalEnergy.Add(deltaE[i] / 1000); // in kJ
            }

            return Tuple.Create(temperatureChanges_implicit, temperatureChanges_analytical, totalEnergy, brakingDistance, dt_List);
        }
    }
}
