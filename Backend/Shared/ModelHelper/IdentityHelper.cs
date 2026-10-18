using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Shared.Interfaces;

namespace Shared.ModelHelper
{
    public sealed class IdentityHelper<TAccountModel>(AccountRulesModel rules, IPasswordHasher<TAccountModel> hasher) : IIdentityHelper<TAccountModel>
        where TAccountModel : class
    {
        public bool IsEmailValid(string email)
        {
            var pattern = rules.RegexForEmail;
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, pattern);
        }


        public bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < rules.MinPasswordLength)
            {
                return false;
            }

            if (password.Length > rules.MaxPasswordLength)
            {
                return false;
            }

            if (rules.RequireDigit && !password.Any(char.IsDigit))
            {
                return false;
            }

            if (rules.RequireUppercase && !password.Any(char.IsUpper))
            {
                return false;
            }

            if (rules.RequireLowercase && !password.Any(char.IsLower))
            {
                return false;
            }

            return !rules.RequiredSpecialCharacter || password.Any(char.IsPunctuation);
        }

        public string GetHashPassword(TAccountModel account, string password)
        {
            return hasher.HashPassword(account, password);
        }

        public bool VerifyPassword(TAccountModel account, string password, string hash)
        {
            return hasher.VerifyHashedPassword(account, hash, password) == PasswordVerificationResult.Success;
        }

        public Tuple<bool, string> ValidateEmailAndPassword(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Tuple.Create(false, "Email is required.");
            }
            return string.IsNullOrWhiteSpace(password) ? Tuple.Create(false, "Password is required.") : Tuple.Create(true, string.Empty);
        }
    }
}