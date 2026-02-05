using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT3
{
    public partial class GameTypePage
    {

        private void Menu_CopyLineToBottom_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCloneSelectedElement(out D3D11Element? clonedElement))
            {
                return;
            }

            int insertIndex = D3D11ElementList.Count;
            if (insertIndex > 0 && string.IsNullOrWhiteSpace(D3D11ElementList[^1].SemanticName))
            {
                insertIndex -= 1;
            }

            D3D11ElementList.Insert(insertIndex, clonedElement);
            EnsureBlankLineAtEnd();
            CalculateAndShowTotalStride();
        }

        private void Menu_CopyLineToNext_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCloneSelectedElement(out D3D11Element? clonedElement))
            {
                return;
            }

            int insertIndex = Math.Min(DataGrid_GameType.SelectedIndex + 1, D3D11ElementList.Count);
            D3D11ElementList.Insert(insertIndex, clonedElement);
            EnsureBlankLineAtEnd();
            CalculateAndShowTotalStride();
        }

        private bool TryCloneSelectedElement(out D3D11Element? clone)
        {
            clone = null;
            int selectedIndex = DataGrid_GameType.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= D3D11ElementList.Count)
            {
                _ = SSMTMessageHelper.Show("请先至少鼠标左键单机选中一个行");
                return false;
            }

            clone = CloneElement(D3D11ElementList[selectedIndex]);
            return true;
        }

        private static D3D11Element CloneElement(D3D11Element source)
        {
            return new D3D11Element
            {
                SemanticName = source.SemanticName,
                Format = source.Format,
                ExtractSlot = source.ExtractSlot,
                ExtractTechnique = source.ExtractTechnique,
                Category = source.Category,
                DrawCategory = source.DrawCategory,
                ByteWidth = source.ByteWidth
            };
        }

        private void EnsureBlankLineAtEnd()
        {
            if (D3D11ElementList.Count == 0 || !string.IsNullOrWhiteSpace(D3D11ElementList[^1].SemanticName))
            {
                AddBlankD3D11ElementLine();
            }
        }


    }
}
