using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Globalization;
using SSMT3;

namespace SSMT3
{
    
    public static class GlobalConfig
    {
        /// <summary>
        /// 此数值跟随SSMT版本变化而变化，用于区分不同版本的配置文件
        /// 在TheHerta3中通过读取这个数值来显示当前TheHerta3版本是否与SSMT版本适配
        /// 如果不适配则会提示用户更新TheHerta3
        /// </summary>
        public static int VersionNumber { get; set; } = 353;
        public static string CurrentGameName { get; set; } = "WWMI";
        public static string CurrentWorkSpace { get; set; } = "";
        public static string SSMTCacheFolderPath { get; set; } = "";
        public static bool Theme { get; set; } = true;
        public static bool Chinese { get; set; } = true;

        //窗口大小
        public static double WindowWidth { get; set; } = 1280;
        public static double WindowHeight { get; set; } = 720;
        public static double WindowLuminosityOpacity { get; set; } = 0.65f;


        public static bool UseSpecificIBListDump { get; set; } = false;


        /// <summary>
        /// 启动程序之后执行的动作
        /// 0 无
        /// 1 最小化到托盘
        /// 2 关闭SSMT
        /// </summary>
        public static int PostRunGameAction { get; set; } = 0;



        /// <summary>
        /// 开启后减少Dump文件总大小，提高Dump速度
        /// 99%的磁盘格式都支持，但是1%的磁盘格式Dump下来天生文件损坏
        /// 所以默认是关闭状态的，作者有需要可以手动开启
        /// </summary>
        public static bool UseSymlinkFeature { get; set; } = true;

        /// <summary>
        /// Mod逆向后的默认贴图转换格式，老高说默认为jpg的话没有透明通道，且质量低
        /// 这里默认还是使用tga吧
        /// </summary>
        public static string TextureConversionFormat { get; set; } = "tga";


        public static string AutoReverseGameName { get; set; } = "GI";
        public static string WWMIReverseStyle { get; set; } = "WWMI";

        public static int PostReverseAction { get; set; } = 0; // Changed to int to store SelectedIndex
        public static bool ConvertOriginalTextures { get; set; } = true;
        public static bool ConvertTexturesToOutputFolder { get; set; } = true;

        public static string ReverseOutputFolder { get; set; } = "";

        public static bool PlayerMode { get; set; } = false;

        public static string GithubToken { get; set; } = "";


