﻿namespace DataLoader.Configuration
{
    internal class DataLoadConfiguration
    {
        public static string SectionName => "DataLoadConfiguration";

        public bool ReloadAllCitiesAndCountries { get; init; }

        public string[]? CountryCodesReloadAddresses { get; init; }
    }
}
