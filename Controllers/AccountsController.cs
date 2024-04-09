using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ThesisProject.Areas.Identity.Data;
using ThesisProject.Data;
using ThesisProject.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ThesisProject.Services;
using Microsoft.AspNetCore.Localization;

namespace ThesisProject.Controllers
{
	[Authorize]
	public class AccountsController : Controller
	{
		private readonly ThesisProjectContext _context;
		private readonly ILogger<AccountsController> _logger;
		private readonly UserManager<ThesisProjectUser> _userManager;
		private readonly IHttpContextAccessor _httpcontextAccessor;
		private readonly IAccountService _accountService;
		public AccountsController(ThesisProjectContext context, ILogger<AccountsController> logger, UserManager<ThesisProjectUser> userManager, IHttpContextAccessor httpcontextAccessor, IAccountService accountService)
		{
			_context = context;
			_logger = logger;
			_userManager = userManager;
			_httpcontextAccessor = httpcontextAccessor;
			_accountService = accountService;
		}
		//variables used in money management
		private double? balance = 0;
		private double? limit = 10000;
		private long cardN = 4265000043120001;
		// total balance in the bank
		public class TotalBalanceMiddleware
		{
			private readonly RequestDelegate _next;

			public TotalBalanceMiddleware(RequestDelegate next)
			{
				_next = next;
			}
			public async Task Invoke(HttpContext context, UserManager<ThesisProjectUser> userManager, ThesisProjectContext dbContext)
			{
				if (userManager?.GetUserId(context.User) != null)
				{
					double? Meur = 0, Mdollar = 0, Mzloty = 0;
					var accounts = dbContext.Account
							.Where(a => a.userId == userManager.GetUserId(context.User) && a.accountType != "Saving")
							.ToList();

					foreach (var account in accounts)
					{
						switch (account.currencyType)
						{
							case "Eur":
								Meur = account.amount * 4.2;
								break;
							case "Dollar":
								Mdollar = account.amount * 4;
								break;
							default:
								Mzloty = account.amount;
								break;
						}
						context.Items["total"] = Meur + Mdollar + Mzloty;
					}
				}

				await _next(context);
			}
		}
		//depositing money
		private async Task<IActionResult> deposit(double? money, long? accountNumber, string name)
		{
			var account = _context.Account.FirstOrDefault(a => a.accountNumber == accountNumber);
			if (_context.Account.Any(a => a.accountNumber == accountNumber))
			{
				var message = new Transaction();
				account.amount += money;
				message.Name = name;
				message.Status = "Successful";
				message.amount = money ?? 0;
				message.currency = account.currencyType;
				message.Description = "Your account have successfully added money to you account";
				message.accountNumber = accountNumber ?? 0;
				message.Tway = "account";
				message.userId = account.userId;
				message.dateOfTransaction = DateTime.Now;
				_context.Update(account);
				_context.Update(message);
				await _context.SaveChangesAsync();
				return RedirectToAction("Details", new { id = account.accountId });
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Please double check the Account/s and try again");
				return View(account);
			}
		}
		//withdrawing money
		private async Task<bool> VerifyPassword(string password)
		{
			var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

			if (user != null)
			{
				return await _userManager.CheckPasswordAsync(user, password);
			}

			return false;
		}
		private async Task<IActionResult> withdraw(double money, long accountNumber, string password, string name, bool? pass)
		{
			var account = _context.Account.FirstOrDefault(a => a.accountNumber == accountNumber);
			if (_context.Account.Any(a => a.accountNumber == accountNumber))
			{
				var message = new Transaction();
				if(pass == true)
				{
                        if (money > account.amount)
                        {
                            ModelState.AddModelError(string.Empty, "Please add more money to your account before making a withdraw");

                            message.Name = name;
                            message.Status = "Unsuccessful";
                            message.amount = money;
                            message.currency = account.currencyType;
                            message.Description = "Please add more money to your account before making a withdraw";
                            message.accountNumber = accountNumber;
                            message.Tway = "account";
                            message.userId = account.userId;
                            message.dateOfTransaction = DateTime.Now;
                            _context.Add(message);
                            await _context.SaveChangesAsync();
                            return RedirectToAction("Details", new { id = account.accountId });
                        }

                        account.amount -= money;
                        message.Name = name;
                        message.Status = "Successful";
                        message.amount = money;
                        message.currency = account.currencyType;
                        message.Description = "Your have successfully removed money from your account";
                        message.accountNumber = accountNumber;
                        message.Tway = "account";
                        message.userId = account.userId;
                        message.dateOfTransaction = DateTime.Now;
                        _context.Add(message);
                        _context.Update(account);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Details", new { id = account.accountId });
                }
				else if (await VerifyPassword(password))
				{
					if (limit < money)
					{
						ModelState.AddModelError(string.Empty, "Please change the limit before making the transaction because the amount exceeds your limit");
						return View(account);
					}
					else
					{
						if (money > account.amount)
						{
							ModelState.AddModelError(string.Empty, "Please add more money to your account before making a withdraw or a transfer");

							message.Name = name;
							message.Status = "Unsuccessful";
							message.amount = money;
							message.currency = account.currencyType;
							message.Description = "Please add more money to your account before making a withdraw or a transfer";
							message.accountNumber = accountNumber;
							message.Tway = "account";
							message.userId = account.userId;
							message.dateOfTransaction = DateTime.Now;
							_context.Update(message);
							await _context.SaveChangesAsync();
							return View(account);
						}

						account.amount -= money;
						message.Name = name;
						message.Status = "Successful";
						message.amount = money;
						message.currency = account.currencyType;
						message.Description = "Your have successfully removed money from your account";
						message.accountNumber = accountNumber;
						message.Tway = "account";
						message.userId = account.userId;
						message.dateOfTransaction = DateTime.Now;
						_context.Update(message);
						_context.Update(account);
						await _context.SaveChangesAsync();
						return RedirectToAction("Details", new { id = account.accountId });
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Incorrect password");
					message.Name = name;
					message.Status = "Unsuccessful";
					message.amount = money;
					message.currency = account.currencyType;
					message.Description = "Attempt to remove money on your account please if you are not the one who did it change your password and report this issue.";
					message.accountNumber = accountNumber;
					message.Tway = "account";
					message.userId = account.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					await _context.SaveChangesAsync();
					return View(account);
				}
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Please double-check the Account/s and try again");
				return View(account);
			}
		}
		// setting a limit
		private Account setL(double? limit, List<Account> accountd)
		{
			foreach (var account in accountd)
			{
				if (_context.Account.Any(a => a.accountNumber == account.accountNumber))
				{
					this.limit = limit;
					var message = new Transaction();
					message.Name = "Limit Change";
					message.Status = "Unsuccessful";
					message.amount = limit ?? 0;
					message.currency = account.currencyType;
					message.Description = "You Limit have been changed from " + account.accountLimit + " to " + limit;
					message.accountNumber = account.accountNumber ?? 0;
					message.Tway = "account";
					message.userId = account.userId;
					message.dateOfTransaction = DateTime.Now;
					account.accountLimit = this.limit;
					_context.Update(message);
					_context.SaveChanges();
					return account;
				}
				else
				{
					_logger.LogError("Please double check the Account/s and try again");
					throw new Exception("Invalid Action");
				}
			}
			_logger.LogError("Please double check the Account/s and try again");
			throw new Exception("Invalid Action");
		}
		// calculating the interest on the account
		private void interest(double interestRate, long accountNumber)
		{
			if (_context.Account.Any(a => a.accountNumber == accountNumber))
			{
				double? money = balance;
				double? interest = money * interestRate;
				balance += interest;
			}
			else
			{
				_logger.LogError("Please double check the Account/s and try again");
			}
		}
		// ****************************************************making a transfer

		private async Task<IActionResult> transfer(long? accountNumberWithdraw, long? accountNumberDeposit, double money, string name, string description, string password)
		{
			var receivingAccount = _context.Account.FirstOrDefault(a => a.accountNumber == accountNumberDeposit);
			var transferingAccount = _context.Account.FirstOrDefault(a => a.accountNumber == accountNumberWithdraw);
			var tranB = transferingAccount.amount;
			var recB = receivingAccount.amount;
			var message = new Transaction();
			var converter = receivingAccount.currencyType + transferingAccount.currencyType;
			double rate = 1;
			if (_context.Account.Any(a => a.accountNumber == accountNumberWithdraw) && _context.Account.Any(a => a.accountNumber == accountNumberDeposit))
			{
				if (receivingAccount.currencyType != transferingAccount.currencyType)
				{
					switch (converter)
					{
						case "EurZloty":
							rate = 0.23809524;
							break;
						case "ZlotyEur":
							rate = 4.2;
							break;
						case "DollarZloty":
							rate = 0.25;
							break;
						case "ZlotyDollar":
							rate = 4;
							break;
						case "DollarEur":
							rate = 1.05;
							break;
						case "EurDollar":
							rate = 1 / 1.05;
							break;
					}
				}
				if (await VerifyPassword(password))
				{
					transferingAccount.amount -= money;
					message.Name = "Transfer";
					message.Status = "Successful";
					message.amount = money;
					message.currency = transferingAccount.currencyType;
					message.Description = "You have successfully transfered " + money + transferingAccount.currencyType + " to " + name + " with the description '" + description;
					message.accountNumber = transferingAccount.accountNumber ?? 0;
					message.Tway = "account";
					message.userId = transferingAccount.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Database.ExecuteSqlInterpolated($"SET IDENTITY_INSERT [dbo].[Transaction] ON");
					_context.Add(message);
					_context.Update(transferingAccount);
					_context.SaveChanges();
					if (transferingAccount.amount + money == tranB)
					{
						var message1 = new Transaction();
						money *= rate;
						receivingAccount.amount += money;
						message1.Name = "Transfer";
						message1.Status = "Successful";
						message1.amount = money;
						message1.currency = receivingAccount.currencyType;
						message1.Description = "You have successfully Recieved " + money + receivingAccount.currencyType + " from " + transferingAccount.ThesisProjectUser.name + " with the description '" + description + "'";
						message1.accountNumber = receivingAccount.accountNumber ?? 0;
						message1.Tway = "account";
						message1.userId = receivingAccount.userId;
						message1.dateOfTransaction = DateTime.Now;
						_context.Update(receivingAccount);
						_context.Add(message1);
						_context.SaveChanges();
						_context.Database.ExecuteSqlInterpolated($"SET IDENTITY_INSERT [dbo].[Transaction] OFF");
						if (receivingAccount.amount - money == recB)
						{
							return RedirectToAction("Details", new { id = transferingAccount.accountId });
						}
						else
						{
							transferingAccount.amount += (money / rate);
							_context.Update(transferingAccount);
							_context.SaveChanges();
							ModelState.AddModelError(string.Empty, "Error making the transaction, please try again");
							return View(transferingAccount);
						}
					}
					ModelState.AddModelError(string.Empty, "Invalid Operation");
					return View(transferingAccount);

				}
				else
				{
					ModelState.AddModelError(string.Empty, "Incorrect password");
					message.Name = "Transfer";
					message.Status = "Unsuccessful";
					message.amount = money;
					message.currency = transferingAccount.currencyType;
					message.Description = "There was an attempt to remove money on your account, please if you are not the one who did it change your password and report this issue.";
					message.accountNumber = transferingAccount.accountNumber ?? 0;
					message.Tway = "account";
					message.userId = transferingAccount.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					await _context.SaveChangesAsync();
					return View(transferingAccount);
				}
			}
			ModelState.AddModelError(string.Empty, "Please double check the Account/s and try again");
			return View(transferingAccount);
		}
		// GET: Accounts
		[HttpGet]
		public IActionResult Index()
		{
			return RedirectToAction("AccountsInfo", new {id=_userManager.GetUserId(User)});

		}
		[HttpGet]
		public async Task<IActionResult> Details(int? id)
		{
			var account = await _context.Account
				.Include(a => a.ThesisProjectUser)
				.Include(a => a.Card)
				.FirstOrDefaultAsync(m => m.accountId == id);
			if (account != null)
			{
				if (account.userId != _userManager.GetUserId(User))
				{
					return NotFound();
				}
				return View(account);
			}
			return View();
		}


		//*******************************creating a bank account
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(string accountName, string currencyType, string accountType)
		{
			var message = new Transaction();
			if (_httpcontextAccessor.HttpContext.User.Identity.IsAuthenticated)
			{
				var existingAccounts = _context.Account
					.Where(m => m.accountType == accountType && m.currencyType == currencyType && m.userId == _userManager.GetUserId(User))
					.ToList();
				if (accountType.ToUpper() == "SAVING")
				{
					var acc = _context.Account.Where(a => a.currencyType == currencyType && a.accountType == "Current");
					if (!acc.Any())
					{
                        ModelState.AddModelError(string.Empty, $"Please first add a current account of {currencyType} currency");
                        return View();
                    }
					if (existingAccounts.Any())
					{
						ModelState.AddModelError(string.Empty, $"You can only have one {accountType} account of {currencyType} currency");
						return View();
					}
				}
				else if (accountType.ToUpper() == "CURRENT" && existingAccounts.Any())
				{
					ModelState.AddModelError(string.Empty, $"You can only have one {accountType} accounts with the {currencyType} currency");
					return View();
				}
				_accountService.CreateAccount(accountName, currencyType, accountType);
				message.Name = "Account Creation";
				message.Description = "Successfully created a " + currencyType + " " + accountType + " account";
				message.Status = "Done";
				message.userId = _userManager.GetUserId(User);
				message.dateOfTransaction = DateTime.Now;
				_context.Update(message);
				_context.SaveChanges();
				return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Please log in to access this feature");
				return RedirectToAction(nameof(Index));
			}
		}
		//************************ creating a card and all it's functionalities
		[HttpGet]
		public IActionResult CreateCardi()
		{
			return View();
		}
		[HttpPost]
		public IActionResult CreateCardi(string cardName, string? currencyType)
		{
			Random random = new Random();
			var card = new Card();
			var message = new Transaction();
			var expiry = DateTime.Now.AddYears(7);
			card.cardName = cardName;
			card.userId = _userManager.GetUserId(User);
			while (_context.Card.Any(a => a.cardNumber == cardN))
			{
				cardN += 1;
			}
			if (cardName == "Multi-currency")
			{
				var cardCheck = _context.Card
					.Where(c => c.cardName == cardName && c.userId == _userManager.GetUserId(User))
					.ToList();
				if (cardCheck.Count >= 2)
				{
					ModelState.AddModelError(string.Empty, "you can have at most two multi-currency cards");
					return View();
				}
				card.cardNumber = cardN;
				card.expiryDate = DateOnly.FromDateTime(expiry);
				card.DateOfCreation = DateTime.Now;
				card.cvv = random.Next(100, 999);
				card.cardStatus = "Active";
				message.userId = card.userId;
				message.Name = "Card creation";
				message.Description = "You have successfully created a " + card.cardName + " card";
				message.Status = "successful";
				message.dateOfTransaction = DateTime.Now;
				_context.Update(message);
				_context.Add(card);
				_context.SaveChanges();
			}
			if (cardName == "Virtual-Card")
			{
				var account = _context.Account.FirstOrDefault(a => a.currencyType == currencyType && a.accountType != "Saving" && a.userId == _userManager.GetUserId(User));
				if (account == null)
				{
					ModelState.AddModelError(string.Empty, $"you don't have any account with the {currencyType} currency");
					return View();
				}
				else if (account.cardId != null)
				{
					ModelState.AddModelError(string.Empty, $"you already have a card linked with your {currencyType} account");
					return View();
				}
                card.cardNumber = cardN;
                expiry = DateTime.Now.AddYears(7);
				card.expiryDate = DateOnly.FromDateTime(expiry);
				card.DateOfCreation = DateTime.Now;
				card.cvv = random.Next(100, 999);
				card.cardStatus = "Active";
				message.userId = _userManager.GetUserId(User);
				message.Name = "Card creation";
				message.Description = "You have successfully created a " + card.cardName + " card linked with your " + account.currencyType + " account";
				message.Status = "successful";
				message.currency = account.currencyType;
				message.dateOfTransaction = DateTime.Now;
				_context.Add(card);
				_context.Update(message);
				_context.SaveChanges();
				account.cardId = card.cardId;
				_context.Update(account);
				_context.SaveChanges();
				return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
			}
			return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
		}
		[HttpGet]
		public IActionResult Cards(string id)
		{
			if (id != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			else
			{
				var card = _context.Card
					.Where(c => c.userId == id && c.cardStatus != "Deleted");
				return View(card.ToList());
			}
		}
		public IActionResult cardInfo(int id)
		{
			var card = _context.Card.FirstOrDefault(c => c.cardId == id);
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			else
			{
				return View(card);
			}
		}
		public IActionResult FreezeCard(int id)
		{
			var card = _context.Card.FirstOrDefault(c => c.cardId == id);
			var message = new Transaction();
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			card.cardStatus = "Frozen";
			message.userId = card.userId;
			message.Name = "Card Frozen";
			message.Description = "You have successfully Frozen your " + card.cardName + " card";
			if (card.cardName == "Virtual-Card")
			{
				message.currency = card.Account.currencyType;
				message.accountNumber = card.Account.accountNumber ?? 0;
			}
			message.Status = "successful";
			message.dateOfTransaction = DateTime.Now;
			_context.Update(message);
			_context.Update(card);
			_context.SaveChanges();
			return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
		}
		public IActionResult DeleteCard(int id)
		{
			var card = _context.Card.FirstOrDefault(c => c.cardId == id);
			var message = new Transaction();
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			card.cardStatus = "Deleted";
			var account = _context.Account.FirstOrDefault(a => a.cardId == id);
			if (account != null)
			{
				account.cardId = null;
				_context.Update(account);
			}
			message.userId = card.userId;
			message.Name = "Card Deleted";
			message.Description = "You have successfully Deleted your " + card.cardName + " card";
			if (card.cardName == "Virtual-Card")
			{
				message.currency = card.Account.currencyType;
				message.accountNumber = card.Account.accountNumber ?? 0;
			}
			message.Status = "successful";
			message.dateOfTransaction = DateTime.Now;
			_context.Update(message);
			_context.Update(card);
			_context.SaveChanges();
			return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
		}
		public IActionResult UnfreezeCard(int id)
		{
			var card = _context.Card.FirstOrDefault(c => c.cardId == id);
			var message = new Transaction();
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			card.cardStatus = "Active";
			message.userId = card.userId;
			message.Name = "Card Unfrozen";
			message.Description = "You have successfully unfrozen your " + card.cardName + " card";
			if (card.cardName == "Virtual-Card")
			{
				message.currency = card.Account.currencyType;
				message.accountNumber = card.Account.accountNumber ?? 0;
			}
			message.Status = "successful";
			message.dateOfTransaction = DateTime.Now;
			_context.Update(message);
			_context.Update(card);
			_context.SaveChanges();
			return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
		}
		[HttpGet]
		public IActionResult BuyWithCardi(int id)
		{
			var card = _context.Card.FirstOrDefault(c => c.cardId == id);
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(card);
		}
		[HttpPost]
		public IActionResult BuyWithCardi(Card model)
		{
			int cardId = model.cardId;
			double? money = model.Account.amount;
			string currency = model.Account.currencyType;

			double? Mbuy = 0;
			var card = _context.Card.FirstOrDefault(c => c.cardId == cardId);
			var message = new Transaction();
			var accounts = _context.Account.Where(a => a.userId == _userManager.GetUserId(User)).ToList();
			foreach(var account in accounts)
			{
				if(account.accountLimit < money)
				{
                    ModelState.AddModelError(string.Empty, "Amount exceeds your limit,Please change your limit before making the transactions");
                    return View(card);
                }
			}
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			if (card == null)
			{
				ModelState.AddModelError(string.Empty, "Error processing your transaction, please try again");
				return View(card);
			}
			if (card.cardStatus == "Frozen")
			{
				ModelState.AddModelError(string.Empty, "You can't use a frozen card. Unfreeze it first or add a new one.");
				return View(card);
			}
			else if (card.cardStatus == "Deleted")
			{
				ModelState.AddModelError(string.Empty, "You are not allowed to use this card.");
				return View(card);
			}

			if (card.cardName == "Multi-currency")
			{
				var accountE = _context.Account
					.FirstOrDefault(a => a.userId == _userManager.GetUserId(User) && a.accountType != "Saving" && a.currencyType == "Eur");
				var accountD = _context.Account
					.FirstOrDefault(a => a.userId == _userManager.GetUserId(User) && a.accountType != "Saving" && a.currencyType == "Dollar");
				var accountZ = _context.Account
									.FirstOrDefault(a => a.userId == _userManager.GetUserId(User) && a.accountType != "Saving" && a.currencyType == "Zloty");
				if (currency == "Eur")
				{
					money *= 4.2;
				}
				else if (currency == "Dollar")
				{
					money *= 4;
				}
				else
				{
					money *= 1;
				}
				if (money < accountZ.amount)
				{
					accountZ.amount -= money;
					_context.Update(accountZ);
					message.Name = "Card Purchase";
					message.Status = "Successful";
					message.amount = money ?? 0;
					message.currency = currency;
					message.Description = "Successfully made a purchase using your " + card.cardName + " card";
					message.accountNumber = accountZ.accountNumber ?? 0;
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Add(message);
					_context.SaveChanges();
					return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
				}
				else if (money < (accountE.amount * 4.2))
				{
					accountE.amount -= (money / 4.2);
					_context.Update(accountE);
					message.Name = "Card Purchase";
					message.Status = "Successful";
					message.amount = money/4.2 ?? 0;
					message.currency = currency;
					message.Description = "Successfully made a purchase using your " + card.cardName + " card";
					message.accountNumber = accountE.accountNumber ?? 0;
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Add(message);
					_context.SaveChanges();
					return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
				}
				else if (money < (accountD.amount * 4))
				{
					accountD.amount -= (money / 4);
					_context.Update(accountD);
					message.Name = "Card Purchase";
					message.Status = "Successful";
					message.amount = money/4 ?? 0;
					message.currency = currency;
					message.Description = "Successfully made a purchase using your " + card.cardName + " card";
					message.accountNumber = accountD.accountNumber ?? 0;
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					_context.SaveChanges();
					return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
				}
				else
				{
					ModelState.AddModelError(string.Empty, "You don't have sufficient funds.");
					message.Name = "Card Purchase";
					message.Status = "Unsuccessful";
					message.amount = money ?? 0;
					message.currency = currency;
					message.Description = "Please add more money to your account before making the transaction";
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Add(message);
					_context.SaveChanges();
					return View(card);
				}



			}
			else
			{
				var account = _context.Account
					.FirstOrDefault(a => a.userId == _userManager.GetUserId(User) && a.accountType != "Saving" && a.cardId == cardId);

				if (money > account.amount)
				{
					ModelState.AddModelError(string.Empty, "You don't have sufficient funds.");
					message.Name = "Card Purchase";
					message.Status = "Unsuccessful";
					message.amount = money ?? 0;
					message.currency = account.currencyType;
					message.Description = "Please add more money to your account before making the transaction";
					message.accountNumber = account.accountNumber ?? 0;
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Add(message);
					_context.SaveChanges();
					return View(card);
				}
				account.amount -= money;
				message.Name = "Card Purchase";
				message.Status = "Successful";
				message.amount = money ?? 0;
				message.currency = account.currencyType;
				message.Description = "Successfully made a purchase using your " + card.cardName + " card";
				message.accountNumber = account.accountNumber ?? 0;
				message.Tway = "Card";
				message.userId = card.userId;
				message.dateOfTransaction = DateTime.Now;
				_context.Add(message);
				_context.Update(account);
				_context.SaveChanges();
			}

			return RedirectToAction("Details", new { id = card.Account.accountId });
		}

		[HttpGet]
		public IActionResult DepositWithCard(int id)
		{
			var card = _context.Card
				.Include(c => c.Account)
				.FirstOrDefault(c => c.cardId == id);
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(card);
		}
		[HttpPost]
		public IActionResult DepositWithCard(int cardId, string? currencyType, double money)
		{
			var card = _context.Card
				.Include(c => c.Account)
				.FirstOrDefault(c => c.cardId == cardId);
            if (card == null)
            {
                ModelState.AddModelError(string.Empty, "Error processing your transaction, please try again");
                return View(card);
            }
            if (card.cardStatus == "Frozen")
            {
                ModelState.AddModelError(string.Empty, "You can't use a frozen card. Unfreeze it first or add a new one.");
                return View(card);
            }
            else if (card.cardStatus == "Deleted")
            {
                ModelState.AddModelError(string.Empty, "You are not allowed to use this card.");
                return View(card);
            }
            var message = new Transaction();
			if (card.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			if (currencyType == null)
			{
				var account = _context.Account.FirstOrDefault(a => a.cardId == cardId);
				if (account == null)
				{
					ModelState.AddModelError(string.Empty, "your card is not linked with a bank account");
					message.Name = "Card Deposit";
					message.Status = "Unsuccessful";
					message.amount = money;
					message.currency = account.currencyType;
					message.Description = "your card is not linked with a bank account, unsuccessful deposit";
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					_context.SaveChanges();
					return View(card);
				}
				else
				{
					account.amount += money;
					message.Name = "Card Deposit";
					message.Status = "Successful";
					message.amount = money;
					message.currency = account.currencyType;
					message.Description = "You have successfully deposited " + money + " in your " + account.currencyType + " account";
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					_context.SaveChanges();
					_context.Update(account);
				}
			}
			else
			{
				var account = _context.Account
					.FirstOrDefault(a => a.currencyType == currencyType && a.accountType != "Saving" && a.userId == _userManager.GetUserId(User));
				if (account == null)
				{
					ModelState.AddModelError(string.Empty, "your card is not linked with a bank account");
					message.Name = "Card Deposit";
					message.Status = "Unsuccessful";
					message.amount = money;
					message.currency = currencyType;
					message.Description = "your card is not linked with a bank account, unsuccessful deposit";
					message.Tway = "Card";
					message.userId = card.userId;
					message.dateOfTransaction = DateTime.Now;
					_context.Update(message);
					_context.SaveChanges();
					return View(card);
				}
				account.amount += money;
				message.Name = "Card Deposit";
				message.Status = "Successful";
				message.amount = money;
				message.currency = currencyType;
				message.Description = "You have successfully deposited " + money + " in your " + account.currencyType + " account";
				message.Tway = "Card";
				message.userId = card.userId;
				message.dateOfTransaction = DateTime.Now;
				_context.Update(message);
				_context.SaveChanges();
				_context.Update(account);
			}
			return RedirectToAction("Cards", new { id = _userManager.GetUserId(User) });
		}
		//************************************************************************** money deposit
		[HttpGet]
		public async Task<IActionResult> Deposit(int? id)
		{
			var account = await _context.Account.FindAsync(id);
			if (account != null)
			{
				if (account.userId != _userManager.GetUserId(User))
				{
					return NotFound();
				}
				return View(account);
			}
			return NotFound();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deposit(int id, double amount)
		{
			var accountD = _context.Account.FirstOrDefault(c => c.accountId == id);
			if (accountD.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			else if(accountD.accountType == "Saving")
			{
				var acc = _context.Account.FirstOrDefault(c => c.currencyType == accountD.currencyType && c.accountType == "Current" && c.userId == _userManager.GetUserId(User));
				if(amount > acc.amount)
				{
                    ModelState.AddModelError(string.Empty, "insufficient balance");
                    return View(accountD);
                }
				await deposit(amount, accountD.accountNumber, "Deposit");
				return await withdraw(amount, acc.accountNumber ?? 0, "", "Withdraw",true);

            }
			return await deposit(amount, accountD.accountNumber, "Deposit");
		}
		//******************************************************************money withdraw
		[HttpGet]
		public async Task<IActionResult> Withdraw(int id)
		{
			var account = await _context.Account.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}
			if (account.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(account);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Withdraw(int id, double amount, string? password)
		{
			var accountD = _context.Account.FirstOrDefault(c => c.accountId == id);
			if (accountD == null)
			{
				return NotFound();
			}
			else if (accountD.userId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            else if (accountD.accountType == "Saving")
            {
                var acc = _context.Account.FirstOrDefault(c => c.currencyType == accountD.currencyType && c.accountType == "Current" && c.userId == _userManager.GetUserId(User));
				if(amount > accountD.amount)
				{
                    ModelState.AddModelError(string.Empty, "insufficient balance");
                    return View(accountD);
                }
                await deposit(amount, acc.accountNumber, "Deposit");
                return await withdraw(amount, accountD.accountNumber ?? 0, "", "Withdraw",true);
            }
            else
			{
				
				limit = accountD?.accountLimit;
				return await withdraw(amount, accountD.accountNumber ?? 0, password, "Withdraw",false);
			}
		}
		//******************************change account limit
		[HttpGet]
		public IActionResult SetLimit(string? id)
		{
			var account = _context.Account.FirstOrDefault(a => a.userId == id);
			if (account == null)
			{
				ModelState.AddModelError(string.Empty, "You don't have any account or Invalid operation");
				return View();
			}
			if (account.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(account);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SetLimit(int accountLimit)
		{
			var accountD = _context.Account.Where(c => c.userId == _userManager.GetUserId(User)).ToList();
			_context.Update(setL(accountLimit, accountD));
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		//*************************************************making a transfer
		[HttpGet]
		public async Task<IActionResult> Transfer(int? id)
		{
			var account = await _context.Account.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}
			if (account.userId != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(account);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Transfer(int id, string name, double amount, long accountNumber, string description, string password)
		{
			var accountD = _context.Account.FirstOrDefault(c => c.accountId == id);
			if (accountD != null)
			{
				if (accountD.userId != _userManager.GetUserId(User))
				{
					return NotFound();
				}
				var RecievingAccountD = _context.Account.FirstOrDefault(c => c.accountNumber == accountNumber);
				limit = accountD?.accountLimit;
				return await transfer(accountD.accountNumber, RecievingAccountD.accountNumber, amount, name, description, password);
			}
			return NotFound();
		}

		[HttpGet]
		public async Task<IActionResult> AccountsInfo(string? id)
		{
			var thesisProjectContext = _context.Account.Include(a => a.ThesisProjectUser)
								.Where(m => m.userId == id);
			if (id != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			//totalbalance();
			return View(await thesisProjectContext.ToListAsync());
		}

		[HttpGet]
		public IActionResult Inbox()
		{
			var messages = _context.Transaction
				.Where(t => t.userId == _userManager.GetUserId(User))
				.OrderByDescending(t => t.dateOfTransaction);
			if (messages == null)
			{
				return View();
			}
			return View(messages.ToList());

		}
		[HttpPost]
		public IActionResult Inbox(string search)
		{
			if (search == null)
			{
				var messages = _context.Transaction
					.Where(t => t.userId == _userManager.GetUserId(User))
					.OrderByDescending(t => t.dateOfTransaction);
				if (messages == null)
				{
					return View();
				}
				return View(messages.ToList());
			}
			var message = _context.Transaction.Where(m => m.Tway.ToUpper() == search.ToUpper() ||
			m.amount.ToString().ToUpper().Contains(search.ToUpper()) || m.Description.ToUpper().Contains(search.ToUpper()) || m.dateOfTransaction.ToString().Contains(search) ||
			m.Name.ToUpper().Contains(search.ToUpper()) && m.userId == _userManager.GetUserId(User));
			return View(message.ToList());
		}

		public IActionResult Messag(int id)
		{
			var message = _context.Transaction.FirstOrDefault(m => m.Id == id && m.userId == _userManager.GetUserId(User));
			return View(message);
		}
		public async Task<IActionResult> Settings(string? id)
		{
			var account = await _userManager.Users
				.FirstOrDefaultAsync(m => m.Id == id);
			if (account != null)
			{
				if (account.Id != _userManager.GetUserId(User))
				{
					return NotFound();
				}
				return View(account);
			}
			return NotFound();
		}
		public IActionResult ChooseDe(string? id)
		{
			var account = _context.Account
								.Where(m => m.userId == id && m.accountType != "Saving")
								.ToList();
			if (id != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			return View(account);
		}
		public IActionResult ChooseW(string? id)
		{
			if (id != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			var account = _context.Account
								.Where(m => m.userId == id && m.accountType != "Saving")
								.ToList();
			return View(account);
		}
		public IActionResult ChooseT(string? id)
		{
			if (id != _userManager.GetUserId(User))
			{
				return NotFound();
			}
			var account = _context.Account
								.Where(m => m.userId == id && m.accountType != "Saving")
								.ToList();
			return View(account);
		}
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Account.FirstOrDefaultAsync(a => a.accountId == id && a.accountType == "Saving");
			var message = new Transaction();
            if (account != null)
            {
				if (account.amount == 0)
				{
                    message.Name = "Account Deletion";
                    message.Status = "Successful";
                    message.currency = account.currencyType;
                    message.Description = "Your have successfully removed money from your account";
                    message.accountNumber = account.accountNumber ?? 0;
                    message.Tway = "account";
                    message.userId = account.userId;
                    message.dateOfTransaction = DateTime.Now;
                    _context.Add(message);
                    _context.Account.Remove(account);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
                }
				ModelState.AddModelError(string.Empty, "please empty your account first");
				return RedirectToAction("Details", new { id = account.accountId });
            }
            return RedirectToAction("AccountsInfo", new { id = _userManager.GetUserId(User) });
        }
        public IActionResult Lang(string? Id)
        {
            string returnUrl = Request.Headers["Referer"].ToString() ?? "/";
            string? culture = Id;
            if (culture != null)
            {
				try
				{
					Response.Cookies.Append(
						CookieRequestCultureProvider.DefaultCookieName,
						CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
						new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
						);
				}
				catch
				{
                    return Redirect(returnUrl);

                }
            }
            return Redirect(returnUrl);
        }
        private bool AccountExists(int id)
		{
			return _context.Account.Any(e => e.accountId == id);
		}
	}
}
