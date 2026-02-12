using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SistemaProducao3D.Data.Context
{
    public class DatabaseContextFactory
        : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=SistemaProducao3D;Username=postgres;Password=123456"
            );

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
