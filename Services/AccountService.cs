using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using ThesisProject.Areas.Identity.Data;
using ThesisProject.Data;
using ThesisProject.Models;

namespace ThesisProject.Services
{
    public class AccountService : IAccountService 
    { 
        private readonly ThesisProjectContext _context;
        private readonly UserManager<ThesisProjectUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService( ThesisProjectContext context,UserManager<ThesisProjectUser> userManager,IHttpContextAccessor httpContextAccessor)  
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        private string userAccountN = "000000000001";
        private long accountNumberCreation()
        {
            string bankcode = "12";
            string branchcode = "0002";
            long accountN = long.Parse(bankcode + branchcode + userAccountN);
            while (_context.Account.Any(a => a.accountNumber == accountN))
            {
                accountN += 1;
            }
            if (_context.Account.Any(a => a.accountNumber == 12000299999900))
            {
                userAccountN = "0000" + userAccountN;
            }
            return accountN;
        }
        private Account createAcc(Account account)
        {
            account.accountNumber = accountNumberCreation();
            account.amount = 0;
            account.accountLimit = 10000;
            account.accountInterest = 0;
            account.userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
            account.DateOfCreation = DateTime.Now;
            string accountType = account.accountType.ToUpper();
            if (accountType == "SAVING")
            {
                account.accountInterest = 0.03;
            };
            return account;
        }
        public void CreateAccount(string accountName, string currencyType, string accountType)
        {
            var ac = new Account();
            ac.currencyType = currencyType;
            ac.accountType = accountType;
            if(accountName == null)
            {
                accountName = "";
            }
            ac.accountName = accountName;

            var account = createAcc(ac);
            _context.Add(account);
            _context.SaveChanges();
        }
    }
}
