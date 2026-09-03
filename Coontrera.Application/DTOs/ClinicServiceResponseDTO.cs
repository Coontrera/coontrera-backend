namespace Coontrera.Application.DTOs
{
    public class ClinicServiceResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Benefits { get; set; } = new();
        public string ImageUrl { get; set; } = string.Empty;
        public string ImageAlt { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime DateRegistered { get; set; }
    }
}
