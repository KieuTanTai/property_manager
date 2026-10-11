using System.Text.RegularExpressions;
using Identity.Interfaces;
using Identity.Models;
using Identity.Models.Account;
using Microsoft.AspNetCore.Identity;

namespace Identity.Utils
{
    public sealed class AccountHelper(AccountRulesModel rules, IPasswordHasher<AccountModel> hasher) : IAccountHelper
    {
        private readonly IPasswordHasher<AccountModel> _hasher = hasher;

        private readonly AccountRulesModel _rules = rules;

        public bool IsEmailValid(string email)
        {
            var pattern = _rules.RegexForEmail;
            return !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, pattern);
        }


        public bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < _rules.MinPasswordLength)
            {
                return false;
            }

            if (password.Length > _rules.MaxPasswordLength)
            {
                return false;
            }

            if (_rules.RequireDigit && !password.Any(char.IsDigit))
            {
                return false;
            }

            if (_rules.RequireUppercase && !password.Any(char.IsUpper))
            {
                return false;
            }

            if (_rules.RequireLowercase && !password.Any(char.IsLower))
            {
                return false;
            }

            return !_rules.RequiredSpecialCharacter || password.Any(char.IsPunctuation);
        }

        public string GetHashPassword(AccountModel account, string password)
        {
            return _hasher.HashPassword(account, password);
        }

        public bool VerifyPassword(AccountModel account, string password, string hash)
        {
            return _hasher.VerifyHashedPassword(account, hash, password) == PasswordVerificationResult.Success;
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