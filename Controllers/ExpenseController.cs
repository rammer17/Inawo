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
    public class ExpenseController : Controller
    {
        public InawoDBContext _dbContext;
        public IConfiguration Configuration { get; }
        public ExpenseController(InawoDBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            Configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<ExpenseGetAllResponse>> GetAll()
        {
            var requesterId = GetUserIdFromToken(User);

            var response = _dbContext.Expenses
                .Where(x => x.AccountId == requesterId)
                .ToList();

            return Ok(response);
        }

        [HttpGet]
        [Authorize]
        public ActionResult<List<ExpenseGetAllResponse>> GetAllByDate(string request)
        {
            var monthReq = int.Parse(request.Split('/')[0]);
            var yearReq = int.Parse(request.Split('/')[1]);

            var requesterId = GetUserIdFromToken(User);

            var response = _dbContext.Expenses
                .Where(x => x.AccountId == requesterId && x.Date.Month == monthReq && x.Date.Year == yearReq)
                .ToList();

            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddExpense(ExpenseAddRequest request)
        {
            var accountId = GetUserIdFromToken(User);
            var account = _dbContext.Users.Where(x => x.Id == accountId).First();
            if (account == null)
                return BadRequest("The is no such user!");
            account.Balance -= request.Amount;
            account.Transactions++;

            var newExpense = new Expense
            {
                Amount = request.Amount,
                Title = request.Title,
                Date = DateTime.Now,
                AccountId = accountId,
                Account = account
            };

            _dbContext.Users.Update(account);
            _dbContext.Expenses.Add(newExpense);
            _dbContext.SaveChanges();

            return Ok();
        }

        [HttpPut]
        [Authorize]
        public ActionResult UpdateExpense(ExpenseUpdateRequest request)
        {
            var accountId = GetUserIdFromToken(User);
            var account = _dbContext.Users.Where(x => x.Id == accountId).FirstOrDefault();
            if (account == null)
                return BadRequest("The is no such user!");

            var expenseForUpdate = _dbContext.Expenses.Where(x => x.Id == request.Id).First();
            if (expenseForUpdate == null)
                return BadRequest("Incorrect expense ID!");
            expenseForUpdate.Amount = request.Amount;
            account.Balance += expenseForUpdate.Amount;
            expenseForUpdate.Amount = request.Amount;
            account.Balance -= expenseForUpdate.Amount;

            _dbContext.Users.Update(account);
            _dbContext.Expenses.Update(expenseForUpdate);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public ActionResult DeleteExpense(ExpenseDeleteRequest request)
        {
            var requesterId = GetUserIdFromToken(User);
            var expenseForDelete = _dbContext.Expenses.Where(x => x.Id == request.Id).FirstOrDefault();
            if (expenseForDelete is null)
                return BadRequest("Such expense does not exist!");
            if (expenseForDelete.AccountId != requesterId)
                return BadRequest("You don't have permission to delete this");

            var account = _dbContext.Users.Where(x => x.Id == requesterId).FirstOrDefault();
            if (account == null)
                return BadRequest("The is no such user!");
            account.Balance += expenseForDelete.Amount;
            account.Transactions--;

            _dbContext.Users.Update(account);
            _dbContext.Remove(expenseForDelete);
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
