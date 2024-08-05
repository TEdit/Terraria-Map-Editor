/* 
Copyright (c) 2024 RussDev7
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Threading;

using TEdit.Editor.Clipboard;
using TEdit.Geometry; /* TE4: using TEdit.Common.Geometry.Primitives; */
using TEdit.ViewModel;
using TEdit.Configuration;

using Bitmap = System.Drawing.Bitmap;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;
using Color = System.Drawing.Color;
using MessageBox = System.Windows.Forms.MessageBox;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Tile = TEdit.Terraria.Tile;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ImageToPixelartEditor.xaml
    /// </summary>
    public partial class ImageToPixelartEditorView : Window
    {
        private readonly WorldViewModel _worldViewModel;
        private static CancellationTokenSource _cancellationTokenSource;
        public static Color GridColor { get; set; } = Color.Red;
        public static List<TileWallData> TileWallDataList { get; set; }
        public static Color[] Clrs { get; set; } = new Color[1];
        public static List<TileWallData> ClrsTileWallData { get; set; }
        public static ImageSource OriginalImageSource { get; set; }
        public static ClipboardBuffer GeneratedSchematic { get; set; }
        public static string SavedSchematicLocation { get; set; }

        #region Initialization

        public ImageToPixelartEditorView(WorldViewModel worldViewModel)
        {
            InitializeComponent();

            // Get the "_wvm" from mother class.
            _worldViewModel = worldViewModel;

            // Set the initial background images.
            #region Initial Images


            #endregion

            // Check the radiobuttons.
            #region Initial Radiobuttons

            Bilinear.IsChecked = true;
            Rotation0.IsChecked = true;
            XAxis.IsChecked = true;
            #endregion

            // Check the checkboxes.
            #region Initial Checkboxes

            GenerateSchematic.IsChecked = false;
            GatherStatistics.IsChecked = true;
            ProgressBar.IsChecked = false;

            UseTiles.IsChecked = true;
            UseWalls.IsChecked = true;
            BuildSafe.IsChecked = true;
            UniqueColors.IsChecked = true;
            #endregion

            // Define original image source.
            OriginalImageSource = BackgroundImage1.Source;

            // Load the color filter.
            _ = BuildColorFilter();
        }
        #endregion

        #region WPF GUI Logic

        #region Form Closing

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion

        #region Setting Images

        // Return an bitmap image from base64.
        public BitmapImage SetBackgroundBase64Image(string base64Data)
        {
            byte[] binaryData = Convert.FromBase64String(base64Data);

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(binaryData);
            bitmapImage.EndInit();

            return bitmapImage;
        }
        #endregion

        #region Numeric Updown Logic

        #region Spacing

        readonly int minvalue1 = 1;
        readonly int maxvalue1 = 100;
        readonly int startvalue1 = 4;

        private void NUDButtonUP1_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox1.Text != "") number = Convert.ToInt32(NUDTextBox1.Text);
            else number = 0;
            if (number < maxvalue1)
                NUDTextBox1.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown1_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox1.Text != "") number = Convert.ToInt32(NUDTextBox1.Text);
            else number = 0;
            if (number > minvalue1)
                NUDTextBox1.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox1.Text != "")
                if (!int.TryParse(NUDTextBox1.Text, out number)) NUDTextBox1.Text = startvalue1.ToString();
            if (number > maxvalue1) NUDTextBox1.Text = maxvalue1.ToString();
            if (number < minvalue1) NUDTextBox1.Text = minvalue1.ToString();
            NUDTextBox1.SelectionStart = NUDTextBox1.Text.Length;
        }

        private void NUDTextBox1_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (int.TryParse(NUDTextBox1.Text, out int number))
            {
                if (e.Delta > 0) // Scroll up
                {
                    if (number < maxvalue1)
                        NUDTextBox1.Text = Convert.ToString(number + 1);
                }
                else if (e.Delta < 0) // Scroll down
                {
                    if (number > minvalue1)
                        NUDTextBox1.Text = Convert.ToString(number - 1);
                }
            }
            else
            {
                NUDTextBox1.Text = startvalue1.ToString(); // Fallback in case of invalid input
            }
            e.Handled = true; // Mark the event as handled to prevent further processing
        }
        #endregion

        #region New Ratio

        readonly int minvalue2 = -200;
        readonly int maxvalue2 = 200;
        readonly int startvalue2 = 0;

        private void NUDButtonUP2_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox2.Text != "") number = Convert.ToInt32(NUDTextBox2.Text);
            else number = 0;
            if (number < maxvalue2)
                NUDTextBox2.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown2_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox2.Text != "") number = Convert.ToInt32(NUDTextBox2.Text);
            else number = 0;
            if (number > minvalue2)
                NUDTextBox2.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox2.Text != "")
                if (!int.TryParse(NUDTextBox2.Text, out number)) NUDTextBox2.Text = startvalue2.ToString();
            if (number > maxvalue2) NUDTextBox2.Text = maxvalue2.ToString();
            if (number < minvalue2) NUDTextBox2.Text = minvalue2.ToString();
            NUDTextBox2.SelectionStart = NUDTextBox2.Text.Length;
        }

        private void NUDTextBox2_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (int.TryParse(NUDTextBox2.Text, out int number))
            {
                if (e.Delta > 0) // Scroll up
                {
                    if (number < maxvalue2)
                        NUDTextBox2.Text = Convert.ToString(number + 1);
                }
                else if (e.Delta < 0) // Scroll down
                {
                    if (number > minvalue2)
                        NUDTextBox2.Text = Convert.ToString(number - 1);
                }
            }
            else
            {
                NUDTextBox2.Text = startvalue1.ToString(); // Fallback in case of invalid input
            }
            e.Handled = true; // Mark the event as handled to prevent further processing
        }
        #endregion

        #region Grid Y Offset

        readonly int minvalue3 = 0;
        readonly int maxvalue3 = 100;
        readonly int startvalue3 = 0;

        private void NUDButtonUP3_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox3.Text != "") number = Convert.ToInt32(NUDTextBox3.Text);
            else number = 0;
            if (number < maxvalue3)
                NUDTextBox3.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown3_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox3.Text != "") number = Convert.ToInt32(NUDTextBox3.Text);
            else number = 0;
            if (number > minvalue3)
                NUDTextBox3.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox3_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox3.Text != "")
                if (!int.TryParse(NUDTextBox3.Text, out number)) NUDTextBox3.Text = startvalue3.ToString();
            if (number > maxvalue3) NUDTextBox3.Text = maxvalue3.ToString();
            if (number < minvalue3) NUDTextBox3.Text = minvalue3.ToString();
            NUDTextBox3.SelectionStart = NUDTextBox3.Text.Length;
        }

        private void NUDTextBox3_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (int.TryParse(NUDTextBox3.Text, out int number))
            {
                if (e.Delta > 0) // Scroll up
                {
                    if (number < maxvalue3)
                        NUDTextBox3.Text = Convert.ToString(number + 1);
                }
                else if (e.Delta < 0) // Scroll down
                {
                    if (number > minvalue3)
                        NUDTextBox3.Text = Convert.ToString(number - 1);
                }
            }
            else
            {
                NUDTextBox3.Text = startvalue1.ToString(); // Fallback in case of invalid input
            }
            e.Handled = true; // Mark the event as handled to prevent further processing
        }
        #endregion

        #region Grid X Offset

        readonly int minvalue4 = 0;
        readonly int maxvalue4 = 100;
        readonly int startvalue4 = 0;

        private void NUDButtonUP4_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox4.Text != "") number = Convert.ToInt32(NUDTextBox4.Text);
            else number = 0;
            if (number < maxvalue4)
                NUDTextBox4.Text = Convert.ToString(number + 1);
        }

        private void NUDButtonDown4_Click(object sender, RoutedEventArgs e)
        {
            int number;
            if (NUDTextBox4.Text != "") number = Convert.ToInt32(NUDTextBox4.Text);
            else number = 0;
            if (number > minvalue4)
                NUDTextBox4.Text = Convert.ToString(number - 1);
        }

        private void NUDTextBox4_TextChanged(object sender, TextChangedEventArgs e)
        {
            int number = 0;
            if (NUDTextBox4.Text != "")
                if (!int.TryParse(NUDTextBox4.Text, out number)) NUDTextBox4.Text = startvalue4.ToString();
            if (number > maxvalue4) NUDTextBox4.Text = maxvalue4.ToString();
            if (number < minvalue4) NUDTextBox4.Text = minvalue4.ToString();
            NUDTextBox4.SelectionStart = NUDTextBox4.Text.Length;
        }

        private void NUDTextBox4_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (int.TryParse(NUDTextBox4.Text, out int number))
            {
                if (e.Delta > 0) // Scroll up
                {
                    if (number < maxvalue4)
                        NUDTextBox4.Text = Convert.ToString(number + 1);
                }
                else if (e.Delta < 0) // Scroll down
                {
                    if (number > minvalue4)
                        NUDTextBox4.Text = Convert.ToString(number - 1);
                }
            }
            else
            {
                NUDTextBox4.Text = startvalue1.ToString(); // Fallback in case of invalid input
            }
            e.Handled = true; // Mark the event as handled to prevent further processing
        }
        #endregion

        #endregion

        #region Scaling Mode Textbox Radiobutton Logic

        private void AEquals_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox) return;

            string text = textBox.Text;
            string validText = Regex.Replace(text, @"[^\d]", "");

            // Enforce max limit of 3 characters
            if (validText.Length > 3)
            {
                validText = validText.Substring(0, 3);
            }

            if (text != validText)
            {
                int selectionStart = textBox.SelectionStart - (text.Length - validText.Length);
                textBox.Text = validText;
                textBox.SelectionStart = Math.Max(0, selectionStart);
            }
        }

        private void SigmaEquals_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.TextBox textBox) return;

            string text = textBox.Text;
            string validText = Regex.Replace(text, @"[^0-9.]", "");

            // Ensure only one decimal point
            int decimalCount = validText.Count(c => c == '.');
            if (decimalCount > 1)
            {
                int firstDecimalIndex = validText.IndexOf('.');
                validText = validText.Substring(0, firstDecimalIndex + 1) + validText.Substring(firstDecimalIndex + 1).Replace(".", "");
            }

            // Handle maximum length with decimal point consideration
            if (validText.Length > 4)
            {
                // If text has a decimal point, we want to ensure it stays valid
                // Extract integer and fractional parts
                string[] parts = validText.Split('.');
                string integerPart = parts[0];
                string fractionalPart = parts.Length > 1 ? parts[1] : "";

                // Enforce max length with consideration of the decimal point
                if (integerPart.Length > 3) // Integer part max length
                {
                    integerPart = integerPart.Substring(0, 3);
                }

                if (fractionalPart.Length > 1) // Fractional part max length
                {
                    fractionalPart = fractionalPart.Substring(0, 1);
                }

                validText = fractionalPart.Length > 0 ? $"{integerPart}.{fractionalPart}" : integerPart;
            }

            if (text != validText)
            {
                int selectionStart = textBox.SelectionStart - (text.Length - validText.Length);
                textBox.Text = validText;
                textBox.SelectionStart = Math.Max(0, selectionStart);
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == Lanczos)
            {
                AEquals.IsReadOnly = false;
            }
            else if (sender == Gaussian)
            {
                SigmaEquals.IsReadOnly = false;
            }
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender == Lanczos)
            {
                AEquals.IsReadOnly = true;
            }
            else if (sender == Gaussian)
            {
                SigmaEquals.IsReadOnly = true;
            }
        }
        #endregion

        #region Color Filter Checked Changed Logic

        private async void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            await BuildColorFilter();
        }
        #endregion

        #endregion

        #region Basic Configuration Controls

        #region Open New Image

        // Open a new image.
        private void OpenNewImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    BackgroundImage1.Source = bitmap;

                    // Define original image source.
                    OriginalImageSource = bitmap;

                    // Set the loaded image data.
                    CurrentImageData.Text = openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image: " + ex.Message);
                }
            }
        }
        #endregion

        #region Refresh Image Ratio

        private async void RefreshRatio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Bitmap bitmapImage = null;
                int zoomFactor = 0;

                Dispatcher.Invoke(() =>
                {
                    // Set the labels data.
                    NewRatioPercentLevelData.Content = "Calc...";

                    // Gather the values from the UI thread.
                    bitmapImage = new Bitmap(BitmapFromSource((BitmapSource)OriginalImageSource));
                    zoomFactor = int.Parse(NUDTextBox2.Text);

                    // Disable ratio, convert, and open buttons.
                    ConvertToPixelArt.IsEnabled = false;
                    OpenNewImage.IsEnabled = false;
                    RefreshRatio.IsEnabled = false;

                    NUDTextBox2.IsEnabled = false;
                    NUDButtonUP2.IsEnabled = false;
                    NUDButtonDown2.IsEnabled = false;
                });

                await Task.Run(() =>
                {
                    Bitmap resizedImage = null;

                    if (zoomFactor == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            // Set the labels data.
                            NewRatioPercentLevelData.Content = int.Parse(NUDTextBox2.Text);

                            // Set image to the original.
                            BackgroundImage1.Source = OriginalImageSource;

                            // Enable ratio, convert, and open buttons.
                            ConvertToPixelArt.IsEnabled = true;
                            OpenNewImage.IsEnabled = true;
                            RefreshRatio.IsEnabled = true;

                            NUDTextBox2.IsEnabled = true;
                            NUDButtonUP2.IsEnabled = true;
                            NUDButtonDown2.IsEnabled = true;
                        });
                        return;
                    }
                    else if (zoomFactor > 0)
                    {
                        // Zoom In.
                        Size newSize = new(bitmapImage.Width * zoomFactor, bitmapImage.Height * zoomFactor);
                        resizedImage = new Bitmap(bitmapImage, newSize);
                    }
                    else if (zoomFactor < 0)
                    {
                        // Zoom Out.
                        zoomFactor *= -1;
                        Size newSize = new(bitmapImage.Width / zoomFactor, bitmapImage.Height / zoomFactor);
                        resizedImage = new Bitmap(bitmapImage, newSize);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        // Set the labels data.
                        NewRatioPercentLevelData.Content = int.Parse(NUDTextBox2.Text);

                        // Set the new image.
                        BackgroundImage1.Source = BitmapToImageSource(resizedImage);

                        // Enable ratio, convert, and open buttons.
                        ConvertToPixelArt.IsEnabled = true;
                        OpenNewImage.IsEnabled = true;
                        RefreshRatio.IsEnabled = true;

                        NUDTextBox2.IsEnabled = true;
                        NUDButtonUP2.IsEnabled = true;
                        NUDButtonDown2.IsEnabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }
        }
        #endregion

        #region Convert Pixelart Button

        // Convert pixelart.
        private async void ConvertToPixelArt_Click(object sender, RoutedEventArgs e)
        {
            // Check the current button content.
            if (ConvertToPixelArt.Content.ToString() == "Convert To Pixel Art")
            {
                // Ensure the color filter is not zero.
                if (ClrsTileWallData.Count == 0)
                {
                    // Display error.
                    MessageBox.Show("The color filter is zero. Adjust your settings.");
                    return;
                }
                
                // Start or restart the conversion.
                _cancellationTokenSource?.Cancel(); // Cancel any existing conversion tasks.
                _cancellationTokenSource = new CancellationTokenSource(); // Create a new CancellationTokenSource for the new operation.
                ConvertToPixelArt.ToolTip = "Cancle the current rendering operation."; // Change button tooltip.
                ConvertToPixelArt.Content = "Cancel Conversion"; // Change button content to indicate the operation can be cancelled.

                try
                {
                    // Call ConvertPixelArt asynchronously with the cancellation token.
                    await ConvertPixelArt(buildSchematic: (bool)GenerateSchematic.IsChecked, cancellationToken: _cancellationTokenSource.Token); // False as we're just building only.
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation.
                    MessageBox.Show("Rendering was cancelled.");
                }
                finally
                {
                    // Reset button content, tooltip, and state.
                    ConvertToPixelArt.ToolTip = "Convert the current image to pixel art.";
                    ConvertToPixelArt.Content = "Convert To Pixel Art";
                }
            }
            else if (ConvertToPixelArt.Content.ToString() == "Cancel Conversion")
            {
                // Cancel the ongoing conversion.
                _cancellationTokenSource?.Cancel();
            }
        }
        #endregion

        #region Save Image

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (BackgroundImage2.Source is not BitmapImage)
            {
                // Image source is set
                MessageBox.Show("ERROR: You need to generate a pixelart first!");
                return;
            }

            using System.Windows.Forms.SaveFileDialog saveFileDialog = new();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
            saveFileDialog.Title = "Save Generated Pixelart";
            saveFileDialog.FileName = "ImageOut"; // Default file name.

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Save the image from PictureBox to the specified file
                ImageFormat format = ImageFormat.Png;
                EncoderParameters encoderParameters = null;

                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        format = ImageFormat.Png;
                        break;
                    case 2:
                        format = ImageFormat.Jpeg;
                        encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L); // Set JPEG quality to 100%.
                        break;
                    case 3:
                        format = ImageFormat.Bmp;
                        break;
                }

                if (format == ImageFormat.Jpeg)
                {
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    BitmapFromSource((BitmapSource)BackgroundImage2.Source).Save(saveFileDialog.FileName, jpegCodec, encoderParameters);
                }
                else
                {
                    BitmapFromSource((BitmapSource)BackgroundImage2.Source).Save(saveFileDialog.FileName, format);
                }

               
                MessageBox.Show("Image Completed.");
            }
        }

        // Get encoder helper.
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion

        #region Copy To Clipboard

        private void CopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GeneratedSchematic == null)
                {
                    MessageBox.Show("The schematic data is empty! Generate a pixelart first!");
                    return;
                }

                if (_worldViewModel.CurrentWorld == null)
                {
                    MessageBox.Show("The 'Copy To Clipboard' feature requires a world to be loaded!");
                    return;
                }

                // Render and copy to clipboard.
                // TE4: GeneratedSchematic.RenderBuffer();
                // TE4: _worldViewModel.Clipboard.LoadedBuffers.Add(GeneratedSchematic);
                // TE4: _worldViewModel.ClipboardSetActiveCommand.Execute(GeneratedSchematic);

                var bufferRendered = new ClipboardBufferPreview(GeneratedSchematic);
                _worldViewModel.Clipboard.LoadedBuffers.Add(bufferRendered);
                _worldViewModel.ClipboardSetActiveCommand.Execute(bufferRendered);

                // Display completion message.
                MessageBox.Show("Schematic copied to clipboard.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Save Schematic To File

        private void SaveSchematicToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GeneratedSchematic == null)
                {
                    MessageBox.Show("The schematic data is empty! Generate a pixelart first!");
                    return;
                }

                // Configure SFD.
                var sfd = new System.Windows.Forms.SaveFileDialog
                {
                    Filter = "TEdit Schematic File|*.TEditSch",
                    Title = "Export Schematic File",
                    InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Schematics")
                };

                // Ensure directory exists.
                if (!Directory.Exists(sfd.InitialDirectory))
                    Directory.CreateDirectory(sfd.InitialDirectory);

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        // Backup the save path.
                        SavedSchematicLocation = sfd.FileName;

                        // Export schematic.
                        GeneratedSchematic.Save(sfd.FileName, _worldViewModel.CurrentWorld?.Version ?? WorldConfiguration.CompatibleVersion);

                        // Set the exported schematic directory data.
                        SaveDirectoryData.Text = sfd.FileName;

                        // Display completion message.
                        MessageBox.Show("Schematic has been saved.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Saving Schematic");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Overwrite Existing File

        private void OverwriteExistingFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GeneratedSchematic == null)
                {
                    MessageBox.Show("The schematic data is empty! Generate a pixelart first!");
                    return;
                }

                if (SavedSchematicLocation == null)
                {
                    MessageBox.Show("You need to have previously saved a schematic first!");
                    return;
                }

                try
                {
                    // Export schematic.
                    GeneratedSchematic.Save(SavedSchematicLocation, _worldViewModel.CurrentWorld?.Version ?? WorldConfiguration.CompatibleVersion);

                    // Display completion message.
                    MessageBox.Show("Schematic has been overwritten.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving Schematic");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Drop File

        // Change the background image1 from file drops.
        private void BackgroundImage1_Drop(object sender, System.Windows.DragEventArgs e)
        {
            var data = e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (data != null)
            {
                if (data is string[] filenames && filenames.Length > 0)
                {
                    // Try Loading Images
                    try
                    {
                        BitmapImage bitmap = new();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filenames[0]);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        BackgroundImage1.Source = bitmap;

                        // Define original image source.
                        OriginalImageSource = bitmap;

                        // Set the loaded image data.
                        CurrentImageData.Text = filenames[0];
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                        return;
                    }
                }
            }
        }

        private void BackgroundImage1_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
        }
        #endregion

        #region DEBUG - Export Entire ColorFilter To Schematic

        private void DebugExportColorFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int elementCount = TileWallDataList.Count;
                int sideLength = (int)Math.Ceiling(Math.Sqrt(elementCount));
                Vector2Int32 schematicSize = new(sideLength, sideLength);
                ClipboardBuffer _generatedSchematic = new(schematicSize, true);

                int x = 0, y = 0;
                foreach (var tile in TileWallDataList)
                {
                    if (tile.Type == "Tile")
                    {
                        _generatedSchematic.Tiles[x, y] = new Tile { Type = (ushort)tile.Id, TileColor = (byte)tile.Paint, IsActive = true };
                    }
                    else
                    {
                        _generatedSchematic.Tiles[x, y] = new Tile { Wall = (ushort)tile.Id, WallColor = (byte)tile.Paint, IsActive = false };
                    }

                    x++;
                    if (x >= schematicSize.X)
                    {
                        x = 0;
                        y++;
                    }
                }

                // Render and copy to clipboard
                // TE4: GeneratedSchematic.RenderBuffer();
                // TE4: _worldViewModel.Clipboard.LoadedBuffers.Add(GeneratedSchematic);
                // TE4: _worldViewModel.ClipboardSetActiveCommand.Execute(GeneratedSchematic);

                var bufferRendered = new ClipboardBufferPreview(GeneratedSchematic);
                _worldViewModel.Clipboard.LoadedBuffers.Add(bufferRendered);
                _worldViewModel.ClipboardSetActiveCommand.Execute(bufferRendered);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        // Convert to pixelart.
        #endregion

        #region Grid Options

        // Change the grid color.
        private void PickGridColor_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Windows.Media.Color color = new()
                {
                    A = colorDialog.Color.A,
                    R = colorDialog.Color.R,
                    G = colorDialog.Color.G,
                    B = colorDialog.Color.B
                };

                GridColor = colorDialog.Color;

                // Set the button's foreground color.
                PickGridColor.Background = new SolidColorBrush(color);
            }
        }
        #endregion

        #region Color Filter Tools

        private async void DelNullColors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapSource bitmapSource = null;

                // Access the UI elements in the UI thread.
                Dispatcher.Invoke(() =>
                {
                    bitmapSource = (BitmapSource)BackgroundImage2.Source;
                });

                if (bitmapSource == null)
                {
                    Dispatcher.Invoke(() => MessageBox.Show("BackgroundImage2 does not have a valid image."));
                    return;
                }

                int width = bitmapSource.PixelWidth;
                int height = bitmapSource.PixelHeight;
                int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
                byte[] pixels = new byte[height * stride];
                bitmapSource.CopyPixels(pixels, stride, 0);

                HashSet<Color> usedColors = new();

                await Task.Run(() =>
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index = y * stride + x * 4;
                            byte b = pixels[index];
                            byte g = pixels[index + 1];
                            byte r = pixels[index + 2];
                            byte a = pixels[index + 3];

                            if (a != 0) // Only consider non-transparent pixels.
                            {
                                usedColors.Add(Color.FromArgb(a, r, g, b));
                            }
                        }
                    }

                    Clrs = Clrs.Where(c => usedColors.Contains(c)).ToArray();
                    ClrsTileWallData = ClrsTileWallData.Where(item => usedColors.Contains(ColorTranslator.FromHtml(item.Color))).ToList();

                    // Debugging.
                    // Dispatcher.Invoke(() => MessageBox.Show(ClrsTileWallData.Count.ToString()));
                });

                // Populate filter data in the UI thread.
                Dispatcher.Invoke(() =>
                {
                    TotalColorsData.Content = TileWallDataList.Count;
                    FilteredColorsData.Content = Clrs.Length;

                    // This gets annoying so its disabled.
                    // MessageBox.Show("Unused colors have been removed from the filter.");
                });
            }
            catch (Exception ex)
            {
                // Handle any exceptions and show error message in the UI thread.
                Dispatcher.Invoke(() => MessageBox.Show($"An error occurred: {ex.Message}"));
            }
        }

        // Reset the build color filter.
        private async void ResetColors_Click(object sender, RoutedEventArgs e)
        {
            await BuildColorFilter();

            // This gets annoying so its disabled.
            // MessageBox.Show("The color filter has been reset.");
        }
        #endregion

        #region TileWallData Class Logic

        public class TileWallData
        {
            public string Type { get; set; } // "Tile" or "Wall"
            public int Id { get; set; }
            public string Name { get; set; }
            public int Paint { get; set; }
            public string Color { get; set; }
            public bool BuildSafe { get; set; } // false by default
        }

        public static async Task<List<TileWallData>> ReadDataAsync(string filePath, IProgress<int> progress)
        {
            var dataList = new List<TileWallData>();

            var doc = await Task.Run(() => XDocument.Load(filePath));
            var elements = doc.Descendants().Where(e => e.Name == "Tile" || e.Name == "Wall").ToList();

            int totalElements = elements.Count;
            for (int i = 0; i < totalElements; i++)
            {
                var element = elements[i];
                var data = new TileWallData
                {
                    Type = element.Name.LocalName,
                    Id = int.Parse(element.Attribute("Id").Value),
                    Name = element.Attribute("Name").Value,
                    Paint = int.Parse(element.Attribute("Paint").Value),
                    Color = element.Attribute("Color").Value,
                    BuildSafe = bool.Parse(element.Attribute("BuildSafe").Value)
                };
                dataList.Add(data);

                // Report progress
                progress?.Report((i + 1) * 100 / totalElements);
            }

            return dataList;
        }

        public static async Task<List<TileWallData>> ReadDataAsync(Stream stream, IProgress<int> progress)
        {
            var dataList = new List<TileWallData>();

            var doc = await Task.Run(() => XDocument.Load(stream));
            var elements = doc.Descendants().Where(e => e.Name == "Tile" || e.Name == "Wall").ToList();

            int totalElements = elements.Count;
            for (int i = 0; i < totalElements; i++)
            {
                var element = elements[i];
                var data = new TileWallData
                {
                    Type = element.Name.LocalName,
                    Id = int.Parse(element.Attribute("Id").Value),
                    Name = element.Attribute("Name").Value,
                    Paint = int.Parse(element.Attribute("Paint").Value),
                    Color = element.Attribute("Color").Value,
                    BuildSafe = bool.Parse(element.Attribute("BuildSafe").Value)
                };
                dataList.Add(data);

                // Report progress
                progress?.Report((i + 1) * 100 / totalElements);
            }

            return dataList;
        }

        public static TileWallData ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var data = new TileWallData();
            var typeStartIndex = line.IndexOf("<") + 1;
            var typeEndIndex = line.IndexOf(" ");
            data.Type = line.Substring(typeStartIndex, typeEndIndex - typeStartIndex);

            data.Id = int.Parse(GetAttributeValue(line, "Id"));
            data.Name = GetAttributeValue(line, "Name");
            data.Paint = int.Parse(GetAttributeValue(line, "Paint"));
            data.Color = GetAttributeValue(line, "Color");
            data.BuildSafe = line.Contains("BuildSafe=\"True\"");

            return data;
        }

        public static string GetAttributeValue(string line, string attributeName)
        {
            var attributeStartIndex = line.IndexOf(attributeName + "=\"") + attributeName.Length + 2;
            var attributeEndIndex = line.IndexOf("\"", attributeStartIndex);
            return line.Substring(attributeStartIndex, attributeEndIndex - attributeStartIndex);
        }
        #endregion

        #region Main Pixelart Conversion

        private async Task ConvertPixelArt(bool buildSchematic = false, CancellationToken cancellationToken = default)
        {
            Bitmap btm = new(1, 1);
            Bitmap bBt = new(1, 1);
            Graphics g = null;

            // Disable all controls on main form excluding certain controls.
            SetEnabledState(this, false, new List<string> { "MainWindow", "MainGrid", "BasicConfigurationGroupBox", "BasicConfigurationCanvas", "ConvertToPixelArt", "PixelArtStatisticsGroupBox", "PixelArtStatisticsCanvas", "ProgressBar1" });

            bool isProgressBarChecked = false;
            bool isGatherStatisticsChecked = false;
            bool isNearestNeighborChecked = false;
            bool isBicubicChecked = false;
            bool isLanczosChecked = false;
            bool isHermiteChecked = false;
            bool isSplineChecked = false;
            bool isGaussianChecked = false;
            bool isBackdropChecked = false;
            bool isShowGridChecked = false;
            bool isRotation90Checked = false;
            bool isRotation270Checked = false;
            int AEqualsValue = 3;
            double SigmaEqualsValue = 1.0;

            Dispatcher.Invoke(() =>
            {
                isProgressBarChecked = (bool)ProgressBar.IsChecked;
                isGatherStatisticsChecked = (bool)GatherStatistics.IsChecked;
                isNearestNeighborChecked = (bool)NearestNeighbor.IsChecked;
                isBicubicChecked = (bool)Bicubic.IsChecked;
                isLanczosChecked = (bool)Lanczos.IsChecked;
                isHermiteChecked = (bool)Hermite.IsChecked;
                isSplineChecked = (bool)Spline.IsChecked;
                isGaussianChecked = (bool)Gaussian.IsChecked;
                isBackdropChecked = (bool)Backdrop.IsChecked;
                isShowGridChecked = (bool)ShowGrid.IsChecked;
                isRotation90Checked = (bool)Rotation90.IsChecked;
                isRotation270Checked = (bool)Rotation270.IsChecked;
                AEqualsValue = int.Parse(AEquals.Text);
                SigmaEqualsValue = double.Parse(SigmaEquals.Text);
            });

            try
            {
                // Populate the colorlist with custom definitions.
                // await BuildColorFilter();

                int num = int.Parse(NUDTextBox1.Text);

                btm = new Bitmap(BitmapFromSource((BitmapSource)BackgroundImage1.Source));
                bBt = new Bitmap(btm.Width, btm.Height);

                // Initialize variables for stats and conversion.
                int renderedCount = 0;

                // Reset the progress bar.
                // int _stepValue = 1;

                // Calculate the total number of non-transparent pixels.
                int totalNonTransparentPixels = CountNonTransparentPixels(btm);

                // Calculate the number of blocks (steps) based on the block size 'num'.
                int blocksPerRow = (btm.Width + num - 1) / num;  // Total blocks in a row. // Width.
                int blocksPerColumn = (btm.Height + num - 1) / num;  // Total blocks in a column. // Height.
                int totalBlocks = blocksPerRow * blocksPerColumn;

                // Define new schematic data.
                if (buildSchematic)
                {
                    // StringBuilder sb2 = new();
                    Vector2Int32 schematicSize = new((isRotation90Checked || isRotation270Checked) ? blocksPerColumn + 1 : blocksPerRow + 1, (isRotation90Checked || isRotation270Checked) ? blocksPerRow + 1 : blocksPerColumn + 1);
                    GeneratedSchematic = new(schematicSize, true);
                }
                else
                {
                    // Remove the old schematic.
                    GeneratedSchematic = null;

                    // Disable schematic controls.
                    Dispatcher.Invoke(() =>
                    {
                        CopyToClipboard.IsEnabled = false;
                        OverwriteExistingFile.IsEnabled = false;
                        SaveSchematicToFile.IsEnabled = false;
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    TotalHeightData.Content = (isRotation90Checked || isRotation270Checked) ? blocksPerRow.ToString() : blocksPerColumn.ToString(); // Check if axis is rotated.
                    TotalWidthData.Content = (isRotation90Checked || isRotation270Checked) ? blocksPerColumn.ToString() : blocksPerRow.ToString(); // Check if axis is rotated.
                    TotalBlocksData.Content = "Calculating...";
                });

                if (isProgressBarChecked)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar1.Minimum = 0;
                        ProgressBar1.Value = 0;
                        ProgressBar1.Maximum = totalBlocks;  // Set the maximum value based on the total number of blocks
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressBar1.Minimum = 0;
                        ProgressBar1.Value = 0;
                    });
                }

                try
                {
                    await Task.Run(() =>
                    {
                        using (g = Graphics.FromImage(bBt))
                        {
                            List<Color> block = new();
                            Color final = Color.Lime;

                            int progressCounter = 0;
                            int progressUpdateInterval = Math.Max(totalBlocks / 100, 1); // Update progress every 1%. // Ensure progressUpdateInterval is at least 1.

                            for (int x = 0; x < btm.Width; x += num)
                            {
                                for (int y = 0; y < btm.Height; y += num)
                                {
                                    // Check for cancellation.
                                    cancellationToken.ThrowIfCancellationRequested();

                                    // Get interpolation type.
                                    if (isNearestNeighborChecked)
                                    {
                                        final = NearestNeighborInterpolation(btm, x, y, num, isBackdropChecked);
                                    }
                                    else if (isBicubicChecked)
                                    {
                                        final = BicubicInterpolation(btm, x, y, isBackdropChecked);
                                    }
                                    else if (isLanczosChecked)
                                    {
                                        final = LanczosInterpolation(btm, x, y, num, AEqualsValue, isBackdropChecked); // Lanczos with a=3.
                                    }
                                    else if (isHermiteChecked)
                                    {
                                        final = HermiteInterpolation(btm, x, y, isBackdropChecked);
                                    }
                                    else if (isSplineChecked)
                                    {
                                        final = SplineInterpolation(btm, x, y, num, isBackdropChecked);
                                    }
                                    else if (isGaussianChecked)
                                    {
                                        final = GaussianInterpolation(btm, x, y, SigmaEqualsValue, isBackdropChecked); // Example with sigma=1.0.
                                    }
                                    else // Else use Bilinear (defualt).
                                    {
                                        final = BilinearInterpolation(btm, x, y, num, isBackdropChecked);
                                    }

                                    // Record schematic data. There should not be any transparent tiles.
                                    if (final.A != 0)
                                    {
                                        SolidBrush sb = new(final);
                                        Rectangle rec = new(x, y, num, num);
                                        g.FillRectangle(sb, rec);
                                    }

                                    // Build schematic from final & GeneratedSchematic.
                                    if (buildSchematic)
                                    {
                                        // Define rotation data.
                                        Point rotationData = new();
                                        Dispatcher.Invoke(() =>
                                        {
                                            rotationData = GetRotationData(x / num, y / num, blocksPerRow, blocksPerColumn);
                                        });

                                        if (final.A == 0)
                                        {
                                            // Set schematic data as "air".
                                            GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Type = 0, IsActive = false };
                                        }
                                        else
                                        {
                                            // Compare the hex codes between the "final" color from Interpolation and the filtered color list.
                                            // Convert final color to HTML string with alpha channel as "FF".
                                            string finalHtmlColor = $"#{final.A:X2}{final.R:X2}{final.G:X2}{final.B:X2}";

                                            // Find the first matching TileWallData where the color matches finalHtmlColor
                                            TileWallData matchedTile = ClrsTileWallData
                                                .FirstOrDefault(t => t.Color.Equals(finalHtmlColor, StringComparison.OrdinalIgnoreCase));

                                            if (matchedTile != null)
                                            {
                                                // Build schematic.
                                                if (matchedTile.Type == "Tile")
                                                {
                                                    GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Type = (ushort)matchedTile.Id, TileColor = (byte)matchedTile.Paint, IsActive = true };
                                                }
                                                else if (matchedTile.Type == "Wall")
                                                {
                                                    GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Wall = (ushort)matchedTile.Id, WallColor = (byte)matchedTile.Paint, IsActive = false };
                                                }
                                            } // There should never be a case where it does not find the tile!
                                        }
                                    }

                                    // Gather statistics.
                                    if (isGatherStatisticsChecked && final.A != 0)
                                        renderedCount++;

                                    // Progress progress bar.
                                    if (isProgressBarChecked)
                                    {
                                        progressCounter++;
                                        if (progressCounter % progressUpdateInterval == 0)
                                        {
                                            Dispatcher.BeginInvoke(() =>
                                            {
                                                ProgressBar1.Value = Math.Min(ProgressBar1.Value + progressUpdateInterval, ProgressBar1.Maximum);
                                            });
                                        }
                                    }
                                }
                            }

                            // Grid function.
                            if (isShowGridChecked)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    HighlightCells(g, bBt.Width, num, int.Parse(NUDTextBox3.Text), int.Parse(NUDTextBox4.Text));
                                });
                            }

                            // Apply rotation if needed
                            Bitmap rotatedImage = new(bBt);
                            Dispatcher.Invoke(() =>
                            {
                                ApplyRotation(rotatedImage);
                            });

                            Dispatcher.Invoke(() =>
                            {
                                // Render Box
                                BackgroundImage2.Source = BitmapToImageSource(rotatedImage);

                                // Update the "schematic".
                                // if (buildSchematic)
                                // richTextBox2.Text = sb2.ToString();

                                // Populate Stats
                                if (isGatherStatisticsChecked)
                                {
                                    // TotalHeightData.Content = (isRotation90Checked || isRotation270Checked) ? blocksPerRow.ToString() : blocksPerColumn.ToString(); // Check if axis is rotated.
                                    // TotalWidthData.Content = (isRotation90Checked || isRotation270Checked) ? blocksPerColumn.ToString() : blocksPerRow.ToString(); // Check if axis is rotated.
                                    TotalBlocksData.Content = renderedCount.ToString();
                                }

                                // Enable schematic controls.
                                if (buildSchematic)
                                {
                                    CopyToClipboard.IsEnabled = true;
                                    OverwriteExistingFile.IsEnabled = true;
                                    SaveSchematicToFile.IsEnabled = true;
                                }
                            });
                        }

                        // End section.
                    }, cancellationToken); // Pass the cancellation token to Task.Run.
                }
                catch (OperationCanceledException)
                {
                    // Opteration was cancled, show message.
                    MessageBox.Show("Rendering operation was cancled!");

                    // Update some controls.
                    TotalBlocksData.Content = "0";
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar1.Value = 0;
                    MessageBox.Show("ERROR: The color filter formatting is invalid!" + ex.ToString());
                    System.Windows.Forms.Clipboard.SetText(ex.ToString());
                });
            }

            // Update Progress
            if (isProgressBarChecked)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar1.Value = ProgressBar1.Maximum;
                });
            }

            // Enable all controls on main form.
            SetEnabledState(this, true);

            // Disable schematic controls.
            if (!buildSchematic)
            {
                Dispatcher.Invoke(() =>
                {
                    CopyToClipboard.IsEnabled = false;
                    OverwriteExistingFile.IsEnabled = false;
                    SaveSchematicToFile.IsEnabled = false;
                });
            }
        }
        #endregion

        #region Supporting Conversion Logic

        #region Scaling Interpolations

        private Color NearestNeighborInterpolation(Bitmap bmp, int x, int y, int num, bool includeTransparent)
        {
            int nx = Math.Min(x + num / 2, bmp.Width - 1);
            int ny = Math.Min(y + num / 2, bmp.Height - 1);
            Color nearestColor = bmp.GetPixel(nx, ny);

            if (includeTransparent || nearestColor.A != 0)
            {
                if (includeTransparent && nearestColor.A == 0)
                    nearestColor = Color.Lime; // Default color for transparency, or you can choose another default color.

                return Clr(new Color[] { nearestColor });
            }
            return nearestColor;
        }

        private Color BilinearInterpolation(Bitmap bmp, int x, int y, int num, bool includeTransparent)
        {
            List<Color> block = new();

            for (int v = 0; v < num; v++)
            {
                for (int c = 0; c < num; c++)
                {
                    if (x + v < bmp.Width && y + c < bmp.Height)
                    {
                        Color color = bmp.GetPixel(x + v, y + c);
                        if (includeTransparent || color.A != 0)
                        {
                            if (includeTransparent && color.A == 0)
                                color = Color.Lime; // Default color for transparency, or you can choose another default color.
                            block.Add(color);
                        }
                    }
                }
            }

            if (block.Count == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            return Clr(block.ToArray());
        }

        private Color BicubicInterpolation(Bitmap bmp, int x, int y, bool includeTransparent)
        {
            double[] dx = new double[4];
            double[] dy = new double[4];

            for (int i = 0; i < 4; i++)
            {
                dx[i] = CubicPolynomial(i - 1);
                dy[i] = CubicPolynomial(i - 1);
            }

            double r = 0, g = 0, b = 0;
            double sum = 0;
            for (int i = -1; i <= 2; i++)
            {
                for (int j = -1; j <= 2; j++)
                {
                    int px = Math.Min(Math.Max(x + i, 0), bmp.Width - 1);
                    int py = Math.Min(Math.Max(y + j, 0), bmp.Height - 1);
                    Color pixel = bmp.GetPixel(px, py);

                    if (includeTransparent || pixel.A != 0)
                    {
                        if (includeTransparent && pixel.A == 0)
                            pixel = Color.Lime; // Default color for transparency, or you can choose another default color.

                        double coeff = dx[i + 1] * dy[j + 1];
                        r += pixel.R * coeff;
                        g += pixel.G * coeff;
                        b += pixel.B * coeff;
                        sum += coeff;
                    }
                }
            }

            if (sum == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
            return Clr(new Color[] { interpolatedColor });
        }

        private Color LanczosInterpolation(Bitmap bmp, int x, int y, int num, int a, bool includeTransparent)
        {
            double r = 0, g = 0, b = 0;
            double sum = 0;

            for (int i = -a + 1; i <= a; i++)
            {
                for (int j = -a + 1; j <= a; j++)
                {
                    int px = Math.Min(Math.Max(x + i, 0), bmp.Width - 1);
                    int py = Math.Min(Math.Max(y + j, 0), bmp.Height - 1);
                    Color pixel = bmp.GetPixel(px, py);

                    if (includeTransparent || pixel.A != 0)
                    {
                        if (includeTransparent && pixel.A == 0)
                            pixel = Color.Lime; // Default color for transparency, or you can choose another default color.

                        double lanczosWeight = LanczosKernel(i / (double)num, a) * LanczosKernel(j / (double)num, a);
                        r += pixel.R * lanczosWeight;
                        g += pixel.G * lanczosWeight;
                        b += pixel.B * lanczosWeight;
                        sum += lanczosWeight;
                    }
                }
            }

            if (sum == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
            return Clr(new Color[] { interpolatedColor });
        }

        private Color HermiteInterpolation(Bitmap bmp, int x, int y, bool includeTransparent)
        {
            double[] dx = new double[2];
            double[] dy = new double[2];

            dx[0] = HermitePolynomial(0);
            dx[1] = HermitePolynomial(1);

            dy[0] = HermitePolynomial(0);
            dy[1] = HermitePolynomial(1);

            double r = 0, g = 0, b = 0;
            double sum = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int px = Math.Min(Math.Max(x + i, 0), bmp.Width - 1);
                    int py = Math.Min(Math.Max(y + j, 0), bmp.Height - 1);
                    Color pixel = bmp.GetPixel(px, py);

                    if (includeTransparent || pixel.A != 0)
                    {
                        if (includeTransparent && pixel.A == 0)
                            pixel = Color.Lime; // Default color for transparency, or you can choose another default color.

                        double coeff = dx[i] * dy[j];
                        r += pixel.R * coeff;
                        g += pixel.G * coeff;
                        b += pixel.B * coeff;
                        sum += coeff;
                    }
                }
            }

            if (sum == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
            return Clr(new Color[] { interpolatedColor });
        }

        private Color SplineInterpolation(Bitmap bmp, int x, int y, int num, bool includeTransparent)
        {
            List<Color> block = new();
            double r = 0, g = 0, b = 0;
            double sum = 0;

            for (int i = -1; i <= 2; i++)
            {
                for (int j = -1; j <= 2; j++)
                {
                    int px = Math.Min(Math.Max(x + i, 0), bmp.Width - 1);
                    int py = Math.Min(Math.Max(y + j, 0), bmp.Height - 1);
                    Color pixel = bmp.GetPixel(px, py);

                    if (includeTransparent || pixel.A != 0)
                    {
                        if (includeTransparent && pixel.A == 0)
                            pixel = Color.Lime; // Default color for transparency, or you can choose another default color.

                        double coeff = SplineKernel(i / (double)num) * SplineKernel(j / (double)num);
                        r += pixel.R * coeff;
                        g += pixel.G * coeff;
                        b += pixel.B * coeff;
                        sum += coeff;
                        block.Add(pixel);
                    }
                }
            }

            if (sum == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
            return Clr(new Color[] { interpolatedColor });
        }

        private Color GaussianInterpolation(Bitmap bmp, int x, int y, double sigma, bool includeTransparent)
        {
            double r = 0, g = 0, b = 0;
            double sum = 0;

            int radius = (int)Math.Ceiling(3 * sigma); // 3-sigma rule to determine kernel size

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int px = Math.Min(Math.Max(x + i, 0), bmp.Width - 1);
                    int py = Math.Min(Math.Max(y + j, 0), bmp.Height - 1);
                    Color pixel = bmp.GetPixel(px, py);

                    if (includeTransparent || pixel.A != 0)
                    {
                        if (includeTransparent && pixel.A == 0)
                            pixel = Color.Lime; // Default color for transparency, or you can choose another default color.

                        double weight = GaussianKernel(i, j, sigma);
                        r += pixel.R * weight;
                        g += pixel.G * weight;
                        b += pixel.B * weight;
                        sum += weight;
                    }
                }
            }

            if (sum == 0)
            {
                // Return transparent if no valid pixels are found.
                return Color.Transparent;
            }

            Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
            return Clr(new Color[] { interpolatedColor });
        }

        private double HermitePolynomial(double x)
        {
            if (x == 0) return 1.0;
            if (x < 0) x = -x;
            double x3 = x * x * x;
            double x2 = x * x;
            return (2 * x3) - (3 * x2) + 1;
        }

        private double LanczosKernel(double x, int a)
        {
            if (x == 0) return 1.0;
            if (x < -a || x > a) return 0.0;

            x *= Math.PI;
            return a * Math.Sin(x) * Math.Sin(x / a) / (x * x);
        }

        private double CubicPolynomial(double x)
        {
            x = Math.Abs(x);
            if (x <= 1)
                return 1.5 * x * x * x - 2.5 * x * x + 1;
            else if (x < 2)
                return -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2;
            else
                return 0;
        }

        private double SplineKernel(double x)
        {
            x = Math.Abs(x);
            if (x <= 1)
            {
                return 1.0 - 2.0 * x * x + x * x * x;
            }
            else if (x < 2)
            {
                return 4.0 - 8.0 * x + 5.0 * x * x - x * x * x;
            }
            return 0.0;
        }

        private double GaussianKernel(int x, int y, double sigma)
        {
            double expPart = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
            double normalPart = 1 / (2 * Math.PI * sigma * sigma);
            return normalPart * expPart;
        }

        private int Clamp(double value)
        {
            return (int)Math.Max(0, Math.Min(255, value));
        }
        #endregion

        #region Build Color Filter

        public async Task BuildColorFilter()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapColors.xml");
            TileWallDataList = new List<TileWallData> { };

            using (var stream = File.OpenRead(filePath))
            {
                TileWallDataList = await ReadDataAsync(stream, null);
            }

            await Task.Run(() =>
            {
                bool useTiles = true;
                bool useWalls = true;
                bool buildSafe = true;
                bool uniqueColors = true;

                Dispatcher.Invoke(() =>
                {
                    useTiles = (bool)UseTiles.IsChecked;
                    useWalls = (bool)UseWalls.IsChecked;
                    buildSafe = (bool)BuildSafe.IsChecked;
                    uniqueColors = (bool)UniqueColors.IsChecked;
                });

                var filteredList = TileWallDataList
                    .Where(data =>
                        ((useTiles && data.Type == "Tile") || (useWalls && data.Type == "Wall")) &&
                        (!buildSafe || data.BuildSafe)
                    )
                    .ToList();

                if (uniqueColors)
                {
                    filteredList = filteredList
                        .GroupBy(data => data.Color)
                        .Select(group => group.First())
                        .ToList();
                }

                // Populate filter data.
                Dispatcher.Invoke(() =>
                {
                    TotalColorsData.Content = TileWallDataList.Count;
                    FilteredColorsData.Content = filteredList.Count;
                });

                // Set the filtered colors count.
                int colorCount = 0;
                Clrs = new Color[filteredList.Count];
                foreach (var item in filteredList)
                {
                    try
                    {
                        Clrs[colorCount] = ColorTranslator.FromHtml(item.Color);
                    }
                    catch
                    {
                        Clrs[colorCount] = Color.Transparent;
                    }
                    colorCount++;
                }

                // Create a backup of the new filtered colors for exporting schematics.
                ClrsTileWallData = filteredList;
            });
        }
        #endregion

        #region Color Comparator

        public Color Clr(Color[] cs)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            for (int i = 0; i < cs.Length; i++)
            {
                r += cs[i].R;
                g += cs[i].G;
                b += cs[i].B;
            }

            r /= cs.Length;
            g /= cs.Length;
            b /= cs.Length;

            int near = 1000;
            int ind = 0;

            for (int cl = 0; cl < Clrs.Length; cl++)
            {
                int valR = (Clrs[cl].R - r);
                int valG = (Clrs[cl].G - g);
                int valB = (Clrs[cl].B - b);

                if (valR < 0) valR = -valR;
                if (valG < 0) valG = -valG;
                if (valB < 0) valB = -valB;

                int total = valR + valG + valB;

                if (total < near)
                {
                    ind = cl;
                    near = total;
                }
            }

            Color c = Clrs[ind];

            return c;
        }
        #endregion

        #region Rotation & Grid Helpers

        private Point GetRotationData(int xInput, int yInput, int maxWidth, int maxHeight)
        {
            int x, y;
            if ((bool)XAxis.IsChecked) // Axis rotation
            {
                if ((bool)Rotation0.IsChecked)
                {
                    x = xInput;
                    y = yInput;
                }
                else if ((bool)Rotation90.IsChecked)
                {
                    x = maxHeight - 1 - yInput;
                    y = xInput;
                }
                else if ((bool)Rotation180.IsChecked)
                {
                    x = maxWidth - 1 - xInput;
                    y = maxHeight - 1 - yInput;
                }
                else if ((bool)Rotation270.IsChecked)
                {
                    x = yInput;
                    y = maxWidth - 1 - xInput;
                }
                else
                {
                    throw new InvalidOperationException("No valid rotation option selected.");
                }
            }
            else // No axis rotation
            {
                if ((bool)Rotation0.IsChecked)
                {
                    x = maxWidth - 1 - xInput;
                    y = yInput;
                }
                else if ((bool)Rotation90.IsChecked)
                {
                    x = yInput;
                    y = xInput;
                }
                else if ((bool)Rotation180.IsChecked)
                {
                    x = xInput;
                    y = maxHeight - 1 - yInput;
                }
                else if ((bool)Rotation270.IsChecked)
                {
                    x = maxWidth - 1 - yInput;
                    y = maxHeight - 1 - xInput;
                }
                else
                {
                    throw new InvalidOperationException("No valid rotation option selected.");
                }
            }

            return new Point(x, y);
        }

        // Grid.
        private void HighlightCells(Graphics g, int numOfCells, int cellSize, int offsetX, int offsetY)
        {
            Pen p = new(GridColor); // Maybe add controls to thickness?
            for (int y = 0; y < numOfCells; ++y)
            {
                g.DrawLine(p, 0, (y * cellSize) + offsetY, numOfCells * cellSize, (y * cellSize) + offsetY);
            }
            for (int x = 0; x < numOfCells; ++x)
            {
                g.DrawLine(p, (x * cellSize) + offsetX, 0, (x * cellSize) + offsetX, numOfCells * cellSize);
            }
        }

        private void ApplyRotation(Bitmap temp)
        {
            // Rotate schematic.
            if ((bool)Rotation90.IsChecked)
                temp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            else if ((bool)Rotation180.IsChecked)
                temp.RotateFlip(RotateFlipType.Rotate180FlipNone);
            else if ((bool)Rotation270.IsChecked)
                temp.RotateFlip(RotateFlipType.Rotate270FlipNone);

            // Flip the Y axis.
            if ((bool)YAxis.IsChecked)
                temp.RotateFlip(RotateFlipType.RotateNoneFlipX);
        }

        private int CountNonTransparentPixels(Bitmap bitmap)
        {
            int nonTransparentPixelCount = 0;

            // Lock the bitmap's bits.
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                // Get the address of the first line.
                IntPtr ptr = bitmapData.Scan0;
                int bytes = Math.Abs(bitmapData.Stride) * bitmap.Height;
                byte[] rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Iterate through the pixel data.
                for (int i = 0; i < rgbValues.Length; i += 4)
                {
                    // The alpha channel is at index 3 in ARGB format.
                    if (rgbValues[i + 3] != 0)
                    {
                        nonTransparentPixelCount++;
                    }
                }
            }
            finally
            {
                // Unlock the bits.
                bitmap.UnlockBits(bitmapData);
            }

            return nonTransparentPixelCount;
        }
        #endregion

        #region Bitmap Converters

        public Bitmap BitmapFromSource(BitmapSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new())
            {
                BitmapEncoder enc = new PngBitmapEncoder(); // Use PngBitmapEncoder for transparency support
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                outStream.Seek(0, SeekOrigin.Begin);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        // Convert Bitmap to ImageSource.
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();

            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapimage = new();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            return bitmapimage;
        }
        #endregion

        #region Disable & Enable WPF Controls

        public void SetEnabledState(UIElement element, bool isEnabled, IEnumerable<string> controlsToExclude = null)
        {
            if (element == null) return;

            if (element is Control control)
            {
                // Check if this control should be excluded.
                if (controlsToExclude == null || !controlsToExclude.Contains(control.Name))
                {
                    // Set the enabled state of the control.
                    control.IsEnabled = isEnabled;
                }
            }

            // Recursively process child elements.
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i) as UIElement;
                SetEnabledState(child, isEnabled, controlsToExclude);
            }
        }
        #endregion

        #endregion
    }
}
