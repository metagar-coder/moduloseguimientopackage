using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Utilities
{
    public class Utileria
    {
        public static dynamic GetAppSettingsValue(string key)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").AddEnvironmentVariables();
            var config = builder.Build();
            return config.GetValue<string>(key);
        }

        public static string CleanUser(string userId)
        {
            return userId.ToLower().Replace("@uv.mx", "").Replace("@estudiantes.uv.mx", "");
        }
    }
}
