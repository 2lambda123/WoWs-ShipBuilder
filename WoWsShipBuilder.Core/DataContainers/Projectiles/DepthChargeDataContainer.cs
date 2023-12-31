using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilder.DataElements.DataElementAttributes;
using WoWsShipBuilder.DataStructures.Projectile;

namespace WoWsShipBuilder.Core.DataContainers;

public partial record DepthChargeDataContainer : ProjectileDataContainer
{
    [DataElementType(DataElementTypes.KeyValue)]
    public int Damage { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "MPS")]
    public string SinkSpeed { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "S")]
    public string DetonationTimer { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public string DetonationDepth { get; set; } = default!;

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "M")]
    public decimal DcSplashRadius { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FireChance { get; set; }

    [DataElementType(DataElementTypes.KeyValueUnit, UnitKey = "PerCent")]
    public decimal FloodingChance { get; set; }

    public Dictionary<float, List<float>> PointsOfDmg { get; set; } = default!;

    public static DepthChargeDataContainer FromChargesName(string name, IEnumerable<(string name, float value)> modifiers)
    {
        var depthCharge = AppData.FindProjectile<DepthCharge>(name);
        float damage = modifiers.FindModifiers("dcAlphaDamageMultiplier").Aggregate(depthCharge.Damage, (current, modifier) => current * modifier);
        decimal minSpeed = (decimal)(depthCharge.SinkingSpeed * (1 - depthCharge.SinkingSpeedRng)) * Constants.KnotsToMps;
        decimal maxSpeed = (decimal)(depthCharge.SinkingSpeed * (1 + depthCharge.SinkingSpeedRng)) * Constants.KnotsToMps;
        decimal minTimer = (decimal)(depthCharge.DetonationTimer - depthCharge.DetonationTimerRng);
        decimal maxTimer = (decimal)(depthCharge.DetonationTimer + depthCharge.DetonationTimerRng);
        decimal minDetDepth = minSpeed * minTimer;
        decimal maxDetDepth = maxSpeed * maxTimer;

        var depthChargeDataContainer = new DepthChargeDataContainer
        {
            Damage = (int)Math.Round(damage, 0),
            FireChance = Math.Round((decimal)depthCharge.FireChance * 100, 2),
            FloodingChance = Math.Round((decimal)depthCharge.FloodChance * 100, 2),
            DcSplashRadius = Math.Round((decimal)depthCharge.ExplosionRadius, 2),
            SinkSpeed = $"{Math.Round(minSpeed, 1)} ~ {Math.Round(maxSpeed, 1)}",
            DetonationTimer = $"{Math.Round(minTimer, 1)} ~ {Math.Round(maxTimer, 1)}",
            DetonationDepth = $"{Math.Round(minDetDepth)} ~ {Math.Round(maxDetDepth)}",
            PointsOfDmg = depthCharge.PointsOfDamage,
        };

        depthChargeDataContainer.UpdateDataElements();

        return depthChargeDataContainer;
    }
}
