using AlgoLab.Api.Data;
using AlgoLab.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoLab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExperimentsController : ControllerBase
{
    private readonly AlgoLabDbContext _db;

    public ExperimentsController(
        AlgoLabDbContext db)
    {
        _db = db;
    }


    [HttpGet]
    public async Task<ActionResult<List<Experiment>>>
        GetAll()
    {
        var experiments =
            await _db.Experiments
                .OrderByDescending(
                    e => e.CreatedAt)
                .ToListAsync();

        return Ok(experiments);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<Experiment>>
        GetById(int id)
    {
        var experiment =
            await _db.Experiments
                .Include(e => e.Points)
                .FirstOrDefaultAsync(
                    e => e.Id == id);

        if (experiment is null)
            return NotFound();

        return Ok(experiment);
    }


    [HttpPost]
    public async Task<ActionResult<Experiment>>
        Create(Experiment experiment)
    {
        experiment.Id = 0;

        experiment.CreatedAt =
            DateTime.UtcNow;

        foreach (var point in experiment.Points)
        {
            point.Id = 0;
        }

        _db.Experiments.Add(
            experiment);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = experiment.Id
            },
            experiment);
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult>
        Delete(int id)
    {
        var experiment =
            await _db.Experiments
                .FirstOrDefaultAsync(
                    e => e.Id == id);

        if (experiment is null)
            return NotFound();

        _db.Experiments.Remove(
            experiment);

        await _db.SaveChangesAsync();

        return NoContent();
    }
}