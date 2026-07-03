using GearFix.Models.ApiModels.AiModels;
using GearFix.Models.AppModels;

namespace GearFix.DTO
{
    public class MessageDto
    {
        public string Message { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public bool IsCurrentCarInfoString { get; set; } = false;
        public bool IsNhtsaString { get; set;  } = false;
        public bool IsDiagnosString { get; set; } = false;
    }

    public class CarJsonDto
    {
        public string Model { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Mileage { get; set; }
        public string? EngineType { get; set; } 
        public string? AdditionalInfo { get; set; } 
        public List<MalfunctionModel> MalfunctionList { get; set; } = new(); 
    }

    public class CarServicesDto
    {
        public string Name { get; set;  } = string.Empty;
        public string Address { get; set;  } = string.Empty;
        public List<string> Rubrics { get; set; } = new();
    }
}
