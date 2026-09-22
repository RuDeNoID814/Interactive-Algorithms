using AlgoLab.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoLab.Api.Data;

public class AlgoLabDbContext : DbContext
{
    public AlgoLabDbContext(
        DbContextOptions<AlgoLabDbContext> options)
        : base(options)
    {
    }

    public DbSet<Experiment> Experiments =>
        Set<Experiment>();

    public DbSet<ExperimentPoint> ExperimentPoints =>
        Set<ExperimentPoint>();
}