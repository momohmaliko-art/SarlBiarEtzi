namespace SarlBiarEtzi.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int Room_Id { get; set; }
        
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime Created_At { get; set; }
        public ChatRoom Room { get; set; }
    }
}