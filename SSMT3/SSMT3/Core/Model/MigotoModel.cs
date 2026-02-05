using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Core.Model
{

    /// <summary>
    /// 可以理解为VertexBuffer类的升级版
    /// 支持动态插入数据，例如形态键数据
    /// 同时也负责fmt文件生成，可以理解为.vb和.fmt的结合体
    /// </summary>
    public class MigotoModel
    {
        /// <summary>
        /// 记录每个分类的buf文件路径，key是分类名称，value是buf文件绝对路径
        /// 初始化MigotoModel时传入
        /// </summary>
        public Dictionary<string, string> CategoryBufFilePathDict { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 记录当前MigotoModel所属的数据类型
        /// 初始化MigotoModel时传入
        /// </summary>
        public D3D11GameType d3D11GameType { get; set; } = null;

   
        /// <summary>
        /// 初始化CategoryBuffer后得到了这个，这个才是我们最终需要使用的，其他的都没用
        /// </summary>
        public List<ElementBuffer> ElementBufferList { get; set; } = new List<ElementBuffer>();

        public int VertexCount { get; set; } = 0;

        public string LogicName { get; set; } = "";

        public MigotoModel(Dictionary<string, string> InCategoryBufFilePathDict, D3D11GameType Ind3D11GameType,string LogicName)
        {
            this.CategoryBufFilePathDict = InCategoryBufFilePathDict;
            this.d3D11GameType = Ind3D11GameType;
            this.LogicName = LogicName;

            foreach (string CategoryName in d3D11GameType.OrderedCategoryNameList)
            {
                string BufFilePath = CategoryBufFilePathDict[CategoryName];

                //这里交给CategoryBuffer来处理，得到一个List<ElementBuffer>
                CategoryBuffer categoryBuffer = new CategoryBuffer(BufFilePath, CategoryName, d3D11GameType);
                ElementBufferList.AddRange(categoryBuffer.ElementBufferList);
            }

            this.VertexCount = ElementBufferList[0].ElementByteDict.Count;
        }


        public void SaveToVBAndFmtFile(string Prefix,string OutputFolder)
        {
            string VBOutputPath = Path.Combine(OutputFolder, Prefix + ".vb");

            List<Dictionary<int, byte[]>> BufDictList = [];
            foreach (ElementBuffer elementBuffer in ElementBufferList)
            {
                BufDictList.Add(elementBuffer.ElementByteDict);
            }

            LOG.Info("分割后的顶点数: " + BufDictList[0].Count.ToString());

            Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
            byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);
            File.WriteAllBytes(VBOutputPath, FinalVB0);
            
            //这里无需担心格式问题，因为咱们规定了Mod逆向出来的.ib文件统一为R32_UINT类型
            //FMT文件默认的就是R32_UINT
            string FmtOutputPath = Path.Combine(OutputFolder, Prefix + ".fmt");
            FmtFile fmtFile = new FmtFile(d3D11GameType);
            fmtFile.LogicName = this.LogicName;
            fmtFile.d3D11ElementList = ElementBufferList.Select(eb => eb.d3D11Element).ToList();
            fmtFile.OutputFmtFileByD3D11ElementList(FmtOutputPath);
        }

        public void SetScale(float ScaleX,float ScaleY,float ScaleZ)
        {
            //获取到POSITION的ElementBuffer
            ElementBuffer PositionBuffer = ElementBufferList.FirstOrDefault(eb => eb.d3D11Element.SemanticName == "POSITION");

            Dictionary<int, byte[]> NewElementByteDict = new Dictionary<int, byte[]>();

            foreach (var kvp in PositionBuffer.ElementByteDict)
            {
                int Index = kvp.Key;
                byte[] OriginalPOSITIONBytes = kvp.Value;

                float ox = BitConverter.ToSingle(OriginalPOSITIONBytes, 0);
                float oy = BitConverter.ToSingle(OriginalPOSITIONBytes, 4);
                float oz = BitConverter.ToSingle(OriginalPOSITIONBytes, 8);

                ox = ox * ScaleX;
                oy = oy * ScaleY;
                oz = oz * ScaleZ;

                // 将差值转换为 float32 并写入 byte[]
                byte[] halfDx = BitConverter.GetBytes(ox);
                byte[] halfDy = BitConverter.GetBytes(oy);
                byte[] halfDz = BitConverter.GetBytes(oz);

                byte[] packed = new byte[12];
                Buffer.BlockCopy(halfDx, 0, packed, 0, 4);
                Buffer.BlockCopy(halfDy, 0, packed, 4, 4);
                Buffer.BlockCopy(halfDz, 0, packed, 8, 4);

                NewElementByteDict[Index] = packed;
            }

            PositionBuffer.ElementByteDict = NewElementByteDict;

            //替换原本列表里的PositionBuffer
            int posIndex = ElementBufferList.FindIndex(eb => eb.d3D11Element.SemanticName == "POSITION");
            ElementBufferList[posIndex] = PositionBuffer;
        }

        public void SelfDivide(int MinNumber, int MaxNumber)
        {
            //在这里根据IB文件的MinNumber和MaxNumber来分割顶点数据
            //构建出新的ElementBufferList
            List<ElementBuffer> NewElementBufferList = new List<ElementBuffer>();

            foreach (ElementBuffer elementBuffer in this.ElementBufferList)
            {
                Dictionary<int, byte[]> newDict = new Dictionary<int, byte[]>();

                int start = Math.Max(MinNumber, 0);
                int end = Math.Min(MaxNumber, elementBuffer.ElementByteDict.Count - 1);
                int newIndex = 0;
                for (int i = start; i <= end; i++)
                {
                    if (elementBuffer.ElementByteDict.TryGetValue(i, out var bytes))
                    {
                        newDict[newIndex] = bytes;
                        newIndex++;
                    }
                }

                ElementBuffer newElementBuffer = new ElementBuffer(elementBuffer.d3D11Element, newDict);
                NewElementBufferList.Add(newElementBuffer);
            }

            this.ElementBufferList = NewElementBufferList;
            this.VertexCount = NewElementBufferList.FirstOrDefault()?.ElementByteDict.Count ?? 0;

        }

    }
}
