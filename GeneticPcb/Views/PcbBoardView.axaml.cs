using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GeneticPcb.Views
{
    public class PcbBoardView : UserControl
    {
        public PcbBoardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}