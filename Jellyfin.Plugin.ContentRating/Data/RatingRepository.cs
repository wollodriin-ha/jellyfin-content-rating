using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Plugin.ContentRating.Data
{
    /// <summary>
    /// Gestion de la base de données des notations
    /// </summary>
    public class RatingRepository : IDisposable
    {
        private readonly string _dbPath;
        private SqliteConnection? _connection;

        public RatingRepository(IApplicationPaths appPaths)
        {
            var dataPath = Path.Combine(appPaths.PluginConfigurationsPath, "ContentRating");
            Directory.CreateDirectory(dataPath);
            _dbPath = Path.Combine(dataPath, "ratings.db");
            Initialize();
        }

        private void Initialize()
        {
            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            var createTableCmd = _connection.CreateCommand();
            createTableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS ContentRatings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL,
                    ItemId TEXT NOT NULL,
                    Rating INTEGER NOT NULL,
                    Timestamp TEXT NOT NULL,
                    UNIQUE(UserId, ItemId)
                )";
            createTableCmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Ajouter ou mettre à jour une notation
        /// </summary>
        public void SaveRating(string userId, string itemId, RatingType rating)
        {
            if (_connection == null) return;

            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO ContentRatings (UserId, ItemId, Rating, Timestamp)
                VALUES (@userId, @itemId, @rating, @timestamp)
                ON CONFLICT(UserId, ItemId) 
                DO UPDATE SET Rating = @rating, Timestamp = @timestamp";
            
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@rating", (int)rating);
            cmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow.ToString("o"));
            
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Vérifier si un utilisateur a déjà noté un contenu
        /// </summary>
        public bool HasUserRated(string userId, string itemId)
        {
            if (_connection == null) return false;

            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM ContentRatings WHERE UserId = @userId AND ItemId = @itemId";
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            
            var result = cmd.ExecuteScalar();
            return Convert.ToInt64(result) > 0;
        }

        /// <summary>
        /// Obtenir les statistiques de notation pour un contenu
        /// </summary>
        public RatingStats GetRatingStats(string itemId)
        {
            if (_connection == null) return new RatingStats { ItemId = itemId };

            var stats = new RatingStats { ItemId = itemId };
            
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT Rating, COUNT(*) as Count FROM ContentRatings WHERE ItemId = @itemId GROUP BY Rating";
            cmd.Parameters.AddWithValue("@itemId", itemId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var rating = (RatingType)reader.GetInt32(0);
                var count = reader.GetInt32(1);
                
                switch (rating)
                {
                    case RatingType.Loved:
                        stats.LovedCount = count;
                        break;
                    case RatingType.Liked:
                        stats.LikedCount = count;
                        break;
                    case RatingType.Meh:
                        stats.MehCount = count;
                        break;
                    case RatingType.Disliked:
                        stats.DislikedCount = count;
                        break;
                    case RatingType.ToDelete:
                        stats.ToDeleteCount = count;
                        break;
                }
                
                stats.TotalRatings += count;
            }
            
            return stats;
        }

        /// <summary>
        /// Obtenir tous les contenus avec X votes "à supprimer"
        /// </summary>
        public List<string> GetItemsToDelete(int threshold)
        {
            if (_connection == null) return new List<string>();

            var items = new List<string>();
            
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                SELECT ItemId, COUNT(*) as DeleteCount 
                FROM ContentRatings 
                WHERE Rating = @toDeleteRating 
                GROUP BY ItemId 
                HAVING DeleteCount >= @threshold";
            
            cmd.Parameters.AddWithValue("@toDeleteRating", (int)RatingType.ToDelete);
            cmd.Parameters.AddWithValue("@threshold", threshold);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(reader.GetString(0));
            }
            
            return items;
        }

        /// <summary>
        /// Supprimer toutes les notations d'un contenu
        /// </summary>
        public void DeleteRatingsForItem(string itemId)
        {
            if (_connection == null) return;

            var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM ContentRatings WHERE ItemId = @itemId";
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}