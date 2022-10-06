namespace Inawo.Models.DB
{
    public class Income
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public int AccountId { get; set; }
        public User Account { get; set; }
    }
}
