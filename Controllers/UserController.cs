using Inawo.Models.DB;
using Inawo.Models.Request;
using Inawo.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
        //[Authorize]
        public ActionResult<List<UserGetResponse>> GetAll()
        {
            SendEmail();

            var users = _dbContext.Users
                .Include(x => x.Incomes)
                .Include(x => x.Expenses)
                .Select(x => new UserGetResponse
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    Password = x.Password,
                    DateCreated = x.DateCreated,
                    Balance = x.Balance,
                    Transactions = x.Transactions
                }).ToList();

            foreach(var user in users)
            {
                user.Incomes = _dbContext.Incomes.Where(x => x.AccountId == user.Id).Select(x => new IncomeGetAllResponse()
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    Amount = x.Amount,
                    Date = x.Date
                });
                user.Expenses = _dbContext.Expenses.Where(x => x.AccountId == user.Id).Select(x => new ExpenseGetAllResponse()
                {
                    Id = x.Id,
                    AccountId = x.AccountId,
                    Amount = x.Amount,
                    Date = x.Date,
                    Title = x.Title
                });
            }

            return Ok(users);
        }
        [HttpGet]
        [Authorize]
        public ActionResult<UserGetResponse> GetInfo(int? id = null)
        {
            if (id == null)
            {
                id = GetUserIdFromToken(User);
            }
            var user = _dbContext.Users
                .Include(x => x.Incomes)
                .Include(x => x.Expenses)
                .Where(x => x.Id == id).First();

            if (user == null)
            {
                return BadRequest("Invalid ID!");
            }

            var response =  new UserGetResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Password = user.Password,
                DateCreated = user.DateCreated,
                Balance = user.Balance,
                Transactions = user.Transactions
            };

            response.Incomes = _dbContext.Incomes.Where(x => x.AccountId == id).Select(x => new IncomeGetAllResponse()
            {
                Id = x.Id,
                AccountId = x.AccountId,
                Amount = x.Amount,
                Date = x.Date
            });
            response.Expenses = _dbContext.Expenses.Where(x => x.AccountId == user.Id).Select(x => new ExpenseGetAllResponse()
            {
                Id = x.Id,
                AccountId = x.AccountId,
                Amount = x.Amount,
                Date = x.Date,
                Title = x.Title
            });


            return Ok(response);
        }

        [HttpPost]
        public ActionResult Register(UserRegisterRequest request)
        {

            if (request.FirstName.Length > 40 || request.FirstName.Length < 3)
                return BadRequest("First name should be longer than 3 and shorter than 40");
            if (request.LastName.Length > 40 || request.LastName.Length < 3)
                return BadRequest("Last name should be longer than 3 and shorter than 40");
            if (request.Password.Length < 8)
                return BadRequest("Password must be longer than 8 symbols.");
            if (_dbContext.Users.Any(x => x.Email == request.Email))
                return BadRequest("There is already an account registered with this email address.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = ComputeSha256Hash(request.Password),
                Transactions = 0,
                Balance = 0,
                DateCreated = DateTime.Now.ToString("D")
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public ActionResult Login(UserLoginRequest request)
        {
            var user = _dbContext.Users
                .Where(x => EF.Functions.Collate(x.Email, "SQL_Latin1_General_CP1_CS_AS") == request.Email && x.Password == ComputeSha256Hash(request.Password))
                .FirstOrDefault();

            if (user == null)
            {
                return BadRequest("Incorrect credentials");
            }

            var token = CreateJwtToken(user);

            var response = new UserLoginResponse
            {
                Token = token
            };
            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        public ActionResult UpdateAccount(UserUpdateRequest request)
        {
            var requesterId = GetUserIdFromToken(User);
            var userForUpdate = _dbContext.Users.Where(x => x.Id == requesterId).FirstOrDefault();

            if (userForUpdate == null)
                return BadRequest("No such account exists!");

            if (userForUpdate.Email != request.Email)
                userForUpdate.Email = request.Email;
            if(userForUpdate.Password != ComputeSha256Hash(request.Password))
                userForUpdate.Password = ComputeSha256Hash(request.Password);

            _dbContext.Update(userForUpdate);
            _dbContext.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Authorize]
        public ActionResult DeleteAccount(UserDeleteRequest request)
        {
            var userForDelete = _dbContext.Users.Where(x => x.Id == request.Id).FirstOrDefault();
            if(userForDelete == null)
                return NotFound();
            _dbContext.Remove(userForDelete);
            _dbContext.SaveChanges();
            return Ok();
        }

        private string CreateJwtToken(User user)
        {
            List<System.Security.Claims.Claim> identityClaims = new List<System.Security.Claims.Claim>();
            identityClaims.Add(new System.Security.Claims.Claim(ClaimTypes.Name, user.Id.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: identityClaims,
                issuer: Configuration["Jwt:Issuer"],
                audience: Configuration["Jwt:Audience"],
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        private string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
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

        private void SendEmail()
        {
            string fromMail = "georgi.rosenov17@gmail.com";
            string fromPassword = "hktfoylqqmsjrebe";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = "Test Subject";
            message.To.Add(new MailAddress("ninebet186@lutota.com"));
            message.Body = "<html><head><style>#customers {font-family: Arial, Helvetica, sans-serif;border-collapse: collapse;width: 100%;}#customers td, #customers th {border: 1px solid #ddd;padding: 8px;}#customers tr:nth-child(even){background-color: #f2f2f2;}#customers tr:hover {background-color: #ddd;}#customers th {padding-top: 12px;padding-bottom: 12px;text-align: left;background-color: #04AA6D;color: white;}</style></head><body><h1>A Fancy Table</h1><table id=\"customers\"><tr><th>Company</th><th>Contact</th><th>Country</th></tr><tr><td>Alfreds Futterkiste</td><td>Maria Anders</td><td>Germany</td></tr><tr><td>Berglunds snabbköp</td><td>Christina Berglund</td><td>Sweden</td></tr><tr><td>Centro comercial Moctezuma</td><td>Francisco Chang</td><td>Mexico</td></tr><tr><td>Ernst Handel</td><td>Roland Mendel</td><td>Austria</td></tr><tr><td>Island Trading</td><td>Helen Bennett</td><td>UK</td></tr><tr><td>Königlich Essen</td><td>Philip Cramer</td><td>Germany</td></tr><tr><td>Laughing Bacchus Winecellars</td><td>Yoshi Tannamuri</td><td>Canada</td></tr><tr><td>Magazzini Alimentari Riuniti</td><td>Giovanni Rovelli</td><td>Italy</td></tr><tr><td>North/South</td><td>Simon Crowther</td><td>UK</td></tr><tr><td>Paris spécialités</td><td>Marie Bertrand</td><td>France</td></tr></table></body></html>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }

    }
}
