﻿using System.Net;
using DynamicData;
using MudBlazor;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Utility;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.Web.Utility;

public static class Helpers
{
    public static string GetIconFromClass(ShipClass shipClass, ShipCategory category)
    {
        string path = ClassToPathHelper.GetSvgPathFromClass(shipClass);
        string stroke = ClassToPathHelper.GetColorFromCategory(category, true)[3..];
        string fill = ClassToPathHelper.GetColorFromCategory(category, false)[3..];
        return $"<path fill=\"#{fill}\" stroke=\"#{stroke}\" stroke-width=\"1\"  d=\"{path}\" />";
    }

    public static string GetNationFlag(IHostEnvironment environment, Nation shipNation, string shipIndex)
    {
        string imgName = File.Exists(Path.Combine(environment.ContentRootPath, "wwwroot", "assets", "nation_flags", $"flag_{shipIndex}.png")) ? shipIndex : shipNation.ShipNationToString();
        return $"/assets/nation_flags/flag_{imgName}.png";
    }

    public static Variant GetVariantFromBool(bool active, Variant variantIfTrue, Variant variantIfFalse)
    {
        return active ? variantIfTrue : variantIfFalse;
    }

    public static Color GetColorFromBool(bool active, Color colorIfTrue, Color colorIfFalse)
    {
        return active ? colorIfTrue : colorIfFalse;
    }

    public static List<string> GetStockConsumableNames(Ship ship)
    {
       return ship.ShipConsumable.GroupBy(consumable => consumable.Slot)
            .Where(consumables => consumables.Any())
            .Select(x => x.First().ConsumableName)
            .ToList();
    }

    public static List<ShipUpgrade> GetStockShipConfiguration(Ship ship)
    {
        return ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
            .OrderBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .Select(module => module.First())
            .ToList();
    }

    public static List<ShipUpgrade> GetFullUpgradedShipConfiguration(Ship ship)
    {
        return ShipModuleHelper.GroupAndSortUpgrades(ship.ShipUpgradeInfo.ShipUpgrades)
            .OrderBy(entry => entry.Key)
            .Select(entry => entry.Value)
            .Select(module => module.Last())
            .ToList();
    }

    public static void UpgradeModuleInShipConfiguration(List<ShipUpgrade> shipUpgrades, ShipUpgrade shipUpgrade)
    {
        ShipUpgrade? oldItem = shipUpgrades.FirstOrDefault(module => module.UcType == shipUpgrade.UcType);
        if (oldItem != null)
        {
            // SelectedModules.Remove(oldItem);
            shipUpgrades.Replace(oldItem, shipUpgrade);
        }
        else
        {
            shipUpgrades.Add(shipUpgrade);
        }
    }
    public static List<ShipUpgrade> GetShipConfigurationFromBuild(IEnumerable<string> storedData, List<ShipUpgrade> upgrades)
    {
        var results = new List<ShipUpgrade>();
        var shipUpgrades = ShipModuleHelper.GroupAndSortUpgrades(upgrades).OrderBy(entry => entry.Key).Select(entry => entry.Value).ToList();
        foreach (List<ShipUpgrade> upgradeList in shipUpgrades)
        {
            results.AddRange(upgradeList.Where(upgrade => storedData.Contains(upgrade.Name.NameToIndex())));
        }

        return results;
    }

    public static async Task<string?> RetrieveLongUrl(string shortUrl)
    {
        // this allows you to set the settings so that we can get the redirect url
        using HttpClient client = new(new HttpClientHandler
        {
            AllowAutoRedirect = false,
        });
        using var response = await client.GetAsync(shortUrl);
        using var content = response.Content;

        // Read the response to see if we have the redirected url
        string? redirectedUrl = null;
        if (response.StatusCode == HttpStatusCode.Found)
        {
            var headers = response.Headers;
            if (headers.Location is not null)
            {
                redirectedUrl = headers.Location.AbsoluteUri;
            }
        }

        return redirectedUrl;
    }
}
