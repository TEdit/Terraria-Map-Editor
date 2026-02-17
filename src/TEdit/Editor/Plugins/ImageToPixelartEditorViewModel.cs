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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Editor.Clipboard;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.ViewModel;

using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using Tile = TEdit.Terraria.Tile;

namespace TEdit.Editor.Plugins;

[IReactiveObject]
public partial class ImageToPixelartEditorViewModel
{
    private readonly WorldViewModel _worldViewModel;
    private CancellationTokenSource? _cancellationTokenSource;

    #region Reactive Properties

    // Enum properties for radio button groups
    [Reactive] private ScalingMode _scalingMode = ScalingMode.Bilinear;
    [Reactive] private RotationAngle _rotationAngle = RotationAngle.None;
    [Reactive] private WorldAxis _worldAxis = WorldAxis.XAxis;

    // Checkbox boolean properties
    [Reactive] private bool _generateSchematic;
    [Reactive] private bool _gatherStatistics = true;
    [Reactive] private bool _showProgressBar;
    [Reactive] private bool _useTiles = true;
    [Reactive] private bool _useWalls = true;
    [Reactive] private bool _buildSafe = true;
    [Reactive] private bool _uniqueColors = true;
    [Reactive] private bool _showGrid;
    [Reactive] private bool _backdrop;

    // Numeric input properties
    [Reactive] private double _spacing = 4;
    [Reactive] private double _ratioValue;
    [Reactive] private int _lanczosA = 3;
    [Reactive] private double _gaussianSigma = 1.0;
    [Reactive] private double _gridYOffset;
    [Reactive] private double _gridXOffset;

    // Display text properties
    [Reactive] private string _newRatioPercentLevelData = "0";
    [Reactive] private string _totalColorsData = "0";
    [Reactive] private string _filteredColorsData = "0";
    [Reactive] private string _totalHeightData = "0";
    [Reactive] private string _totalWidthData = "0";
    [Reactive] private string _totalBlocksData = "0";
    [Reactive] private string _currentImageData = "Default";
    [Reactive] private string _saveDirectoryData = "None";

    // Image properties
    [Reactive] private System.Windows.Media.ImageSource? _backgroundImage1Source;
    [Reactive] private System.Windows.Media.ImageSource? _backgroundImage2Source;
    [Reactive] private System.Windows.Media.ImageSource? _originalImageSource;

    // Progress properties
    [Reactive] private double _progressValue;
    [Reactive] private double _progressMaximum = 100;

    // UI state properties
    [Reactive] private bool _isConvertEnabled = true;
    [Reactive] private bool _isOpenNewImageEnabled = true;
    [Reactive] private bool _isRefreshRatioEnabled = true;
    [Reactive] private bool _isRatioNumberBoxEnabled = true;
    [Reactive] private bool _isCopyToClipboardEnabled;
    [Reactive] private bool _isSaveSchematicEnabled;
    [Reactive] private bool _isOverwriteEnabled;
    [Reactive] private bool _isLanczosAEnabled;
    [Reactive] private bool _isSigmaEnabled;
    [Reactive] private bool _isConverting;
    [Reactive] private string _convertButtonText = "Convert To Pixel Art";
    [Reactive] private System.Windows.Media.Brush? _gridColorBrush;

    // Data properties
    [Reactive] private Color _gridColor = Color.Red;
    [Reactive] private List<TileWallData>? _tileWallDataList;
    [Reactive] private Color[] _clrs = new Color[1];
    [Reactive] private List<TileWallData>? _clrsTileWallData;
    [Reactive] private ClipboardBuffer? _generatedSchematic;
    [Reactive] private string? _savedSchematicLocation;

    #endregion

    #region Interactions

    public Interaction<Unit, string?> OpenFileInteraction { get; } = new();
    public Interaction<SaveImageRequest, string?> SaveImageInteraction { get; } = new();
    public Interaction<SaveSchematicRequest, string?> SaveSchematicInteraction { get; } = new();
    public Interaction<Unit, Color?> PickColorInteraction { get; } = new();
    public Interaction<string, Unit> ShowMessageInteraction { get; } = new();
    public Interaction<bool, Unit> CloseDialogInteraction { get; } = new();

