using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GeneticPcb.Views
{
    public class SegmentsView : UserControl
    {
        public SegmentsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}