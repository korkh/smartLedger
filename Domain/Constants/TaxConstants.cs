namespace Domain.Constants
{
    public static class TaxConstants
    {
        // Порог НДС по годам (из Excel)
        public static readonly Dictionary<int, decimal> NdsThresholds = new()
        {
            { 2023, 69000000m },
            { 2024, 73840000m },
            { 2025, 78000000m },
            { 2026, 82000000m },
        };

        // Значение по умолчанию, если год не найден
        public const decimal DefaultNdsThreshold = 73840000m;
    }
}
