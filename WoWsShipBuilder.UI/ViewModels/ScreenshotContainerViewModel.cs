﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    internal class ScreenshotContainerViewModel : ViewModelBase
    {
        public ScreenshotContainerViewModel()
        {
            var (ship, _) = DataHelper.LoadPreviewShip(ShipClass.Cruiser, 10, Nation.Usa);
            CaptainSkillSelectorViewModel = new(ship.ShipClass, ship.ShipNation);
            SignalSelectorViewModel = new();
            ShipModuleViewModel = new(ship.ShipUpgradeInfo);
            UpgradePanelViewModel = new(ship);
            ConsumableViewModel = new(ship);
            BuildName = ship.Name;
            ShipData = ship;
        }

        public ScreenshotContainerViewModel(Build build, Ship ship)
        {
            CaptainSkillSelectorViewModel = new(ship.ShipClass, ship.ShipNation);
            CaptainSkillSelectorViewModel.LoadBuild(build.Skills, build.Captain);
            SignalSelectorViewModel = new();
            SignalSelectorViewModel.LoadBuild(build.Signals);
            ShipModuleViewModel = new(ship.ShipUpgradeInfo);
            ShipModuleViewModel.LoadBuild(build.Modules);
            UpgradePanelViewModel = new(ship);
            UpgradePanelViewModel.LoadBuild(build.Upgrades);
            ConsumableViewModel = new(ship);
            ConsumableViewModel.LoadBuild(build.Consumables);
            BuildName = build.BuildName;
            ShipData = ship;
        }

        public string BuildName { get; }

        public Ship ShipData { get; }

        public CaptainSkillSelectorViewModel CaptainSkillSelectorViewModel { get; }

        public SignalSelectorViewModel SignalSelectorViewModel { get; }

        public ShipModuleViewModel ShipModuleViewModel { get; }

        public UpgradePanelViewModel UpgradePanelViewModel { get; }

        public ConsumableViewModel ConsumableViewModel { get; }

        public static void RenderScreenshot(Window window, string outputFileName)
        {
            var screenshotContainer = window.FindControl<ScreenshotContainer>("ScreenshotContainer");
            var pixelSize = new PixelSize((int)screenshotContainer.Width, (int)screenshotContainer.Height);
            using var bitmap = new RenderTargetBitmap(pixelSize);
            var size = pixelSize.ToSize(1);
            screenshotContainer.Measure(size);
            screenshotContainer.Arrange(new(size));
            bitmap.Render(screenshotContainer);
            bitmap.Save(outputFileName);
        }
    }
}
