namespace PDFHub.API.Models.DTOs;

public class SummarizationProgressEvent
{
    public int Progress { get; set; } // 0-100%
    public string Stage { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public bool IsFailed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Summary { get; set; }
}
