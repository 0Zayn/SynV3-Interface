using System.Windows.Media;
using System.Windows;

namespace SynV3
{
    public static class VisualTree
    {
        public static T FindAncestor<T>(DependencyObject Current) where T : DependencyObject
        {
            while (Current != null)
            {
                if (Current is T)
                    return (T)Current;

                Current = VisualTreeHelper.GetParent(Current);
            }
            return null;
        }
    }
}
