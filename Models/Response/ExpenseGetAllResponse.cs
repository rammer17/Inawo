﻿namespace Inawo.Models.Response
{
    public class ExpenseGetAllResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public int AccountId { get; set; }

    }
}
