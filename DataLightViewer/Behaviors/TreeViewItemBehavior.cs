using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataLightViewer.Behaviors
{
    public class TreeViewItemBehavior
    {
        #region ContextMenuBehavior

        public static DependencyProperty ContextMenuClickedProperty =
            DependencyProperty.RegisterAttached("ContextMenuClicked", typeof(bool), typeof(TreeViewItemBehavior),
            new UIPropertyMetadata(false, OnContextMenuClicked));

        public static bool GetContextMenuClicked(TreeViewItem treeViewItem)
            => (bool)treeViewItem.GetValue(ContextMenuClickedProperty);

        public static void SetContextMenuClicked(TreeViewItem treeViewItem, bool value)
            => treeViewItem.SetValue(ContextMenuClickedProperty, value);


        private static void OnContextMenuClicked(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as TreeViewItem;
            if (item == null)
                return;

            if ((bool)e.NewValue)
                item.MouseRightButtonDown += OnMouseRightButtonDown;
            else
                item.MouseRightButtonDown -= OnMouseRightButtonDown;
        }

        private static void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item)
            {
                item.IsSelected = true;
                e.Handled = true;
            }
        }
        #endregion

        #region BringToViewBehavior

        public static DependencyProperty IsBroughtIntoViewWhenSelectedProperty =
            DependencyProperty.RegisterAttached("IsBroughtIntoViewWhenSelected", typeof(bool), typeof(TreeViewItemBehavior), new UIPropertyMetadata(OnIsBroughtIntoViewWhenSelectedChanged));

        public static bool GetIsBroughtIntoViewWhenSelected(TreeViewItem treeViewItem)
            => (bool)treeViewItem.GetValue(IsBroughtIntoViewWhenSelectedProperty);

        public static void SetIsBroughtIntoViewWhenSelected(
          TreeViewItem treeViewItem, bool value)
            => treeViewItem.SetValue(IsBroughtIntoViewWhenSelectedProperty, value);

        private static void OnIsBroughtIntoViewWhenSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = d as TreeViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                item.Selected += OnTreeViewItemSelected;
            else
                item.Selected -= OnTreeViewItemSelected;
        }

        private static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (!ReferenceEquals(sender, e.OriginalSource))
                return;

            if (e.OriginalSource is TreeViewItem item)
                item.BringIntoView();
        }

        #endregion
    }
}
