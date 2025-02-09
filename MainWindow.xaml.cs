using System;
using System.Windows;
using System.Windows.Media;

namespace BrakeDiscSimulation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Привязываем обработчики событий к ползункам
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
            DecelerationSlider.ValueChanged += DecelerationSlider_ValueChanged;
        }

        // Обработчик изменения значения ползунка скорости
        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpeedValue.Text = ((int)SpeedSlider.Value).ToString(); // Обновление текстового значения
        }

        // Обработчик изменения значения ползунка замедления
        private void DecelerationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DecelerationValue.Text = ((int)DecelerationSlider.Value).ToString(); // Обновление текстового значения
        }

        // Обработчик нажатия на кнопку "Старт"
        private void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            double speed = SpeedSlider.Value; // Получаем значение начальной скорости
            double deceleration = DecelerationSlider.Value; // Получаем значение замедления

            double heating = CalculateHeating(speed, deceleration); // Рассчитываем нагрев
            UpdateDiscColor(heating); // Обновляем цвет диска в зависимости от нагрева
            TemperatureValue.Text = $"Температура: {heating:F1} °C"; // Отображаем температуру
        }

        // Функция расчета нагрева тормозного диска
        private double CalculateHeating(double speed, double deceleration)
        {
            // Параметры системы
            double m = 1000; // Масса автомобиля в кг
            double KPD = 0.7; // Коэффициент преобразования энергии в тепло
            double cmetal = 490; // Удельная теплоемкость металла тормозного диска
            double rho = 7800; // Плотность материала тормозного диска (сталь)
            double d = 0.3; // Диаметр тормозного диска в метрах
            double w = 0.05; // Ширина контактной зоны тормоза (м)
            double tlayer = 0.006; // Толщина слоя, который нагревается (м)

            // Расчет площади контактной зоны и массы нагреваемого слоя
            double Acontact = Math.PI * d * w; // Площадь контактной зоны (м²)
            double Vlayer = Acontact * tlayer; // Объем слоя, который нагревается (м³)
            double mlayer = rho * Vlayer; // Масса нагреваемого слоя одного тормозного диска (кг)
            double meff = 4 * mlayer; // Эффективная масса всех частей тормоза, участвующих в нагреве

            // Перевод скорости в м/с
            double speedInMps = speed / 3.6;

            // Энергия, преобразованная в тепло из-за торможения
            double deltaE = m / 2 * Math.Pow(speedInMps, 2); // Кинетическая энергия торможения

            // Расчет работы торможения: W = F * d
            // F = m * a
            // d = v^2 / (2 * a)
            double brakingDistance = Math.Pow(speedInMps, 2) / (2 * deceleration); // Расстояние торможения (м)
            double workDone = m * deceleration * brakingDistance; // Работа, выполненная тормозами (Дж)

            // Энергия, преобразованная в тепло (с учетом КПД)
            double totalEnergy = deltaE + workDone;

            // Расчет изменения температуры (в градусах Цельсия)
            double deltaT = KPD * totalEnergy / (meff * cmetal); // Температура диска после торможения

            return deltaT; // Возвращаем рассчитанный нагрев (изменение температуры)
        }

        // Функция обновления цвета тормозного диска в зависимости от температуры
        private void UpdateDiscColor(double heating)
        {
            // Градация цвета в зависимости от температуры: от серого к красному
            byte red = (byte)Math.Min(255, heating * 2); // Изменяем коэффициент для плавной градации
            byte green = (byte)Math.Max(0, 255 - red); // Уменьшаем зеленый цвет с увеличением температуры
            Disc.Fill = new SolidColorBrush(Color.FromRgb(red, green, 0)); // Устанавливаем цвет диска в зависимости от нагрева
        }
    }
}
