using System;
using System.Windows;
using System.Windows.Media;

namespace BrakeDiscSimulation
{
    public partial class MainWindow : Window
    {
        private Calculations calculations;
        public MainWindow()
        {
            InitializeComponent();
            calculations = new Calculations();

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

            calculations.CalculateHeating(speed, deceleration); // Рассчитываем нагрев
            UpdateDiscColor(200); // Обновляем цвет диска в зависимости от нагрева
            TemperatureValue.Text = $"Температура: {200:F1} °C"; // Отображаем температуру
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
