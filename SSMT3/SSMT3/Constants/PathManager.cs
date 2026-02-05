using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;

namespace SSMT3
{
    public class PathManager
    {
        public static string analyse_options = "deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds";
        public static string SSMT_Version = "V3.6.0";
        public static string SSMT_Title = " " + SSMT_Version;

        public static string Name_Plugin_Upx = "upx.exe";
        public static string Name_Plugin_Texconv = "texconv.exe";
        public static string Name_GlobalConfigFileName = "SSMT3-Config.json";
        public static string Name_Plugin_GoodWorkGI = "GoodWorkGI.exe";
        public static string Name_Plugin_3DmigotoSwordLv5 = "3Dmigoto-Sword-Lv5.exe";

        public static string Link_SSMTTechniqueCommunitySponsorPage = "https://afdian.com/item/ec74ee782b2f11efb5a052540025c377";
        public static string Name_3DmigotoLoaderExe = "Run.exe";
        public static string Name_Plugin_FFMPEG = "ffmpeg.exe";


        public static string Path_Plugin_FFMPEG
        {
            get
            {
                return Path.Combine(PathManager.Path_ResourcesFolder, Name_Plugin_FFMPEG);
            }
        }

        public static string Path_Default3DmigotoLoaderExe
        {
            get
            {
                return Path.Combine(PathManager.Path_ResourcesFolder, Name_3DmigotoLoaderExe);
            }
        }

        public static string Path_TexconvExe
        {
            get
            {
                return Path.Combine(PathManager.Path_ResourcesFolder, Name_Plugin_Texconv);
            }
        }

        public static string Path_UpxExe
        {
            get
            {
                return Path.Combine(PathManager.Path_ResourcesFolder, Name_Plugin_Upx);
            }
        }



