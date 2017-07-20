// Copyright (c) Microsoft. All rights reserved.

using IoTCoreDefaultApp.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MyClasses;
using Windows.System.Threading;

namespace IoTCoreDefaultApp
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        private CoreDispatcher MainPageDispatcher;
        private DispatcherTimer timer;
        private DispatcherTimer GetStattimer;
        private DispatcherTimer countdown;
        private ThreadPoolTimer timerInt;
        private ConnectedDevicePresenter connectedDevicePresenter;

        public CoreDispatcher UIThreadDispatcher
        {
            get
            {
                return MainPageDispatcher;
            }

            set
            {
                MainPageDispatcher = value;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            MainPageDispatcher = Window.Current.Dispatcher;

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.DataContext = LanguageManager.GetInstance();

            timerInt = ThreadPoolTimer.CreatePeriodicTimer(GetStatTime, TimeSpan.FromSeconds(60));

            
            

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(20);

            this.Loaded += async (sender, e) => 
            {
                await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    UpdateBoardInfo();
                    UpdateNetworkInfo();
                    UpdateDateTime();
                    UpdateConnectedDevices();
                    timer.Start();
                });
            };
            this.Unloaded += (sender, e) =>
            {
                timer.Stop();
            };
        }

        private async void GetStatTime(ThreadPoolTimer timer)
        {
            GetStats();
            
        }

        private async void GetStatsNow()
        {

        }

        private async void Test()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://public-api.wordpress.com/rest/v1.1/sites/56752040/stats/summary/?fields=views&period=year&num=5");
            request.Headers["Authorization"] = "Bearer YourApiKeyHere";
            string foxGreet = @" 
              /^._
,___,--~~~~--' /'~  FoxDeploy Traffic Checking App!
`~--~\ )___,)/ '
    (/\\_(/\\_      Let's check the stats!
                ";

            
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.  
            WebResponse response = await request.GetResponseAsync();
            // Display the status.  
            
            // Get the stream containing content returned by the server.  
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            //Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.  
            var split = responseFromServer.Split(',');

            string Hits = split[2].Split(':')[1];

           
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(Constants.HasDoneOOBEKey))
            {
                ApplicationData.Current.LocalSettings.Values[Constants.HasDoneOOBEKey] = Constants.HasDoneOOBEValue;
            }

            base.OnNavigatedTo(e);
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                UpdateNetworkInfo();
            });
        }

        private void timer_Tick(object sender, object e)
        {
            UpdateDateTime();
            // maybe add GetStats here
            //HitCounter.Text = "Changed";
            Test();
        }

        private void UpdateBoardInfo()
        {
            BoardName.Text = "FoxDeploy Hit Checker";
            //BoardImage.Source = new BitmapImage(DeviceInfoPresenter.GetBoardImageUri());

            ulong version = 0;
            if (!ulong.TryParse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion, out version))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                OSVersion.Text = loader.GetString("OSVersionNotAvailable");
            }
            else
            {
                OSVersion.Text = String.Format(CultureInfo.InvariantCulture,"{0}.{1}.{2}.{3}",
                    (version & 0xFFFF000000000000) >> 48,
                    (version & 0x0000FFFF00000000) >> 32,
                    (version & 0x00000000FFFF0000) >> 16,
                    version & 0x000000000000FFFF);
            }

            GetStats();
        }

        private void UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();
            CurrentTime.Text = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
        }

        private async void GetStats()
        {
            
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (var client2 = new HttpClient(handler))
            {
                // your code
                string url = "https://public-api.wordpress.com/rest/v1.1/sites/56752040/stats/summary/?fields=views&period=year&num=5";
                //client.DefaultRequestHeaders.Add();
                client2.DefaultRequestHeaders.Add("Authorization", "Bearer YourKeyHere");

                HttpResponseMessage response1 = await client2.GetAsync(url);
                string ham = await response1.Content.ReadAsStringAsync();

                //resume here, working on parsin JSON
                //JsonValue root = JsonValue.Parse(ham);

                var meme = JsonObject.Parse(ham);
                var viewsdesuka = meme.GetNamedValue("views").GetNumber();

                var cat = "Lyla";

                await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    HitCounter.Text = viewsdesuka.ToString("N0");
                });

                //HitCounter.Text =viewsdesuka.ToString();
                cat = "Lyla";

                }

            
        }

        private async void UpdateNetworkInfo()
        {
            this.DeviceName.Text = DeviceInfoPresenter.GetDeviceName();
            this.IPAddress1.Text = NetworkPresenter.GetCurrentIpv4Address();
            this.NetworkName1.Text = NetworkPresenter.GetCurrentNetworkName();
            this.NetworkInfo.ItemsSource = await NetworkPresenter.GetNetworkInformation();
        }

        private void UpdateConnectedDevices()
        {
            connectedDevicePresenter = new ConnectedDevicePresenter(MainPageDispatcher);
            this.ConnectedDevices.ItemsSource = connectedDevicePresenter.GetConnectedDevices();
        }

        private void ShutdownButton_Clicked(object sender, RoutedEventArgs e)
        {
            ShutdownDropdown.IsOpen = true;
        }

        private void CommandLineButton_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationUtils.NavigateToScreen(typeof(CommandLinePage));
        }

        private void SettingsButton_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationUtils.NavigateToScreen(typeof(Settings));
        }

        private void Tutorials_Clicked(object sender, RoutedEventArgs e)
        {
            NavigationUtils.NavigateToScreen(typeof(TutorialMainPage));
        }

        private void ShutdownHelper(ShutdownKind kind)
        {
            new System.Threading.Tasks.Task(() =>
            {
                ShutdownManager.BeginShutdown(kind, TimeSpan.FromSeconds(0));
            }).Start();
        }

        private void ShutdownListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FrameworkElement;
            if (item == null)
            {
                return;
            }
            switch (item.Name)
            {
                case "ShutdownOption":
                    ShutdownHelper(ShutdownKind.Shutdown);
                    break;
                case "RestartOption":
                    ShutdownHelper(ShutdownKind.Restart);
                    break;
            }
        }

        private void ShutdownDropdown_Opened(object sender, object e)
        {
            var w = ShutdownListView.ActualWidth;
            if (w == 0)
            {
                // trick to recalculate the size of the dropdown
                ShutdownDropdown.IsOpen = false;
                ShutdownDropdown.IsOpen = true;
            }
            var offset = -(ShutdownListView.ActualWidth - ShutdownButton.ActualWidth);
            ShutdownDropdown.HorizontalOffset = offset;
        }
    }
}
