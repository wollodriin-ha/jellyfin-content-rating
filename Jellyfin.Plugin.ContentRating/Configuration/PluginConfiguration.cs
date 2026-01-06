using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.ContentRating.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public PluginConfiguration()
        {
            DeleteThreshold = 3;
            AutoDeleteEnabled = true;
            MinimumWatchPercentage = 90;
            NotifyOnDelete = true;
            ExcludedLibraries = new string[0];
        }

        public int DeleteThreshold { get; set; }
        public bool AutoDeleteEnabled { get; set; }
        public int MinimumWatchPercentage { get; set; }
        public bool NotifyOnDelete { get; set; }
        public string[] ExcludedLibraries { get; set; }
    }
}