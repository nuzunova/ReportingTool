using Reporting.Data;
using System;

namespace Reporting.Models
{
    public class ArrivalViewModel
    {
        public int EmployeeId { get; set; }
        public DateTime When { get; set; }

        public ArrivalViewModel(Arrival arrival)
        {
            EmployeeId = arrival.EmployeeId;
            When = arrival.When;
        }
    }
}