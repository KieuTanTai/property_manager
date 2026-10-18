
namespace Shared.Interfaces
{
    public interface IIdentityHelper<in TAccountModel> where TAccountModel : class
    {
        bool IsEmailValid(string email);
        bool IsPasswordValid(string password);




        string GetHashPassword(TAccountModel account, string password);

        bool VerifyPassword(TAccountModel account, string password, string hash);

        public Tuple<bool, string> ValidateEmailAndPassword(string email, string password);
    }
}