using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Core.Model
{
    public class CategoryBuffer
    {

        /// <summary>
        /// 数据类型是什么
        /// 用于初始化CategoryBuffer类
        /// </summary>
        public string BufFilePath { get; set; } = "";

        /// <summary>
        /// 属于这个数据类型的哪个分类
        /// 用于初始化CategoryBuffer类
        /// </summary>
        public string CategoryName { get; set; } = "";

        /// <summary>
        /// 所属的数据类型
        /// 用于初始化CategoryBuffer类
        /// </summary>
        public D3D11GameType d3D11GameType { get; set; } = null;


        /// <summary>
        /// 初始化CategoryBuffer后得到了这个，这个才是我们最终需要使用的，其他的都没用
        /// </summary>
        public List<ElementBuffer> ElementBufferList { get; set; } = new List<ElementBuffer>();

        public CategoryBuffer(string InBufFilePath,string InCategoryName, D3D11GameType InD3D11GameType)
        {

            this.BufFilePath = InBufFilePath;
            this.CategoryName = InCategoryName;
            this.d3D11GameType = InD3D11GameType;

            //解析为ElementBuffer列表，供后续步骤解析使用
            int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];

            // 读取文件的全部二进制内容
            byte[] fileBytes = File.ReadAllBytes(BufFilePath);
            //这里真的保险吗？一般情况下不会出现游戏里的数据全是0的情况吧，如果全是0哪还有什么意义呢？
            //当然也许确实有意义，比如BLENDINDICES全为0占位。
            //不过是很罕见的情况，因为如果BLENDINDICES全为0占位，则在读取时就无法通过

            Dictionary<int, byte[]> CategoryIndexBytes = new Dictionary<int, byte[]>();
            int totalChunks = (int)Math.Ceiling(fileBytes.Length / (double)CategoryStride);

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * CategoryStride;
                // 计算当前块的实际长度，注意最后一个块可能比chunkSize小
                int currentChunkSize = Math.Min(CategoryStride, fileBytes.Length - startIndex);
                byte[] chunk = new byte[currentChunkSize];

                // 使用Buffer.BlockCopy高效复制数组片段
                Buffer.BlockCopy(fileBytes, startIndex, chunk, 0, currentChunkSize);

                // 将分割好的块添加到结果字典中
                CategoryIndexBytes.Add(i, chunk);
            }

            //拆分为每个Element对应的Bytes
            List<D3D11Element> CategoryD3D11ElementList = new List<D3D11Element>();
            foreach (string ElementName in d3D11GameType.OrderedFullElementList)
            {
                D3D11Element d3D11Element = d3D11GameType.ElementNameD3D11ElementDict[ElementName];
                if (d3D11Element.Category != this.CategoryName)
                {
                    continue;
                }

                CategoryD3D11ElementList.Add(d3D11Element);
            }

            int StartIndex = 0;
            foreach (D3D11Element categoryD3D11Element in CategoryD3D11ElementList)
            {
                //对于每个Element，都计算出它的起始索引和结束索引
                int EndIndex = StartIndex + categoryD3D11Element.ByteWidthInt;

                Dictionary<int, byte[]> ElementByteDict = new Dictionary<int, byte[]>();
                foreach (var kvp in CategoryIndexBytes)
                {
                    int index = kvp.Key;
                    byte[] value = kvp.Value;

                    
                    // 从 value 中提取从 StartIndex 到 EndIndex 的字节
                    // 长度是 EndIndex - StartIndex
                    int length = EndIndex - StartIndex;
                    if (length > 0 && StartIndex + length <= value.Length)
                    {
                        byte[] elementBytes = new byte[length];
                        Buffer.BlockCopy(value, StartIndex, elementBytes, 0, length);
                        ElementByteDict.Add(index, elementBytes);
                    }
                }

                ElementBuffer elementBuffer = new ElementBuffer(categoryD3D11Element, ElementByteDict);
                this.ElementBufferList.Add(elementBuffer);

                StartIndex = EndIndex;
            }


        }


    }
}
