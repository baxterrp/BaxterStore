namespace BaxterStore.Data.POCOs.Users
{
    public class UserDataEntity : DataEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
