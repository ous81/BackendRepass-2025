using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendRetake_2025.Models;
using AutoMapper;
using System.Security.Claims;
using BackendRetake_2025.Data;
using BackendRetake_2025.DTOs;

namespace BackendRetake_2025.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly IMapper _mapper;

    public ReviewsController(CinemaDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet("movies/{movieId}")]
    public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetMovieReviews(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ReviewDTO>>(reviews));
    }

    // ---------------------- SERIES NOT IMPLEMENTED YET ----------------------
    /*
    [HttpGet("series/{seriesId}")]
    public async Task<ActionResult<IEnumerable<ReviewDTO>>> GetSeriesReviews(int seriesId)
    {
        var series = await _context.Series.FindAsync(seriesId);
        if (series == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.SeriesId == seriesId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<ReviewDTO>>(reviews));
    }
    */
    // -----------------------------------------------------------------------

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDTO>> CreateReview(ReviewCreate request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        // Only MovieId allowed for now — Series not implemented
        if (!request.MovieId.HasValue)
        {
            return BadRequest("MovieId must be provided. Series reviews are disabled until Series module is implemented.");
        }

        var movie = await _context.Movies.FindAsync(request.MovieId.Value);
        if (movie == null)
        {
            return NotFound("Movie not found");
        }

        var existingReview = await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId.Value &&
                                      r.MovieId == request.MovieId);

        if (existingReview != null)
        {
            return BadRequest("You have already reviewed this movie");
        }

        var review = _mapper.Map<Review>(request);
        review.UserId = userId.Value;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var createdReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == review.Id);

        var dto = _mapper.Map<ReviewDTO>(createdReview);

        return CreatedAtAction(nameof(GetMovieReviews), new { movieId = request.MovieId }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ReviewDTO>> UpdateReview(int id, ReviewUpdate request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != userId)
        {
            return Forbid();
        }

        review.Text = request.Text;
        review.Rating = request.Rating;
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updatedReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        return Ok(_mapper.Map<ReviewDTO>(updatedReview));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        var userRole = GetCurrentUserRole();

        if (review.UserId != userId && userRole != "Admin")
        {
            return Forbid();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }

    private string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }
}
