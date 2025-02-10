using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrakeDiscSimulation
{
    internal class DiscParameter
    {
        public double M { get; set; } // Масса автомобиля (кг)
        public double KPD { get; set; } // Коэффициент преобразования энергии в тепло
        public double Cmetal { get; set; } // Удельная теплоемкость металла тормозного диска
        public double Rho { get; set; } // Плотность материала тормозного диска (сталь)
        public double D { get; set; } // Диаметр тормозного диска (м)
        public double W { get; set; } // Ширина контактной зоны тормоза (м)
        public double Tlayer { get; set; } // Толщина слоя, который нагревается (м)


        public double Acontact { get; set; } // Площадь контактной зоны (м²)
        public double Vlayer { get; set; } // Объем слоя, который нагревается (м³)
        public double Mlayer { get; set; } // Масса нагреваемого слоя одного тормозного диска (кг)
        public double Meff { get; set; } // Эффективная масса всех частей тормоза, участвующих в нагреве
    }
}
