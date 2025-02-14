namespace BrakeDiscSimulation
{
    internal class Calculations
    {
        private Car Car_;

        private void InitParameters(string material)
        {   
            Car_ = new Car();

            if (material == "Keramik")
            {
                Car_.BrakeDisc = new BrakeDisc(0.3, 0.03, 0.014, 2800, 1100, 10e-6, 0.92, 3.2e9);
            }
            else if (material == "Gray Cast Iron")
            {
                Car_.BrakeDisc = new BrakeDisc(0.3, 0.03, 0.01, 7800, 460, 10e-5, 0.85, 1.8e9);
            }

        }


        private List<double> CalculatePadWear(double t, double dt, double initialPadThickness, List<double> energy, List<double> brakingDistance, List<double> temperature)
        {
            List<double> padWearList = new List<double>();
            double currentThickness = initialPadThickness;

            double wearCoefficient = Car_.BrakeDisc.WearCoefficient;
            double cop = Car_.BrakeDisc.COP;
            double padHardness = Car_.BrakeDisc.padHardness * 0.7;

            for (int i = 0; i < energy.Count; i++)
            {
                if (i > 0 && energy[i] == energy[i - 1])
                {
                    padWearList.Add(padWearList[i - 1]);
                    continue;
                }
                if (currentThickness <= 0)
                {
                    padWearList.Add(0);
                    continue;
                }

                double Q_heat = energy[i];


                double d = (i == 0) ? brakingDistance[i] : brakingDistance[i] - brakingDistance[i - 1];
                if (d <= 0) d = dt; 


                double F_brake = Q_heat / (cop * d);

                double wearRate = wearCoefficient * (F_brake * d) / padHardness;

                currentThickness -= wearRate;
                double deltaThickness = initialPadThickness - currentThickness;
                if (currentThickness < 0) currentThickness = 0;

                padWearList.Add(deltaThickness);
            }

            return padWearList;
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

                    double deltaE = 0.5 * Car_.Mass * (Math.Pow(v0, 2) - Math.Pow(vNext, 2));
                    energyLog.Add(deltaE);
                    speed.Add(vNext);
                }

            }

            return Tuple.Create(energyLog, speed);

        }

        private List<double> Calculate_Distance(double t, double dt, double v0, double a)
        {
            List<double> brakingDistance = new List<double>();
            double s_last = 0;

            for (double i = 0; i <= t; i += dt)
            {
                double v = v0 - a * i;
                if (v <= 0) 
                {
                    brakingDistance.Add(s_last);
                    continue; 
                }

                double s = v0 * i - 0.5 * a * Math.Pow(i, 2); 
                s_last = s; 
                brakingDistance.Add(s);
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

        public Tuple<List<double>, List<double>, List<double>, List<double>, List<double>, List<double>, List<double>> CalculateHeating_ReturnEverything(
        double dt, double t, double speed, double deceleration, double car_mass, string material)
        {
            InitParameters(material);
            Car_.Mass = car_mass;

            var deltaE_speed = Calculate_deltaE_speed(t, dt, speed / 3.6, deceleration);
            List<double> deltaE = deltaE_speed.Item1;
            List<double> speed_every_Period = deltaE_speed.Item2;
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
                Q_in.Add(Car_.BrakeDisc.COP * deltaE[i]);
            }


            for (int i = 0; i < dt_List.Count; i++)
            {
                Tcurrent_analytical = Q_in[i] / (Car_.BrakeDisc.EffectiveMass * Car_.BrakeDisc.HeatCapacity);


                if (i == 0)
                {
                    Tcurrent_implicit = Tcurrent_analytical;
                }
                else if (i < dt_List.Count - 1 && i != 0)
                {
                    Tcurrent_implicit = (Tcurrent_implicit + dt * (Q_in[i + 1] / (Car_.BrakeDisc.EffectiveMass * Car_.BrakeDisc.HeatCapacity))) / (0.9995 + dt); // very strongly smoothed
                }

                temperatureChanges_implicit.Add(Tcurrent_implicit);
                temperatureChanges_analytical.Add(Tcurrent_analytical);
                totalEnergy.Add(deltaE[i] / 1000); // in kJ
            }
            List<double> padWear = CalculatePadWear(t, dt, Car_.BrakeDisc.Thickness, deltaE, brakingDistance, temperatureChanges_analytical);
            return Tuple.Create(temperatureChanges_implicit, temperatureChanges_analytical, totalEnergy, brakingDistance, dt_List, speed_every_Period, padWear);
        }

    }
}
