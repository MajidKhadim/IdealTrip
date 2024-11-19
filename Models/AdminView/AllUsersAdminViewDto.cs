namespace IdealTrip.Models.AdminView
{
    public class AllUsersAdminViewDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Role { get; set; }
    }
}
