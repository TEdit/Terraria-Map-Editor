// Author: Josh Twist
// Source: http://thejoyofcode.com/WPF_ScrollViewer_Thumbnail.aspx


using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TEdit.UI.Xaml;


/// <summary>
/// Interaction logic for ScrollViewerThumbnail.xaml
/// </summary>
public class ScrollViewerThumbnail : UserControl
{
    private const string PART_Highlight = "PART_Highlight";
    private const string PART_View = "PART_View";

    public static readonly DependencyProperty ScrollViewerProperty =
        DependencyProperty.Register("ScrollViewer", typeof (ScrollViewer), typeof (ScrollViewerThumbnail),
                                    new UIPropertyMetadata(null));

    public static readonly DependencyProperty HighlightFillProperty =
        DependencyProperty.Register("HighlightFill",
                                    typeof (Brush),
                                    typeof (ScrollViewerThumbnail),
                                    new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 255, 255, 0))));

    static ScrollViewerThumbnail()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof (ScrollViewerThumbnail),
                                                 new FrameworkPropertyMetadata(typeof (ScrollViewerThumbnail)));
    }

    public ScrollViewer ScrollViewer
    {
        get { return (ScrollViewer) GetValue(ScrollViewerProperty); }
        set { SetValue(ScrollViewerProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ScrollViewer. This enables animation, styling, binding, etc...

    public Brush HighlightFill
    {
        get { return (Brush) GetValue(HighlightFillProperty); }
        set { SetValue(HighlightFillProperty, value); }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        var partHighlight = (Thumb) Template.FindName(PART_Highlight, this);
        partHighlight.DragDelta += PartHighlightDragDelta;

        var partView = (Rectangle) Template.FindName(PART_View, this);
        partView.MouseDown += PartViewMouseDown;
        //partView.MouseMove += partView_MouseMove;
    }

    private void PartViewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            Point loc = e.GetPosition((IInputElement) sender);
            ScrollViewer.ScrollToVerticalOffset(loc.Y);
            ScrollViewer.ScrollToHorizontalOffset(loc.X);
        }
    }

    private void PartViewMouseDown(object sender, MouseButtonEventArgs e)
    {
        Point loc = e.GetPosition((IInputElement) sender);
        ScrollViewer.ScrollToVerticalOffset(loc.Y);
        ScrollViewer.ScrollToHorizontalOffset(loc.X);
    }

    private void PartHighlightDragDelta(object sender, DragDeltaEventArgs e)
    {
        ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + e.VerticalChange);
        ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + e.HorizontalChange);
    }
}