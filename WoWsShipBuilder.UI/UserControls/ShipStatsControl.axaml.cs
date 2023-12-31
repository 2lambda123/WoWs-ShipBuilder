using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging;
using Splat;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.UI.Extensions;
using WoWsShipBuilder.UI.ViewModels.DispersionPlot;
using WoWsShipBuilder.UI.Views;
using WoWsShipBuilder.ViewModels.ShipVm;
using static WoWsShipBuilder.UI.ViewModels.DispersionPlot.DispersionGraphViewModel;

namespace WoWsShipBuilder.UI.UserControls
{
    public class ShipStatsControl : UserControl
    {
        public ShipStatsControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OpenDispersionGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Dispersion);
        }

        public void OpenDispersionPlotWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Plot);
        }

        public void OpenBallisticGraphWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Ballistic);
        }

        public void OpenShellTrajectoryWindow(object sender, PointerReleasedEventArgs e)
        {
            OpenGraphsWindow(sender, e, Tabs.Trajectory);
        }

        public void OpenTurretAnglesWindow(object sender, PointerReleasedEventArgs e)
        {
            var dc = (ShipStatsControlViewModel)DataContext!;
            var win = new FiringAngleWindow
            {
                DataContext = new FiringAngleViewModel(dc.CurrentShipStats!.MainBatteryDataContainer!.OriginalMainBatteryData.Guns),
            };
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        private void OpenGraphsWindow(object sender, PointerReleasedEventArgs e, Tabs tab)
        {
            var dc = DataContext as ShipStatsControlViewModel;
            var mainBattery = dc!.CurrentShipStats!.MainBatteryDataContainer!;
            var win = new DispersionGraphsWindow();
            var textBlock = (TextBlock)sender;
            string shellIndex = ((ShellDataContainer)textBlock.DataContext!).Name;
            var shell = AppData.FindProjectile<ArtilleryShell>(shellIndex);
            win.DataContext = new DispersionGraphViewModel(Locator.Current.GetRequiredService<ILogger<DispersionGraphViewModel>>(), win, mainBattery.DispersionData, (double)mainBattery.Range * 1000, dc.CurrentShipStats.Index, shell, tab, mainBattery.Sigma);
            win.Show((Window)this.GetVisualRoot());
            e.Handled = true;
        }

        public void OpenDepthChargeDamageDistributionChart(object sender, PointerReleasedEventArgs e)
        {
            if (DataContext is not ShipStatsControlViewModel vm)
            {
                e.Handled = true;
                return;
            }

            var dataContext = vm.CurrentShipStats?.DepthChargeLauncherDataContainer?.DepthCharge ?? vm.CurrentShipStats?.AswAirstrikeDataContainer?.Weapon as DepthChargeDataContainer;
            if (dataContext != null)
            {
                var win = new DepthChargeDamageDistributionChartWindow
                {
                    DataContext = new DepthChargeDamageDistributionChartRecord(dataContext.Damage, dataContext.DcSplashRadius, dataContext.PointsOfDmg),
                };
                win.Show((Window)this.GetVisualRoot());
            }

            e.Handled = true;
        }

        private void ShowTorpedoAngles(object? sender, PointerReleasedEventArgs e)
        {
            if (DataContext is ShipStatsControlViewModel viewModel)
            {
                var win = new FiringAngleWindow
                {
                    DataContext = new FiringAngleViewModel(viewModel.CurrentShipStats!.TorpedoArmamentDataContainer!.TorpedoLaunchers),
                };
                win.Show((Window)this.GetVisualRoot());
                e.Handled = true;
            }
        }
    }
}
