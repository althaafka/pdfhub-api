using Microsoft.AspNetCore.Identity;

namespace PDFHub.API.Models.Domains;

public class PdfFiles
{
    public int Id {get; set;}
    public string FileName {get; set;} = string.Empty;
    public string FilePath {get; set;} = string.Empty;
    public long FileSize {get; set;} // bytes
    public string Description {get; set;} = string.Empty;
    public string UserId {get; set;} = string.Empty;
    public string? Summary {get; set;}
    public bool IsDeleted {get;set;} = false;

    // Relation
    public IdentityUser User {get; set;}
}