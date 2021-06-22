using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CameraSample
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Microsoft.UI.Xaml.Window
    {
        private VideoCapture capture;
        private Mat frame;
        private Bitmap imageAlternate;
        private Bitmap image;
        private bool isUsingImageAlternate;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            capture = new VideoCapture(0);
            _ = capture.Open(0);

            Timer traditionalTimer = new Timer();
            traditionalTimer.Interval = 100;
            traditionalTimer.Elapsed += TraditionalTimer_Elapsed;
            traditionalTimer.Start();

            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(100);
            //timer.Tick += Timer_Tick;
            //timer.Start();
        }

        private void TraditionalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                frame = new Mat();
                _ = capture.Read(frame);
                if (frame != null)
                {
                    if (imageAlternate == null)
                    {
                        isUsingImageAlternate = true;
                        imageAlternate = BitmapConverter.ToBitmap(frame);
                    }
                    else if (image == null)
                    {
                        isUsingImageAlternate = false;
                        image = BitmapConverter.ToBitmap(frame);
                    }

                    _ = DispatcherQueue.TryEnqueue(async () =>
                      {
                          CameraPreview.Source = isUsingImageAlternate ? await BitmapToImageSource(imageAlternate) : await BitmapToImageSource(image);
                      });
                }
            }
            catch (Exception)
            {
                _ = DispatcherQueue.TryEnqueue(() =>
                  {
                      CameraPreview.Source = null;
                  });
            }
            finally
            {
                if (frame != null)
                {
                    frame.Dispose();
                }

                if (isUsingImageAlternate && image != null)
                {
                    image.Dispose();
                    image = null;
                }
                else if (!isUsingImageAlternate && imageAlternate != null)
                {
                    imageAlternate.Dispose();
                    imageAlternate = null;
                }
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                frame = new Mat();
                _ = capture.Read(frame);
                if (frame != null)
                {
                    if (imageAlternate == null)
                    {
                        isUsingImageAlternate = true;
                        imageAlternate = BitmapConverter.ToBitmap(frame);
                    }
                    else if (image == null)
                    {
                        isUsingImageAlternate = false;
                        image = BitmapConverter.ToBitmap(frame);
                    }

                    CameraPreview.Source = isUsingImageAlternate ? await BitmapToImageSource(imageAlternate) : await BitmapToImageSource(image);

                }
            }
            catch (Exception)
            {
                CameraPreview.Source = null;
            }
            finally
            {
                if (frame != null)
                {
                    frame.Dispose();
                }

                if (isUsingImageAlternate && image != null)
                {
                    image.Dispose();
                    image = null;
                }
                else if (!isUsingImageAlternate && imageAlternate != null)
                {
                    imageAlternate.Dispose();
                    imageAlternate = null;
                }
            }
        }

        private async Task<BitmapImage> BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                await bitmapimage.SetSourceAsync(memory.AsRandomAccessStream());
                return bitmapimage;
            }
        }
    }
}
