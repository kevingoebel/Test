using App1.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;

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
        private const string SRGS_FILE = "Grammar\\grammar.xml";
        private SpeechRecognizer recognizer;
        private const string TAG_TARGET = "target";
        private const string TAG_CMD = "cmd";
        private const string TAG_DEVICE = "device";
        private const string DEVICE_LIGHT = "LIGHT";
        private const string STATE_ON = "ON";
        private const string STATE_OFF = "OFF";
        private const string TARGET_BATHROOM = "BATHROOM";
        private static GpioPin bathroomLightPin = null;
        private static GpioController gpio = null;
        private const int BATHROOM_LIGHT_PIN = 13;

        public MainPage()
        {
            this.InitializeComponent();
            FillControlls();
            SetTraffic();
            SetLocation();
            Unloaded += MainPage_Unloaded;
            initializeSpeechRecognizer();
            //initializeGPIO();
            //SetImageTick();
        }
        public async void SetLocation()
        {
            BasicGeoposition location = new BasicGeoposition();
            location.Latitude = 50.95;
            location.Longitude = 5.87;
            Geopoint myLocation = new Geopoint(location);


            // Set the map location.
            mapControl.Center = myLocation;
            mapControl.ZoomLevel = 12;
            mapControl.LandmarksVisible = true;
            mapControl.TrafficFlowVisible = true;

            //var accessStatus = await Geolocator.RequestAccessAsync();
            //switch (accessStatus)
            //{
            //    case GeolocationAccessStatus.Allowed:

            //        //// Get the current location.
            //        //Geolocator geolocator = new Geolocator();
            //        //Geoposition pos = await geolocator.GetGeopositionAsync();
            //        //Geopoint myLocation = pos.Coordinate.Point;

            //        BasicGeoposition location = new BasicGeoposition();
            //        location.Latitude = 50.95;
            //        location.Longitude = 5.87;
            //        Geopoint myLocation = new Geopoint(location);


            //        // Set the map location.
            //        mapControl.Center = myLocation;
            //        mapControl.ZoomLevel = 12;
            //        mapControl.LandmarksVisible = true;
            //        mapControl.TrafficFlowVisible = true;

            //        break;

            //    case GeolocationAccessStatus.Denied:
            //        // Handle the case  if access to location is denied.
            //        break;

            //    case GeolocationAccessStatus.Unspecified:
            //        // Handle the case if  an unspecified error occurs.
            //        break;
            //}
        }

        private async void MainPage_Unloaded(object sender, object args)
        {
            // Stop recognizing
            await recognizer.ContinuousRecognitionSession.StopAsync();

            recognizer.Dispose();
            recognizer = null;
        }
        private void initializeGPIO()
        {
            // Initialize GPIO controller
            //gpio = GpioController.GetDefault();

            //bathroomLightPin = gpio.OpenPin(BATHROOM_LIGHT_PIN);

            //bathroomLightPin.SetDriveMode(GpioPinDriveMode.Output);

            //bathroomLightPin.Write(GpioPinValue.Low);
        }
        // Initialize Speech Recognizer and start async recognition
        private async void initializeSpeechRecognizer()
        {
            // Initialize recognizer
            recognizer = new SpeechRecognizer();

            // Set event handlers
            recognizer.StateChanged += RecognizerStateChanged;
            recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

            // Load Grammer file constraint
            string fileName = String.Format(SRGS_FILE);
            StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);

            SpeechRecognitionGrammarFileConstraint grammarConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);

            // Add to grammer constraint
            recognizer.Constraints.Add(grammarConstraint);

            // Compile grammer
            SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();

            Debug.WriteLine("Status: " + compilationResult.Status.ToString());

            // If successful, display the recognition result.
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Debug.WriteLine("Result: " + compilationResult.ToString());

                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Debug.WriteLine("Status: " + compilationResult.Status);
            }
        }
        // Recognizer generated results
        private void RecognizerResultGenerated(SpeechContinuousRecognitionSession session, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Output debug strings
            Debug.WriteLine(args.Result.Status);
            Debug.WriteLine(args.Result.Text);

            int count = args.Result.SemanticInterpretation.Properties.Count;

            Debug.WriteLine("Count: " + count);
            Debug.WriteLine("Tag: " + args.Result.Constraint.Tag);

            // Check for different tags and initialize the variables
            String target = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_TARGET) ?
                            args.Result.SemanticInterpretation.Properties[TAG_TARGET][0].ToString() :
                            "";

            String cmd = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_CMD) ?
                            args.Result.SemanticInterpretation.Properties[TAG_CMD][0].ToString() :
                            "";

            String device = args.Result.SemanticInterpretation.Properties.ContainsKey(TAG_DEVICE) ?
                            args.Result.SemanticInterpretation.Properties[TAG_DEVICE][0].ToString() :
                            "";

            // Whether state is on or off
            bool isOn = cmd.Equals(STATE_ON);

            Debug.WriteLine("Target: " + target + ", Command: " + cmd + ", Device: " + device);

            if (device.Equals(DEVICE_LIGHT))
            {
                // Check target location
                if (target.Equals(TARGET_BATHROOM))
                {
                    Debug.WriteLine("BATHROOM LIGHT " + (isOn ? STATE_ON : STATE_OFF));

                    // Turn on the bedroom light
                    WriteGPIOPin(bathroomLightPin, isOn ? GpioPinValue.High : GpioPinValue.Low);
                }
                else
                {
                    Debug.WriteLine("Unknown Target");
                }
            }
            else
            {
                Debug.WriteLine("Unknown Device");
            }

            /*foreach (KeyValuePair<String, IReadOnlyList<string>> child in args.Result.SemanticInterpretation.Properties)
            {
                Debug.WriteLine(child.Key + " = " + child.Value.ToString());
                foreach (String val in child.Value)
                {
                    Debug.WriteLine("Value = " + val);
                }
            }*/
        }

        // Recognizer state changed
        private void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Debug.WriteLine("Speech recognizer state: " + args.State.ToString());
        }
        private void WriteGPIOPin(GpioPin pin, GpioPinValue value)
        {
            pin.Write(value);
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
        
        private void SetTraffic()
        {
            //webviewTraffic.Source = new Uri("http://www.anwb.nl/verkeer/verkeerstool.js");
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
            //lblWeatherCity.Text = dailyWeather.name;
            lblWeatherType.Text = char.ToUpper(description[0]) + description.Substring(1);
            lblWeatherMax.Text = $"{dailyWeather.main.temp_max.ToString("0.0")}°";
            lblWeatherMin.Text = $"{dailyWeather.main.temp_min.ToString("0.0")}°";
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

            lblForeCastMin1.Text = $"{foreCast.list[1].temp.min.ToString("0.0")}°";
            lblForeCastMin2.Text = $"{foreCast.list[2].temp.min.ToString("0.0")}°";
            lblForeCastMin3.Text = $"{foreCast.list[3].temp.min.ToString("0.0")}°";
            lblForeCastMin4.Text = $"{foreCast.list[4].temp.min.ToString("0.0")}°";

            lblForeCastMax1.Text = $"{foreCast.list[1].temp.max.ToString("0.0")}°";
            lblForeCastMax2.Text = $"{foreCast.list[2].temp.max.ToString("0.0")}°";
            lblForeCastMax3.Text = $"{foreCast.list[3].temp.max.ToString("0.0")}°";
            lblForeCastMax4.Text = $"{foreCast.list[4].temp.max.ToString("0.0")}°";

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
                    return new BitmapImage(new Uri("ms-appx:///Image/Mist.png"));
                case "Fog":
                    return new BitmapImage(new Uri("ms-appx:///Image/Mist.png"));
                case "Snow":
                    return new BitmapImage(new Uri("ms-appx:///Image/Snow.png"));
                default:
                    break;
            }
            return new BitmapImage(new Uri("ms-appx:///Image/Sun.png")); ;
        }
        private async void SetImageTick()
        {
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Sun.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Rain.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Clouds.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Snow.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Lightning.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/FewClouds.png"));
            await Task.Delay(TimeSpan.FromSeconds(3));
            weatherImage.Source = new BitmapImage(new Uri("ms-appx:///Image/Mist.png"));
        }
    }
}
