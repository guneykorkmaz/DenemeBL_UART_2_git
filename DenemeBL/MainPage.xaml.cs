using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.BluetoothLE;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace DenemeBL
{
    public partial class MainPage : ContentPage
    {
        int sps = 400;
        int filterTime;
        int meanX;
        int timeLenght1;
        int timeLenght2;
        int delayTime;

        public static List<int> dat1 { get; set; } = new List<int>();
        DataModel list;
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatchTest = new Stopwatch();
        IAdapter adapter;
        IDevice device;
        IGattService service;
        IGattCharacteristic notify;
        IGattCharacteristic write;
        IDisposable results;
        string platform = DeviceInfo.Platform.ToString();
        float width = (float)DeviceDisplay.MainDisplayInfo.Width - 20f * (float)DeviceDisplay.MainDisplayInfo.Density;
        float height;
        bool cancel = false;

        SKPaint bPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("131313"),
            StrokeWidth = DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 35,
            StrokeCap = SKStrokeCap.Round
        };

        SKPaint cPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = DeviceInfo.Platform == DevicePlatform.iOS ? 20 : 35,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPaint gpaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White,
            StrokeWidth = 1
        };

        SKPaint gmpaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.White.WithAlpha(100),
            StrokeWidth = 1
        };

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.LimeGreen,
            StrokeWidth = 2,
            IsAntialias = true
        };

        SKPaint dpaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse("131313"),
            StrokeWidth = DeviceInfo.Platform == DevicePlatform.iOS ? 40 : 80
        };

        SKPath[] path = new SKPath[30];
        SKPath[] dpath = new SKPath[30];
        SKPath bPath = new SKPath();
        SKPath cPath = new SKPath();
        SKPath gPath = new SKPath();
        SKPath gmPath = new SKPath();

        public MainPage()
        {
            InitializeComponent();

            filterTime = sps / 100;
            meanX = filterTime / 2;
            timeLenght1 = (int)(sps * 2 * 1.2);
            timeLenght2 = (int)(sps * 6 * 1.2);
            delayTime = int.Parse(Math.Ceiling(20000m / sps).ToString());

            for (int i = 0; i < path.Length; i++)
            {
                path[i] = new SKPath();
                dpath[i] = new SKPath();
            }

            height = 5 * width / 12;

            //Horizontal lines
            for (var i = 0; i <= 5; i++)
            {
                gPath.MoveTo(1, i * (height - 2) / 5 + 1);
                gPath.LineTo(width - 1, i * (height - 2) / 5 + 1);
            }

            for (var i = 0; i <= 25; i++)
            {
                gmPath.MoveTo(1, i * (height - 2) / 25 + 1);
                gmPath.LineTo(width - 1, i * (height - 2) / 25 + 1);
            }

            // Vertical lines
            for (var i = 0; i <= 12; i++)
            {
                gPath.MoveTo(i * (width - 2) / 12 + 1, 1);
                gPath.LineTo(i * (width - 2) / 12 + 1, height - 1);
            }

            for (var i = 0; i <= 60; i++)
            {
                gmPath.MoveTo(i * (width - 2) / 60 + 1, 1);
                gmPath.LineTo(i * (width - 2) / 60 + 1, height - 1);
            }

            var refresh = new Refresh();
            refresh.Refreshing += Refreshing;
            this.BindingContext = refresh;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            notes.WidthRequest = Xamarin.Forms.Application.Current.MainPage.Width - 80;
            Xamarin.Forms.Application.Current.MainPage.Padding = new Thickness(0, On<iOS>().SafeAreaInsets().Top, 0, 0);
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                chartiOS.HeightRequest = height / DeviceDisplay.MainDisplayInfo.Density;
            }
            else
            {
                chartAndro.HeightRequest = height / DeviceDisplay.MainDisplayInfo.Density;
            }
            mTime.SelectedIndex = 0;
            rfr.IsRefreshing = true;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                page.TranslationY = 70;
            }
            await connect();
            rfr.IsRefreshing = false;
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await page.TranslateTo(0, 0, 200, Easing.CubicInOut);
            }
        }

        private void Refreshing(object sender, Refresh e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    page.TranslationY = 70;
                }
                if (device == null)
                {
                    await connect();
                    rfr.IsRefreshing = false;
                }
                else if (device.IsConnected())
                {
                    info.Text = "Device is already connected.";
                    rfr.IsRefreshing = false;
                }
                else
                {
                    await connect();
                    rfr.IsRefreshing = false;
                }
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await page.TranslateTo(0, 0, 200, Easing.CubicInOut);
                }
            });
        }

        async void btnRecord_Clicked(object sender, EventArgs e)
        {
            btnTapped((Button)sender);
            if (device == null || !device.IsConnected())
            {
                return;
            }
            if (btnRecord.Text == "Stop Recording")
            {
                await write.Write(Encoding.ASCII.GetBytes("0"));
                cancel = true;
                btnRecord.Text = "Record ECG";
                return;
            }
            sendContainer.IsVisible = false;
            if (dat1.Count != 0)
            {
                dat1.Clear();
            }
            if (results != null)
            {
                results.Dispose();
            }
            results = notify.RegisterAndNotify().Subscribe(n =>
            {
                var dataString = Encoding.UTF8.GetString(n.Data);
                try
                {
                    list = JsonConvert.DeserializeObject<DataModel>(dataString);
                    dat1.AddRange(list.l1);
                }
                catch
                {
                    Debug.WriteLine("cant convert");
                    Debug.WriteLine(dataString);
                }
                Debug.WriteLine(stopwatchTest.ElapsedMilliseconds + ", " + dat1.Count());
            });
            await write.Write(Encoding.ASCII.GetBytes(platform));
            await write.Write(Encoding.ASCII.GetBytes(mTime.SelectedItem.ToString().Split(' ')[0]));
            stopwatchTest.Restart();
            await Task.Delay(1000);
            //Debug.WriteLine(dat1.Count);
            btnRecord.Text = "Stop Recording";
            draw(int.Parse(mTime.SelectedItem.ToString().Split(' ')[0]) * sps);
        }

        void btnSend_Clicked(object sender, EventArgs e)
        {

        }

        async void btnTapped(Button btn)
        {
            await btn.FadeTo(0, 150, Easing.CubicInOut);
            await btn.FadeTo(1, 150, Easing.CubicInOut);
        }

        async Task connect()
        {
            adapter = await CrossBleAdapter.AdapterScanner.FindAdapters();
            await Task.Delay(100);
            try
            {
                device = await adapter.ScanUntilDeviceFound("CurAlive").Timeout(TimeSpan.FromMilliseconds(3000));
                info.Text = "CurAlive is found.";
                try
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        await device.ConnectWait().Timeout(TimeSpan.FromMilliseconds(10000));
                        //device.Connect(new ConnectionConfig { AndroidConnectionPriority = ConnectionPriority.High });
                        await device.RequestMtu(512).Timeout(TimeSpan.FromMilliseconds(5000));
                    }
                    else
                    {
                        await device.ConnectWait().Timeout(TimeSpan.FromMilliseconds(3000));
                    }
                    info.Text = "Connected to CurAlive";

                    device.WhenAnyCharacteristicDiscovered().Subscribe(subs =>
                    {
                        if (subs.Properties == CharacteristicProperties.Notify)
                        {
                            notify = subs;
                        }
                        else
                        {
                            write = subs;
                        }
                    });

                    /*service = await device.DiscoverServices();
                    service.DiscoverCharacteristics().Subscribe(subs =>
                    {
                        if (subs.Properties == CharacteristicProperties.Notify)
                        {
                            notify = subs;
                        }
                        else
                        {
                            write = subs;
                        }
                    },
                    err =>
                    {
                        Debug.WriteLine(err.Message);
                        info.Text = err.Message;
                    },
                    () =>
                    {
                        info.Text = "CurAlive is ready.";
                    });*/
                }
                catch
                {
                    info.Text = "Can not connect CurAlive. Pull to reconnect.";
                }
            }
            catch
            {
                info.Text = "Device is not found. Make sure your device is open. Pull to reconnect.";
            }
        }

        async Task draw(int count)
        {
            cancel = false;
            resetPaths();
            float mean1 = (float)dat1.Take(dat1.Count).Average();
            stopwatch.Restart();
            for (var i = 1; i <= count; i++)
            {
                if (i % filterTime == 0)
                {
                    if (cancel)
                    {
                        stopwatch.Stop();
                        btnRecord.Text = "Record ECG";
                        return;
                    }
                    float x = ((i - meanX) / (float)timeLenght1 * width) % width;
                    float lead1 = (float)dat1.Skip(i - filterTime).Take(filterTime).Average();
                    float y = 3 * height / 4 - (lead1 - mean1) / 5000 * 2 * height / 5;

                    if (i % 20 == 0 && i > timeLenght1)
                    {
                        dpath[((i - 1) / timeLenght1) - 1].MoveTo(x, 0);
                        dpath[((i - 1) / timeLenght1) - 1].LineTo(x, height);
                    }

                    if (i % timeLenght1 == 0 && i >= timeLenght2)
                    {
                        path[i / timeLenght1 - 2].Reset();
                        dpath[i / timeLenght1 - 3].Reset();
                    }

                    path[(i - 1) / timeLenght1].LineTo(x, y);

                    cPath.LineTo((width - cPaint.StrokeWidth) * i / count + cPaint.StrokeWidth / 2, 0);

                    if (i % 20 == 0)
                    {
                        if (DeviceInfo.Platform == DevicePlatform.iOS)
                        {
                            chartiOS.InvalidateSurface();
                            counteriOS.InvalidateSurface();
                        }
                        else
                        {
                            chartAndro.InvalidateSurface();
                            counterAndro.InvalidateSurface();
                        }

                        var delay = stopwatch.ElapsedMilliseconds % delayTime;
                        await Task.Delay(delayTime - (int)delay);
                        //Debug.WriteLine(i + ", " + dat1.Count + ", " + stopwatch.ElapsedMilliseconds);
                    }
                }
            }
            stopwatch.Stop();
            btnRecord.Text = "Record ECG";
            sendContainer.IsVisible = true;
        }

        public void resetPaths()
        {
            for (var i = 0; i < path.Length; i++)
            {
                path[i].Reset();
                path[i].MoveTo(0, 3 * height / 4);
                dpath[i].Reset();
            }

            cPath.Reset();
            cPath.MoveTo(cPaint.StrokeWidth / 2, 0);
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                chartiOS.InvalidateSurface();
                counteriOS.InvalidateSurface();
            }
            else
            {
                chartAndro.InvalidateSurface();
                counterAndro.InvalidateSurface();
            }
        }

        private void count_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            canvas.Translate(0, cPaint.StrokeWidth / 2);

            bPath.MoveTo(cPaint.StrokeWidth / 2, 0);
            bPath.LineTo(width - cPaint.StrokeWidth / 2, 0);

            canvas.DrawPath(bPath, bPaint);
            canvas.DrawPath(cPath, cPaint);
        }

        private void chart_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColor.Parse("131313"));

            for (var i = 0; i < path.Length; i++)
            {
                canvas.DrawPath(path[i], paint);
                canvas.DrawPath(dpath[i], dpaint);
            }

            canvas.DrawPath(gPath, gpaint);
            canvas.DrawPath(gmPath, gmpaint);

        }

    }
}