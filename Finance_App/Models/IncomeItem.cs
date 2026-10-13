using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance_App.Models
{
    public class IncomeItem
    {
        public int IncomeId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
    }
}
