using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;


namespace _2023pz_trrepo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/budget")]
    public class BudgetController : ControllerBase
    {
        private readonly ETDbContext _dbContext;

        public BudgetController(ETDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("userWallets")]
        public async Task<IActionResult> GetAllUserWallets()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exists!");

            List<Wallet> wallets = await _dbContext.Wallets.Where(x => x.UserId == userId).ToListAsync();

            if (wallets == null || wallets.Count == 0)
                return NotFound("User has no wallets.");
                
            return Ok(wallets);
        }

        [HttpGet("budgetCategories")]
        public async Task<IActionResult> BudgetCategories()
        {
            List<BudgetCategory> budgetCategoriesList = await _dbContext.BudgetCategories.ToListAsync();
            if (budgetCategoriesList.IsNullOrEmpty())
                return NotFound("Cant find any BudgetCategories!");

            return Ok(budgetCategoriesList);
        }

        private double CalculateTotalIncome(Wallet wallet)
        {

            if (wallet == null || wallet.Incomes == null)
                return 0;

            double totalIncome = wallet.Incomes.Sum(i => i.Amount);

            return totalIncome;
        }

        private double CalculateTotalExpenditure(Wallet wallet)
        {

            if (wallet == null || wallet.Expenditures == null)
                return 0;

            double totalExpenditure = wallet.Expenditures.Sum(i => i.Amount);

            return totalExpenditure;
        }

        [HttpPost("createBudget")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto budgetDetails)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id.Equals(budgetDetails.walletId));
            var budgetCategory = await _dbContext.BudgetCategories.FirstOrDefaultAsync(x => x.Id.Equals(budgetDetails.budgetCategoryId));

            if (wallet == null || budgetCategory == null)
                return BadRequest("Error finding wallet or budget category!");

            var newBudget = new Budget
            {
                Name = budgetDetails.name,
                TotalIncome = budgetDetails.totalIncome,
                TotalExpenditure = budgetDetails.totalExpenditure,
                Wallets = new List<Wallet> { wallet },
                BudgetCategories = new List<BudgetCategory> { budgetCategory }
            };

            _dbContext.Budgets.Add(newBudget);
            await _dbContext.SaveChangesAsync();

            return Ok("Successfully created budget!");
        }


        [HttpGet("showBudgets")]
        public async Task<IActionResult> ShowBudgetsForWallet()
        {
            var budgets = await _dbContext.Budgets.ToListAsync();

            if (budgets == null)
                return NotFound("No budgets for selected wallet.");

            return Ok(budgets);
        }

    }

    public class CreateBudgetDto
    {
        public string name { get; set; }
        public double totalIncome { get; set; }
        public double totalExpenditure { get; set; }
        public long walletId { get; set; }
        public long budgetCategoryId { get; set; }
    }
}