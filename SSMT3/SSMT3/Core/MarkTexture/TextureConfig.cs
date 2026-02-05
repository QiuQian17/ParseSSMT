using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using SSMT3;
using SSMT3.InfoItemClass;
using SSMT3.Core.MarkTexture;

namespace SSMT3
{
    public class TextureDeduped
    {
        public string TrianglelistPsFileName { get; set; } = "";
        public string FALogDedupedFileName { get; set; } = "";
        public string FADataDedupedFileName { get; set; } = "";
    }

    public class ImageItem
    {
        /// <summary>
        /// 这里显示的是Trianglelist中的000001这种DrawCall开头的贴图名称
        /// </summary>
        public string FileName { get; set; } = "";

        /// <summary>
        /// ImageSource是以转换好格式的Deduped贴图路径创建的，便于正常显示在软件中
        /// </summary>
        public BitmapImage ImageSource { get; set; }

        /// <summary>
        /// 由槽位、大小、参加渲染的Deduped贴图 组成
        /// </summary>
        public string InfoBar { get; set; } = "";

        /// <summary>
        /// 当前贴图被标记的类型，被标记后将参与自动贴图过程
        /// </summary>
        public string MarkName { get; set; } = "";

        /// <summary>
        /// 当前贴图写到自动ini中后的类型，Hash风格或者Slot风格
        /// </summary>
        public string MarkStyle { get; set; } = "";

        public string PixelSlot { get; set; } = "";

        public bool Render { get; set; } = false;

        public string Suffix { get; set; } = "";
    }

    public class SlotObject
    {
        public string Slot { get; set; } = "";
        public string MarkName { get; set; } = "";
        public string MarkStyle { get; set; } = "";
        public bool Render { get; set; } = false;
        public string Suffix { get; set; } = "";
    }

    public class TextureConfig
    {
        /// <summary>
        /// 当前贴图配置保存到TextureConfigs里
        /// </summary>
        /// <param name="imageCollection"></param>
        /// <param name="TextureConfigSavePath"></param>
        public static void SaveTextureConfig(ObservableCollection<ImageItem> imageCollection, string TextureConfigSavePath)
        {
            JObject jObject = DBMTJsonUtils.CreateJObject();


            string PSHash = "";
            JArray SlotList = new JArray();
            foreach (ImageItem item in imageCollection)
            {
                if (PSHash == "")
                {
                    PSHash = DBMTStringUtils.GetPSHashFromFileName(item.FileName);
                }

                JObject slot_object = DBMTJsonUtils.CreateJObject();
                slot_object["Slot"] = item.PixelSlot;
                slot_object["MarkName"] = item.MarkName;
                slot_object["MarkStyle"] = item.MarkStyle;
                slot_object["Render"] = item.Render;
                slot_object["Suffix"] = Path.GetExtension(item.FileName);

                SlotList.Add(slot_object);
            }

            JArray PixelShaderHashArray = new JArray();

            if (File.Exists(TextureConfigSavePath))
            {
                //如果存在，则读取并修改，这样能够确保PSHashList正确得到累积
                jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);

                JArray jarryy = (JArray)jObject["PixelShaderHashList"];
                List<string> PixelShaderHashList = jarryy.ToObject<List<string>>();
                
                //先把已有的加回去
                foreach (string pshash in PixelShaderHashList)
                {
                    PixelShaderHashArray.Add(pshash);
                }

                //如果没有就加进去
                if (!PixelShaderHashList.Contains(PSHash))
                {
                    PixelShaderHashArray.Add(PSHash);
                }
                jObject["PixelShaderHashList"] = PixelShaderHashArray;
            }
            else
            {
                //在当前TextureConfig文件夹中，创建一个Json文件，内容来源于imageCollection
                PixelShaderHashArray.Add(PSHash);
            }

            jObject["SlotList"] = SlotList;
            jObject["PixelShaderHashList"] = PixelShaderHashArray;

            DBMTJsonUtils.SaveJObjectToFile(jObject, TextureConfigSavePath);
        }


