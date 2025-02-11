using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrakeDiscSimulation
{
    public partial class MainWindow : Window
    {
        List<double> temperature;
        List<double> totalenergy;
        List<double> breakingDistance;
        List<double> dt_time_List;

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
            bool correctValues = ValidateTextBoxInput(dt_TextBox, Time_TextBox);

            if (!correctValues)
            {
                double.TryParse(dt_TextBox.Text, out var dt);
                double.TryParse(Time_TextBox.Text, out var time);
                double speed = SpeedSlider.Value; // Получаем значение начальной скорости
                double deceleration = DecelerationSlider.Value; // Получаем значение замедления

                var result = calculations.CalculateHeating_ReturnEverything(dt, time, speed, deceleration); // Рассчитываем нагрев
                temperature = result.Item1;
                totalenergy = result.Item2;
                breakingDistance = result.Item3;
                dt_time_List = result.Item4;
                UpdateDiscColor(200); // Обновляем цвет диска в зависимости от нагрева
                TemperatureValue.Text = $"Температура: {200:F1} °C"; // Отображаем температуру
            }

        }

        private bool ValidateTextBoxInput(TextBox textBox_dt, TextBox textBox_time)
        {
            double.TryParse(textBox_dt.Text, out var dt);
            double.TryParse(textBox_time.Text, out var time);

            if (string.IsNullOrWhiteSpace(textBox_time.Text) || !double.TryParse(textBox_time.Text, out _) || string.IsNullOrWhiteSpace(textBox_dt.Text) || !double.TryParse(textBox_dt.Text, out _))
            {
                textBox_time.BorderBrush = Brushes.Red;
                textBox_time.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_time.Text = "Enter a value!";
                textBox_time.Foreground = Brushes.Gray; // Серый текст
                textBox_time.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (dt > time)
            {
                textBox_dt.BorderBrush = Brushes.Red;
                textBox_dt.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_dt.Text = "dt must be less or equal to time!";
                textBox_dt.Foreground = Brushes.Gray; // Серый текст
                textBox_dt.FontStyle = FontStyles.Italic;
                return true;
            }
            else
            {
                textBox_time.BorderBrush = Brushes.Gray;
                textBox_time.Background = Brushes.White;
                textBox_time.Foreground = Brushes.Black;
                textBox_time.FontStyle = FontStyles.Normal;
                return false;
            }
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
