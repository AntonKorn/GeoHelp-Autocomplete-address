namespace DataSeeder.Configuration
{
    public class SeederConfiguration
    {
        public static string Section = "SeederConfiguration";

        public bool SeedCititesAndCountries { get; set; }

        public string[]? CountryCodesSeedAddresses { get; set; }
    }
}
