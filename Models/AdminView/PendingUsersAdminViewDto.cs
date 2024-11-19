using IdealTrip.Models.Enums;

namespace IdealTrip.Models.AdminView
{
    public class PendingUsersAdminViewDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public ProofStatus Status { get; set; }
        public List<ProofDto> Proofs { get; set; }
    }

    public class ProofDto
    {
        public string DocumentType { get; set; }
        public string DocumentPath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
