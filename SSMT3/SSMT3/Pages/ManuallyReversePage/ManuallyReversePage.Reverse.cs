using Microsoft.UI.Xaml;
using SSMT3.Core.Model;
using Sword.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Pages.ManuallyReversePage
{
    public partial class ManuallyReversePage
    {
        /// <summary>
        /// 清除上一次逆向完成的在Reversed目录下的文件
        /// </summary>
        private async Task CleanLastReversedFiles()
        {
            //先检测Reversed目录下
            if (Directory.Exists(PathManager.Path_ReversedFolder))
            {


                string[] ReversedFileList = Directory.GetFiles(PathManager.Path_ReversedFolder);

                if (ReversedFileList.Length != 0)
                {
                    bool IfDeleteReversedFiles = await SSMTMessageHelper.ShowConfirm("检测到您当前Reversed目录下含有之前的逆向文件，是否清除？", "Your Reversed folder is not empty,do you want to delete all files in it?");
                    if (IfDeleteReversedFiles)
                    {
                        Directory.GetFiles(PathManager.Path_ReversedFolder).ToList().ForEach(file =>
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                LOG.Error("删除Reversed目录下的文件失败: " + file + " 错误信息: " + ex.Message);
                            }
                        });

                        _ = SSMTMessageHelper.Show("已清除Reversed目录下的所有文件。", "All files in your Reversed folder have been deleted.");
                    }
                }
            }
        }

        private List<D3D11GameType> MatcheGameTypeList(D3D11GameTypeLv2 d3D11GameTypeLv2, Dictionary<string, string> CategoryBufFilePathDict)
        {
            List<D3D11GameType> MatchedGameTypeList = [];
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {
                LOG.Info("尝试匹配数据类型: " + d3D11GameType.GameTypeName);
                int CategoryCount = d3D11GameType.CategoryStrideDict.Count;
                int BufferFileCount = CategoryBufFilePathDict.Count;
                LOG.Info("CategoryCount:" + CategoryCount.ToString());
                LOG.Info("BufferFileCount:" + BufferFileCount.ToString());

                if (CategoryCount != BufferFileCount)
                {
                    LOG.NewLine("当前数据类型分类数量和CategoryBuffer文件数量不匹配，跳过此数据类型匹配。");
                    continue;
                }

                

                string PositionFilePath = CategoryBufFilePathDict["Position"];
                int FileSize = (int)DBMTFileUtils.GetFileSize(PositionFilePath);

                if (!d3D11GameType.CategoryStrideDict.ContainsKey("Position"))
                {
                    LOG.NewLine("当前数据类型不含有Position分类，跳过此数据类型。");
                    continue;
                }

                int Stride = d3D11GameType.CategoryStrideDict["Position"];
                int VertexCount = FileSize / Stride;

                int YuShu = FileSize % Stride;
                if (YuShu != 0)
                {
                    LOG.NewLine("当前数据类型的Position分类文件大小无法被步长整除，跳过此数据类型。");
                    continue;
                }

                bool AllCategoryMatch = true;
                foreach (var item in d3D11GameType.CategoryStrideDict)
                {
                    string CategoryName = item.Key;
                    int CategoryStride = item.Value;

                    if (!CategoryBufFilePathDict.ContainsKey(CategoryName))
                    {
                        AllCategoryMatch = false;
                        break;
                    }

                    string CategoryBufFilePath = CategoryBufFilePathDict[CategoryName];
                    int CategoryBufFileSize = (int)DBMTFileUtils.GetFileSize(CategoryBufFilePath);
                    int CategoryBufVertexCount = CategoryBufFileSize / CategoryStride;

                    if (CategoryBufVertexCount != VertexCount)
                    {
                        AllCategoryMatch = false;
                        break;
                    }
                }

                if (AllCategoryMatch)
                {
                    MatchedGameTypeList.Add(d3D11GameType);
                    LOG.Info("数据类型: " + d3D11GameType.GameTypeName + " 匹配成功。");
                }

                LOG.NewLine();
            }
            LOG.NewLine();
            return MatchedGameTypeList;
        }


        private async void Button_ManuallyReverseModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await CleanLastReversedFiles();

                string ModReverseOutputFolderPath = Path.Combine(PathManager.Path_ReversedFolder);
                if (!Directory.Exists(ModReverseOutputFolderPath))
                {
                    Directory.CreateDirectory(ModReverseOutputFolderPath);
                }

                LOG.Initialize(PathManager.Path_LogsFolder);

                //先搞一个Category，BufFilePath的字典，顺便过滤空行
                Dictionary<string, string> CategoryBufFilePathDict = new Dictionary<string, string>();

                LOG.Info("CategoryBufFilePathDict::");
                foreach (CategoryBufferItem categoryBufferItem in CategoryBufferItemList)
                {
                    if (categoryBufferItem.Category.Trim() == "" || categoryBufferItem.BufFilePath.Trim() == "")
                    {
                        continue;
                    }

                    CategoryBufFilePathDict[categoryBufferItem.Category] = categoryBufferItem.BufFilePath;
                    LOG.Info("Category: " + categoryBufferItem.Category + " BufFilePath: " + categoryBufferItem.BufFilePath);
                }
                LOG.NewLine();

                //获取Position文件的大小，根据步长计算顶点数
                if (!CategoryBufFilePathDict.ContainsKey("Position"))
                {
                    _ = SSMTMessageHelper.Show("当前填写的CategoryBuffer文件列表中，未找到分类为Position的条目，请重新填写。");
                    return;
                }

                D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(ComboBox_GameTypeName.SelectedItem.ToString());
                List<D3D11GameType> MatchedGameTypeList = MatcheGameTypeList(d3D11GameTypeLv2, CategoryBufFilePathDict);

                if (MatchedGameTypeList.Count == 0)
                {
                    _ = SSMTMessageHelper.Show("未检测到任何满足条件的数据类型。可能的原因有：\n1.尚未添加此数据类型，可以去数据类型管理页面添加\n 2.未正确填写Buffer文件，请详细检查并重试。");
                    return;
                }

                //对于每个匹配的数据类型，都输出ib vb fmt文件
                foreach (D3D11GameType d3D11GameType in MatchedGameTypeList)
                {
                    MigotoModel migotoModel = new MigotoModel(CategoryBufFilePathDict, d3D11GameType,"");

                    if (ShapeKeyPositionBufferItemList.Count > 0)
                    {
                        //这里如果ShapeKey填写了其他的Buffer，则计算出形态键数据
                        ElementBuffer OriginalPOSITIONBuffer = migotoModel.ElementBufferList.FirstOrDefault(eb => eb.d3D11Element.SemanticName == "POSITION");

                        //逐个追加形态键数据，每个Buffer都要计算
                        int ShapeKeyId = 0;
                        foreach (ShapeKeyPositionBufferItem shapeKeyPositionBufferItem in ShapeKeyPositionBufferItemList)
                        {
                            string BufFilePath = shapeKeyPositionBufferItem.BufFilePath;
                            CategoryBuffer ShapeKeyPositionCategoryBuffer = new CategoryBuffer(BufFilePath, "Position", d3D11GameType);

                            foreach (ElementBuffer shapeKeyElementBuffer in ShapeKeyPositionCategoryBuffer.ElementBufferList)
                            {
                                if (shapeKeyElementBuffer.d3D11Element.SemanticName != "POSITION")
                                {
                                    continue;
                                }

                                //TODO 这里拿到了原始的POSITION数据，以及ShapeKey的POSITION数据，开始计算差值
                                Dictionary<int, byte[]> OriginalPOSITIONElementByteDict = OriginalPOSITIONBuffer.ElementByteDict;
                                Dictionary<int, byte[]> ShapeKeyPOSITIONElementByteDict = shapeKeyElementBuffer.ElementByteDict;

                                string POSITIONFormat = OriginalPOSITIONBuffer.d3D11Element.Format;
                                int POSITIONByteWidth = OriginalPOSITIONBuffer.d3D11Element.ByteWidthInt;

                                Dictionary<int, byte[]> ShapekeyByteDict = new Dictionary<int, byte[]>();

                                foreach (var item in OriginalPOSITIONElementByteDict)
                                {
                                    int Index = item.Key;
                                    if (!ShapeKeyPOSITIONElementByteDict.TryGetValue(Index, out byte[] ShapeKeyPOSITIONBytes))
                                    {
                                        continue;
                                    }

                                    byte[] OriginalPOSITIONBytes = item.Value;

                                    float ox = BitConverter.ToSingle(OriginalPOSITIONBytes, 0);
                                    float oy = BitConverter.ToSingle(OriginalPOSITIONBytes, 4);
                                    float oz = BitConverter.ToSingle(OriginalPOSITIONBytes, 8);

                                    float sx = BitConverter.ToSingle(ShapeKeyPOSITIONBytes, 0);
                                    float sy = BitConverter.ToSingle(ShapeKeyPOSITIONBytes, 4);
                                    float sz = BitConverter.ToSingle(ShapeKeyPOSITIONBytes, 8);

                                    float dx = sx - ox;
                                    float dy = sy - oy;
                                    float dz = sz - oz;

                                    // 将差值转换为 Half (float16) 并写入 byte[]
                                    byte[] halfDx = BitConverter.GetBytes((Half)dx);
                                    byte[] halfDy = BitConverter.GetBytes((Half)dy);
                                    byte[] halfDz = BitConverter.GetBytes((Half)dz);

                                    byte[] packed = new byte[6];
                                    Buffer.BlockCopy(halfDx, 0, packed, 0, 2);
                                    Buffer.BlockCopy(halfDy, 0, packed, 2, 2);
                                    Buffer.BlockCopy(halfDz, 0, packed, 4, 2);

                                    ShapekeyByteDict.Add(Index, packed);
                                }

                                //拼接ShapeKey的Element
                                D3D11Element d3d11Element = new D3D11Element();
                                d3d11Element.SemanticName = "SHAPEKEY";
                                d3d11Element.SemanticIndex = ShapeKeyId;
                                d3d11Element.Format = "R16G16B16_FLOAT";
                                d3d11Element.ByteWidth = "6";

                                ElementBuffer elementBuffer = new ElementBuffer(d3d11Element, ShapekeyByteDict);
                                migotoModel.ElementBufferList.Add(elementBuffer);

                                ShapeKeyId++;

                                break;
                            }

                        }

                    }



                    //获取顶点数
                    int MeshVertexCount = migotoModel.VertexCount;
                    LOG.Info("顶点数: " + MeshVertexCount.ToString());

                    int Count = 0;
                    foreach (IndexBufferItem indexBufferItem in IndexBufferItemList)
                    {
                        if (indexBufferItem.IBFilePath.Trim() == "")
                        {
                            continue;
                        }

                        string IBFileName = Path.GetFileNameWithoutExtension(indexBufferItem.IBFilePath);

                        string SingleGameTypeFolderPath = Path.Combine(ModReverseOutputFolderPath, "ManuallyReverse_" + d3D11GameType.GameTypeName + "\\");
                        if (!Directory.Exists(SingleGameTypeFolderPath))
                        {
                            Directory.CreateDirectory(SingleGameTypeFolderPath);
                        }

                        string NamePrefix = "Mesh-" + Count.ToString() + "-" + IBFileName;
                        string IBOutputPath = Path.Combine(SingleGameTypeFolderPath, NamePrefix + ".ib");

                        migotoModel.SaveToVBAndFmtFile(NamePrefix, SingleGameTypeFolderPath);

                        IndexBufferBufFile IBBufFile = new IndexBufferBufFile(indexBufferItem.IBFilePath, indexBufferItem.Format);
                        IBBufFile.SelfCleanExtraVertexIndex(MeshVertexCount);
                        IBBufFile.SaveToFile_UInt32(IBOutputPath, 0);

                        Count += 1;
                    }


                }
                LOG.SaveFile(PathManager.Path_LogsFolder);

                GlobalConfig.ReverseOutputFolder = ModReverseOutputFolderPath;
                GlobalConfig.SaveConfig();

                if (GlobalConfig.PostReverseAction == 0)
                {
                    SSMTCommandHelper.ShellOpenFolder(ModReverseOutputFolderPath);
                }
                else if (GlobalConfig.PostReverseAction == 1)
                {
                    _ = SSMTMessageHelper.Show("逆向成功!", "Reverse Success!");
                }
                else if (GlobalConfig.PostReverseAction == 2)
                {
                    _ = SSMTCommandHelper.ShellOpenFile(PathManager.Path_LatestSSMTLogFile);
                }
            }
            catch (Exception ex)
            {
                LOG.SaveFile(PathManager.Path_LogsFolder);
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }




    }
}
