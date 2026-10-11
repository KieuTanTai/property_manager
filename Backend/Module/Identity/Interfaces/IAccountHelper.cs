using Identity.Models.Account;

namespace Identity.Interfaces
{
    public interface IAccountHelper
    {
        bool IsEmailValid(string email);
        bool IsPasswordValid(string password);


        string GetHashPassword(AccountModel account, string password);

        bool VerifyPassword(AccountModel account, string password, string hash);

        public Tuple<bool, string> ValidateEmailAndPassword(string email, string password);
    }
}