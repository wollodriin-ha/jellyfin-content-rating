using System;
using System.Net.Mime;
using Jellyfin.Plugin.ContentRating.Data;
using MediaBrowser.Common.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.ContentRating.Api
{
    /// <summary>
    /// Contrôleur API pour gérer les notations
    /// </summary>
    [ApiController]
    [Route("ContentRating")]
    [Produces(MediaTypeNames.Application.Json)]
    public class RatingController : ControllerBase
    {
        private readonly RatingRepository _repository;

        public RatingController(IApplicationPaths appPaths)
        {
            _repository = new RatingRepository(appPaths);
        }

        /// <summary>
        /// Soumettre une notation pour un contenu
        /// </summary>
        /// <param name="itemId">ID du contenu</param>
        /// <param name="rating">Type de notation (0=Adoré, 1=Aimé, 2=Bof, 3=Pas aimé, 4=À supprimer)</param>
        /// <returns>Résultat de l'opération</returns>
        [HttpPost("Rate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult SubmitRating([FromQuery] string itemId, [FromQuery] int rating)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return BadRequest("ItemId est requis");
            }

            if (rating < 0 || rating > 4)
            {
                return BadRequest("Rating doit être entre 0 et 4");
            }

            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId invalide");
            }

            var ratingType = (RatingType)rating;
            _repository.SaveRating(userId, itemId, ratingType);

            return Ok(new { success = true, message = "Notation enregistrée" });
        }

        /// <summary>
        /// Obtenir les statistiques de notation pour un contenu
        /// </summary>
        /// <param name="itemId">ID du contenu</param>
        /// <returns>Statistiques de notation</returns>
        [HttpGet("Stats")]
        [Authorize(Policy = "RequiresElevation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<RatingStats> GetStats([FromQuery] string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return BadRequest("ItemId est requis");
            }

            var stats = _repository.GetRatingStats(itemId);
            return Ok(stats);
        }

        /// <summary>
        /// Vérifier si un utilisateur a déjà noté un contenu
        /// </summary>
        /// <param name="itemId">ID du contenu</param>
        /// <returns>True si l'utilisateur a déjà noté</returns>
        [HttpGet("HasRated")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> HasRated([FromQuery] string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return BadRequest("ItemId est requis");
            }

            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId invalide");
            }

            var hasRated = _repository.HasUserRated(userId, itemId);
            return Ok(new { hasRated });
        }
    }

    /// <summary>
    /// Extensions pour récupérer l'ID utilisateur
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this System.Security.Claims.ClaimsPrincipal user)
        {
            return user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                ?? user.FindFirst("sub")?.Value 
                ?? string.Empty;
        }
    }
}