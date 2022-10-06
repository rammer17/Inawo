using Inawo.Models.DB;
using Inawo.Models.Request;
using Inawo.Models.Response;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [Authorize]
        public ActionResult<List<IncomeGetAllResponse>> GetAll()
        {
            var requesterId = GetUserIdFromToken(User);

            var response = _dbContext.Incomes
                .Where(x => x.AccountId == requesterId)
                .ToList();

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<IncomeGetAllResponse>> GetAllByDate(string request)
        {
            var monthReq = int.Parse(request.Split('/')[0]);
            var yearReq = int.Parse(request.Split('/')[1]);

            var requesterId = GetUserIdFromToken(User);

            var response = _dbContext.Incomes
                .Where(x => x.AccountId == requesterId && x.Date.Month == monthReq && x.Date.Year == yearReq)
                .ToList();

            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddIncome (IncomeAddRequest request)
        {
            var accountId = GetUserIdFromToken(User);
            var account = _dbContext.Users.Where(x => x.Id == accountId).First();
            if (account == null)
                return BadRequest("The is no such user!");
            account.Balance += request.Amount;
            account.Transactions++;

            var newIncome = new Income
            {
                Amount = request.Amount,
                Date = DateTime.Now,
                AccountId = accountId,
                Account = account,
            };

            _dbContext.Users.Update(account);
            _dbContext.Incomes.Add(newIncome);
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPut]
        [Authorize]
        public ActionResult UpdateIncome(IncomeUpdateRequest request)
        {
            var accountId = GetUserIdFromToken(User);
            var account = _dbContext.Users.Where(x => x.Id == accountId).FirstOrDefault();
            if (account == null)
                return BadRequest("The is no such user!");

            var incomeForUpdate = _dbContext.Incomes.Where(x => x.Id == request.Id).First();
            if (incomeForUpdate == null)
                return BadRequest("Incorrect income ID!");
            account.Balance -= incomeForUpdate.Amount;
            incomeForUpdate.Amount = request.Amount;
            account.Balance += incomeForUpdate.Amount;

            _dbContext.Users.Update(account);
            _dbContext.Incomes.Update(incomeForUpdate);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public ActionResult DeleteIncome (IncomeDeleteRequest request)
        {
            var requesterId = GetUserIdFromToken(User);
            var incomeForDelete = _dbContext.Incomes.Where(x => x.Id == request.Id).FirstOrDefault();
            if (incomeForDelete is null)
                return BadRequest("Such income does not exist!");
            if (incomeForDelete.AccountId != requesterId)
                return BadRequest("You don't have permission to delete this");

            var account = _dbContext.Users.Where(x => x.Id == requesterId).FirstOrDefault();
            if (account == null)
                return BadRequest("The is no such user!");
            account.Balance -= incomeForDelete.Amount;
            account.Transactions--;

            _dbContext.Users.Update(account);
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



