using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace GeneticPcb.Views
{
    public class SolderingPointsView : UserControl
    {
        public SolderingPointsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}