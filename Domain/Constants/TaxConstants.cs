namespace Domain.Constants
{
    public static class TaxConstants
    {
        // Thresholds from your "Справочник" sheet
        public static readonly Dictionary<int, decimal> NdsThresholds = new()
        {
            { 2023, 69000000m },
            { 2024, 73840000m },
            { 2025, 78000000m },
            { 2026, 82000000m },
        };

        // Default value if year is not found (20,000 MRP estimate)
        public const decimal DefaultNdsThreshold = 73840000m;

        public static readonly ServiceType[] NdsAffectingServices =
        [
            ServiceType.BankStatement,
            ServiceType.CargoCustoms, // ЭАВР / СНТ
            ServiceType.TaxCalculation,
            ServiceType.InventoryWriteOff,
        ];
    }
}
