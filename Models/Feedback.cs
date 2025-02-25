using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdealTrip.Models
{
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ServiceType { get; set; }  // e.g., "TourGuide", "Hotel"
        public Guid ServiceId { get; set; }      // ID from respective service table
        public string FeedbackText { get; set; }
        public decimal Rating { get; set; }      // 1-5 stars
        public DateTime Date { get; set; } = DateTime.Now;
        public ApplicationUser? User { get; set; }
    }

}
