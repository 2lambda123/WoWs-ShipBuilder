﻿using System;
using System.Collections.Generic;
using NLog;
using WoWsShipBuilder.Core.Services;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class Localizer
    {
        private static readonly Lazy<Localizer> InstanceProducer = new(() => new());
        private static readonly Logger Logger = Logging.GetLogger("Localization");

        private readonly IAppDataService appDataService;

        private Dictionary<string, string> languageData = new();
        private CultureDetails? locale;

        public Localizer()
            : this(DesktopAppDataService.Instance)
        {
        }

        public Localizer(IAppDataService appDataService)
        {
            this.appDataService = appDataService;

            // Prevent exception when using Avalonia previewer due to uninitialized settings.
            var updateLanguage = AppData.IsInitialized ? AppData.Settings.SelectedLanguage : AppConstants.DefaultCultureDetails;
            UpdateLanguage(updateLanguage, true);
        }

        public static Localizer Instance => InstanceProducer.Value;

        public (bool IsLocalized, string Localization) this[string key] =>
            languageData.TryGetValue(key.ToUpperInvariant(), out var localization) ? (true, localization) : (false, key);

        public void UpdateLanguage(CultureDetails newLocale, bool forceLanguageUpdate)
        {
            if (locale == newLocale && languageData.Count > 0 && !forceLanguageUpdate)
            {
                Logger.Info("Old and new locale are identical. Ignoring localization update.");
                return;
            }

            var serverType = AppData.IsInitialized ? AppData.Settings.SelectedServerType : ServerType.Live;
            Dictionary<string, string>? localLanguageData = appDataService.ReadLocalizationData(serverType, newLocale.LocalizationFileName);
            if (localLanguageData == null)
            {
                Logger.Warn("Unable to load localization data for locale {0}.", newLocale);
                return;
            }

            locale = newLocale;
            languageData = localLanguageData;
            Logger.Info("Updated localization data to locale {0}.", newLocale);
        }
    }
}
