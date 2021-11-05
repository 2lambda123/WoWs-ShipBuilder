using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.Views
{
    public partial class ShipSelectionWindow : Window
    {
        public ShipSelectionWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void RunResearch(object sender, PointerReleasedEventArgs e)
        {
            if (e.Source is not TextBlock && e.Source is not ContentPresenter)
            {
                var box = sender as AutoCompleteBox;
                box!.IsDropDownOpen = true;
                e.Handled = true;
            }
        }

        public void ShowResult(object sender, CancelEventArgs e)
        {
            var viewModel = DataContext as ShipSelectionWindowViewModel;
            viewModel!.UpdateResult();
        }
    }
}
