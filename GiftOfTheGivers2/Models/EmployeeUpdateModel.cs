using System;

namespace giftofthegivers2.function.Models
{
    public class EmployeeUpdateModel
    {
       
        /// Unique identifier for the employee.
       
        public string EmployeeId { get; set; }

       
        /// Full name of the employee posting the update.
        
        public string EmployeeName { get; set; }

        
        /// Department or division the employee belongs to (e.g., Logistics, Operations).
        
        public string Department { get; set; }

        
        /// The text content of the update or field report.
        
        public string UpdateText { get; set; }

        
        /// Timestamp of when the update was recorded. Defaults to UTC current time.
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}