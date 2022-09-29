using Microsoft.EntityFrameworkCore;

namespace Inawo
{
    public class InawoDBContext: DbContext
    {
        public InawoDBContext(DbContextOptions<InawoDBContext> options) : base(options) { }

    }
}
