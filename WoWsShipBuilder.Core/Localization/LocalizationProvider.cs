﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;

namespace WoWsShipBuilder.Core.Localization;

public class LocalizationProvider : ILocalizationProvider
{
    private readonly Dictionary<CultureDetails, Dictionary<string, string>> localizationData = new();

    private readonly IAppDataService appDataService;

    public LocalizationProvider(IAppDataService appDataService)
    {
        this.appDataService = appDataService;
    }

    public async Task RefreshDataAsync(ServerType serverType, params CultureDetails[] supportedCultures)
    {
        foreach (var culture in supportedCultures)
        {
            var cultureLocalization = await appDataService.ReadLocalizationData(serverType, culture.LocalizationFileName) ?? throw new InvalidOperationException("Localization data not found");
            localizationData[culture] = cultureLocalization;
        }
    }

    public string? GetString(string key, CultureDetails cultureDetails)
    {
        if (!localizationData.TryGetValue(cultureDetails, out Dictionary<string, string>? cultureLocalization))
        {
            return null;
        }

        cultureLocalization.TryGetValue(key.ToUpperInvariant(), out string? localization);
        return localization;
    }
}
