using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrakeDiscSimulation
{
    internal class Calculations
    {
        public DiscParameter Disc;
        public Calculations()
        {
            InitParameters();
        }
        private void InitParameters()
        {
            Disc = new DiscParameter();
            // Параметры системы
            Disc.M = 1500;
            Disc.KPD = 0.7;
            Disc.Cmetal = 490; // thermal heat capacity
            Disc.Rho = 7800;
            Disc.D = 0.3;
            Disc.W = 0.05;
            Disc.Tlayer = 0.006;
            Disc.Acontact = Math.PI * Disc.D * Disc.W; // Площадь контактной зоны (м²)
            Disc.Vlayer = Disc.Acontact * Disc.Tlayer; // Объем слоя, который нагревается (м³)
            Disc.Mlayer = Disc.Rho * Disc.Vlayer; // Масса нагреваемого слоя одного тормозного диска (кг)
            Disc.Meff = 4 * Disc.Mlayer; // Эффективная масса всех частей тормоза, участвующих в нагреве

        }

        private Tuple<List<double>, List<double>> Calculate_deltaE_speed(double t, double dt, double v0, double a)
        {
            List<double> speed = new List<double>();
            List<double> energyLog = new List<double>();
            double totalEnergy = 0.0;

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
                    totalEnergy += deltaE;
                    energyLog.Add(totalEnergy);
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
        // Функция расчета нагрева тормозного диска
        public Tuple<List<double>, List<double>, List<double>, List<double>> CalculateHeating_ReturnEverything(double dt, double t, double speed, double deceleration)
        {
            var deltaE_speed = Calculate_deltaE_speed(t, dt, speed / 3.6, deceleration);
            List<double> deltaE = deltaE_speed.Item1;
            List<double> speed_perPeriods = deltaE_speed.Item2;
            List<double> breakingDistance = Calculate_Distance(t, dt, speed / 3.6, deceleration);
            List<double> dt_List = Make_dt_List(dt, t);

            List<double> temperatureChanges = new List<double>();
            List<double> totalEnergy = new List<double>();
            List<double> time_dt = new List<double>();

            for (int i = 0; i < dt_List.Count; i++)
            {
                
                double workDone = Disc.M * deceleration * speed_perPeriods[i]; // Работа, выполненная тормозами (Дж)
                double Energy_OnePeriod = deltaE[i] + workDone; 
                double deltaT = Disc.KPD * Energy_OnePeriod / (Disc.Meff * Disc.Cmetal); // Температура диска после торможения

                totalEnergy.Add(Energy_OnePeriod / 1000); // in kJ
                temperatureChanges.Add(deltaT);
            }
            // Энергия, преобразованная в тепло (с учетом КПД)
            //double totalEnergy = deltaE[deltaE.Count - 1] + workDone;
            return Tuple.Create(temperatureChanges, totalEnergy, breakingDistance, dt_List);
        }
    }
}
