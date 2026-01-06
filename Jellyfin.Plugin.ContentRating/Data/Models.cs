using System;

namespace Jellyfin.Plugin.ContentRating.Data
{
    /// <summary>
    /// Type de notation
    /// </summary>
    public enum RatingType
    {
        Loved,      // Adoré
        Liked,      // Aimé
        Meh,        // Bof
        Disliked,   // Pas aimé
        ToDelete    // À supprimer
    }

    /// <summary>
    /// Représente une notation d'un utilisateur sur un contenu
    /// </summary>
    public class ContentRating
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
        public RatingType Rating { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Statistiques de notation pour un contenu
    /// </summary>
    public class RatingStats
    {
        public string ItemId { get; set; } = string.Empty;
        public int LovedCount { get; set; }
        public int LikedCount { get; set; }
        public int MehCount { get; set; }
        public int DislikedCount { get; set; }
        public int ToDeleteCount { get; set; }
        public int TotalRatings { get; set; }
    }
}