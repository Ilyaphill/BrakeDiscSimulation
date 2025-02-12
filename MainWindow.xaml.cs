using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrakeDiscSimulation
{
    public partial class MainWindow : Window
    {
        // ToDo сделать wait опциональным, добавить это в xaml
        // ToDo добавить стоп
        // ToDo нужно сделать так, чтобы была строчка выбора графика вместо всех -> подравить его, чтобы график всегда был именно по элементу 
        // ToDo Implizit and Explizit => сделать + должны быть выборычными

        List<double> temperature;
        List<double> totalenergy;
        List<double> breakingDistance;
        List<double> dt_time_List;

        private Calculations calculations;
        public MainWindow()
        {
            InitializeComponent();
            calculations = new Calculations();


            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
            DecelerationSlider.ValueChanged += DecelerationSlider_ValueChanged;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpeedValue.Text = ((int)SpeedSlider.Value).ToString(); 
        }

        private void DecelerationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DecelerationValue.Text = ((int)DecelerationSlider.Value).ToString();
        }

        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            bool correctValues = ValidateTextBoxInput(dt_TextBox, Time_TextBox);

            if (!correctValues)
            {
                PlotResult.Plot.Clear();

                List<double> xValues = new List<double>();
                List<double> yValues = new List<double>();

                double.TryParse(dt_TextBox.Text, out var dt);
                double.TryParse(Time_TextBox.Text, out var time);
                double speed = SpeedSlider.Value; 
                double deceleration = DecelerationSlider.Value; 

                var result = calculations.CalculateHeating_ReturnEverything(dt, time, speed, deceleration); 
                temperature = result.Item1;
                totalenergy = result.Item2;
                breakingDistance = result.Item3;
                dt_time_List = result.Item4;

                for (int i = 0; i < dt_time_List.Count; i++)
                {
                    double wait = 1000 * dt;
                    int waitInt = (int)Math.Round(wait); 
                    await Task.Delay(waitInt);
                    TemperatureValue.Text = $"Temperature Difference: {temperature[i]:F1} °C";
                    UpdateDiscColor(temperature[i]);

                    if (ComboBox.SelectedItem is ComboBoxItem selecteditem)
                    {
                        string content = selecteditem.Content.ToString();

                        switch (content)
                        {
                            case "Temperature":
                                yValues.Add(temperature[i]);

                                PlotResult.Plot.Title("Temperature over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Temperature (°C)";
                                break;

                            case "Energy":
                                yValues.Add(totalenergy[i]);

                                PlotResult.Plot.Title("Energy over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Energy used (kJ)";
                                break;

                            case "Distance":
                                yValues.Add(breakingDistance[i]);

                                PlotResult.Plot.Title("Distance over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Distance (m)";
                                break;

                            default:
                                yValues.Add(temperature[i]);

                                PlotResult.Plot.Title("Temperature over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Temperature (°C)";
                                break;
                        }
                    }
                    xValues.Add(dt_time_List[i]);

                    PlotResult.Plot.Clear();
                    PlotResult.Plot.Add.Scatter(xValues.ToArray(), yValues.ToArray());
                    PlotResult.Plot.Axes.AutoScale();
                    PlotResult.Plot.Axes.Bottom.Label.Text = "Time (s)";
                    PlotResult.Plot.Legend.IsVisible = true;
                    PlotResult.Refresh();

                }

            }

        }

        private bool ValidateTextBoxInput(TextBox textBox_dt, TextBox textBox_time)
        {
            double.TryParse(textBox_dt.Text, out var dt);
            double.TryParse(textBox_time.Text, out var time);

            if (string.IsNullOrWhiteSpace(textBox_time.Text) || !double.TryParse(textBox_time.Text, out _))
            {
                textBox_time.BorderBrush = Brushes.Red;
                textBox_time.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_time.Text = "Enter a value!";
                textBox_time.Foreground = Brushes.Gray; 
                textBox_time.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (string.IsNullOrWhiteSpace(textBox_dt.Text) || !double.TryParse(textBox_dt.Text, out _))
            {
                textBox_dt.BorderBrush = Brushes.Red;
                textBox_dt.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_dt.Text = "Enter a value!";
                textBox_dt.Foreground = Brushes.Gray; 
                textBox_dt.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (dt > time)
            {
                textBox_dt.BorderBrush = Brushes.Red;
                textBox_dt.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_dt.Text = "dt must be less or equal to time!";
                textBox_dt.Foreground = Brushes.Gray; 
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

        private void UpdateDiscColor(double heating)
        {

            double[] tempStops = new double[]
            {
                0, 
                300,
                500,
                700,
                800,
                900,
                1000 
            };

            Color[] colorStops = new Color[]
            {
                Color.FromRgb(80, 80, 80),     // ~0°C (cold, dark gray)
                Color.FromRgb(128, 0, 0),      // ~300°C (dark red)
                Color.FromRgb(255, 0, 0),      // ~500°C (bright red)
                Color.FromRgb(255, 128, 0),    // ~700°C (mix of red and yellow)
                Color.FromRgb(255, 255, 0),    // ~800°C (yellow)
                Color.FromRgb(255, 255, 128),  // ~900°C (brigt yellow)
                Color.FromRgb(255, 255, 255)   // ~1000°C (white)
            };

            int n = tempStops.Length; 

            if (heating <= tempStops[0])
            {
                Disc.Fill = new SolidColorBrush(colorStops[0]);
                return;
            }
            if (heating >= tempStops[n - 1])
            {
                Disc.Fill = new SolidColorBrush(colorStops[n - 1]);
                return;
            }

            Color finalColor = colorStops[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                double tLow = tempStops[i];
                double tHigh = tempStops[i + 1];

                if (heating >= tLow && heating <= tHigh)
                {
                    double fraction = (heating - tLow) / (tHigh - tLow);

                    byte rLow = colorStops[i].R;
                    byte gLow = colorStops[i].G;
                    byte bLow = colorStops[i].B;

                    byte rHigh = colorStops[i + 1].R;
                    byte gHigh = colorStops[i + 1].G;
                    byte bHigh = colorStops[i + 1].B;

                    byte r = (byte)(rLow + (rHigh - rLow) * fraction);
                    byte g = (byte)(gLow + (gHigh - gLow) * fraction);
                    byte b = (byte)(bLow + (bHigh - bLow) * fraction);

                    finalColor = Color.FromRgb(r, g, b);
                    break; 
                }
            }

            Disc.Fill = new SolidColorBrush(finalColor);
        }
    }
}
