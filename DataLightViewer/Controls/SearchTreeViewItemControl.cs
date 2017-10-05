using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DataLightViewer.Controls
{
    public class SearchTreeViewItemControl : Control
    {
        #region Dependency properties

        public static readonly DependencyProperty TreeViewItemsSourceProperty 
            = DependencyProperty.Register("TreeViewItemsSource",
                typeof(IEnumerable),
                typeof(SearchTreeViewItemControl)
                );

        public static readonly DependencyProperty SearchTextProperty
            = DependencyProperty.Register("SearchText",
                typeof(string),
                typeof(SearchTreeViewItemControl));

        public static readonly DependencyProperty SearchCommandProperty
            = DependencyProperty.Register("SearchCommand",
                typeof(ICommand),
                typeof(SearchTreeViewItemControl));

        public static readonly DependencyProperty ClearCommandProperty
            = DependencyProperty.Register("ClearCommand",
                typeof(ICommand),
                typeof(SearchTreeViewItemControl));

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

        public ICommand SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        public ICommand ClearCommand
        {
            get { return (ICommand)GetValue(ClearCommandProperty); }
            set { SetValue(ClearCommandProperty, value); }
        }

        #endregion

        static SearchTreeViewItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchTreeViewItemControl), new FrameworkPropertyMetadata(typeof(SearchTreeViewItemControl)));
        }

        public SearchTreeViewItemControl()
        {
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    SearchCommand.Execute(null);
            };
        }
    }
}
