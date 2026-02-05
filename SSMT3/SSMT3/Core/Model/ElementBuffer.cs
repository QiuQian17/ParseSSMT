using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3.Core.Model
{
    /// <summary>
    /// 用于记录Element对应的数据
    /// 这样在外面就能用List<ElementBuffer>的形式来表示所有Element的数据
    /// 最后对这个列表进行合并，得到最终的顶点数据，以及最终的fmt文件
    /// </summary>
    public class ElementBuffer
    {

        public D3D11Element d3D11Element { get; set; } = null;

        public Dictionary<int, byte[]> ElementByteDict { get; set; } = new Dictionary<int, byte[]>();

        public ElementBuffer(D3D11Element d3D11Element, Dictionary<int, byte[]> elementByteDict)
        {
            this.d3D11Element = d3D11Element;
            ElementByteDict = elementByteDict;
        }
    }
}
