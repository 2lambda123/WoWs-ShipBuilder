using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.Core.Builds;
using WoWsShipBuilder.Core.Utility;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Aircraft;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Projectile;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Upgrade;

namespace WoWsShipBuilder.Core.DataProvider;

/// <summary>
/// The main storage for app data and settings at runtime.
/// </summary>
public static class AppData
{
    /// <summary>
    /// Gets or sets a value indicating whether application settings have been initialized.
    /// </summary>
    public static bool IsInitialized { get; set; }

    /// <summary>
    /// Gets or sets the current data version name.
    /// </summary>
    public static string? DataVersion { get; set; }

    /// <summary>
    /// Gets the ship dictionary for the currently selected nation.
    /// </summary>
    public static Dictionary<string, Ship> ShipDictionary { get; } = new();

    /// <summary>
    /// Gets or sets the list of available consumables.
    /// </summary>
    public static Dictionary<string, Consumable> ConsumableList { get; set; } = new();

    /// <summary>
    /// Gets the projectile cache, mapping a nation to the actual projectile dictionary for that nation.
    /// For easier access to data, have a look at the two linked utility methods.
    /// </summary>
    /// <seealso cref="FindProjectile"/>
    /// <seealso cref="FindProjectile{T}"/>
    public static Dictionary<Nation, Dictionary<string, Projectile>> ProjectileCache { get; } = new();

    /// <summary>
    /// Gets the aircraft cache, mapping a nation to the actual aircraft dictionary for that nation.
    /// </summary>
    /// <seealso cref="FindAircraft"/>
    public static ConcurrentDictionary<Nation, Dictionary<string, Aircraft>> AircraftCache { get; } = new();

    public static ConcurrentDictionary<Nation, Dictionary<string, Exterior>> ExteriorCache { get; } = new();

    public static ConcurrentDictionary<Nation, Dictionary<string, Captain>> CaptainCache { get; } = new();

    public static Dictionary<string, Modernization> ModernizationCache { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of <see cref="ShipSummary">ship summaries</see> that are currently available.
    /// </summary>
    public static List<ShipSummary> ShipSummaryList { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of the last used builds.
    /// </summary>
    public static List<Build> Builds { get; set; } = new();

#if DEBUG
    public static bool IsDebug => true;
#else
    public static bool IsDebug => false;
#endif

    public static void ResetCaches()
    {
        ShipSummaryList.Clear();
        ConsumableList.Clear();
        ModernizationCache.Clear();
        ProjectileCache.Clear();
        AircraftCache.Clear();
        ShipDictionary.Clear();
        DataVersion = null;
        Logging.Logger.LogInformation("Cleared all appdata caches");
    }

    /// <summary>
    /// Helper method to retrieve a <see cref="Projectile"/> object from the <see cref="ProjectileCache"/> dictionary with only its index.
    /// </summary>
    /// <param name="projectileIndex">The WG index of the projectile object.</param>
    /// <returns>The projectile object with the specified index.</returns>
    public static Projectile FindProjectile(string projectileIndex)
    {
        var nation = GameDataHelper.GetNationFromIndex(projectileIndex);
        return ProjectileCache[nation][projectileIndex];
    }

    /// <summary>
    /// Helper method to retrieve a <see cref="Projectile"/> object with its concrete type instead of the projectile base type.
    /// </summary>
    /// <param name="projectileIndex">The WG index of the projectile object.</param>
    /// <typeparam name="T">The specific subtype or Projectile to cast the object to.</typeparam>
    /// <returns>The projectile object with the specified index.</returns>
    /// <exception cref="System.InvalidCastException">Occurs if the specified generic type does not match the type of the retrieved object.</exception>
    public static T FindProjectile<T>(string projectileIndex) where T : Projectile
    {
        return (T)FindProjectile(projectileIndex);
    }

    /// <summary>
    /// Reads aircraft data from the <see cref="AppData.AircraftCache"/> and returns the result.
    /// </summary>
    /// <param name="aircraftName">The name of the aircraft, <b>MUST</b> start with the aircraft's index.</param>
    /// <returns>The requested aircraft.</returns>
    /// <exception cref="KeyNotFoundException">Occurs if the aircraft name does not exist in the aircraft data.</exception>
    public static Aircraft FindAircraft(string aircraftName)
    {
        var nation = GameDataHelper.GetNationFromIndex(aircraftName);
        return AircraftCache[nation][aircraftName];
    }

    public static Ship FindShipFromSummary(ShipSummary summary) => ShipDictionary[summary.Index];
}
