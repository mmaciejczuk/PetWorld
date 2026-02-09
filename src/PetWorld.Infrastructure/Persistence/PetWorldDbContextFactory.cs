using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PetWorld.Infrastructure.Persistence;

public sealed class PetWorldDbContextFactory : IDesignTimeDbContextFactory<PetWorldDbContext>
{
    public PetWorldDbContext CreateDbContext(string[] args)
    {
        // Design-time default (local). W dockerze i tak override przez ENV w Web.
        var cs = "Server=localhost;Port=3306;Database=petworld;User=petworld;Password=petworld;TreatTinyAsBoolean=true;";

        var options = new DbContextOptionsBuilder<PetWorldDbContext>()
            .UseMySql(cs, ServerVersion.AutoDetect(cs))
            .Options;

        return new PetWorldDbContext(options);
    }
}
