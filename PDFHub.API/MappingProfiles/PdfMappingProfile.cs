using AutoMapper;
using PDFHub.API.Models.Domains;
using PDFHub.API.Models.DTOs;

namespace PDFHub.API.MappingProfiles;

public class PdfMappingProfile : Profile
{
    public PdfMappingProfile()
    {
        CreateMap<PdfFiles, UploadPdfResponse>()
            .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
