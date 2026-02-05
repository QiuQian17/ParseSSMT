using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SSMT3;
using SSMT3.Pages.TextureToolBoxPage;
using SSMT3.SSMTHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace SSMT3
{
    public enum FpsOption
    {
        Fps30,
        Fps60
    }

    public sealed partial class TextureToolBoxPage : Page
    {
        // Backing fields
        private string SelectedTextureFilePath;
        private string SelectedVideoFilePath;
        private string DynamicTextureModGenerateFolderPath;
        private FpsOption SelectedFpsOption = FpsOption.Fps30;

        public TextureToolBoxPage()
        {
            this.InitializeComponent();

            // Load config
            var config = TextToolBoxConfig.Load();
            SelectedTextureFilePath = config.SelectedTextureFilePath;
            SelectedVideoFilePath = config.SelectedVideoFilePath;
            DynamicTextureModGenerateFolderPath = config.DynamicTextureModGenerateFolderPath;
            SelectedFpsOption = (FpsOption)config.SelectedFpsOption;

            // Initialize UI values
            TextBox_OriginalTextureFile.Text = SelectedTextureFilePath;
            TextBox_VideoFile.Text = SelectedVideoFilePath;
            TextBox_DynamicTextureModGenerateFolder.Text = DynamicTextureModGenerateFolderPath;
            ComboBox_FpsOption.SelectedIndex = SelectedFpsOption == FpsOption.Fps30 ?0 :1;

            // Initialize Batch UI values
            TextBox_BatchInputPath.Text = config.BatchInputPath;
            TextBox_BatchOutputPath.Text = config.BatchOutputPath;
            ComboBox_BatchFormat.Text = config.BatchFormat;
            TextBox_BatchWidth.Text = config.BatchWidth;
            TextBox_BatchHeight.Text = config.BatchHeight;
            TextBox_BatchMips.Text = config.BatchMips;
            CheckBox_BatchSRGB.IsChecked = config.BatchSRGB;
            CheckBox_BatchPremulAlpha.IsChecked = config.BatchPremulAlpha;
            CheckBox_BatchAlpha.IsChecked = config.BatchAlpha;
            CheckBox_BatchSepAlpha.IsChecked = config.BatchSepAlpha;
            CheckBox_BatchPow2.IsChecked = config.BatchPow2;
            CheckBox_BatchVFlip.IsChecked = config.BatchVFlip;
            CheckBox_BatchHFlip.IsChecked = config.BatchHFlip;
            CheckBox_BatchOverwrite.IsChecked = config.BatchOverwrite;
        }

        private void SaveConfig()
        {
            var config = new TextToolBoxConfig
            {
                SelectedTextureFilePath = this.SelectedTextureFilePath,
                SelectedVideoFilePath = this.SelectedVideoFilePath,
                DynamicTextureModGenerateFolderPath = this.DynamicTextureModGenerateFolderPath,
                SelectedFpsOption = (int)this.SelectedFpsOption,

                BatchInputPath = TextBox_BatchInputPath.Text,
                BatchOutputPath = TextBox_BatchOutputPath.Text,
                BatchFormat = ComboBox_BatchFormat.Text,
                BatchWidth = TextBox_BatchWidth.Text,
                BatchHeight = TextBox_BatchHeight.Text,
                BatchMips = TextBox_BatchMips.Text,
                BatchSRGB = CheckBox_BatchSRGB.IsChecked ?? false,
                BatchPremulAlpha = CheckBox_BatchPremulAlpha.IsChecked ?? false,
                BatchAlpha = CheckBox_BatchAlpha.IsChecked ?? false,
                BatchSepAlpha = CheckBox_BatchSepAlpha.IsChecked ?? false,
                BatchPow2 = CheckBox_BatchPow2.IsChecked ?? false,
                BatchVFlip = CheckBox_BatchVFlip.IsChecked ?? false,
                BatchHFlip = CheckBox_BatchHFlip.IsChecked ?? false,
                BatchOverwrite = CheckBox_BatchOverwrite.IsChecked ?? true
            };
            config.Save();
        }

        // Event handlers
        private async void Button_ChooseOriginalTextureFile_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(".dds");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedTextureFilePath = path;
                TextBox_OriginalTextureFile.Text = path;
                SaveConfig();
            }
        }

        private async void Button_ChooseVideoFile_Click(object sender, RoutedEventArgs e)
        {
            var supportedFormats = new List<string> { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".webm", ".wmv", ".gif" };
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(supportedFormats);
            if (!string.IsNullOrEmpty(path))
            {
                SelectedVideoFilePath = path;
                TextBox_VideoFile.Text = path;
                SaveConfig();
            }
        }

        private async void Button_ChooseDynamicTextureModGenerateFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (!string.IsNullOrEmpty(folderPath))
            {
                DynamicTextureModGenerateFolderPath = folderPath;
                TextBox_DynamicTextureModGenerateFolder.Text = folderPath;
                SaveConfig();
            }
        }

        private void Button_SetDynamicTextureModGenerateFolderToMods_Click(object sender, RoutedEventArgs e)
        {
            DynamicTextureModGenerateFolderPath = PathManager.Path_ModsFolder;
            TextBox_DynamicTextureModGenerateFolder.Text = DynamicTextureModGenerateFolderPath;
            SaveConfig();
        }

        private async void TextBox_OriginalTextureFile_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }

            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count == 0)
            {
                return;
            }

            if (items[0] is StorageFile file && !string.IsNullOrEmpty(file.Path))
            {
                SelectedTextureFilePath = file.Path;
                TextBox_OriginalTextureFile.Text = file.Path;
                SaveConfig();
            }
        }

        private async void TextBox_VideoFile_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }

            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count == 0)
            {
                return;
            }

            if (items[0] is StorageFile file && !string.IsNullOrEmpty(file.Path))
            {
                SelectedVideoFilePath = file.Path;
                TextBox_VideoFile.Text = file.Path;
                SaveConfig();
            }
        }

        private void TextBox_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void ComboBox_FpsOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_FpsOption.SelectedIndex ==0) SelectedFpsOption = FpsOption.Fps30; else SelectedFpsOption = FpsOption.Fps60;
            SaveConfig();
        }

        private async void Button_GenerateDynamicTextureMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LOG.Info("GenerateDynamicTextureMod: start");

                // ... (validation checks) ...
                if (string.IsNullOrWhiteSpace(SelectedTextureFilePath) || !File.Exists(SelectedTextureFilePath))
                {
                    await SSMTMessageHelper.Show("请先选择原始贴图文件（.dds）。", "Please choose the original texture file (.dds) first.");
                    return;
                }
                if (!SelectedTextureFilePath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
                {
                    await SSMTMessageHelper.Show("请选择一个 .dds 格式的贴图文件。", "Please select a .dds texture file.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedVideoFilePath) || !File.Exists(SelectedVideoFilePath))
                {
                    await SSMTMessageHelper.Show("请先选择视频文件。", "Please choose the video file.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(DynamicTextureModGenerateFolderPath))
                {
                    await SSMTMessageHelper.Show("请先选择动态贴图Mod生成的文件夹位置。", "Please choose the output folder for generated dynamic texture mod.");
                    return;
                }
                if (!Directory.Exists(DynamicTextureModGenerateFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(DynamicTextureModGenerateFolderPath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Info("GenerateDynamicTextureMod: create output folder failed: " + ex.Message);
                        await SSMTMessageHelper.Show("无法创建输出文件夹，请检查路径权限。", "Cannot create output folder, please check permissions.");
                        return;
                    }
                }

                // Show progress
                StackPanel_DynamicModProgress.Visibility = Visibility.Visible;
                ProgressBar_DynamicMod.IsIndeterminate = true;
                TextBlock_DynamicModProgress.Text = "Initializing...";
                Button_GenerateDynamicTextureMod.IsEnabled = false;

                string dynamicTextureModDir = Path.Combine(DynamicTextureModGenerateFolderPath, "DynamicTextureMod");
                if (Directory.Exists(dynamicTextureModDir))
                {
                    Directory.Delete(dynamicTextureModDir, true);
                }
                Directory.CreateDirectory(dynamicTextureModDir);

                var (width, height, format) = GetDdsInfo(SelectedTextureFilePath);

                string tempPngDir = Path.Combine(Path.GetTempPath(), "SSMT_TempPngFrames");
                if (Directory.Exists(tempPngDir)) Directory.Delete(tempPngDir, true);
                Directory.CreateDirectory(tempPngDir);

                TextBlock_DynamicModProgress.Text = "Extracting frames from video...";

                bool vFlip = ToggleSwitch_VerticalFlip.IsOn;
                bool hFlip = ToggleSwitch_HorizontalFlip.IsOn;
                await Task.Run(() => ExtractAndFlipFrames(SelectedVideoFilePath, tempPngDir, vFlip, hFlip));

                TextBlock_DynamicModProgress.Text = "Converting frames to DDS...";
                ProgressBar_DynamicMod.IsIndeterminate = false;
                await ConvertPngToDdsAsync(tempPngDir, dynamicTextureModDir, width, height, format, (current, total) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ProgressBar_DynamicMod.Maximum = total;
                        ProgressBar_DynamicMod.Value = current;
                        TextBlock_DynamicModProgress.Text = $"Converting: {current}/{total}";
                    });
                });

                int ddsFileCount = Directory.GetFiles(dynamicTextureModDir, "*.dds", SearchOption.TopDirectoryOnly).Length;
                LOG.Info($"DynamicTextureMod directory contains {ddsFileCount} DDS files.");

                string TextureHash = Path.GetFileName(SelectedTextureFilePath).Split("_")[0];

                CoreFunctions.GenerateDynamicTextureMod(dynamicTextureModDir, TextureHash, ".dds");

                SSMTCommandHelper.ShellOpenFolder(dynamicTextureModDir);
            }
            catch (Exception ex)
            {
                LOG.Info("GenerateDynamicTextureMod error: " + ex.ToString());
                await SSMTMessageHelper.Show("生成动态贴图Mod时发生错误：" + ex.Message, "Error occurred while generating dynamic texture mod: " + ex.Message);
            }
            finally
            {
                StackPanel_DynamicModProgress.Visibility = Visibility.Collapsed;
                Button_GenerateDynamicTextureMod.IsEnabled = true;
            }
        }

        private (int width, int height, string format) GetDdsInfo(string ddsPath)
        {
            string tempPath = Path.GetTempPath().TrimEnd('\\');
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = PathManager.Path_TexconvExe,
                // Output to temp folder to avoid "file exists" error and avoid overwriting source
                // TrimEnd('\\') is used to avoid escaping the closing quote
                Arguments = $"-nologo -y -o \"{tempPath}\" \"{ddsPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using Process proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            foreach (string line in output.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("reading") && trimmed.Contains("(") && trimmed.Contains(")"))
                {
                    int l = trimmed.IndexOf('(');
                    int r = trimmed.IndexOf(')', l + 1);
                    if (l >= 0 && r > l)
                    {
                        string info = trimmed.Substring(l + 1, r - l - 1);
                        string[] parts = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            // parts[0] might be "1920x1920,11" (width x height, mips)
                            string dimPart = parts[0];
                            int commaIndex = dimPart.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                dimPart = dimPart.Substring(0, commaIndex);
                            }

                            string[] wh = dimPart.Split('x');
                            if (wh.Length == 2 && int.TryParse(wh[0], out int width) && int.TryParse(wh[1], out int height))
                            {
                                string format = parts[1];
                                LOG.Info($"GetDdsInfo: path={ddsPath}, width={width}, height={height}, format={format}");
                                return (width, height, format);
                            }
                        }
                    }
                }
            }

            LOG.Info($"GetDdsInfo failed to parse. texconv stdout:\n{output}\n texconv stderr:\n{error}");
            throw new Exception("无法解析DDS属性: " + (output + error));
        }

        private void ExtractAndFlipFrames(string videoPath, string outputDir, bool vFlip, bool hFlip)
        {
            int fps = SelectedFpsOption == FpsOption.Fps30 ? 30 : 60;

            string arguments = "";
            
            arguments = arguments + $"-i \"{videoPath}\" -vf \"fps={fps},scale=-1:-1";
            
            if (vFlip)
            {
                arguments = arguments + ",vflip";
            }

            if (hFlip)
            {
                arguments = arguments + ",hflip";
            }

            arguments = arguments + " \" ";
            
            arguments = arguments + $"\"{outputDir}\\frame_%05d.png\"";

            /*
                
             简洁说明每一部分含义：
                •	-i "{videoPath}"：指定输入文件（videoPath 为视频路径）。
                •	-vf "fps={fps},scale=-1:-1,vflip,hflip"：视频滤镜链（按顺序执行）
                •	fps={fps}：按指定帧率导出帧（例如 30 或 60）。
                •	scale=-1:-1：按原始宽高比例保持尺寸（-1 表示自动计算），不做缩放。
                •	vflip：垂直翻转帧（上下翻转）。
                •	hflip：水平翻转帧（左右翻转）。
                •	"{outputDir}\frame_%05d.png"：输出文件模式，按序号生成 PNG 文件（%05d 表示 5 位零填充序号，如 frame_00001.png）。
            */

            var psi = new ProcessStartInfo
            {
                FileName = PathManager.Path_Plugin_FFMPEG,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            LOG.Info($"ExtractAndFlipFrames stdout:\n{output}");
            LOG.Info($"ExtractAndFlipFrames stderr:\n{error}");
        }

        private async Task ConvertPngToDdsAsync(string pngDir, string ddsDir, int width, int height, string format, Action<int, int> progressCallback)
        {
            Directory.CreateDirectory(ddsDir);
            var pngFiles = Directory.GetFiles(pngDir, "frame_*.png");
            int total = pngFiles.Length;
            int current = 0;

            await Task.Run(() =>
            {
                foreach (var png in pngFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(png);
                    string ddsOut = Path.Combine(ddsDir, $"{fileName}.dds");
                    var psi = new ProcessStartInfo
                    {
                        FileName = PathManager.Path_TexconvExe,
                        Arguments = $"-f {format} -w {width} -h {height} -o \"{ddsDir}\" \"{png}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var proc = Process.Start(psi);
                    proc.WaitForExit();
                    current++;
                    progressCallback?.Invoke(current, total);
                }
            });
        }

        // --- Batch Converter Event Handlers ---

        private async void Button_BatchSelectFile_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(new List<string> { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".tif", ".tga", ".dds" });
            if (!string.IsNullOrEmpty(path))
            {
                TextBox_BatchInputPath.Text = path;
                SaveConfig();
            }
        }

        private async void Button_BatchSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (!string.IsNullOrEmpty(path))
            {
                TextBox_BatchInputPath.Text = path;
                SaveConfig();
            }
        }

        private async void Button_BatchSelectOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (!string.IsNullOrEmpty(path))
            {
                TextBox_BatchOutputPath.Text = path;
                SaveConfig();
            }
        }

        private async void Button_BatchPickFormat_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(".dds");
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    (int width, int height, string format) = GetDdsInfo(path);
                    if (!string.IsNullOrEmpty(format))
                    {
                        ComboBox_BatchFormat.Text = format;
                        // Also try to select it in the list if it exists
                        foreach (ComboBoxItem item in ComboBox_BatchFormat.Items)
                        {
                            if (item.Content.ToString().Equals(format, StringComparison.OrdinalIgnoreCase))
                            {
                                ComboBox_BatchFormat.SelectedItem = item;
                                break;
                            }
                        }
                        await SSMTMessageHelper.Show($"已读取并设置格式: {format}", $"Format set to: {format}");
                        SaveConfig();
                    }
                }
                catch (Exception ex)
                {
                    await SSMTMessageHelper.Show("读取DDS格式失败: " + ex.Message, "Failed to read DDS format: " + ex.Message);
                }
            }
        }

        private async void Button_BatchConvert_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig(); // Save config before starting conversion
            string inputPath = TextBox_BatchInputPath.Text;
            if (string.IsNullOrWhiteSpace(inputPath) || (!File.Exists(inputPath) && !Directory.Exists(inputPath)))
            {
                await SSMTMessageHelper.Show("请选择有效的输入文件或文件夹。", "Please select a valid input file or folder.");
                return;
            }

            string outputDir = TextBox_BatchOutputPath.Text;
            if (string.IsNullOrWhiteSpace(outputDir))
            {
                // If output is empty, use source directory
                outputDir = File.Exists(inputPath) ? Path.GetDirectoryName(inputPath) : inputPath;
            }

            if (!Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch
                {
                    await SSMTMessageHelper.Show("无法创建输出文件夹。", "Cannot create output folder.");
                    return;
                }
            }

            string format = ComboBox_BatchFormat.Text;
            if (string.IsNullOrWhiteSpace(format))
            {
                format = "BC7_UNORM"; // Default fallback
            }

            // Build common arguments
            var args = new List<string>();

            // Format
            args.Add($"-f {format}");

            // Output
            args.Add($"-o \"{outputDir}\"");

            // Dimensions
            if (!string.IsNullOrWhiteSpace(TextBox_BatchWidth.Text)) args.Add($"-w {TextBox_BatchWidth.Text}");
            if (!string.IsNullOrWhiteSpace(TextBox_BatchHeight.Text)) args.Add($"-h {TextBox_BatchHeight.Text}");
            if (!string.IsNullOrWhiteSpace(TextBox_BatchMips.Text)) args.Add($"-m {TextBox_BatchMips.Text}");

            // Flags
            if (CheckBox_BatchSRGB.IsChecked == true) args.Add("-srgb");
            if (CheckBox_BatchPremulAlpha.IsChecked == true) args.Add("-pmalpha");
            if (CheckBox_BatchAlpha.IsChecked == true) args.Add("-alpha");
            if (CheckBox_BatchSepAlpha.IsChecked == true) args.Add("-sepalpha");
            if (CheckBox_BatchPow2.IsChecked == true) args.Add("-pow2");
            if (CheckBox_BatchVFlip.IsChecked == true) args.Add("-vflip");
            if (CheckBox_BatchHFlip.IsChecked == true) args.Add("-hflip");
            if (CheckBox_BatchOverwrite.IsChecked == true) args.Add("-y");

            string commonArgs = string.Join(" ", args);
            LOG.Info($"Batch Convert Common Arguments: {commonArgs}");

            // Show progress
            StackPanel_BatchConvertProgress.Visibility = Visibility.Visible;
            ProgressBar_BatchConvert.IsIndeterminate = true;
            TextBlock_BatchConvertProgress.Text = "Initializing...";
            Button_BatchConvert.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    if (Directory.Exists(inputPath))
                    {
                        // Directory mode: Find all supported images and process them one by one
                        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ".dds", ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".tif", ".tga"
                        };

                        var files = Directory.GetFiles(inputPath)
                                             .Where(f => extensions.Contains(Path.GetExtension(f)))
                                             .ToArray();

                        LOG.Info($"Found {files.Length} images in directory: {inputPath}");

                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ProgressBar_BatchConvert.IsIndeterminate = false;
                            ProgressBar_BatchConvert.Maximum = files.Length;
                            ProgressBar_BatchConvert.Value = 0;
                        });

                        for (int i = 0; i < files.Length; i++)
                        {
                            var file = files[i];
                            var psi = new ProcessStartInfo
                            {
                                FileName = PathManager.Path_TexconvExe,
                                Arguments = $"{commonArgs} \"{file}\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true
                            };
                            using var proc = Process.Start(psi);
                            proc.WaitForExit();
                            
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                ProgressBar_BatchConvert.Value = i + 1;
                                TextBlock_BatchConvertProgress.Text = $"Processing: {i + 1}/{files.Length}";
                            });
                        }
                    }
                    else
                    {
                        // Single file mode
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            ProgressBar_BatchConvert.IsIndeterminate = true;
                            TextBlock_BatchConvertProgress.Text = "Processing single file...";
                        });

                        var psi = new ProcessStartInfo
                        {
                            FileName = PathManager.Path_TexconvExe,
                            Arguments = $"{commonArgs} \"{inputPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        using var proc = Process.Start(psi);
                        string output = proc.StandardOutput.ReadToEnd();
                        string error = proc.StandardError.ReadToEnd();
                        proc.WaitForExit();

                        LOG.Info($"Texconv Output: {output}");
                        if (!string.IsNullOrEmpty(error)) LOG.Info($"Texconv Error: {error}");
                    }
                });

                await SSMTMessageHelper.Show("转换完成！", "Conversion completed!");
                SSMTCommandHelper.ShellOpenFolder(outputDir);
            }
            catch (Exception ex)
            {
                LOG.Info($"Batch Convert Error: {ex}");
                await SSMTMessageHelper.Show("转换过程中发生错误: " + ex.Message, "Error during conversion: " + ex.Message);
            }
            finally
            {
                StackPanel_BatchConvertProgress.Visibility = Visibility.Collapsed;
                Button_BatchConvert.IsEnabled = true;
            }
        }
    }
}
