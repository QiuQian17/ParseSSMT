using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT3;
using SSMT3.Core.MarkTexture;

namespace SSMT3
{
    public partial class CoreFunctions
    {
        /// <summary>
        /// 读取每个TrianglelistTextures里的贴图文件对应Deduped文件保存到Json文件供后续贴图设置使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        public static void Generate_TrianglelistDedupedFileName_Json(bool ReverseExtract = false)
        {
            LOG.Info("Generate_TrianglelistDedupedFileName_Json::Start");
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }
                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                LOG.Info("FAInfo.FolderPath: " + FAInfo.FolderPath);
                List<string> TrianglelistTextureFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);
                LOG.Info("TrianglelistTextureFileNameList Size: " + TrianglelistTextureFileNameList.Count.ToString());

                JObject Trianglelist_DedupedFileName_JObject = DBMTJsonUtils.CreateJObject();
                foreach (string TrianglelistTextureFileName in TrianglelistTextureFileNameList)
                {
                    string Hash = DBMTStringUtils.GetFileHashFromFileName(TrianglelistTextureFileName);
                    string DedupedTextureFileName = Hash + "_" + FrameAnalysisLogUtilsV2.Get_DedupedFileName(TrianglelistTextureFileName, FAInfo.FolderPath, FAInfo.LogFilePath);
                    string FADedupedFileName = FrameAnalysisDataUtils.GetDedupedTextureFileName(FAInfo.FolderPath, TrianglelistTextureFileName);
                    LOG.Info("Hash: " + Hash);
                    LOG.Info("DedupedTextureFileName: " + DedupedTextureFileName);

                    if (FADedupedFileName.Trim() != "")
                    {
                        FADedupedFileName = Hash + "_" + FADedupedFileName;
                    }

                    JObject TextureProperty = DBMTJsonUtils.CreateJObject();
                    TextureProperty["FALogDedupedFileName"] = DedupedTextureFileName;
                    TextureProperty["FADataDedupedFileName"] = FADedupedFileName;

                    Trianglelist_DedupedFileName_JObject[TrianglelistTextureFileName] = TextureProperty;
                }

                string TrianglelistDedupedFileNameJsonName = "TrianglelistDedupedFileName.json";
                string TrianglelistDedupedFileNameJsonPath = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder + DrawIB + "\\", TrianglelistDedupedFileNameJsonName);
                DBMTJsonUtils.SaveJObjectToFile(Trianglelist_DedupedFileName_JObject, TrianglelistDedupedFileNameJsonPath);
            }

            LOG.Info("Generate_TrianglelistDedupedFileName_Json::End");

        }

        /// <summary>
        /// 读取每个Component的DrawIndexList并保存到Json文件供贴图设置页面使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<string>>> Generate_ComponentName_DrawCallIndexList_Json(bool ReverseExtract = false)
        {
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::Start");
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = new Dictionary<string, Dictionary<string, List<string>>>();

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = FrameAnalysisDataUtils.Read_ComponentName_MatchFirstIndex_Dict(FAInfo.FolderPath, DrawIB);

                JObject ComponentName_DrawIndexList_JObject = DBMTJsonUtils.CreateJObject();

                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = new Dictionary<string, List<string>>();

                foreach (var item in ComponentName_MatchFirstIndex_Dict)
                {
                    List<string> DrawCallIndexList = new List<string>();

                    if (ReverseExtract)
                    {
                        DrawCallIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByMatchFirstIndex(DrawIB, item.Value);
                    }
                    else
                    {
                        DrawCallIndexList = FrameAnalysisDataUtils.Read_DrawCallIndexList(FAInfo.FolderPath, DrawIB, item.Key);
                    }

                    ComponentName_DrawIndexList_JObject[item.Key] = new JArray(DrawCallIndexList);
                    ComponentName_DrawCallIndexList[item.Key] = DrawCallIndexList;
                }

                string SaveFileName = "ComponentName_DrawCallIndexList.json";
                string SaveJsonFilePath = Path.Combine(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\"), SaveFileName);
                DBMTJsonUtils.SaveJObjectToFile(ComponentName_DrawIndexList_JObject, SaveJsonFilePath);

                DrawIB_ComponentName_DrawCallIndexList_Dict_Dict[DrawIB] = ComponentName_DrawCallIndexList;
            }
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::End");
            return DrawIB_ComponentName_DrawCallIndexList_Dict_Dict;
        }


        

        /// <summary>
        /// 在正向提取和逆向提取后都需要做的事情
        /// </summary>
        public static void PostDoAfterExtract(bool ReverseExtract = false)
        {
            LOG.Info("提取DedupedTextures并转换为jpg格式，用于后续贴图标记流程:");
            CoreFunctions.ExtractDedupedTextures();
            SSMTTextureHelper.ConvertDedupedTexturesToTargetFormat();


            //(1) 贴图标记功能前置1
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = Generate_ComponentName_DrawCallIndexList_Json();

            //(2) 贴图标记功能前置2
            Generate_TrianglelistDedupedFileName_Json();

            //(3)自动检测贴图配置并自动上贴图
            MarkInfoItem markInfoItem = new MarkInfoItem();
            TextureConfig.ApplyAllPossibleTextureConfig(markInfoItem);


        }


    }
}
