namespace WoWsShipBuilder.Core.DataUI
{
    public record SecondaryBatteryUI
    {
        public string SecondaryName { get; set; } = default!;

        public decimal SecondaryRange { get; set; }

        public decimal SecondaryDamage { get; set; }

        public decimal SecondaryPenetration { get; set; }

        public decimal? SecondaryFireChance { get; set; }

        public decimal SecondaryReload { get; set; }

        public decimal SecondaryRoF { get; set; }

        public decimal SecondarySigma { get; set; }

        public decimal SecondaryHorizontalDisp { get; set; }

        public decimal SecondaryVerticalDisp { get; set; }

        public decimal SecondaryShellVelocity { get; set; }
    }
}