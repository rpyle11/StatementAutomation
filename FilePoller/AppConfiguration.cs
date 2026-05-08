namespace FilePoller
{
    public class AppConfiguration
    {
        public AppConfiguration(IHostEnvironment environment)
        {
            var configurationBuilder = new ConfigurationBuilder();

            var settingsPath = environment.EnvironmentName.ToLower() switch
            {
                "development" => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.development.json"),
                "staging" => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.staging.json"),
                _ => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.production.json")
            };

            configurationBuilder.AddJsonFile(settingsPath, false);
            var builder = configurationBuilder.Build();
           
            ConnectionString = builder.GetSection("ConnectionStrings").GetSection("DataConnection").Value;
            
        }
        public string? ConnectionString { get; }
    }
}
