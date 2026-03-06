namespace Domain.Constants
{
    public static class TaxConstants
    {
        // Thresholds from your "Справочник" sheet
        public static readonly Dictionary<int, decimal> NdsThresholds = new()
        {
            { 2023, 69000000m },
            { 2024, 73840000m },
        };

        // Default value if year is not found (20,000 MRP estimate)
        public const decimal DefaultNdsThreshold = 73840000m;
    }
}