        public static string Path_LatestFrameAnalysisDedupedFolder
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "deduped\\");
            }
        }

        public static string Path_LogsFolder
        {
            get {
                
                string LogsFolder = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Logs\\");
                
                if (!Directory.Exists(LogsFolder))
                {
                    Directory.CreateDirectory(LogsFolder);
                }

                return LogsFolder;
            }
        }



        public static string Path_LatestSSMTLogFile
        {
            get
            {
                string logsPath = PathManager.Path_LogsFolder;
                if (!Directory.Exists(logsPath))
                {
                    return "";
                }
                string[] logFiles = Directory.GetFiles(logsPath); ;
                List<string> logFileList = new List<string>();
                foreach (string logFile in logFiles)
                {
                    string logfileName = Path.GetFileName(logFile);
                    if (logfileName.EndsWith(".log") && logfileName.Length > 15)
                    {
                        logFileList.Add(logfileName);
                    }
                }

                logFileList.Sort();


                if (logFileList.Count == 0)
                {
                    return "";
                }
                else
                {
                    string LogFilePath = logsPath + "\\" + logFileList[^1];
                    return LogFilePath;
                }
            }
        }


        public static string Path_TotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "WorkSpace\\");
            }
        }

        public static string Path_CurrentGameTotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(PathManager.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\");
            }
        }

        public static string Path_CurrentWorkSpaceFolder
        {
            get
            {
                string CurrentWorkSpaceFolder = Path.Combine(PathManager.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\" + GlobalConfig.CurrentWorkSpace + "\\");
                return CurrentWorkSpaceFolder;
            }
        }

        public static string Path_CurrentWorkSpace_ConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder, "Config.json");
            }
        }

        public static string Path_CurrentWorkSpace_SkipIBConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder, "SkipIBConfig.json");
            }
        }


        public static string Path_CurrentWorkSpace_VSCheckConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder, "VSCheckConfig.json");
            }
        }




        public static string Path_ModManageConfig
        {
            get { return Path.Combine(PathManager.Path_SSMT3GlobalConfigsFolder, "ModManageConfig.json"); }
        }

        public static string Path_TexturePageIndexConfig
        {
            get { return Path.Combine(PathManager.Path_SSMT3GlobalConfigsFolder, "TexturePageIndexConfig.json"); }
        }






        public static string Path_BaseFolder
        {
            get
            {
                return AppContext.BaseDirectory;
            }
        }

        public static string Path_ModsFolder
        {
            get { return Path.Combine(Path_3DmigotoLoaderFolder, "Mods\\"); }
        }


        public static string Path_GamesFolder
        {
            get { return Path.Combine(PathManager.Path_SSMT3GlobalConfigsFolder, "Games\\"); }
        }

        public static string Path_CurrentGamesFolder
        {
            get { return Path.Combine(PathManager.Path_GamesFolder, GlobalConfig.CurrentGameName + "\\"); }
        }

        public static string Path_CurrentGameConfigJson
        {
            get { return Path.Combine(PathManager.Path_CurrentGamesFolder, "Config.json"); }
        }

        public static string Path_GamesIconConfigJson
        {
            get { return Path.Combine(PathManager.Path_GamesFolder, "GameIconConfig.json"); }
        }

        public static string Path_CurrentWorkSpaceGeneratedModFolder
        {
            get
            {
                string retpath = Path.Combine(PathManager.Path_ModsFolder, "SSMTGeneratedMod\\Default\\Mod_" + GlobalConfig.CurrentWorkSpace + "\\");
                if (!Directory.Exists(retpath))
                {
                    Directory.CreateDirectory(retpath);
                }

                return retpath;

            }
        }
        public static string CurrentGameMigotoFolder
        {
            get
            {
                GameConfig gameConfig = new GameConfig();
                return gameConfig.MigotoPath;
            }
        }

        public static string Path_3DmigotoLoaderFolder
        {
            get { return PathManager.CurrentGameMigotoFolder; }
        }

        public static string Path_D3DXINI
        {
            get { return Path.Combine(PathManager.CurrentGameMigotoFolder, "d3dx.ini"); }
        }


        //DumpIBListConfig
        public static string Path_DumpIBListConfig
        {
            get { return Path.Combine(PathManager.CurrentGameMigotoFolder, "DumpIBListConfig.ini"); }
        }

        public static string Path_ResourcesFolder
        {
            get { return Path.Combine(PathManager.Path_BaseFolder, "Resources\\"); }
        }

        public static string Path_TextureConfigsFolder
        {
            get { return Path.Combine(Path_SSMT3GlobalConfigsFolder, "TextureConfigs\\"); }
        }

        public static string Path_GameTypeConfigsFolder
        {

            get { return Path.Combine(PathManager.Path_ResourcesFolder, "GameType\\"); }
        }

        public static string Path_CurrentSelected_GameTypeFolder
        {
            get { 
                GameConfig gameConfig = new GameConfig();
                return Path.Combine(PathManager.Path_GameTypeConfigsFolder, gameConfig.GameTypeName + "\\"); 
            }
        }

        public static string Path_GameTextureConfigFolder
        {
            get { return Path.Combine(Path_TextureConfigsFolder, GlobalConfig.CurrentGameName + "\\"); }
        }



        public static string Path_PluginsFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Plugins\\"); }
        }

        public static string LatestFrameAnalysisFolderName
        {
            get
            {
                List<string> frameAnalysisFileList = SSMTResourceUtils.GetFrameAnalysisFileNameList();

                if (frameAnalysisFileList.Count > 0)
                {
                    return frameAnalysisFileList.Last();
                }

                return "";
            }
        }

        /// <summary>
        /// 最新的FrameAnalysis文件夹
        /// </summary>
        public static string Path_LatestFrameAnalysisFolder
        {
            get
            {

                if (LatestFrameAnalysisFolderName != "")
                {
                    return Path.Combine(Path_3DmigotoLoaderFolder, LatestFrameAnalysisFolderName + "\\");
                }
                else
                {
                    return "";
                }
            }
        }

        public static string Path_LatestFrameAnalysisLogTxt
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "log.txt");
            }
        }


        //起别名方便使用
        public static string WorkFolder
        {
            get
            {
                return Path_LatestFrameAnalysisFolder;
            }
        }

        /// <summary>
        /// 肯定是要放在我们的全局配置文件夹里，放别的地方不得劲
        /// </summary>
        public static string Path_MainConfig_Global
        {
            get { return Path.Combine(Path_SSMT3GlobalConfigsFolder, PathManager.Name_GlobalConfigFileName); }
        }

        /// <summary>
        /// 全局的配置文件存储位置
        /// 因为有些东西，比如贴图标记的配置，最理想的情况下是随着用户不断地使用
        /// 最后所有之前标记过的PS Hash都能直接快速进行识别然后自动标记
        /// 此时存在一个随时会变动的缓存文件夹中就会导致缓存文件夹每次发生变化后
        /// 这些标记过的配置都丢失了
        /// 为此我们单独开一个文件夹来存储这些内容
        /// </summary>
        public static string Path_SSMT3GlobalConfigsFolder
        {
            get { return Path.Combine(Path_AppDataLocal, "SSMT3GlobalConfigs\\"); }
        }

        public static string Path_AppDataLocal
        {
            get
            { // 如果你需要非漫游配置文件路径（AppData\Local），可以这样做：
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return localAppDataPath;
            }
        }

        public static string Path_TextToolBoxConfig
        {
            get { return Path.Combine(Path_SSMT3GlobalConfigsFolder, "TextToolBoxConfig.json"); }
        }

        public static string Path_ConfigsFolder
        {
            get { 

                string configFolderPath = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Configs\\");

                if (!Directory.Exists(configFolderPath))
                {
                    Directory.CreateDirectory(configFolderPath);
                }

                return configFolderPath; 
            }
        }

        public static string Path_RunResultJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunResult.json"); }
        }

        public static string Path_RunInputJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunInput.json"); }
        }

        //逆向插件是放在缓存文件夹下面的Plugins文件夹下的
        public static string Path_SwordLv5Exe
        {
            get { return Path.Combine(PathManager.Path_PluginsFolder, "3Dmigoto-Sword-Lv5.exe"); }
        }

        public static string Path_ReversedFolder
        {
            get { return Path.Combine(PathManager.Path_BaseFolder, "Reversed\\"); }
        }

        public static string Path_ManuallyReversePageConfig
        {
            get { return Path.Combine(PathManager.Path_SwordGlobalConfigsFolder, "ManuallyReversePageConfig.json"); }
        }

        public static string Path_GameTypePageConfig
        {
            get { return Path.Combine(PathManager.Path_SSMT3GlobalConfigsFolder, "GameTypePageConfig.json"); }
        }

        public static string Path_SwordGlobalConfigsFolder
        {
            get { return Path.Combine(PathManager.Path_AppDataLocal, "SwordGlobalConfigs\\"); }
        }

    }
}