        public static Dictionary<string, string> Read_PixeSlot_MarkName_Dict(string TextureConfigSavePath)
        {
            Dictionary<string, string> PixeSlot_MarkName_Dict = new Dictionary<string, string>();

            if(File.Exists(TextureConfigSavePath))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);
                JArray jArray = (JArray)jObject["SlotList"];
                foreach (JObject jobj in jArray)
                {
                    PixeSlot_MarkName_Dict[jobj["Slot"].ToString()] = jobj["MarkName"].ToString();
                }
            }
            
            return PixeSlot_MarkName_Dict;
        }

        public static Dictionary<string, SlotObject> Read_PixelSlot_SlotObject_Dict(string TextureConfigSavePath)
        {
            Dictionary<string, SlotObject> PixeSlot_SlotObject_Dict = new Dictionary<string, SlotObject>();

            if (File.Exists(TextureConfigSavePath))
            {
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigSavePath);
                JArray jArray = (JArray)jObject["SlotList"];
                foreach (JObject jobj in jArray)
                {
                    SlotObject slotobj = new SlotObject();
                    slotobj.Slot = jobj["Slot"].ToString();
                    slotobj.MarkName = jobj["MarkName"].ToString();
                    if (jobj.ContainsKey("MarkStyle"))
                    {
                        slotobj.MarkStyle = jobj["MarkStyle"].ToString();
                    }
                    else
                    {
                        slotobj.MarkStyle = "Hash";
                    }

                    slotobj.Render = (bool)jobj["Render"];
                    slotobj.Suffix = jobj["Suffix"].ToString();

                    PixeSlot_SlotObject_Dict[jobj["Slot"].ToString()] = slotobj;
                }
            }

            return PixeSlot_SlotObject_Dict;
        }


        public static Dictionary<string, JObject> Get_TextureConfigName_JObject_Dict()
        {
            LOG.Info("Get_TextureConfigName_JObject_Dict::Start");

            //读取所有的贴图配置
            Dictionary<string, JObject> TextureConfigName_JObject_Dict = new Dictionary<string, JObject>();
            LOG.Info("当前游戏的贴图配置文件夹路径: " + PathManager.Path_GameTextureConfigFolder);

            if (Directory.Exists(PathManager.Path_GameTextureConfigFolder))
            {
                string[] TextureConfigFilePathList = Directory.GetFiles(PathManager.Path_GameTextureConfigFolder);
                foreach (string TextureConfigFilePath in TextureConfigFilePathList)
                {
                    string TextureConfigName = Path.GetFileNameWithoutExtension(TextureConfigFilePath);
                    JObject TextureConfigJObject = DBMTJsonUtils.ReadJObjectFromFile(TextureConfigFilePath);
                    TextureConfigName_JObject_Dict[TextureConfigName] = TextureConfigJObject;
                }
            }
            LOG.Info("贴图配置数量: " + TextureConfigName_JObject_Dict.Count.ToString());
            LOG.Info("Get_TextureConfigName_JObject_Dict::End");
            return TextureConfigName_JObject_Dict;
        }


        public static List<string> FindMatch_TextureConfigNameListV2(List<ImageItem> ImageList, Dictionary<string, JObject> TextureConfigName_JObject_Dict)
        {
            List<string> MatchedTextureConfigNameList = new List<string>();

            string PsHash = DBMTStringUtils.GetPSHashFromFileName(ImageList[0].FileName);

            foreach (var pair in TextureConfigName_JObject_Dict)
            {
                string TextureConfigName = pair.Key;

                if (PsHash == TextureConfigName)
                {
                    MatchedTextureConfigNameList.Add(TextureConfigName);
                }
            }

            return MatchedTextureConfigNameList;
        }
        

        public static string GetConvertedTexturesFolderPath(string DrawIB)
        {
            string TextureFormatString = "jpg";
            string DedupedTexturesConvertFolderPath = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\DedupedTextures_" + TextureFormatString + "\\");
            return DedupedTexturesConvertFolderPath;
        }

        public static Dictionary<string,TextureDeduped> Read_TrianglelistDedupedFileNameDict_FromJson(string DrawIB)
        {
            Dictionary<string, TextureDeduped> keyValuePairs = new Dictionary<string, TextureDeduped>();

            string TrianglelistDedupedFileNameJsonPath = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder + DrawIB + "\\", "TrianglelistDedupedFileName.json");
            if (!File.Exists(TrianglelistDedupedFileNameJsonPath))
            {
                return keyValuePairs;
            }

            JObject TrianglelistDedupedFileNameJObject = DBMTJsonUtils.ReadJObjectFromFile(TrianglelistDedupedFileNameJsonPath);

            // 遍历JObject中的所有属性
            foreach (var property in TrianglelistDedupedFileNameJObject.Properties())
            {
                TextureDeduped textureDeduped = new TextureDeduped();
                string key = property.Name;
                textureDeduped.TrianglelistPsFileName = key;
                // 获取每个键对应的值（这也是一个JToken）
                var value = property.Value as JObject;

                if (value != null)
                {
                    textureDeduped.FALogDedupedFileName = value["FALogDedupedFileName"].ToString();
                    textureDeduped.FADataDedupedFileName = value["FADataDedupedFileName"].ToString();
                }
                keyValuePairs[key] = textureDeduped;
            }

            return keyValuePairs;
        }


        public static List<ImageItem> Read_ImageItemList(string DrawIB,string DrawCallIndex)
        {
            Debug.WriteLine("Read_ImageItemList::");
            string ConvertedTextureFolderPath = GetConvertedTexturesFolderPath(DrawIB);

            Dictionary<string, TextureDeduped> TrianglelistFileName_TextureDeduped_Dict = Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);

            Dictionary<string, string> TextureFileName_TextureSourceFilePath_Dict = new Dictionary<string, string>();
            Dictionary<string, string> PixelSlot_TextureFileName_Dict = new Dictionary<string, string>();

            foreach (var item in TrianglelistFileName_TextureDeduped_Dict)
            {
                string TextureFileName = item.Key;

                if (!TextureFileName.StartsWith(DrawCallIndex))
                {
                    continue;
                }
                //string TrianglelistTextureHash = DBMTStringUtils.GetFileHashFromFileName(TextureFileName);
                string DedupedTextureFileName = item.Value.FALogDedupedFileName;

                string ConvertedTextureFileName = "";
                if (DedupedTextureFileName.EndsWith(".dds"))
                {
                    string AutoTextureFormat = "jpg";
                    ConvertedTextureFileName = Path.GetFileNameWithoutExtension(DedupedTextureFileName) + "." + AutoTextureFormat;
                }
                else
                {
                    ConvertedTextureFileName = DedupedTextureFileName;
                }
                string ConvertexTextureFilePath = Path.Combine(ConvertedTextureFolderPath, ConvertedTextureFileName);
                Debug.WriteLine(TextureFileName);
                Debug.WriteLine(ConvertedTextureFileName);

                if (File.Exists(ConvertexTextureFilePath))
                {
                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(TextureFileName);

                    //如果已经出现了该槽位并且是dds，则不能使用jpg格式贴图进行替换
                    if (PixelSlot_TextureFileName_Dict.ContainsKey(PixelSlot))
                    {
                        string ExistsTextureFileName = PixelSlot_TextureFileName_Dict[PixelSlot];
                        if (!ExistsTextureFileName.EndsWith(".dds"))
                        {
                            TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                        }
                    }
                    else
                    {
                        PixelSlot_TextureFileName_Dict[PixelSlot] = TextureFileName;
                        TextureFileName_TextureSourceFilePath_Dict.Add(TextureFileName, ConvertexTextureFilePath);
                    }
                }
                else
                {
                    Debug.WriteLine($"ConvertexTextureFilePath: {ConvertexTextureFilePath}" + "不存在");
                }
            }


            Debug.WriteLine("贴图数量" + TextureFileName_TextureSourceFilePath_Dict.Count.ToString());

            //然后读取当前ImageCollection对应的贴图配置列表
            List<ImageItem> ImageList = [];
            foreach (var item in TextureFileName_TextureSourceFilePath_Dict)
            {
                try
                {
                    string TextureFilePath = item.Value;

                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(item.Key);
                    string PixelSlotStr = "Slot: " + PixelSlot;
                    //Debug.WriteLine("PixelSlot: " + PixelSlot);
                    TextureDeduped textureDeduped = TrianglelistFileName_TextureDeduped_Dict[item.Key];
                    string DedupedFileName = textureDeduped.FADataDedupedFileName;

                    string DedupedInfo = "";
                    bool Render = false;
                    if (DedupedFileName == "")
                    {
                        DedupedInfo = "Render: False";
                        Render = false;
                    }
                    else
                    {
                        DedupedInfo = "Render: " + DedupedFileName;
                        Render = true;
                    }

                    //这里注意指定DecodePixelType，不然无法获取长宽
                    BitmapImage bitmap = new BitmapImage
                    {
                        UriSource = new Uri(TextureFilePath)
                    };

                    int width = 0, height = 0;

                    // 强制同步（阻塞当前线程）
                    Task.Run(async () =>
                    {
                        var file = await StorageFile.GetFileFromPathAsync(TextureFilePath);
                        using var stream = await file.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        width = (int)decoder.PixelWidth;
                        height = (int)decoder.PixelHeight;
                    }).Wait(); // 阻塞直到完成

                    //Nico:
                    //大部分游戏默认使用都是Hash风格，但是少部分游戏例如GI、ZZZ等等
                    //在不同的Shader中贴图槽位会发生变化，或者游戏频繁更新导致贴图Hash值变化
                    //或者存在按距离加载时，不同距离的贴图Hash不同的情况
                    //为了解决这个问题，GI使用ORFix技术，ZZZ使用SlotFix技术
                    //这种情况下作者使用Hash风格贴图标记反而是最优解
                    string DefaultMarkStyle = "Hash";

                    GameConfig gameConfig = new GameConfig();

                    
                    if (gameConfig.LogicName == LogicName.CTXMC)
                    {
                        //第五人格 Neox2引擎 推荐使用Slot风格贴图标记
                        DefaultMarkStyle = "Slot";
                    }
                    else if (gameConfig.LogicName == LogicName.IdentityV2)
                    {
                        //第五人格 Neox3引擎 推荐使用Slot风格贴图标记
                        DefaultMarkStyle = "Slot";
                    }
                    else if (gameConfig.LogicName == LogicName.GIMI)
                    {
                        //GI 推荐使用Slot风格贴图标记
                        DefaultMarkStyle = "Slot";
                    }
                    else if (gameConfig.LogicName == LogicName.ZZMI)
                    {
                        //ZZZ 推荐使用Slot风格贴图标记
                        DefaultMarkStyle = "Slot";
                    }

                    // 添加文件名和图片到集合
                    ImageList.Add(new ImageItem
                        {
                            FileName = item.Key,
                            ImageSource = bitmap,
                            InfoBar = PixelSlotStr + "    Size: " + width.ToString() + " * " + height.ToString() + "    " + DedupedInfo,
                            PixelSlot = PixelSlot,
                            MarkName = "",
                            MarkStyle = DefaultMarkStyle,
                            Render = Render,
                            Suffix = Path.GetExtension(item.Key)
                        }); ;
                }
                catch (FileNotFoundException fnfEx)
                {
                    Debug.WriteLine($"File not found: {fnfEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading image from {item.Value}: {ex.Message}");
                }
            }


            return ImageList;
        }

        public static void ApplyEmptyTextureForDrawIB(string DrawIB)
        {
            Debug.WriteLine("应用Empty自动贴图: ");

            //先获取所有的TYPE_开头的数据类型文件夹
            List<string> TypeFolderPathList = DrawIBConfig.GetDrawIBOutputGameTypeFolderPathList(DrawIB);

            foreach (string TargetFolderPath in TypeFolderPathList)
            {
                //修改tmp.json
                string TmpJsonPath = Path.Combine(TargetFolderPath, "tmp.json");
                if (File.Exists(TmpJsonPath))
                {
                    LOG.Info("开始修改");
                    JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TmpJsonPath);
                    jObject["ComponentTextureMarkUpInfoListDict"] = JObject.FromObject(new Dictionary<string, string>());
                    LOG.Info(jObject["ComponentTextureMarkUpInfoListDict"].ToString());
                    DBMTJsonUtils.SaveJObjectToFile(jObject, TmpJsonPath);
                }
            }

        }


        public static void ApplyAllPossibleTextureConfig(MarkInfoItem markInfoItem)
        {
            //后续全部是自动识别并应用贴图的流程，这个应该在TextureConfig里设立Static方法
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            //要自动检测贴图的前提条件是存在贴图配置文件夹
            if (!Directory.Exists(PathManager.Path_GameTextureConfigFolder))
            {
                return;
            }

            //检测贴图配置数量，如果一个都没有那就不用检测了
            Dictionary<string, JObject> TextureConfigName_JObject_Dict = TextureConfig.Get_TextureConfigName_JObject_Dict();
            if (TextureConfigName_JObject_Dict.Count == 0)
            {
                return;
            }

            //开始自动贴图识别流程，自动识别满足条件的第一个贴图配置，并将其应用到自动贴图。
            LOG.Info("开始自动贴图识别流程");
            foreach (string DrawIB in DrawIBList)
            {
                
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    LOG.Info("跳过DrawIB: " + DrawIB + "，因为没有提取成功。");
                    continue;
                }


                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = DrawIBConfig.Get_ComponentName_DrawCallIndexList_Dict_FromJson(DrawIB);
                LOG.Info("ComponentName_DrawCallIndexList:" + ComponentName_DrawCallIndexList.Count.ToString());

                foreach (var pair in ComponentName_DrawCallIndexList)
                {
                    //对每个ComponentName都进行处理:
                    string ComponentName = pair.Key;
                    List<string> DrawCallIndexList = pair.Value;

                    LOG.Info("ComponentName_DrawCallIndexList:" + ComponentName_DrawCallIndexList.Count.ToString());
                    LOG.Info("DrawCallIndexList:" + DrawCallIndexList.Count.ToString());

                    bool findMatchTextureConfig = false;
                    string MatchTextureConfigName = "";
                    List<ImageItem> MatchImageList = new List<ImageItem>();
                    foreach (string DrawCallIndex in DrawCallIndexList)
                    {
                        //TODO 这里获取到的ImageList是空的
                        List<ImageItem> ImageList = TextureConfig.Read_ImageItemList(DrawIB, DrawCallIndex);

                        //如果当前ImageList是空的，则不需要进行识别了，肯定识别不到
                        if (ImageList.Count == 0)
                        {
                            continue;
                        }
                        LOG.Info("DrawCall: " + DrawCallIndex + " ImageListSize: " + ImageList.Count.ToString());


                        List<string> MatchedTextureConfigNameList = TextureConfig.FindMatch_TextureConfigNameListV2(ImageList, TextureConfigName_JObject_Dict);
                        if (MatchedTextureConfigNameList.Count != 0)
                        {
                            //暂时先指定为第一个
                            MatchTextureConfigName = MatchedTextureConfigNameList[0];

                            bool allowbreak = false;
                            //但是如果指定了markInfoItem，那么就必须精确匹配到Hash值才行
                            //然后覆盖默认的
                            if (markInfoItem.DrawIB != "")
                            {
                                LOG.Info("指定了DrawIB: " + markInfoItem.DrawIB);
                                LOG.Info("指定了PSHashValue: " + markInfoItem.PSHashValue);
                                LOG.Info("指定了CompoenntName: " + markInfoItem.CompoenntName);

                                LOG.Info("当前DrawIB: " + DrawIB);
                                LOG.Info("当前ComponentName: " + ComponentName);
                                LOG.Info("当前DrawCallIndex: " + DrawCallIndex);

                                foreach (string MatchedHash in MatchedTextureConfigNameList)
                                {
                                    LOG.Info("可选MatchedHash: " + MatchedHash);
                                }


                                //虽然默认使用第一个，但是如果制定了MarkInfo，那么则要根据PSHash再匹配一次才行。
                                if (MatchedTextureConfigNameList.Contains(markInfoItem.PSHashValue))
                                {
                                    LOG.Info("Hash值也匹配到了");
                                    MatchTextureConfigName = markInfoItem.PSHashValue;
                                    allowbreak = true;
                                }

                            }
                            else
                            {
                                allowbreak = true;
                            }


                            MatchImageList = ImageList;
                            if (allowbreak)
                            {
                                findMatchTextureConfig = true;
                                break;
                            }
                        }
                    }

                    if (!findMatchTextureConfig)
                    {
                        LOG.Info("未找到任何匹配的贴图");
                        continue;
                    }

                    LOG.Info("找到了匹配的贴图配置: " + MatchTextureConfigName);
                    //根据MatchTextureConfigName读取MarkName

                    string TextureConfigSavePath = PathManager.Path_GameTextureConfigFolder + MatchTextureConfigName + ".json";
                    LOG.Info("TextureConfigSavePath: " + TextureConfigSavePath);
                    if (File.Exists(TextureConfigSavePath))
                    {
                        Dictionary<string, SlotObject> PixeSlot_SlotObject_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                        LOG.Info("Count: " + MatchImageList.Count.ToString());
                        for (int i = 0; i < MatchImageList.Count; i++)
                        {
                            ImageItem imageItem = MatchImageList[i];
                            if (PixeSlot_SlotObject_Dict.ContainsKey(imageItem.PixelSlot))
                            {
                                SlotObject sobj = PixeSlot_SlotObject_Dict[imageItem.PixelSlot];
                                string MarkName = sobj.MarkName;

                                imageItem.MarkName = MarkName;
                                imageItem.MarkStyle = sobj.MarkStyle;


                                MatchImageList[i] = imageItem;

                                LOG.Info(MatchImageList[i].MarkName);
                            }

                        }
                    }

                    //执行到这里说明此ComponentName已匹配到对应的贴图，那么直接应用。
                    TextureConfig.ApplyTextureConfig(MatchImageList, DrawIB, ComponentName);

                }

            }

        }

        public static void ApplyTextureConfig(List<ImageItem> imageCollection,string DrawIB, string ComponentName,bool ModifyToTmpJson = true)
        {
            LOG.Info("应用自动贴图: ");
            string PartName = ComponentName.Substring("Component ".Length);
            
            //先获取所有的TYPE_开头的数据类型文件夹
            List<string> TypeFolderPathList = DrawIBConfig.GetDrawIBOutputGameTypeFolderPathList(DrawIB);

            //随后对于每个贴图，都复制贴图过去，顺便拼装一个PartName_PixelSlot = 目标文件名列表 的Dict
            //因为我们当前只操作当前PartName，所以直接来个列表就搞定了

            Dictionary<string, TextureDeduped> dictionary = TextureConfig.Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);

            List<TextureMarkUpInfo> TextureMarkUpInfoList = new List<TextureMarkUpInfo>();
            foreach (ImageItem imageItem in imageCollection)
            {

                //有标记的才能参与自动贴图
                if (imageItem.MarkName.Trim() == "")
                {
                    continue;
                }

                LOG.Info("应用: " + imageItem.MarkName);

                string suffix = Path.GetExtension(imageItem.FileName);
                string CurrentDrawIBOutputFolder = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

                string FALogDedupedFileName = dictionary[imageItem.FileName].FALogDedupedFileName;
                string ImageSourcePath = CurrentDrawIBOutputFolder + "DedupedTextures\\" + FALogDedupedFileName;
                string TextureHash = DBMTStringUtils.GetFileHashFromFileName(imageItem.FileName);

                //string TargetImageFileName = DrawIB + "_" +  PartName  + "_" + TextureHash + "_" + imageItem.MarkStyle + "_" + imageItem.MarkName  + suffix;
                //新的贴图标记不需要那么麻烦，直接DrawIB-PartName-MarkName就行，我们把关键信息都写到tmp.json中
                string TargetImageFileName = DrawIB + "-" +  PartName  + "-"  + imageItem.MarkName  + suffix;

                //拼接ResourceReplace
                TextureMarkUpInfoList.Add(new TextureMarkUpInfo { 
                    MarkName = imageItem.MarkName,
                    MarkHash = TextureHash,
                    MarkSlot = imageItem.PixelSlot,
                    MarkType = imageItem.MarkStyle,
                    MarkFileName = TargetImageFileName
                });

                //复制贴图过去
                foreach (string TargetFolderPath in TypeFolderPathList)
                {
                    string TargetImageFilePath = Path.Combine(TargetFolderPath, TargetImageFileName);
                    LOG.Info("Copy: " + ImageSourcePath + " To " + TargetImageFilePath);

                    //首先，如果贴图已经存在，就不需要重新复制了，节省一点性能
                    File.Copy(ImageSourcePath, TargetImageFilePath, true);

                }
            }

            JArray TextureMarkUpInfoJArray = new JArray();
            foreach (TextureMarkUpInfo textureMarkUpInfo in TextureMarkUpInfoList)
            {
                TextureMarkUpInfoJArray.Add(textureMarkUpInfo.GetJObject());
            }

            
            foreach (string TargetFolderPath in TypeFolderPathList)
            {
                //修改tmp.json
                string TmpJsonPath = Path.Combine(TargetFolderPath, "tmp.json");
                JObject jObject = DBMTJsonUtils.ReadJObjectFromFile(TmpJsonPath);
                JObject PartNameTextureResourceReplaceListObj = (JObject)jObject["ComponentTextureMarkUpInfoListDict"];

                if (ModifyToTmpJson)
                {
                    PartNameTextureResourceReplaceListObj[PartName] = TextureMarkUpInfoJArray;
                }
                else
                {
                    PartNameTextureResourceReplaceListObj.Remove(PartName);
                }
                    
                //jObject["PartNameTextureResourceReplaceList"] = PartNameTextureResourceReplaceListObj;
                jObject["ComponentTextureMarkUpInfoListDict"] = PartNameTextureResourceReplaceListObj;
                DBMTJsonUtils.SaveJObjectToFile(jObject, TmpJsonPath);
            }
            

        }


        public static List<string> Get_TrianglelistTexturesFileNameList(string FrameAnalysisFolderPath,string DrawIB,bool ReverseExtract = false)
        {
            FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);
            LOG.Info("Get_TrianglelistTexturesFileNameList::Start");
            List<string> TrianglelistTexturesFileNameList = [];

            List<string> TrianglelistIndexList = [];
            if (!ReverseExtract)
            {
                TrianglelistIndexList = FrameAnalysisDataUtils.Get_TrianglelistIndexListByDrawIB(FrameAnalysisFolderPath, DrawIB);
            }
            else
            {
                TrianglelistIndexList = FrameAnalysisLogUtilsV2.Get_DrawCallIndexList_ByHash(DrawIB, false,FAInfo.LogFilePath);
            }

            LOG.Info("TrianglelistIndexList Size: " + TrianglelistIndexList.Count.ToString());

            foreach (string Index in TrianglelistIndexList)
            {
                List<string> PsTextureAllFileNameList = FrameAnalysisDataUtils.FilterTextureFileNameList(FrameAnalysisFolderPath, Index + "-ps-t");
                foreach (string PsTextureFileName in PsTextureAllFileNameList)
                {
                    TrianglelistTexturesFileNameList.Add(PsTextureFileName);
                }
            }
            LOG.Info("Get_TrianglelistTexturesFileNameList::End");

            return TrianglelistTexturesFileNameList;
        }


    }
}
