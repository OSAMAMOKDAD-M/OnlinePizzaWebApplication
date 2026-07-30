namespace OnlinePizzaWebApplication.Data
{
    /// <summary>Central definition of the application roles. No role name is hardcoded elsewhere.</summary>
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string User = "User";

        public static readonly string[] All = { SuperAdmin, Owner, Admin, User };

        /// <summary>Roles allowed to manage the restaurant profile and general settings.</summary>
        public const string Management = SuperAdmin + "," + Owner + "," + Admin;
    }
}
