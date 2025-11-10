using Microsoft.EntityFrameworkCore;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public ApplicationDbContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
    }

}
