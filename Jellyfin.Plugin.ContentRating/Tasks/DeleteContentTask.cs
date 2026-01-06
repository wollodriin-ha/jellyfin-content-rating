using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.ContentRating.Data;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.ContentRating.Tasks
{
    public class DeleteContentTask : IScheduledTask
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<DeleteContentTask> _logger;

        public DeleteContentTask(ILibraryManager libraryManager, ILogger<DeleteContentTask> logger)
        {
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public string Name => "Vérifier le contenu mal noté";
        public string Description => "Liste le contenu avec trop de votes 'à supprimer'";
        public string Category => "Content Rating";
        public string Key => "ContentRatingCheckTask";

        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            if (Plugin.Instance == null)
            {
                _logger.LogWarning("Plugin instance is null");
                return;
            }

            var config = Plugin.Instance.Configuration;

            _logger.LogInformation($"Recherche de contenu avec {config.DeleteThreshold}+ votes 'à supprimer'");

            // Pour l'instant, on log juste. La suppression automatique sera ajoutée plus tard
            _logger.LogInformation("Tâche complétée");
            
            await Task.CompletedTask;
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            // Pas de déclenchement automatique par défaut
            return new TaskTriggerInfo[] { };
        }
    }
}