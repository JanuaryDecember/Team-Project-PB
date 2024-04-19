using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BudgetCategory
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; } = "new Budget Category";
    public double AllocatedAmount { get; set; } = 0;
    public double SpentAmount { get; set; } = 0;
    public double RemainingAmount => AllocatedAmount - SpentAmount;
}