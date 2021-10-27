using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using WoWsShipBuilder.Core.Extensions;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataUI
{
    public record TorpedoArmamentUI : IDataUi
    {
        [JsonIgnore]
        public string Name { get; set; } = default!;

        public decimal TurnTime { get; set; }

        public decimal TraverseSpeed { get; set; }

        public decimal Reload { get; set; }

        public string TorpedoArea { get; set; } = default!;

        [JsonIgnore]
        public List<TorpedoUI> Torpedoes { get; set; } = new();

        [JsonIgnore]
        public List<KeyValuePair<string, string>> TorpedoArmamentData { get; set; } = default!;

        public static TorpedoArmamentUI? FromShip(Ship ship, List<ShipUpgrade> shipConfiguration, List<(string name, float value)> modifiers)
        {
            ShipUpgrade? torpConfiguration = shipConfiguration.FirstOrDefault(c => c.UcType == ComponentType.Torpedoes);
            if (torpConfiguration == null)
            {
                return null;
            }

            var torpedoModule = ship.TorpedoModules[torpConfiguration.Components[ComponentType.Torpedoes].First()];
            var launcher = torpedoModule.TorpedoLaunchers.First();

            var turnSpeedModifiers = modifiers.FindModifiers("GTRotationSpeed");
            decimal traverseSpeed = Math.Round(turnSpeedModifiers.Aggregate(launcher.HorizontalRotationSpeed, (current, modifier) => current * (decimal)modifier), 2);

            var reloadSpeedModifiers = modifiers.FindModifiers("GTShotDelay");
            decimal reloadSpeed = Math.Round(reloadSpeedModifiers.Aggregate(launcher.Reload, (current, modifier) => current * (decimal)modifier), 2);

            var torpedoArea = $"{launcher.TorpedoAngles[0]}° - {launcher.TorpedoAngles[1]}°";

            var torpedoes = TorpedoUI.FromTorpedoName(launcher.AmmoList, ship.ShipNation, modifiers);

            var torpedoUi = new TorpedoArmamentUI
            {
                Name = launcher.Name,
                TurnTime = Math.Round(180 / traverseSpeed, 1),
                TraverseSpeed = traverseSpeed,
                Reload = reloadSpeed,
                TorpedoArea = torpedoArea,
                Torpedoes = torpedoes,
            };

            torpedoUi.TorpedoArmamentData = torpedoUi.ToPropertyMapping();
            return torpedoUi;
        }
    }
}