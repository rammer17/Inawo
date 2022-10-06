namespace Inawo.Models.DB
{
    public class User
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string DateCreated { get; set; }
        public double Balance { get; set; }
        public int Transactions { get; set; }
        public ICollection<Income> Incomes { get; set; }
        public ICollection<Expense> Expenses { get; set; }
    }
}
