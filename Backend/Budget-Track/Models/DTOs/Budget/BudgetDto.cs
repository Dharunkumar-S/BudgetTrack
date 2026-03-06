#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget_Track.Models.DTOs.Budget
{
    public class BudgetDto
    {
        public int BudgetID { get; set; }
        public required string Title { get; set; }
        public string? Code { get; set; }
        public int DepartmentID { get; set; }
        public required string DepartmentName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountAllocated { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountSpent { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountRemaining { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? Notes { get; set; }
        public int CreatedByUserID { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedByEmployeeID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? DeletedByUserID { get; set; }
        public string? DeletedByName { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsDeleted { get; set; }
    }
}
