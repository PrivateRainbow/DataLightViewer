using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataLightViewer.Helpers
{
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            while (GetSelectedTreeViewItemParent(item) != null)
            {
                var parent = GetSelectedTreeViewItemParent(item);
                if (parent != null)
                    return parent.GetDepth() + 1;

                item = parent;
            }
            return 0;
        }

        public static TreeViewItem GetSelectedTreeViewItemParent(this TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }
    }
}
