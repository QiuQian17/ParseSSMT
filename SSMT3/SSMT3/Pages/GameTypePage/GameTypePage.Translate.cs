using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class GameTypePage
    {

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TranslatePage();
        }

        private void TranslatePage()
        {
            bool isChinese = GlobalConfig.Chinese;

            TextBlock_SelectGameTypeTitle.Text = isChinese ? "选择数据类型" : "Select Game Type";
            TextBlock_D3D11ElementListTitle.Text = isChinese ? "D3D11Element列表" : "D3D11 Element List";

            Menu_File.Title = isChinese ? "文件" : "File";
            Menu_ClearD3D11ElementList.Text = isChinese ? "清空D3D11Element列表" : "Clear D3D11Element List";
            Menu_SaveD3D11ElementList.Text = isChinese ? "保存当前数据类型" : "Save Current Game Type";
            Menu_OpenGameTypeFolder.Text = isChinese ? "打开数据类型文件夹" : "Open GameType Folder";
            Menu_OpenGameTypeFile.Text = isChinese ? "打开指定数据类型文件" : "Open GameType File";

            Menu_DeleteD3D11ElementLine.Text = isChinese ? "删除选中行" : "Delete Selected Row";
            Menu_CopyLineToBottom.Text = isChinese ? "复制此行内容并添加在最底部" : "Duplicate Row To Bottom";
            Menu_CopyLineToNext.Text = isChinese ? "复制此行内容并添加在下一行" : "Duplicate Row Below";

            Button_RecalculateTotalStride.Content = isChinese ? "重新计算步长" : "Recalculate Stride";

            Button_AddNewD3D11ElementLine.Content = isChinese ? "新增数据类型行" : "Add D3D11 Element Row";
            Button_AddNewD3D11ElementLineAfterChooseLine.Content = isChinese ? "在选定行后新增数据类型行" : "Insert Row After Selection";
            Button_ClearD3D11ElementLine.Content = isChinese ? "清除所有数据类型行" : "Clear All D3D11 Element Rows";
        }

    }
}
