using App1.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timerDateTime;
        DispatcherTimer timerWeather;
        DailyTemp dailyWeather;
        ForeCast foreCast;

        public MainPage()
        {
            this.InitializeComponent();
            FillControlls();
            //SetImageTick();
        }
        private async void FillControlls()
        {
            dailyWeather = await OpenWeather.GetDailyWeather(50.95, 5.97);
            foreCast = await OpenWeather.GetForeCastWeather(50.95, 5.97, 5);

            SetTimer();
            SetWeather();
            SetDateTime();
            SetForeCast();
        }
        private void SetTimer()
        {
            timerDateTime = new DispatcherTimer();
            timerDateTime.Tick += timerDateTime_Tick;
            timerDateTime.Interval = new TimeSpan(0, 0, 1);
            timerDateTime.Start();

            timerWeather = new DispatcherTimer();
            timerWeather.Tick += TimerWeather_Tick;
            timerWeather.Interval = new TimeSpan(1, 0, 0);
            timerWeather.Start();
        }

        private void TimerWeather_Tick(object sender, object e)
        {
            SetWeather();
            SetForeCast();
        }

        private void timerDateTime_Tick(object sender, object e)
        {
            SetDateTime();
        }
        private void SetWeather()
        {
            var description = dailyWeather.weather[0].description;

            weatherImage.Source = SetImage(dailyWeather.weather[0].main);
            lblTemp.Text = $"{dailyWeather.main.temp.ToString("0.0")}°";
            lblWeatherType.Text = char.ToUpper(description[0]) + description.Substring(1);
            lblWeatherCity.Text = dailyWeather.name;
        }
        private void SetDateTime()
        {
            lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            lblCalender.Text = DateTime.Now.ToString("dd-MMMM-yyyy");
        }
        private void SetForeCast()
        {
            lblForeCastDay1.Text = Enum.GetName(typeof(DayOfWeek), GetIntOfWeek(1));
            lblForeCastDay2.Text = Enum.GetName(typeof(DayOfWeek), GetIntOfWeek(2));
            lblForeCastDay3.Text = Enum.GetName(typeof(DayOfWeek), GetIntOfWeek(3));
            lblForeCastDay4.Text = Enum.GetName(typeof(DayOfWeek), GetIntOfWeek(4));

            lblForeCast1.Text = $"{foreCast.list[1].temp.max.ToString("0.0")}°";
            lblForeCast2.Text = $"{foreCast.list[2].temp.max.ToString("0.0")}°";
            lblForeCast3.Text = $"{foreCast.list[3].temp.max.ToString("0.0")}°";
            lblForeCast4.Text = $"{foreCast.list[4].temp.max.ToString("0.0")}°";

            lblForeCastIcon1.Source = SetImage(foreCast.list[1].weather[0].main);
            lblForeCastIcon2.Source = SetImage(foreCast.list[2].weather[0].main);
            lblForeCastIcon3.Source = SetImage(foreCast.list[3].weather[0].main);
            lblForeCastIcon4.Source = SetImage(foreCast.list[4].weather[0].main);
        }
        private int GetIntOfWeek(int index)
        {
            var day = (int)DateTime.Now.DayOfWeek;

            return (day + index) <= 6 ? (day + index) : (day + index) - 7;
        }

        private BitmapImage SetImage(string weather)
        {
            switch (weather)
            {
                case "Clear":
                    return new BitmapImage(new Uri("ms-appx:///Image/Sun.png"));
                case "Rain":
                    return new BitmapImage(new Uri("ms-appx:///Image/Rain.png"));
                case "Clouds":
                    return new BitmapImage(new Uri("ms-appx:///Image/Clouds.png"));
                case "Mist":
                    return new BitmapImage(new Uri("ms-appx:///Image/Clouds.png"));
                case "Fog":
                    return new BitmapImage(new Uri("ms-appx:///Image/Clouds.png"));
                case "Snow":
                    return new BitmapImage(new Uri("ms-appx:///Image/Snow.png"));
                default:
                    break;
            }
            return new BitmapImage(new Uri("ms-appx:///Image/Sun.png")); ;
        }
        //private async void SetImageTick()
        //{
        //    weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Sun.png"));
        //    await Task.Delay(TimeSpan.FromSeconds(3));
        //    weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Rain.png"));
        //    await Task.Delay(TimeSpan.FromSeconds(3));
        //    weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Clouds.png"));
        //    await Task.Delay(TimeSpan.FromSeconds(3));
        //    weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Snow.png"));
        //}
    }
}
