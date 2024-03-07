using AForge.Video.DirectShow;
using AForge.Video;
using AForge.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using WPF_MachineService.Service;
using static AForge.Math.FourierTransform;
using System;
using System.Collections.Generic;
using System.Linq;
namespace WPF_MachineService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection? _filterInfoCollectionVideoDevices; 
        private VideoCaptureDevice[]? _videoCaptureDeviceSources;
        private int selectedCameraIndex = 0;
        private int captureCount = 1;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MachineWindow_Loaded;
            Closing += MachineWindow_Closing;
        }

        /// <summary>
        /// Load Main Window, Sceen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MachineWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            _filterInfoCollectionVideoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (_filterInfoCollectionVideoDevices != null && _filterInfoCollectionVideoDevices.Count >= 0)
            {
                _videoCaptureDeviceSources = new VideoCaptureDevice[1];
                _videoCaptureDeviceSources[0] = new VideoCaptureDevice(_filterInfoCollectionVideoDevices[0].MonikerString);
                _videoCaptureDeviceSources[0].NewFrame += VideoSource_BitMapFrame;
                _videoCaptureDeviceSources[0].Start();
            }
            else
            {
                MessageBox.Show("Không đủ thiết bị video.");
            }

        }
        /// <summary>
        /// Use Bitmap in Video && Image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void VideoSource_BitMapFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            try
            {
                Bitmap videoCapture = (Bitmap)eventArgs.Frame.Clone();
                imgVideo.Dispatcher.Invoke(() => DisplayImageInImageView(videoCapture, imgVideo));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in VideoSource_BitMapFrame: {ex.Message}");
            }
        }

        /// <summary>
        /// Conver , dispay image
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="imageView"></param>
        private void DisplayImageInImageView(Bitmap frame, System.Windows.Controls.Image imageView)
        {
            System.Windows.Media.Imaging.BitmapImage bitmapImage = ToBitMapImage(frame);

            if (bitmapImage != null)
            {
                imageView.Source = bitmapImage;
            }
        }
        private System.Windows.Media.Imaging.BitmapImage ToBitMapImage(Bitmap bitmap)
        {
            System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        /// <summary>
        /// Closing window 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MachineWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_videoCaptureDeviceSources != null)
                {
                    foreach (var videoSource in _videoCaptureDeviceSources)
                    {
                        if (videoSource != null && videoSource.IsRunning)
                        {
                            await Task.Run(() =>
                            {
                                videoSource.SignalToStop();
                                videoSource.WaitForStop();
                            });
                        }
                    }
                    _videoCaptureDeviceSources = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi dừng video source: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Bitmap HelpToBitMapImage(BitmapSource capturedBitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(capturedBitmapSource));
                enc.Save(memoryStream);
                bitmap = new Bitmap(memoryStream);
            }
            return bitmap;
        }
        private void btTakePictureImage_Click(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                if (_videoCaptureDeviceSources != null)
                {
                    if (selectedCameraIndex >= 0)
                    {
                        var videoSource = _videoCaptureDeviceSources[selectedCameraIndex];
                        if (videoSource != null && videoSource.IsRunning)
                        {
                            BitmapSource capturedBitmapSource = (BitmapSource)imgVideo.Source;
                            if (capturedBitmapSource != null)
                            {
                                Bitmap capturedBitmap = HelpToBitMapImage(capturedBitmapSource);
                                string subfolderName = DateTime.Now.ToString("yyyyMMdd");
                                string subfolderPath = System.IO.Path.Combine("D:\\Dev\\BE\\WPF\\WPF_MachineService\\SavePic", subfolderName);

                                if (!Directory.Exists(subfolderPath))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(subfolderPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Windows.MessageBox.Show($"Error creating subfolder: {ex.Message}");
                                        return;
                                    }
                                }
                                string folderName = $"{DateTime.Now:yyyyMMdd}_{captureCount++}";
                                string folderPath = System.IO.Path.Combine(subfolderPath, folderName);
                                if (!Directory.Exists(folderPath))
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(folderPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Windows.MessageBox.Show($"Error creating folder: {ex.Message}");
                                        return;
                                    }
                                }
                                string fileName = $"capture_{DateTime.Now:HHmmss}.png";

                                string filePath = System.IO.Path.Combine(folderPath, fileName);
                                string filePathPython = System.IO.Path.Combine("C:\\Yolov8\\ultralytics\\yolov8-silva\\inference\\images", fileName);
                                LoadDetectionData();
                                try
                                {
                                    capturedBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                                    capturedBitmap.Save(filePathPython, System.Drawing.Imaging.ImageFormat.Bmp);
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.MessageBox.Show($"Error saving capture: {ex.Message}");
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Bitmap source is null.");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Selected video source is not running.");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid camera index.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error capturing frame: {ex.Message}");
            }

        }

        private async void LoadDetectionData()
        {
            string detectjsonFilePath = @"D:\Dev\BE\WPF\WPF_MachineService\WPF_MachineService\WPF_MachineService\Detection_results.json";

            if (File.Exists(detectjsonFilePath))
            {
                try
                {
                 
                    List<Detection> productsFromJson;
                    using (StreamReader r = new StreamReader(detectjsonFilePath))
                    {
                        string json = r.ReadToEnd();
                        productsFromJson = JsonConvert.DeserializeObject<List<Detection>>(json);
                    }
                   
                    List<string> productNames = new List<string>();
                    foreach (var detection in productsFromJson)
                    {
                        productNames.Add(detection.name);
                    }

                    lvListView.ItemsSource = productNames;
                }
                catch (JsonException ex)
                {
                    MessageBox.Show("Error reading JSON file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("File not found: " + detectjsonFilePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }


    
}