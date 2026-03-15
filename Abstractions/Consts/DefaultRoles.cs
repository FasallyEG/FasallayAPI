namespace Fasally.Abstractions.Consts;

public static class DefaultRoles
{
    public partial class Admin
    {
        public const string Name = nameof(Admin);
        public const string Id = "019b5f05-109b-7d57-b81a-90ca7bf87de6";
        public const string ConcurrencyStamp = "019b5f05-109b-7bd7-9d47-3b7ff87a444f";
    }

    public partial class Member
    {
        public const string Name = nameof(Member);
        public const string Id = "019b5f05-109b-71ac-bbd0-1543b80178a9";
        public const string ConcurrencyStamp = "019b5f05-109b-78a1-9393-f1990a5a7d58";
    }
}