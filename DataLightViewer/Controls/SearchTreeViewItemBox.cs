using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DataLightViewer.Controls
{
    public class SearchTreeViewItemBox : Control
    {
        #region Dependency properties

        public static readonly DependencyProperty TreeViewItemsSourceProperty
            = DependencyProperty.Register("TreeViewItemsSource",
                typeof(IEnumerable),
                typeof(SearchTreeViewItemBox)
                );

        public static readonly DependencyProperty SearchTextProperty
            = DependencyProperty.Register("SearchText", typeof(string), typeof(SearchTreeViewItemBox));

        #endregion

        #region Properties

        public IEnumerable TreeViewItemsSource
        {
            get { return (IEnumerable)GetValue(TreeViewItemsSourceProperty); }
            set { SetValue(TreeViewItemsSourceProperty, value); }
        }

        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        #endregion

        static SearchTreeViewItemBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTreeViewItemBox), new FrameworkPropertyMetadata(typeof(SearchTreeViewItemBox)));
        }


        public SearchTreeViewItemBox()
        {
            //InitializeComponent();

            //ClearFilter.Click += (obj, e) =>
            //{
            //    SearchTextBox.Clear();
            //};
        }
    }
}
