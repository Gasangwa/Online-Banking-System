using ThesisProject.Areas.Identity.Data;

namespace ThesisProject.Models
{
    public class Card
    {
        public int cardId { get; set; }
        public long cardNumber { get; set; }
        public string cardName { get; set; }
        public DateOnly expiryDate { get; set; }
        public DateTime DateOfCreation { get; set; }
        public int cvv { get; set; }
        public string cardStatus { get; set; }
        public string userId { get; set; } // foreign key
        public ThesisProjectUser ThesisProjectUser { get; set; } // navigation property
        public Account Account { get; set; } // navigation property
        public Card() { }
    }
}
