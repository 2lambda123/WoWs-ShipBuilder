using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using Splat;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;

namespace WoWsShipBuilder.UI.Settings
{
    [SuppressMessage("System.IO.Abstractions", "IO0002", Justification = "This class is never tested.")]
    [SuppressMessage("System.IO.Abstractions", "IO0006", Justification = "This class is never tested.")]
    public static class AppSettingsHelper
    {
        private static readonly string SettingFile = Path.Combine(DesktopAppDataService.Instance.DefaultAppDataDirectory, "settings.json");

        public static void SaveSettings() => Locator.Current.GetServiceSafe<IDataService>().StoreAsync(AppData.Settings, SettingFile);

        public static void LoadSettings()
        {
            if (File.Exists(SettingFile))
            {
                Logging.Logger.Info("Trying to load settings from settings file...");
                var settings = Locator.Current.GetServiceSafe<IDataService>().Load<AppSettings>(SettingFile);
                if (settings == null)
                {
                    Logging.Logger.Error("Unable to parse local settings file. Creating empty settings instance.");
                    settings = new();
                }

                AppData.Settings = settings;
            }
            else
            {
                Logging.Logger.Info("No settings file found, creating new settings...");
                AppData.Settings = new();
            }

            AppData.IsInitialized = true;
            Logging.Logger.Info("Settings initialized.");
            UpdateThreadCulture(AppData.Settings.SelectedLanguage.CultureInfo);
        }

        private static void UpdateThreadCulture(CultureInfo cultureInfo)
        {
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}
