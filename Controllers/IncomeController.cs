using Inawo.Models.DB;
using Inawo.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inawo.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class IncomeController : Controller
    {
        public InawoDBContext _dbContext;
        public IConfiguration Configuration { get; }
        public IncomeController(InawoDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            Configuration = configuration;
        }

        [HttpPost]
        public ActionResult AddIncome (IncomeAddRequest request)
        {
            var accountId = GetUserIdFromToken(User);

            var newIncome = new Income
            {
                Amount = request.Amount,
                Date = DateTime.Now,
                AccountId = accountId
            };

            _dbContext.Incomes.Add(newIncome);
            _dbContext.SaveChanges();

            return Ok(newIncome);
        }

        [HttpDelete]
        public ActionResult DeleteIncome (IncomeDeleteRequest request)
        {
            var requesterId = GetUserIdFromToken(User);
            var incomeForDelete = _dbContext.Incomes.Where(x => x.Id == request.Id).FirstOrDefault();
            if (incomeForDelete is null)
                return BadRequest("Such income does not exist!");
            if (incomeForDelete.AccountId != requesterId)
                return BadRequest("You don't have permission to delete this");
            _dbContext.Remove(incomeForDelete);
            _dbContext.SaveChanges();
            return Ok();
        }


        private int GetUserIdFromToken(ClaimsPrincipal user)
        {
            string? userIdRawValue = user.FindFirst(ClaimTypes.Name)?.Value;
            int userId;
            if (userIdRawValue != null)
            {
                if (int.TryParse(userIdRawValue, out userId))
                {
                    return userId;
                }
                else
                {
                    throw new Exception("Could not parse id value from JWT claim to an integer");
                }
            }
            else
            {
                throw new Exception("Id value is null");
            }
        }
    }
}



