using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrakeDiscSimulation
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
                brakingDistance.Add(s);
            }

            return brakingDistance;
        }

        // Функция расчета нагрева тормозного диска
        public double CalculateHeating(double speed, double deceleration)
        {
            List<double> deltaE = Calculate_deltaE(3.5, 0.1, speed / 3.6, deceleration);
            List<double> breakingDistance = Calculate_Distance(3.5, 0.1, speed / 3.6, deceleration);

            double workDone = Disc.M * deceleration * breakingDistance[breakingDistance.Count - 1] ; // Работа, выполненная тормозами (Дж)
             
            // Энергия, преобразованная в тепло (с учетом КПД)
            double totalEnergy = deltaE[deltaE.Count - 1] + workDone;

            // Расчет изменения температуры (в градусах Цельсия)
            double deltaT = Disc.KPD * totalEnergy / (Disc.Meff * Disc.Cmetal); // Температура диска после торможения

            return deltaT; // Возвращаем рассчитанный нагрев (изменение температуры)
        }
    }
}
