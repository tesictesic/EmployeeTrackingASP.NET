using System.ComponentModel.DataAnnotations;

namespace EmployeeTrackingC_.Models
{
    public class EmployeeTable
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public double TotalDaysWorked => Math.Ceiling((EndTimeUtc - StarTimeUtc).TotalHours);
    }
    public class EmployeeSearch
    {
        public string? keyword { get; set; }
        public string? orderBy { get; set; }
    }
}
