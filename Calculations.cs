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
            Disc.Cmetal = 490;
            Disc.Rho = 7800;
            Disc.D = 0.3;
            Disc.W = 0.05;
            Disc.Tlayer = 0.006;
            Disc.Acontact = Math.PI * Disc.D * Disc.W; // Площадь контактной зоны (м²)
            Disc.Vlayer = Disc.Acontact * Disc.Tlayer; // Объем слоя, который нагревается (м³)
            Disc.Mlayer = Disc.Rho * Disc.Vlayer; // Масса нагреваемого слоя одного тормозного диска (кг)
            Disc.Meff = 4 * Disc.Mlayer; // Эффективная масса всех частей тормоза, участвующих в нагреве

        }

        private List<double> Calculate_deltaE(double t, double dt, double v0, double a)
        {
            List<double> List_deltaE = new List<double>();

            for (double i = 0; i <= t; i += dt)
            {
                double v = v0 - a * i;
                if (v == 0)
                {

                }
                double deltaE = Disc.M / 2 * (Math.Pow(v0, 2) - Math.Pow(v, 2));
                List_deltaE.Add(deltaE);
            }

            return List_deltaE;

        }
        private List<double> Calculate_Distance(double t, double dt, double v0, double a)
        {
            List<double> brakingDistance = new List<double>();
            for (double i = 0; i <= t; i += dt)
            {

                double s = v0 * i - a * i;

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
            List<double> deltaE = Calculate_deltaE(t, dt, speed / 3.6, deceleration);
            List<double> breakingDistance = Calculate_Distance(t, dt, speed / 3.6, deceleration);
            List<double> dt_List = Make_dt_List(dt, t);

            List<double> temperatureChanges = new List<double>();
            List<double> totalEnergy = new List<double>();
            List<double> time_dt = new List<double>();

            //double workDone = Disc.M * deceleration * breakingDistance[breakingDistance.Count - 1] ; // Работа, выполненная тормозами (Дж)
            for (int i = 0; i < dt_List.Count; i++)
            {
                
                double workDone = Disc.M * deceleration * breakingDistance[i]; // Работа, выполненная тормозами (Дж)
                double Energy_OnePeriod = deltaE[i] + workDone; // Энергия, преобразованная в тепло (с учетом КПД)
                double deltaT = Disc.KPD * Energy_OnePeriod / (Disc.Meff * Disc.Cmetal); // Температура диска после торможения

                totalEnergy.Add(Energy_OnePeriod);
                temperatureChanges.Add(deltaT);
            }
            // Энергия, преобразованная в тепло (с учетом КПД)
            //double totalEnergy = deltaE[deltaE.Count - 1] + workDone;
            return Tuple.Create(temperatureChanges, totalEnergy, breakingDistance, dt_List);
        }
    }
}
