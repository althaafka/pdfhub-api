namespace PDFHub.API.Models.DTOs;

public class EditPdfResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
