using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TEditWPF.Controls
{
    /// <summary>
    /// Interaction logic for ScrollViewerThumbnail.xaml
    /// </summary>
    public class ScrollViewerThumbnail : UserControl
    {
        static ScrollViewerThumbnail()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollViewerThumbnail), new FrameworkPropertyMetadata(typeof(ScrollViewerThumbnail)));
        }

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollViewer. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollViewerProperty =
        DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollViewerThumbnail), new UIPropertyMetadata(null));

        public Brush HighlightFill
        {
            get { return (Brush)GetValue(HighlightFillProperty); }
            set { SetValue(HighlightFillProperty, value); }
        }

        public static readonly DependencyProperty HighlightFillProperty =
            DependencyProperty.Register("HighlightFill",
                typeof(Brush),
                typeof(ScrollViewerThumbnail),
                new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 255, 255, 0))));

        private const string PART_Highlight = "PART_Highlight";
        private const string PART_View = "PART_View";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var partHighlight = (Thumb)this.Template.FindName(PART_Highlight, this);
            partHighlight.DragDelta += PartHighlightDragDelta;

            var partView = (Rectangle)this.Template.FindName(PART_View, this);
            partView.MouseDown += PartViewMouseDown;
            //partView.MouseMove += partView_MouseMove;
        }

        void PartViewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var loc = e.GetPosition((IInputElement)sender);
                ScrollViewer.ScrollToVerticalOffset(loc.Y);
                ScrollViewer.ScrollToHorizontalOffset(loc.X);
            }
        }

        void PartViewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //
            var loc = e.GetPosition((IInputElement)sender);
            ScrollViewer.ScrollToVerticalOffset(loc.Y);
            ScrollViewer.ScrollToHorizontalOffset(loc.X);
        }

        void PartHighlightDragDelta(object sender, DragDeltaEventArgs e)
        {
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + e.VerticalChange);
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + e.HorizontalChange);
        }
    }
}
