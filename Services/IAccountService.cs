namespace ThesisProject.Services
{
    public interface IAccountService
    {
        void CreateAccount(string accountName, string currencyType, string accountType);
    }
}
