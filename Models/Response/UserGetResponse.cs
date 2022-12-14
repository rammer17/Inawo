using Inawo.Models.DB;

namespace Inawo.Models.Response
{
    public class UserGetResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DateCreated { get; set; }
        public double Balance { get; set; }
        public int Transactions { get; set; }
        public IEnumerable<IncomeGetAllResponse> Incomes { get; set; }
        public IEnumerable<ExpenseGetAllResponse> Expenses { get; set; }
    }
}
