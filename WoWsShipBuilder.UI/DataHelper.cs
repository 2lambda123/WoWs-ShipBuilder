using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Data;
using WoWsShipBuilder.Core.DataContainers;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Utility;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;
using WoWsShipBuilder.ViewModels.ShipVm;
using ShipUpgrade = WoWsShipBuilder.DataStructures.Ship.ShipUpgrade;

namespace WoWsShipBuilder.UI;

// TODO: fix this entire mess
public static class DataHelper
{
    public static readonly IReadOnlyList<Modernization> PlaceholderBaseList = new List<Modernization> { UpgradePanelViewModelBase.PlaceholderModernization };

    public static ILocalizer DemoLocalizer { get; } = new DemoLocalizerImpl();

    public static Build CreateTestBuild(string name = "test-build") => new(name, null!, Nation.Common, null!, null!, null!, null!, null!, null!);

    public static (Ship Ship, List<ShipUpgrade> Configuration) LoadPreviewShip(ShipClass shipClass, int tier, Nation nation, ServerType serverType = ServerType.Dev1)
    {
        Console.WriteLine("Test1");
        LoadNationFiles(nation, serverType);
        Console.WriteLine("Test2");

        var ship = ReadLocalJsonData<Ship>(nation, ServerType.Live)!
            .Select(entry => entry.Value)
            .First(ship => ship.ShipClass == shipClass && ship.Tier == tier);

        Console.WriteLine("Test3");
        var configuration = ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
            .Select(entry => entry.Value.FirstOrDefault())
            .Where(item => item != null)
            .Cast<ShipUpgrade>()
            .ToList();

        Console.WriteLine("Test4");

        return (ship, configuration);
    }

    public static ShipSummary GetPreviewShipSummary(ShipClass shipClass, int tier, Nation nation)
    {
        var ship = LoadPreviewShip(shipClass, tier, nation);
        var dataService = new DesktopDataService(new FileSystem());
        string fileName = dataService.CombinePaths(DesktopAppDataService.PreviewInstance.GetDataPath(ServerType.Live), "Summary", "Common.json");
        var summaryTask = DesktopAppDataService.PreviewInstance.DeserializeFile<List<ShipSummary>>(fileName);
        return summaryTask.Result!.First(summary => summary.Index == ship.Ship.Index);
    }

    public static MainViewModelParams GetPreviewViewModelParams(ShipClass shipClass, int tier, Nation nation)
    {
        return new(LoadPreviewShip(shipClass, tier, nation).Ship, GetPreviewShipSummary(shipClass, tier, nation));
    }

    public static TurretModule GetPreviewTurretModule(ShipClass shipClass, int tier, Nation nation)
    {
        var testData = LoadPreviewShip(shipClass, tier, nation);
        var currentShipStats = ShipDataContainer.CreateFromShip(testData.Ship, testData.Configuration, new());
        return currentShipStats.MainBatteryDataContainer!.OriginalMainBatteryData;
    }

    private static void LoadNationFiles(Nation nation, ServerType serverType)
    {
        var newEntries = ReadLocalJsonData<Ship>(nation, serverType)!;
        foreach ((string key, var value) in newEntries)
        {
            AppData.ShipDictionary[key] = value;
        }

        AppData.ProjectileCache.SetIfNotNull(nation, ReadLocalJsonData<Projectile>(nation, serverType));
        AppData.AircraftCache.SetIfNotNull(nation, ReadLocalJsonData<Aircraft>(nation, serverType));
        AppData.ConsumableList = ReadLocalJsonData<Consumable>(Nation.Common, serverType)!;
        AppData.ModernizationCache = ReadLocalJsonData<Modernization>(Nation.Common, serverType)!;
    }

    private static Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType)
    {
        string categoryString = GameDataHelper.GetCategoryString<T>();
        string nationString = GameDataHelper.GetNationString(nation);
        var dataService = new DesktopDataService(new FileSystem());
        string fileName = dataService.CombinePaths(DesktopAppDataService.PreviewInstance.GetDataPath(serverType), categoryString, $"{nationString}.json");
        return dataService.Load<Dictionary<string, T>>(fileName);
    }

    public class DemoLocalizerImpl : ILocalizer
    {
        public LocalizationResult this[string key] => GetGameLocalization(key);

        public LocalizationResult GetGameLocalization(string key) => new(true, key);

        public LocalizationResult GetAppLocalization(string key) => new(true, key);

        public LocalizationResult GetGameLocalization(string key, CultureDetails language) => new(true, key);

        public LocalizationResult GetAppLocalization(string key, CultureDetails language) => new(true, key);
    }
}