        /// <summary>
        /// 使用古法读取，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void ReadConfig()
        {
            try
            {
                //只有存在这个全局配置文件时，才读取
                if (File.Exists(PathManager.Path_MainConfig_Global))
                {
                    //读取配置时优先读取全局的
                    JObject SettingsJsonObject = DBMTJsonUtils.ReadJObjectFromFile(PathManager.Path_MainConfig_Global);

                    //古法读取
                    if (SettingsJsonObject.ContainsKey("CurrentGameName"))
                    {
                        CurrentGameName = (string)SettingsJsonObject["CurrentGameName"];
                    }

                    if (SettingsJsonObject.ContainsKey("CurrentWorkSpace"))
                    {
                        CurrentWorkSpace = (string)SettingsJsonObject["CurrentWorkSpace"];
                    }

                    if (SettingsJsonObject.ContainsKey("DBMTWorkFolder"))
                    {
                        SSMTCacheFolderPath = (string)SettingsJsonObject["DBMTWorkFolder"];
                    }
                    
                    //WindowWidth
                    if (SettingsJsonObject.ContainsKey("WindowWidth"))
                    {
                        WindowWidth = (double)SettingsJsonObject["WindowWidth"];
                    }

                    //WindowHeight
                    if (SettingsJsonObject.ContainsKey("WindowHeight"))
                    {
                        WindowHeight = (double)SettingsJsonObject["WindowHeight"];
                    }


                    //WindowLuminosityOpacity
                    if (SettingsJsonObject.ContainsKey("WindowLuminosityOpacity"))
                    {
                        WindowLuminosityOpacity = (double)SettingsJsonObject["WindowLuminosityOpacity"];
                    }



              

                    if (SettingsJsonObject.ContainsKey("Theme"))
                    {
                        Theme = (bool)SettingsJsonObject["Theme"];
                    }

                    if (SettingsJsonObject.ContainsKey("Chinese"))
                    {
                        Chinese = (bool)SettingsJsonObject["Chinese"];
                    }

                   

                    //UseSymlinkFeature
                    if (SettingsJsonObject.ContainsKey("UseSymlinkFeature"))
                    {
                        UseSymlinkFeature = (bool)SettingsJsonObject["UseSymlinkFeature"];
                    }

                    //TextureConversionFormat
                    if (SettingsJsonObject.ContainsKey("TextureConversionFormat"))
                    {
                        TextureConversionFormat = (string)SettingsJsonObject["TextureConversionFormat"];
                    }

                    //AutoReverseGameName
                    if (SettingsJsonObject.ContainsKey("AutoReverseGameName"))
                    {
                        AutoReverseGameName = (string)SettingsJsonObject["AutoReverseGameName"];
                    }
                    //WWMIReverseStyle
                    if (SettingsJsonObject.ContainsKey("WWMIReverseStyle"))
                    {
                        WWMIReverseStyle = (string)SettingsJsonObject["WWMIReverseStyle"];
                    }

                    //PostReverseAction
                    if (SettingsJsonObject.ContainsKey("PostReverseAction"))
                    {
                        PostReverseAction = (int)SettingsJsonObject["PostReverseAction"];
                    }
                    //ConvertOriginalTextures

                    if (SettingsJsonObject.ContainsKey("ConvertOriginalTextures"))
                    {
                        ConvertOriginalTextures = (bool)SettingsJsonObject["ConvertOriginalTextures"];
                    }
                    //ConvertTexturesToOutputFolder
                    if (SettingsJsonObject.ContainsKey("ConvertTexturesToOutputFolder"))
                    {
                        ConvertTexturesToOutputFolder = (bool)SettingsJsonObject["ConvertTexturesToOutputFolder"];
                    }

                    //ReverseOutputFolder
                    if (SettingsJsonObject.ContainsKey("ReverseOutputFolder"))
                    {
                        ReverseOutputFolder = (string)SettingsJsonObject["ReverseOutputFolder"];
                    }

                    //GithubToken
                    if (SettingsJsonObject.ContainsKey("GithubToken"))
                    {
                        GithubToken = (string)SettingsJsonObject["GithubToken"];
                    }

                    //PostRunGameAction
                    if (SettingsJsonObject.ContainsKey("PostRunGameAction"))
                    {
                        PostRunGameAction = (int)SettingsJsonObject["PostRunGameAction"];
                    }

                    //PlayerMode
                    if (SettingsJsonObject.ContainsKey("PlayerMode"))
                    {
                        PlayerMode = (bool)SettingsJsonObject["PlayerMode"];
                    }

                    //UseSpecificIBListDump
                    if (SettingsJsonObject.ContainsKey("UseSpecificIBListDump"))
                    {
                        UseSpecificIBListDump = (bool)SettingsJsonObject["UseSpecificIBListDump"];
                    }

                    //VersionNumber不读取，只存储用于通知TheHerta3提供适配信息显示
                    //if (SettingsJsonObject.ContainsKey("VersionNumber"))
                    //{
                    //    VersionNumber = (int)SettingsJsonObject["VersionNumber"];
                    //}
                }


            }
            catch (Exception ex) {
                //如果全局的配置文件读取错误的话，直接删掉重新保存一个全局的配置文件
                //这是因为蓝屏的时候这里的配置文件会直接被损坏。
                ex.ToString();
                File.Delete(PathManager.Path_MainConfig_Global);
                GlobalConfig.SaveConfig();
            }
        }

        /// <summary>
        /// 使用古法保存，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                //古法保存
                JObject SettingsJsonObject = new JObject();
                SettingsJsonObject["VersionNumber"] = VersionNumber;

                SettingsJsonObject["CurrentGameName"] = CurrentGameName;
                SettingsJsonObject["CurrentWorkSpace"] = CurrentWorkSpace;
                SettingsJsonObject["DBMTWorkFolder"] = SSMTCacheFolderPath;
                SettingsJsonObject["WindowWidth"] = WindowWidth;
                SettingsJsonObject["WindowHeight"] = WindowHeight;
                SettingsJsonObject["WindowLuminosityOpacity"] = WindowLuminosityOpacity;
                SettingsJsonObject["Theme"] = Theme;
                SettingsJsonObject["Chinese"] = Chinese;
           
                SettingsJsonObject["UseSymlinkFeature"] = UseSymlinkFeature;

                SettingsJsonObject["TextureConversionFormat"] = TextureConversionFormat;
                SettingsJsonObject["AutoReverseGameName"] = AutoReverseGameName;
                SettingsJsonObject["WWMIReverseStyle"] = WWMIReverseStyle;
                SettingsJsonObject["PostReverseAction"] = PostReverseAction;
                SettingsJsonObject["ConvertOriginalTextures"] = ConvertOriginalTextures;
                SettingsJsonObject["ConvertTexturesToOutputFolder"] = ConvertTexturesToOutputFolder;

                SettingsJsonObject["ReverseOutputFolder"] = ReverseOutputFolder;
                SettingsJsonObject["PostRunGameAction"] = PostRunGameAction;
                SettingsJsonObject["PlayerMode"] = PlayerMode;
                SettingsJsonObject["UseSpecificIBListDump"] = UseSpecificIBListDump;

                SettingsJsonObject["GithubToken"] = GithubToken;

                //写出内容
                string WirteStirng = SettingsJsonObject.ToString();

                //保存配置时，全局配置也顺便保存一份
                File.WriteAllText(PathManager.Path_MainConfig_Global, WirteStirng);
            }
            catch (Exception ex)
            {
                //保存失败就算了，也不是非得保存不可。
                //很难想象没法在AppData\\Local下面写文件的情况会发生。
                ex.ToString();
            }
        }


    }
}
