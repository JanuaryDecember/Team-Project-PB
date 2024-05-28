using System.Security.Claims;
using expenses_tracker_pb.Server.Enums;
using expenses_tracker_pb.Server.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace _2023pz_trrepo.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/budget")]
    public class BudgetController : ControllerBase
    {
        private readonly EtDbContext _dbContext;

        public BudgetController(EtDbContext dbContext)
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
            List<Category> budgetCategoriesList =
                await _dbContext.Categories.Where(x => x.Type == CategoryType.Expenditure).ToListAsync();
            if (budgetCategoriesList.IsNullOrEmpty())
                return NotFound("Cant find any BudgetCategories!");

            return Ok(budgetCategoriesList);
        }

        [HttpPost("createBudget")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto budgetDetails)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exists!");
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));

            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id.Equals(budgetDetails.walletId));
            var budgetCategory =
                await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id.Equals(budgetDetails.categoryId));

            if (wallet == null || budgetCategory == null || user == null)
                return BadRequest("Error finding wallet, budget category, or user!");

            var newBudget = new Budget
            {
                Name = budgetDetails.name,
                TotalIncome = budgetDetails.totalIncome,
                TotalExpenditure = budgetDetails.totalExpenditure,
                Wallet = wallet,
                BudgetCategory = budgetCategory,
                Owner = user,
            };
            _dbContext.Budgets.Add(newBudget);
            await _dbContext.SaveChangesAsync();

            return Ok("Successfully created budget!");
        }


        [HttpGet("showBudgets")]
        public async Task<IActionResult> ShowBudgetsForWallet()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exist!");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));

            var budgets = await _dbContext.Budgets
                .Include(b => b.Wallet)
                .Include(b => b.BudgetCategory)
                .Where(x => x.Owner == user)
                .ToListAsync();

            if (budgets == null)
                return NotFound("No budgets for selected wallet.");

            var budgetDtos = budgets.Select(b => new BudgetDto
            {
                Id = b.Id,
                Name = b.Name,
                TotalIncome = b.TotalIncome,
                TotalExpenditure = b.TotalExpenditure,
                RemainingBalance = b.RemainingBalance,
                WalletName = b.Wallet.Name,
                WalletId = b.Wallet.Id,
                BudgetCategoryName = b.BudgetCategory.Name,
                BudtedCategoryId = b.BudgetCategory.Id
            }).ToList();

            return Ok(budgetDtos);
        }

        [HttpDelete("deleteBudget/{budgetId}")]
        public async Task<IActionResult> DeleteBudget(long budgetId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exist!");


            var budget = await _dbContext.Budgets.FirstOrDefaultAsync(x => x.Id == budgetId && x.Owner.Id == userId);

            if (budget == null)
                return NotFound("Budget not found or you do not have permission to delete it.");

            _dbContext.Budgets.Remove(budget);
            await _dbContext.SaveChangesAsync();

            return Ok("Budget successfully deleted!");
        }

        [HttpPut("editBudget/{budgetId}")]
        public async Task<IActionResult> EditBudget(long budgetId, [FromBody] CreateBudgetDto editedBudget)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exist!");

            var budget = await _dbContext.Budgets.FirstOrDefaultAsync(x => x.Id == budgetId && x.Owner.Id == userId);

            if (budget == null)
                return NotFound("Budget not found or you do not have permission to edit it.");

            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(x => x.Id.Equals(editedBudget.walletId));
            var budgetCategory =
                await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id.Equals(editedBudget.categoryId));

            budget.Name = editedBudget.name;
            budget.TotalIncome = editedBudget.totalIncome;
            budget.TotalExpenditure = editedBudget.totalExpenditure;
            budget.Wallet = wallet;
            budget.BudgetCategory = budgetCategory;

            _dbContext.Budgets.Update(budget);
            await _dbContext.SaveChangesAsync();

            return Ok("Budget successfully updated!");
        }

        [HttpGet("checkBudget")]
        public async Task<IActionResult> checkBudgetForWallet()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exist!");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            var budgetList = await _dbContext.Budgets
                .Include(b => b.Wallet)
                .Include(b => b.BudgetCategory)
                .Where(x => x.Owner == user)
                .ToListAsync();

            if (budgetList == null)
                return NotFound("No budgets for selected wallet.");

            var negativeBudgets = budgetList.Where(b => b.RemainingBalance < 0)
                .Select(b => new BudgetDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    TotalIncome = b.TotalIncome,
                    TotalExpenditure = b.TotalExpenditure,
                    RemainingBalance = b.RemainingBalance,
                    WalletName = b.Wallet.Name,
                    BudgetCategoryName = b.BudgetCategory.Name
                })
                .ToList();

            return Ok(negativeBudgets);
        }

        [HttpGet("budgetTransactions/{budgetId}")]
        public async Task<IActionResult> GetBudgetTransactions(long budgetId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return NotFound("User does not exist!");

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id.Equals(userId));
            var budget = await _dbContext.Budgets
                .Include(b => b.Wallet)
                .Include(b => b.BudgetCategory)
                .FirstOrDefaultAsync(x => x.Id == budgetId && x.Owner == user);

            if (budget == null)
                return NotFound("Budget not found or you do not have permission to view it.");

            var transactions = await _dbContext.Transactions
                .Where(t => t.Wallet == budget.Wallet && t.Category == budget.BudgetCategory &&
                            t.Type.Equals(TransactionTypeEnum.Expense))
                .ToListAsync();

            return transactions.Count.Equals(0) ? NotFound("No transactions for selected budget.") : Ok(transactions);
        }

        public class CreateBudgetDto
        {
            public string name { get; set; }
            public double totalIncome { get; set; }
            public double totalExpenditure { get; set; }
            public long walletId { get; set; }
            public long categoryId { get; set; }
        }

        public class BudgetDto
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public double TotalIncome { get; set; }
            public double TotalExpenditure { get; set; }
            public double RemainingBalance { get; set; }
            public string WalletName { get; set; }
            public long WalletId { get; set; }
            public string BudgetCategoryName { get; set; }
            public long BudtedCategoryId { get; set; }
        }
    }
}