using CommunityToolkit.Mvvm.ComponentModel;
using GearFix.DTO;
using System.Text.Json.Serialization;

namespace GearFix.Models.AppModels;

public class CarModel
{
    public string Model { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Vin { get; set;  } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string? EngineType { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? ImagePath { get; set; }
    public List<MalfunctionModel> Malfunctions { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
}

public partial class MalfunctionModel : ObservableObject
{
    [ObservableProperty]
    public string _title = string.Empty;
    [ObservableProperty]
    public string _description = string.Empty;
    [ObservableProperty]
    public DateOnly _date;
    [ObservableProperty]
    public int _danger;
}

public class JsonDataModel
{
    public List<CarModel> Cars { get; set; } = new();
    public List<string> Keys { get; set; } = new();
}