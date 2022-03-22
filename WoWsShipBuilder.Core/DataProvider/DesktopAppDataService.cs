using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Newtonsoft.Json;
using Splat;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public class DesktopAppDataService : IAppDataService
    {
        #region Static Fields and Constants

        private const string ShipBuilderName = "WoWsShipBuilder";

#pragma warning disable CS8603
        private static readonly Lazy<DesktopAppDataService> InstanceValue = new(() => Locator.Current.GetService<DesktopAppDataService>() ?? PreviewInstance);
#pragma warning restore CS8603

        #endregion

        private readonly IFileSystem fileSystem;

        private DesktopAppDataService()
            : this(new FileSystem())
        {
        }

        public DesktopAppDataService(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            DefaultAppDataDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ShipBuilderName);
        }

        public static DesktopAppDataService Instance => InstanceValue.Value;

        public static DesktopAppDataService PreviewInstance { get; } = new(new FileSystem());

        public string DefaultAppDataDirectory { get; }

        public string AppDataDirectory
        {
            get
            {
                if (AppData.IsInitialized)
                {
                    return AppData.Settings.CustomDataPath ?? DefaultAppDataDirectory;
                }

                return DefaultAppDataDirectory;
            }
        }

        public string AppDataImageDirectory => fileSystem.Path.Combine(AppDataDirectory, "Images");

        public string BuildImageOutputDirectory => AppData.Settings.CustomImagePath ?? fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), ShipBuilderName);

        public string GetDataPath(ServerType serverType)
        {
            string serverName = serverType.StringName();
            return fileSystem.Path.Combine(AppDataDirectory, "json", serverName);
        }

        /// <summary>
        /// Gets the localization directory for the selected server type.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The directory path of the current localization directory.</returns>
        public string GetLocalizationPath(ServerType serverType) => fileSystem.Path.Combine(GetDataPath(serverType), "Localization");

        /// <summary>
        /// Find the list of currently installed localizations.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <param name="includeFileType">Specifies whether the list of installed locales should contain the file extensions for each file.</param>
        /// <returns>A possibly empty list of installed locales.</returns>
        public List<string> GetInstalledLocales(ServerType serverType, bool includeFileType = true)
        {
            fileSystem.Directory.CreateDirectory(GetLocalizationPath(serverType));
            var files = fileSystem.Directory.GetFiles(GetLocalizationPath(serverType)).Select(file => fileSystem.FileInfo.FromFileName(file));
            return includeFileType ? files.Select(file => file.Name).ToList() : files.Select(file => fileSystem.Path.GetFileNameWithoutExtension(file.Name)).ToList();
        }

        /// <summary>
        /// Helper method to create the path for a build image file.
        /// </summary>
        /// <param name="buildName">The name of the saved build.</param>
        /// <param name="shipName">The name of the ship of the build.</param>
        /// <returns>The path where the generated image should be stored.</returns>
        public string GetImageOutputPath(string buildName, string shipName)
        {
            string directory = BuildImageOutputDirectory;
            fileSystem.Directory.CreateDirectory(directory);
            return fileSystem.Path.Combine(directory, shipName + " - " + buildName + ".png");
        }

        /// <summary>
        /// Read a dictionary from the local app data directory.
        /// </summary>
        /// <param name="nation">The selected nation.</param>
        /// <param name="serverType">The selected server type.</param>
        /// <typeparam name="T">The data type of the values of the dictionary.</typeparam>
        /// <returns>A dictionary containing the deserialized file content.</returns>
        public Dictionary<string, T>? ReadLocalJsonData<T>(Nation nation, ServerType serverType)
        {
            string categoryString = IAppDataService.GetCategoryString<T>();
            string nationString = IAppDataService.GetNationString(nation);
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), categoryString, $"{nationString}.json");
            return DeserializeFile<Dictionary<string, T>>(fileName);
        }

        /// <summary>
        /// Read the current version info from the app data directory.
        /// </summary>
        /// <param name="serverType">The selected server type.</param>
        /// <returns>The local VersionInfo or null if none was found.</returns>
        public VersionInfo? ReadLocalVersionInfo(ServerType serverType)
        {
            string filePath = fileSystem.Path.Combine(GetDataPath(serverType), "VersionInfo.json");
            return DeserializeFile<VersionInfo>(filePath);
        }

        public List<ShipSummary> GetShipSummaryList(ServerType serverType)
        {
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), "Summary", "Common.json");
            return DeserializeFile<List<ShipSummary>>(fileName) ?? new List<ShipSummary>();
        }

        public void LoadNationFiles(Nation nation)
        {
            var server = AppData.Settings.SelectedServerType;
            if (AppData.ShipDictionary?.FirstOrDefault() == null || AppData.ShipDictionary.First().Value.ShipNation != nation)
            {
                AppData.ShipDictionary = ReadLocalJsonData<Ship>(nation, server);
            }

            AppData.ProjectileCache.SetIfNotNull(nation, ReadLocalJsonData<Projectile>(nation, server));
            AppData.AircraftCache.SetIfNotNull(nation, ReadLocalJsonData<Aircraft>(nation, server));
            AppData.ConsumableList ??= ReadLocalJsonData<Consumable>(Nation.Common, server);
        }

        /// <summary>
        /// Reads projectile data from the current <see cref="AppData.ProjectileCache"/> and returns the result.
        /// Initializes the data for the nation of the provided projectile name if it is not loaded already.
        /// </summary>
        /// <param name="projectileName">The name of the projectile, <b>MUST</b> start with the projectile's index.</param>
        /// <returns>The projectile for the specified name.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the projectile name does not exist in the projectile data.</exception>
        public Projectile GetProjectile(string projectileName)
        {
            var nation = IAppDataService.GetNationFromIndex(projectileName);
            if (!AppData.ProjectileCache.ContainsKey(nation))
            {
                AppData.ProjectileCache.SetIfNotNull(nation, ReadLocalJsonData<Projectile>(nation, AppData.Settings.SelectedServerType));
            }

            return AppData.ProjectileCache[nation][projectileName];
        }

        /// <summary>
        /// Reads projectile data from the current <see cref="AppData.ProjectileCache"/> and returns the result, cast to the requested type.
        /// Initializes the data for the nation of the provided projectile name if it is not loaded already.
        /// </summary>
        /// <param name="projectileName">The name of the projectile, <b>MUST</b> start with the projectile's index.</param>
        /// <typeparam name="T">
        /// The requested return type. Must be a sub type of <see cref="Projectile"/>.
        /// The caller is responsible for ensuring that the requested projectile is of the specified type.<br/>
        /// <b>This method does not handle exceptions caused by an invalid cast!</b>
        /// </typeparam>
        /// <returns>The requested projectile, cast to the specified type.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the projectile name does not exist in the projectile data.</exception>
        public T GetProjectile<T>(string projectileName) where T : Projectile
        {
            return (T)GetProjectile(projectileName);
        }

        /// <summary>
        /// Reads aircraft data from the current <see cref="AppData.AircraftCache"/> and returns the result.
        /// Initializes the data for the nation of the provided aircraft name if it is not loaded already.
        /// </summary>
        /// <param name="aircraftName">The name of the aircraft, <b>MUST</b> start with the aircraft's index.</param>
        /// <returns>The requested aircraft.</returns>
        /// <exception cref="KeyNotFoundException">Occurs if the aircraft name does not exist in the aircraft data.</exception>
        public Aircraft GetAircraft(string aircraftName)
        {
            var nation = IAppDataService.GetNationFromIndex(aircraftName);
            if (!AppData.AircraftCache.ContainsKey(nation))
            {
                AppData.AircraftCache.SetIfNotNull(nation, ReadLocalJsonData<Aircraft>(nation, AppData.Settings.SelectedServerType));
            }

            return AppData.AircraftCache[nation][aircraftName];
        }

        public Dictionary<string, string>? ReadLocalizationData(ServerType serverType, string language)
        {
            string fileName = fileSystem.Path.Combine(GetDataPath(serverType), "Localization", $"{language}.json");
            return fileSystem.File.Exists(fileName) ? DeserializeFile<Dictionary<string, string>>(fileName) : null;
        }

        public Ship? GetShipFromSummary(ShipSummary summary, bool changeDictionary = true)
        {
            Ship? ship = null;

            if (summary.Nation.Equals(AppData.CurrentLoadedNation))
            {
                ship = AppData.ShipDictionary![summary.Index];
            }
            else
            {
                var shipDict = ReadLocalJsonData<Ship>(summary.Nation, AppData.Settings.SelectedServerType);
                if (shipDict != null)
                {
                    ship = shipDict[summary.Index];
                    if (changeDictionary)
                    {
                        AppData.ShipDictionary = shipDict;
                        AppData.CurrentLoadedNation = summary.Nation;
                    }
                }
            }

            return ship;
        }

        /// <summary>
        /// Save string compressed <see cref="Build"/> to the disk.
        /// </summary>
        public void SaveBuilds()
        {
            var path = fileSystem.Path.Combine(DefaultAppDataDirectory, "builds.json");
            var builds = AppData.Builds.Select(build => build.CreateStringFromBuild()).ToList();
            string buildsString = JsonConvert.SerializeObject(builds);
            fileSystem.File.WriteAllText(path, buildsString);
        }

        public void LoadBuilds()
        {
            string path = fileSystem.Path.Combine(DefaultAppDataDirectory, "builds.json");
            if (fileSystem.File.Exists(path))
            {
                var rawBuildList = JsonConvert.DeserializeObject<List<string>>(fileSystem.File.ReadAllText(path));
                AppData.Builds = rawBuildList?.Select(Build.CreateBuildFromString).ToList() ?? new List<Build>();
            }
        }

        internal T? DeserializeFile<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("The provided file path must not be empty.");
            }

            if (!fileSystem.File.Exists(filePath))
            {
                Logging.Logger.Warn($"Tried to load file {filePath}, but it was not found.");
                return default;
            }

            using Stream fs = fileSystem.File.OpenRead(filePath);
            var streamReader = new StreamReader(fs);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }
    }
}