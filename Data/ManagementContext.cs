using Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Management.Data
{
    public class ManagementContext : DbContext
    {
        public ManagementContext(DbContextOptions<ManagementContext> options) : base(options) { }

        public DbSet<DataTable> Data { get; set; } = null;

    }
}
