using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_App.Models
{
    public class ExpenseItem
    {
        public int ExpenseId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
        public int CategoryId { get; set; }
    }
}
