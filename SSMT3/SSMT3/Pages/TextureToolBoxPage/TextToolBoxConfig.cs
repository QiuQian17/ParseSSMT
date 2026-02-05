using Newtonsoft.Json.Linq;
using SSMT3;
using System;
using System.IO;

namespace SSMT3.Pages.TextureToolBoxPage
{
    public class TextToolBoxConfig
    {
        public string SelectedTextureFilePath { get; set; } = string.Empty;
        public string SelectedVideoFilePath { get; set; } = string.Empty;
        public string DynamicTextureModGenerateFolderPath { get; set; } = string.Empty;
        public int SelectedFpsOption { get; set; } = 0;

        // Batch Convert Settings
        public string BatchInputPath { get; set; } = string.Empty;
        public string BatchOutputPath { get; set; } = string.Empty;
        public string BatchFormat { get; set; } = "BC7_UNORM";
        public string BatchWidth { get; set; } = string.Empty;
        public string BatchHeight { get; set; } = string.Empty;
        public string BatchMips { get; set; } = string.Empty;
        public bool BatchSRGB { get; set; } = false;
        public bool BatchPremulAlpha { get; set; } = false;
        public bool BatchAlpha { get; set; } = false;
        public bool BatchSepAlpha { get; set; } = false;
        public bool BatchPow2 { get; set; } = false;
        public bool BatchVFlip { get; set; } = false;
        public bool BatchHFlip { get; set; } = false;
        public bool BatchOverwrite { get; set; } = true;


        public TextToolBoxConfig() { }

        public static TextToolBoxConfig Load()
        {
            if (File.Exists(PathManager.Path_TextToolBoxConfig))
            {
                JObject jobj = DBMTJsonUtils.ReadJObjectFromFile(PathManager.Path_TextToolBoxConfig);
                return new TextToolBoxConfig
                {
                    SelectedTextureFilePath = jobj["SelectedTextureFilePath"]?.ToString() ?? string.Empty,
                    SelectedVideoFilePath = jobj["SelectedVideoFilePath"]?.ToString() ?? string.Empty,
                    DynamicTextureModGenerateFolderPath = jobj["DynamicTextureModGenerateFolderPath"]?.ToString() ?? string.Empty,
                    SelectedFpsOption = jobj["SelectedFpsOption"]?.ToObject<int>() ?? 0,

                    BatchInputPath = jobj["BatchInputPath"]?.ToString() ?? string.Empty,
                    BatchOutputPath = jobj["BatchOutputPath"]?.ToString() ?? string.Empty,
                    BatchFormat = jobj["BatchFormat"]?.ToString() ?? "BC7_UNORM",
                    BatchWidth = jobj["BatchWidth"]?.ToString() ?? string.Empty,
                    BatchHeight = jobj["BatchHeight"]?.ToString() ?? string.Empty,
                    BatchMips = jobj["BatchMips"]?.ToString() ?? string.Empty,
                    BatchSRGB = jobj["BatchSRGB"]?.ToObject<bool>() ?? false,
                    BatchPremulAlpha = jobj["BatchPremulAlpha"]?.ToObject<bool>() ?? false,
                    BatchAlpha = jobj["BatchAlpha"]?.ToObject<bool>() ?? false,
                    BatchSepAlpha = jobj["BatchSepAlpha"]?.ToObject<bool>() ?? false,
                    BatchPow2 = jobj["BatchPow2"]?.ToObject<bool>() ?? false,
                    BatchVFlip = jobj["BatchVFlip"]?.ToObject<bool>() ?? false,
                    BatchHFlip = jobj["BatchHFlip"]?.ToObject<bool>() ?? false,
                    BatchOverwrite = jobj["BatchOverwrite"]?.ToObject<bool>() ?? true
                };
            }
            return new TextToolBoxConfig();
        }

        public void Save()
        {
            JObject jobj = DBMTJsonUtils.CreateJObject();
            jobj["SelectedTextureFilePath"] = SelectedTextureFilePath;
            jobj["SelectedVideoFilePath"] = SelectedVideoFilePath;
            jobj["DynamicTextureModGenerateFolderPath"] = DynamicTextureModGenerateFolderPath;
            jobj["SelectedFpsOption"] = SelectedFpsOption;

            jobj["BatchInputPath"] = BatchInputPath;
            jobj["BatchOutputPath"] = BatchOutputPath;
            jobj["BatchFormat"] = BatchFormat;
            jobj["BatchWidth"] = BatchWidth;
            jobj["BatchHeight"] = BatchHeight;
            jobj["BatchMips"] = BatchMips;
            jobj["BatchSRGB"] = BatchSRGB;
            jobj["BatchPremulAlpha"] = BatchPremulAlpha;
            jobj["BatchAlpha"] = BatchAlpha;
            jobj["BatchSepAlpha"] = BatchSepAlpha;
            jobj["BatchPow2"] = BatchPow2;
            jobj["BatchVFlip"] = BatchVFlip;
            jobj["BatchHFlip"] = BatchHFlip;
            jobj["BatchOverwrite"] = BatchOverwrite;

            DBMTJsonUtils.SaveJObjectToFile(jobj, PathManager.Path_TextToolBoxConfig);
        }
    }
}
