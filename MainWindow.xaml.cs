using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BrakeDiscSimulation
{
    public partial class MainWindow : Window
    {
        // ToDo сделать wait опциональным, добавить это в xaml
        // ToDo добавить скорость

        List<double> temperature_analytical;
        List<double> totalenergy;
        List<double> breakingDistance;
        List<double> dt_time_List;
        List<double> temperature_implicit;

        private Calculations calculations;
        
        public MainWindow()
        {
            InitializeComponent();
            calculations = new Calculations();

            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
            DecelerationSlider.ValueChanged += DecelerationSlider_ValueChanged;
            ComboBox.SelectionChanged += SelectionChanged;
        }

        #region Sliders and list for diagramms

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SpeedValue.Text = ((int)SpeedSlider.Value).ToString(); 
        }

        private void DecelerationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DecelerationValue.Text = ((int)DecelerationSlider.Value).ToString();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlotSelectedData_AfterSimulation();
        }
        
        #endregion
        
        private async void StartSimulation_Click(object sender, RoutedEventArgs e)
        {
            bool correctValues = ValidateTextBoxInput(dt_TextBox, Time_TextBox, mass_car_TextBox, thermal_capacity_TextBox);

            if (!correctValues)
            {
                PlotResult.Plot.Clear();

                List<double> xValues = new List<double>();
                List<double> yValues = new List<double>();

                List<double> buffer_temperature_analytical = new List<double>();
                List<double> buffer_temperature_implicit = new List<double>();
                List<double> buffer_distance = new List<double>();
                List<double> buffer_energy = new List<double>();

                double.TryParse(dt_TextBox.Text, out var dt);
                double.TryParse(Time_TextBox.Text, out var time);
                double.TryParse(mass_car_TextBox.Text, out var mass_car);
                double.TryParse(thermal_capacity_TextBox.Text, out var thermal_capacity);

                double speed = SpeedSlider.Value; 
                double deceleration = DecelerationSlider.Value; 

                var result = calculations.CalculateHeating_ReturnEverything(dt, time, speed, deceleration, mass_car, thermal_capacity); 
                
                temperature_analytical = result.Item2;
                temperature_implicit = result.Item1;
                totalenergy = result.Item3;
                breakingDistance = result.Item4;
                dt_time_List = result.Item5;

                for (int i = 0; i < dt_time_List.Count; i++)
                {
                    double wait = 1000 * dt;
                    int waitInt = (int)Math.Round(wait); 
                    await Task.Delay(waitInt);

                    TemperatureValue.Text = $"Temperature Difference: {temperature_analytical[i]:F1} °C";
                    UpdateDiscColor(temperature_analytical[i]);

                    buffer_distance.Add(breakingDistance[i]);
                    buffer_energy.Add(totalenergy[i]);
                    buffer_temperature_analytical.Add(temperature_analytical[i]);
                    buffer_temperature_implicit.Add(temperature_implicit[i]);

                    if (ComboBox.SelectedItem is ComboBoxItem selecteditem)
                    {
                        string content = selecteditem.Content.ToString();

                        xValues.Add(dt_time_List[i]);

                        switch (content)
                        {
                            case "Temperature":
                                var line1 = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_temperature_analytical.ToArray());
                                line1.Color = ScottPlot.Colors.Blue;

                                var line2 = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_temperature_implicit.ToArray());
                                line2.Color = ScottPlot.Colors.Red;

                                PlotResult.Plot.Title("Temperature over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Temperature (°C)";
                                break;

                            case "Energy":
                                var energyLine = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_energy.ToArray());

                                PlotResult.Plot.Title("Energy over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Energy (kJ)";
                                break;

                            case "Distance":
                                var distanceLine = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_distance.ToArray());

                                PlotResult.Plot.Title("Distance over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Distance (m)";
                                break;

                            default:
                                var defaultLine1 = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_temperature_analytical.ToArray());
                                defaultLine1.Color = ScottPlot.Colors.Blue;

                                var defaultLine2 = PlotResult.Plot.Add.Scatter(xValues.ToArray(), buffer_temperature_implicit.ToArray());
                                defaultLine2.Color = ScottPlot.Colors.Red;

                                PlotResult.Plot.Title("Temperature over Time");
                                PlotResult.Plot.Axes.Left.Label.Text = "Temperature (°C)";
                                break;
                        }
                    }
                    PlotResult.Plot.Axes.Bottom.Label.Text = "Time (s)";
                    PlotResult.Plot.Axes.AutoScale();
                    PlotResult.Refresh();

                }

            }

        }

        private bool ValidateTextBoxInput(TextBox textBox_dt, TextBox textBox_time, TextBox textBox_car_mass, TextBox textBox_heat_capacity)
        {
            double.TryParse(textBox_dt.Text, out var dt);
            double.TryParse(textBox_time.Text, out var time);
            double.TryParse(textBox_car_mass.Text, out var mass);
            double.TryParse(textBox_heat_capacity.Text, out var heat);

            if (string.IsNullOrWhiteSpace(textBox_time.Text) || !double.TryParse(textBox_time.Text, out _))
            {
                textBox_time.BorderBrush = Brushes.Red;
                textBox_time.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_time.Text = "Enter a correct value!";
                textBox_time.Foreground = Brushes.Gray; 
                textBox_time.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (string.IsNullOrWhiteSpace(textBox_dt.Text) || !double.TryParse(textBox_dt.Text, out _))
            {
                textBox_dt.BorderBrush = Brushes.Red;
                textBox_dt.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_dt.Text = "Enter a correct value!";
                textBox_dt.Foreground = Brushes.Gray; 
                textBox_dt.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (string.IsNullOrWhiteSpace(textBox_car_mass.Text) || !double.TryParse(textBox_car_mass.Text, out _))
            {
                textBox_car_mass.BorderBrush = Brushes.Red;
                textBox_car_mass.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_car_mass.Text = "Enter a correct value!";
                textBox_car_mass.Foreground = Brushes.Gray;
                textBox_car_mass.FontStyle = FontStyles.Italic;
                return true;
            }
            else if (string.IsNullOrWhiteSpace(textBox_heat_capacity.Text) || !double.TryParse(textBox_heat_capacity.Text, out _))
            {
                textBox_heat_capacity.BorderBrush = Brushes.Red;
                textBox_heat_capacity.Background = new SolidColorBrush(Color.FromRgb(255, 230, 230));
                textBox_heat_capacity.Text = "Enter a correct value!";
                textBox_heat_capacity.Foreground = Brushes.Gray;
                textBox_heat_capacity.FontStyle = FontStyles.Italic;
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

        private void PlotSelectedData_AfterSimulation()
        {

            if (temperature_analytical == null || totalenergy == null || breakingDistance == null || dt_time_List == null)
                return;

            string selectedText = "Temperature";
            if (ComboBox.SelectedItem is ComboBoxItem selectedItem)
                selectedText = selectedItem.Content.ToString();

            double[] xArray = dt_time_List.ToArray();
            double[] yArray;

            PlotResult.Plot.Clear();
            
            switch (selectedText)
            {
                case "Energy":
                    yArray = totalenergy.ToArray();
                    PlotResult.Plot.Title("Energy over Time");
                    PlotResult.Plot.Axes.Left.Label.Text = "Energy (kJ)";
                    PlotResult.Plot.Add.Scatter(xArray, yArray);
                    break;

                case "Distance":
                    yArray = breakingDistance.ToArray();
                    PlotResult.Plot.Title("Distance over Time");
                    PlotResult.Plot.Axes.Left.Label.Text = "Distance (m)";
                    PlotResult.Plot.Add.Scatter(xArray, yArray);
                    break;

                default: // "Temperature"
                    yArray = temperature_analytical.ToArray();
                    PlotResult.Plot.Title("Temperature over Time");
                    PlotResult.Plot.Axes.Left.Label.Text = "Temperature (°C)";
                    
                    var line1 = PlotResult.Plot.Add.Scatter(xArray, temperature_analytical.ToArray());
                    line1.Color = ScottPlot.Colors.Blue;

                    var line2 = PlotResult.Plot.Add.Scatter(xArray, temperature_implicit.ToArray());
                    line2.Color = ScottPlot.Colors.Red;
                    break;
            }

            PlotResult.Plot.Axes.Bottom.Label.Text = "Time (s)";
            PlotResult.Plot.Axes.AutoScale();
            PlotResult.Refresh();
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
