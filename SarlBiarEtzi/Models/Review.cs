namespace SarlBiarEtzi.Models
{
    public class Review
{
    public int Id { get; set; }
    public string Email { get; set; }
    public int Stars { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
}