using Microsoft.AspNetCore.Mvc;

namespace Inawo.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        public InawoDBContext _dbContext;
        public IConfiguration Configuration { get; }
        public UserController(InawoDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            Configuration = configuration;
        }

        [HttpGet]
        public string Test()
        {
            return ("It is working!");
        }


    }
}