    #endregion

    #region Constructor

    public ImageToPixelartEditorViewModel(WorldViewModel worldViewModel)
    {
        _worldViewModel = worldViewModel;
        SetupObservables();
        LoadDefaultImage();
        _ = BuildColorFilter();
    }

    private void LoadDefaultImage()
    {
        try
        {
            var defaultImage = new BitmapImage(new Uri("pack://application:,,,/TEdit;component/Images/Pixelart/globeTest.png"));
            BackgroundImage1Source = defaultImage;
            OriginalImageSource = defaultImage;
        }
        catch
        {
            // Default image not found, leave as null
        }
    }

    private void SetupObservables()
    {
        // Update Lanczos/Gaussian textbox enabled state based on scaling mode
        this.WhenAnyValue(x => x.ScalingMode)
            .Subscribe(mode =>
            {
                IsLanczosAEnabled = mode == ScalingMode.Lanczos;
                IsSigmaEnabled = mode == ScalingMode.Gaussian;
            });

        // Rebuild color filter when filter settings change
        this.WhenAnyValue(
                x => x.UseTiles,
                x => x.UseWalls,
                x => x.BuildSafe,
                x => x.UniqueColors)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await BuildColorFilter());

        // Update GridColorBrush when GridColor changes
        this.WhenAnyValue(x => x.GridColor)
            .Subscribe(color =>
            {
                GridColorBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            });
    }

    #endregion

    #region Commands

    [ReactiveCommand]
    private async Task OpenNewImage()
    {
        var filePath = await OpenFileInteraction.Handle(Unit.Default);
        if (filePath != null)
        {
            LoadImageFromPath(filePath);
        }
    }

    [ReactiveCommand]
    private async Task RefreshRatio()
    {
        try
        {
            NewRatioPercentLevelData = "Calc...";
            IsConvertEnabled = false;
            IsOpenNewImageEnabled = false;
            IsRefreshRatioEnabled = false;
            IsRatioNumberBoxEnabled = false;

            var zoomFactor = (int)RatioValue;

            if (OriginalImageSource == null) return;

            await Task.Run(() =>
            {
                Bitmap? resizedImage = null;
                var bitmapImage = new Bitmap(BitmapFromSource((BitmapSource)OriginalImageSource));

                if (zoomFactor == 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NewRatioPercentLevelData = ((int)RatioValue).ToString();
                        BackgroundImage1Source = OriginalImageSource;
                    });
                }
                else if (zoomFactor > 0)
                {
                    Size newSize = new(bitmapImage.Width * zoomFactor, bitmapImage.Height * zoomFactor);
                    resizedImage = new Bitmap(bitmapImage, newSize);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NewRatioPercentLevelData = ((int)RatioValue).ToString();
                        BackgroundImage1Source = BitmapToImageSource(resizedImage);
                    });
                }
                else
                {
                    var absZoom = Math.Abs(zoomFactor);
                    Size newSize = new(bitmapImage.Width / absZoom, bitmapImage.Height / absZoom);
                    resizedImage = new Bitmap(bitmapImage, newSize);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NewRatioPercentLevelData = ((int)RatioValue).ToString();
                        BackgroundImage1Source = BitmapToImageSource(resizedImage);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            await ShowMessageInteraction.Handle($"ERROR: {ex.Message}");
        }
        finally
        {
            IsConvertEnabled = true;
            IsOpenNewImageEnabled = true;
            IsRefreshRatioEnabled = true;
            IsRatioNumberBoxEnabled = true;
        }
    }

    [ReactiveCommand]
    private async Task ConvertToPixelArt()
    {
        if (IsConverting)
        {
            _cancellationTokenSource?.Cancel();
            return;
        }

        if (ClrsTileWallData == null || ClrsTileWallData.Count == 0)
        {
            await ShowMessageInteraction.Handle("The color filter is zero. Adjust your settings.");
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        IsConverting = true;
        ConvertButtonText = "Cancel Conversion";

        try
        {
            await ConvertPixelArtInternal(GenerateSchematic, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            await ShowMessageInteraction.Handle("Rendering was cancelled.");
        }
        finally
        {
            IsConverting = false;
            ConvertButtonText = "Convert To Pixel Art";
        }
    }

    [ReactiveCommand]
    private async Task SaveImage()
    {
        if (BackgroundImage2Source == null)
        {
            await ShowMessageInteraction.Handle("ERROR: You need to generate a pixelart first!");
            return;
        }

        var request = new SaveImageRequest();
        var filePath = await SaveImageInteraction.Handle(request);

        if (filePath != null)
        {
            try
            {
                var bitmap = BitmapFromSource((BitmapSource)BackgroundImage2Source);
                var format = GetImageFormatFromPath(filePath);

                if (format == ImageFormat.Jpeg)
                {
                    var jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                    bitmap.Save(filePath, jpegCodec, encoderParameters);
                }
                else
                {
                    bitmap.Save(filePath, format);
                }

                await ShowMessageInteraction.Handle("Image Completed.");
            }
            catch (Exception ex)
            {
                await ShowMessageInteraction.Handle($"Error saving image: {ex.Message}");
            }
        }
    }

    [ReactiveCommand]
    private async Task CopyToClipboard()
    {
        if (GeneratedSchematic == null)
        {
            await ShowMessageInteraction.Handle("The schematic data is empty! Generate a pixelart first!");
            return;
        }

        if (_worldViewModel.CurrentWorld == null)
        {
            await ShowMessageInteraction.Handle("The 'Copy To Clipboard' feature requires a world to be loaded!");
            return;
        }

        try
        {
            var bufferRendered = new ClipboardBufferPreview(GeneratedSchematic);
            _worldViewModel.Clipboard.LoadedBuffers.Add(bufferRendered);
            _worldViewModel.ClipboardSetActiveCommand.Execute(bufferRendered);
            _worldViewModel.SelectedTabIndex = 3;

            await ShowMessageInteraction.Handle("Schematic copied to clipboard.");
        }
        catch (Exception ex)
        {
            await ShowMessageInteraction.Handle(ex.ToString());
        }
    }

    [ReactiveCommand]
    private async Task SaveSchematicToFile()
    {
        if (GeneratedSchematic == null)
        {
            await ShowMessageInteraction.Handle("The schematic data is empty! Generate a pixelart first!");
            return;
        }

        var request = new SaveSchematicRequest();
        var filePath = await SaveSchematicInteraction.Handle(request);

        if (filePath != null)
        {
            try
            {
                SavedSchematicLocation = filePath;
                GeneratedSchematic.Save(filePath, _worldViewModel.CurrentWorld?.Version ?? WorldConfiguration.CompatibleVersion);
                SaveDirectoryData = filePath;
                await ShowMessageInteraction.Handle("Schematic has been saved.");
            }
            catch (Exception ex)
            {
                await ShowMessageInteraction.Handle($"Error Saving Schematic: {ex.Message}");
            }
        }
    }

    [ReactiveCommand]
    private async Task OverwriteExistingFile()
    {
        if (GeneratedSchematic == null)
        {
            await ShowMessageInteraction.Handle("The schematic data is empty! Generate a pixelart first!");
            return;
        }

        if (SavedSchematicLocation == null)
        {
            await ShowMessageInteraction.Handle("You need to have previously saved a schematic first!");
            return;
        }

        try
        {
            GeneratedSchematic.Save(SavedSchematicLocation, _worldViewModel.CurrentWorld?.Version ?? WorldConfiguration.CompatibleVersion);
            await ShowMessageInteraction.Handle("Schematic has been overwritten.");
        }
        catch (Exception ex)
        {
            await ShowMessageInteraction.Handle($"Error Saving Schematic: {ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task PickGridColor()
    {
        var color = await PickColorInteraction.Handle(Unit.Default);
        if (color.HasValue)
        {
            GridColor = color.Value;
        }
    }

    [ReactiveCommand]
    private async Task DelNullColors()
    {
        if (BackgroundImage2Source == null) return;

        try
        {
            var bitmapSource = (BitmapSource)BackgroundImage2Source;
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

                        if (a != 0)
                        {
                            usedColors.Add(Color.FromArgb(a, r, g, b));
                        }
                    }
                }

                Clrs = Clrs.Where(c => usedColors.Contains(c)).ToArray();
                ClrsTileWallData = ClrsTileWallData?.Where(item => usedColors.Contains(ColorTranslator.FromHtml(item.Color))).ToList();
            });

            TotalColorsData = TileWallDataList?.Count.ToString() ?? "0";
            FilteredColorsData = Clrs.Length.ToString();
        }
        catch (Exception ex)
        {
            await ShowMessageInteraction.Handle($"An error occurred: {ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task ResetColors()
    {
        await BuildColorFilter();
    }

    [ReactiveCommand]
    private async Task Cancel()
    {
        await CloseDialogInteraction.Handle(false);
    }

    #endregion

    #region Public Methods

    public void LoadImageFromPath(string filePath)
    {
        try
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            BackgroundImage1Source = bitmap;
            OriginalImageSource = bitmap;
            CurrentImageData = filePath;
        }
        catch (Exception ex)
        {
            _ = ShowMessageInteraction.Handle($"Error loading image: {ex.Message}");
        }
    }

    #endregion

    #region Color Filter Logic

    public async Task BuildColorFilter()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapColors.xml");
        TileWallDataList = new List<TileWallData>();

        using (var stream = File.OpenRead(filePath))
        {
            TileWallDataList = await ReadDataAsync(stream, null);
        }

        await Task.Run(() =>
        {
            var filteredList = TileWallDataList
                .Where(data =>
                    ((UseTiles && data.Type == "Tile") || (UseWalls && data.Type == "Wall")) &&
                    (!BuildSafe || data.BuildSafe)
                )
                .ToList();

            if (UniqueColors)
            {
                filteredList = filteredList
                    .GroupBy(data => data.Color)
                    .Select(group => group.First())
                    .ToList();
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                TotalColorsData = TileWallDataList.Count.ToString();
                FilteredColorsData = filteredList.Count.ToString();
            });

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

            ClrsTileWallData = filteredList;
        });
    }

    #endregion

    #region Pixel Art Conversion

    private async Task ConvertPixelArtInternal(bool buildSchematic, CancellationToken cancellationToken)
    {
        if (BackgroundImage1Source == null) return;

        Bitmap btm = new(1, 1);
        Bitmap bBt = new(1, 1);

        try
        {
            int num = (int)Spacing;
            btm = new Bitmap(BitmapFromSource((BitmapSource)BackgroundImage1Source));
            bBt = new Bitmap(btm.Width, btm.Height);

            int renderedCount = 0;
            int blocksPerRow = (btm.Width + num - 1) / num;
            int blocksPerColumn = (btm.Height + num - 1) / num;
            int totalBlocks = blocksPerRow * blocksPerColumn;

            bool isRotation90Or270 = RotationAngle == RotationAngle.Rotate90 || RotationAngle == RotationAngle.Rotate270;

            if (buildSchematic)
            {
                Vector2Int32 schematicSize = new(
                    isRotation90Or270 ? blocksPerColumn + 1 : blocksPerRow + 1,
                    isRotation90Or270 ? blocksPerRow + 1 : blocksPerColumn + 1);
                GeneratedSchematic = new ClipboardBuffer(schematicSize, true);
            }
            else
            {
                GeneratedSchematic = null;
                IsCopyToClipboardEnabled = false;
                IsOverwriteEnabled = false;
                IsSaveSchematicEnabled = false;
            }

            TotalHeightData = isRotation90Or270 ? blocksPerRow.ToString() : blocksPerColumn.ToString();
            TotalWidthData = isRotation90Or270 ? blocksPerColumn.ToString() : blocksPerRow.ToString();
            TotalBlocksData = "Calculating...";

            if (ShowProgressBar)
            {
                ProgressValue = 0;
                ProgressMaximum = totalBlocks;
            }
            else
            {
                ProgressValue = 0;
            }

            await Task.Run(() =>
            {
                using var g = Graphics.FromImage(bBt);
                Color final = Color.Lime;

                int progressCounter = 0;
                int progressUpdateInterval = Math.Max(totalBlocks / 100, 1);

                for (int x = 0; x < btm.Width; x += num)
                {
                    for (int y = 0; y < btm.Height; y += num)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        final = ScalingMode switch
                        {
                            ScalingMode.NearestNeighbor => NearestNeighborInterpolation(btm, x, y, num, Backdrop),
                            ScalingMode.Bicubic => BicubicInterpolation(btm, x, y, Backdrop),
                            ScalingMode.Lanczos => LanczosInterpolation(btm, x, y, num, LanczosA, Backdrop),
                            ScalingMode.Hermite => HermiteInterpolation(btm, x, y, Backdrop),
                            ScalingMode.Spline => SplineInterpolation(btm, x, y, num, Backdrop),
                            ScalingMode.Gaussian => GaussianInterpolation(btm, x, y, GaussianSigma, Backdrop),
                            _ => BilinearInterpolation(btm, x, y, num, Backdrop)
                        };

                        if (final.A != 0)
                        {
                            using SolidBrush sb = new(final);
                            Rectangle rec = new(x, y, num, num);
                            g.FillRectangle(sb, rec);
                        }

                        if (buildSchematic && GeneratedSchematic != null)
                        {
                            Point rotationData = GetRotationData(x / num, y / num, blocksPerRow, blocksPerColumn);

                            if (final.A == 0)
                            {
                                GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Type = 0, IsActive = false };
                            }
                            else
                            {
                                string finalHtmlColor = $"#{final.A:X2}{final.R:X2}{final.G:X2}{final.B:X2}";
                                TileWallData? matchedTile = ClrsTileWallData?
                                    .FirstOrDefault(t => t.Color.Equals(finalHtmlColor, StringComparison.OrdinalIgnoreCase));

                                if (matchedTile != null)
                                {
                                    if (matchedTile.Type == "Tile")
                                    {
                                        GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Type = (ushort)matchedTile.Id, TileColor = (byte)matchedTile.Paint, IsActive = true };
                                    }
                                    else if (matchedTile.Type == "Wall")
                                    {
                                        GeneratedSchematic.Tiles[rotationData.X, rotationData.Y] = new Tile { Wall = (ushort)matchedTile.Id, WallColor = (byte)matchedTile.Paint, IsActive = false };
                                    }
                                }
                            }
                        }

                        if (GatherStatistics && final.A != 0)
                            renderedCount++;

                        if (ShowProgressBar)
                        {
                            progressCounter++;
                            if (progressCounter % progressUpdateInterval == 0)
                            {
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    ProgressValue = Math.Min(ProgressValue + progressUpdateInterval, ProgressMaximum);
                                });
                            }
                        }
                    }
                }

                if (ShowGrid)
                {
                    HighlightCells(g, bBt.Width, num, (int)GridYOffset, (int)GridXOffset);
                }

                Bitmap rotatedImage = new(bBt);
                ApplyRotation(rotatedImage);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    BackgroundImage2Source = BitmapToImageSource(rotatedImage);

                    if (GatherStatistics)
                    {
                        TotalBlocksData = renderedCount.ToString();
                    }

                    if (buildSchematic)
                    {
                        IsCopyToClipboardEnabled = true;
                        IsOverwriteEnabled = true;
                        IsSaveSchematicEnabled = true;
                    }
                });
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TotalBlocksData = "0";
            throw;
        }
        catch (Exception ex)
        {
            ProgressValue = 0;
            await ShowMessageInteraction.Handle($"ERROR: The color filter formatting is invalid! {ex}");
        }
        finally
        {
            if (ShowProgressBar)
            {
                ProgressValue = ProgressMaximum;
            }
        }
    }

    #endregion

    #region Scaling Interpolations

    private Color NearestNeighborInterpolation(Bitmap bmp, int x, int y, int num, bool includeTransparent)
    {
        int nx = Math.Min(x + num / 2, bmp.Width - 1);
        int ny = Math.Min(y + num / 2, bmp.Height - 1);
        Color nearestColor = bmp.GetPixel(nx, ny);

        if (includeTransparent || nearestColor.A != 0)
        {
            if (includeTransparent && nearestColor.A == 0)
                nearestColor = Color.Lime;

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
                            color = Color.Lime;
                        block.Add(color);
                    }
                }
            }
        }

        if (block.Count == 0)
            return Color.Transparent;

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
                        pixel = Color.Lime;

                    double coeff = dx[i + 1] * dy[j + 1];
                    r += pixel.R * coeff;
                    g += pixel.G * coeff;
                    b += pixel.B * coeff;
                    sum += coeff;
                }
            }
        }

        if (sum == 0)
            return Color.Transparent;

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
                        pixel = Color.Lime;

                    double lanczosWeight = LanczosKernel(i / (double)num, a) * LanczosKernel(j / (double)num, a);
                    r += pixel.R * lanczosWeight;
                    g += pixel.G * lanczosWeight;
                    b += pixel.B * lanczosWeight;
                    sum += lanczosWeight;
                }
            }
        }

        if (sum == 0)
            return Color.Transparent;

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
                        pixel = Color.Lime;

                    double coeff = dx[i] * dy[j];
                    r += pixel.R * coeff;
                    g += pixel.G * coeff;
                    b += pixel.B * coeff;
                    sum += coeff;
                }
            }
        }

        if (sum == 0)
            return Color.Transparent;

        Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
        return Clr(new Color[] { interpolatedColor });
    }

    private Color SplineInterpolation(Bitmap bmp, int x, int y, int num, bool includeTransparent)
    {
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
                        pixel = Color.Lime;

                    double coeff = SplineKernel(i / (double)num) * SplineKernel(j / (double)num);
                    r += pixel.R * coeff;
                    g += pixel.G * coeff;
                    b += pixel.B * coeff;
                    sum += coeff;
                }
            }
        }

        if (sum == 0)
            return Color.Transparent;

        Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
        return Clr(new Color[] { interpolatedColor });
    }

    private Color GaussianInterpolation(Bitmap bmp, int x, int y, double sigma, bool includeTransparent)
    {
        double r = 0, g = 0, b = 0;
        double sum = 0;

        int radius = (int)Math.Ceiling(3 * sigma);

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
                        pixel = Color.Lime;

                    double weight = GaussianKernel(i, j, sigma);
                    r += pixel.R * weight;
                    g += pixel.G * weight;
                    b += pixel.B * weight;
                    sum += weight;
                }
            }
        }

        if (sum == 0)
            return Color.Transparent;

        Color interpolatedColor = Color.FromArgb(Clamp(r / sum), Clamp(g / sum), Clamp(b / sum));
        return Clr(new Color[] { interpolatedColor });
    }

    private static double HermitePolynomial(double x)
    {
        if (x == 0) return 1.0;
        if (x < 0) x = -x;
        double x3 = x * x * x;
        double x2 = x * x;
        return (2 * x3) - (3 * x2) + 1;
    }

    private static double LanczosKernel(double x, int a)
    {
        if (x == 0) return 1.0;
        if (x < -a || x > a) return 0.0;

        x *= Math.PI;
        return a * Math.Sin(x) * Math.Sin(x / a) / (x * x);
    }

    private static double CubicPolynomial(double x)
    {
        x = Math.Abs(x);
        if (x <= 1)
            return 1.5 * x * x * x - 2.5 * x * x + 1;
        else if (x < 2)
            return -0.5 * x * x * x + 2.5 * x * x - 4 * x + 2;
        else
            return 0;
    }

    private static double SplineKernel(double x)
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

    private static double GaussianKernel(int x, int y, double sigma)
    {
        double expPart = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
        double normalPart = 1 / (2 * Math.PI * sigma * sigma);
        return normalPart * expPart;
    }

    private static int Clamp(double value)
    {
        return (int)Math.Max(0, Math.Min(255, value));
    }

    #endregion

    #region Color Matching

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

        return Clrs[ind];
    }

    #endregion

    #region Rotation & Grid Helpers

    private Point GetRotationData(int xInput, int yInput, int maxWidth, int maxHeight)
    {
        int x, y;
        if (WorldAxis == WorldAxis.XAxis)
        {
            (x, y) = RotationAngle switch
            {
                RotationAngle.Rotate90 => (maxHeight - 1 - yInput, xInput),
                RotationAngle.Rotate180 => (maxWidth - 1 - xInput, maxHeight - 1 - yInput),
                RotationAngle.Rotate270 => (yInput, maxWidth - 1 - xInput),
                _ => (xInput, yInput)
            };
        }
        else
        {
            (x, y) = RotationAngle switch
            {
                RotationAngle.Rotate90 => (yInput, xInput),
                RotationAngle.Rotate180 => (xInput, maxHeight - 1 - yInput),
                RotationAngle.Rotate270 => (maxWidth - 1 - yInput, maxHeight - 1 - xInput),
                _ => (maxWidth - 1 - xInput, yInput)
            };
        }

        return new Point(x, y);
    }

    private void HighlightCells(Graphics g, int numOfCells, int cellSize, int offsetX, int offsetY)
    {
        using Pen p = new(GridColor);
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
        switch (RotationAngle)
        {
            case RotationAngle.Rotate90:
                temp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                break;
            case RotationAngle.Rotate180:
                temp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                break;
            case RotationAngle.Rotate270:
                temp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                break;
        }

        if (WorldAxis == WorldAxis.YAxis)
            temp.RotateFlip(RotateFlipType.RotateNoneFlipX);
    }

    #endregion

    #region Bitmap Converters

    public static Bitmap BitmapFromSource(BitmapSource bitmapSource)
    {
        Bitmap bitmap;
        using (MemoryStream outStream = new())
        {
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapSource));
            enc.Save(outStream);
            outStream.Seek(0, SeekOrigin.Begin);
            bitmap = new Bitmap(outStream);
        }
        return bitmap;
    }

    private static BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
        using MemoryStream memory = new();

        bitmap.Save(memory, ImageFormat.Png);
        memory.Position = 0;
        BitmapImage bitmapimage = new();
        bitmapimage.BeginInit();
        bitmapimage.StreamSource = memory;
        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapimage.EndInit();

        return bitmapimage;
    }

    private static ImageFormat GetImageFormatFromPath(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".bmp" => ImageFormat.Bmp,
            _ => ImageFormat.Png
        };
    }

    private static ImageCodecInfo? GetEncoder(ImageFormat format)
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

    #region TileWallData

    public class TileWallData
    {
        public string Type { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Paint { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool BuildSafe { get; set; }
    }

    public static async Task<List<TileWallData>> ReadDataAsync(Stream stream, IProgress<int>? progress)
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
                Id = int.Parse(element.Attribute("Id")!.Value),
                Name = element.Attribute("Name")!.Value,
                Paint = int.Parse(element.Attribute("Paint")!.Value),
                Color = element.Attribute("Color")!.Value,
                BuildSafe = bool.Parse(element.Attribute("BuildSafe")!.Value)
            };
            dataList.Add(data);

            progress?.Report((i + 1) * 100 / totalElements);
        }

        return dataList;
    }

    #endregion
}

#region Request Types

public record SaveImageRequest;
public record SaveSchematicRequest;

#endregion
