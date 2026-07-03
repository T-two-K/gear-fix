using GearFix.Models.ApiModels.AiModels;
using GearFix.ViewModels;

namespace GearFix.Models.AppModels
{
    public class BaseDialogModel 
    {
        public string Title { get; set; } = string.Empty;
        public string ButtonContent { get; set; } = string.Empty;
        public bool DialogResult { get; set; } = false;
    }

    public class ChangePasswordDialogModel : BaseDialogModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
    }

    public class DetailedDescriptionMalcfunctionDialogModel : BaseDialogModel
    {
        public Malfunction? Malfunction { get; set; } 
    }

    public class ListOfMalfunctionsDialogModel : BaseDialogModel
    {
        public GeminiMalfunctionResponse DiagnosInfo { get; set; } = new();
        public Malfunction? SelectedMalfunction { get; set; } = null;
    }

    public class AiApiKeyDialogModel : BaseDialogModel
    {
        public string ApiKey { get; set; } = string.Empty;
    }

    public class WarningDialogModel : BaseDialogModel
    {
        public string WarningContent { get; set; } = string.Empty;
    }

    public class EditCarDialogModel : BaseDialogModel
    {
        public CarModel? SelectedCar { get; set; }
    }

    public class EditMalfunctionDialogModel : BaseDialogModel
    {
        public MalfunctionModel? SelectedMalfunction { get; set; }
        public CarModel? SelectedCar { get; set; }
    }

    public class SelectCarDialogModel : BaseDialogModel
    {
        public List<CarModel> Cars { get; set; } = new();
        public CarModel? CurrentCar { get; set; }
    }
}
