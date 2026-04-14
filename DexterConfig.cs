using System.Collections.Generic;

namespace Dexter
{
    public class DexterConfig
    {
        public List<AppConfig> Apps { get; set; } = new();
    }

    public class AppConfig
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public string LaunchTarget { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public List<string> ProcessNames { get; set; } = new();
    }
}