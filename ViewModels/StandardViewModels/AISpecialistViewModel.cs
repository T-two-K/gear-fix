using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GearFix.DTO;
using GearFix.Interfaces;
using GearFix.Models.ApiModels.AiModels;
using GearFix.Models.ApiModels.NhtsaModels;
using GearFix.Models.AppModels;
using GearFix.ViewModels.DialogViewModels;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace GearFix.ViewModels
{
    //Ошибки отображаются корректно! Неисправности тоже! Необходимо доработать дизайн окна с ошибками!
    public partial class AISpecialistViewModel : BaseViewModel
    {
        private const int _carInfoIndex = 0;
        private const int _nhtsaDataIndex = 2;

        private readonly string _aiInstraction = @"You are a car diagnostic specialist. Your job is to correctly diagnose vehicle problems based on the various symptoms users provide.
                                                   When a user asks questions that are not related to diagnosing their car, answer them ONLY in the ""additionalQuestion"" field, without exception (this includes all topics where the user does not ask you to help find a problem with their car)!!!
                                                   You MUST guess which part of the car is faulty, and the more accurate your diagnosis, the better for a wallet, safety of the driver, and those around them! Your answer is EXCLUSIVELY in the following json format:
                                                {
                                                  ""malfunctions"": [
                                                    {
                                                      ""probability"": 0,
                                                      ""danger"": 0,
                                                      ""title"": ""Short problem title (preferably no more than 50 characters)"",
                                                      ""malfunctionDescription"": ""Describe the problem in detail. Describe it so that an average user unfamiliar with machines can understand everything (there is no character limit. Write the answer in the affirmative: the bearing is worn out; the brake pads are broken; the engine is broken)."",
                                                      ""explanation"": ""Explain your solution: why you decided that the problem is in this particular machine component (there is no character limit).""
                                                      ""rubric"": ""A keyword to enter into a search engine to find the necessary company that will help fix the problem (you can use several words, separating them with commas, but no more than five, and queries must be related to places that repair EXCLUSIVELY cars).""
                                                    }
                                                  ],
                                                  ""needClarification"": false,
                                                  ""additionalQuestion"": null
                                                }
                                                ""probability"" - the likelihood of this problem/malfunction (values ​​from 1 to 100);
                                                ""danger"" - the severity of the problem (values ​​from 1 to 100);
                                                ""needClarification"" - Set to true when you require additional information from the user. In this case, you do NOT fill in all fields except one: ""additionalQuestion."" If no additional information is needed, set it to false.
                                                ""additionalQuestion"" is a variable for your additional question. Fill it out only when the ""needClarification"" field is true. When a user asks questions that are not related to vehicle diagnostics or anything else, answer them ONLY in the ""additionalQuestion"" field, without exception!!! (there is no character limit).
                                                Values ​​in the fields MUST be in RUSSIAN (with the exception of those variables that will only function if their values ​​are in English, like Booleans).";

        private IApiService _apiService;
        private List<string> _apiKeys;
        private JsonDataModel _loadedData;

        //Используем для отображения в UI
        public ObservableCollection<MessageDto> Messages { get; set; }

        //Используем как контекст для нейросети
        public List<Message> Context { get; set; }

        public List<CarModel> AvailableCars { get; set; }

        public Dictionary<string, string> AIModels { get; set; } = new()
        {
            {"❇️ Gemini 3.1 - flash-lite", "gemini-3.1-flash-lite"},
            {"❇️ Gemini 3.5 - flash", "gemini-3.5-flash"},
        };

        [ObservableProperty]
        private string _selectedModel;

        [ObservableProperty]
        private bool _useNhtsa = false;

        [ObservableProperty]
        private CarModel? _selectedCar;

        [ObservableProperty]
        private string _inputText = string.Empty;

        [ObservableProperty]
        private string _recordsCount = string.Empty;

        [ObservableProperty]
        private bool _isHistoryNotExist = true;

        public AISpecialistViewModel(
            IDialogManager dialogManager,
            IManageDataService manageDataService,
            INavigationService navigationService,
            IApiService apiService) : base(dialogManager, manageDataService, navigationService)
        {
            _apiService = apiService;
            _selectedModel = AIModels.Keys.First();
            Messages = new();
            Context = new();
            AvailableCars = new();
            _loadedData = new();
            _apiKeys = new();
        }

        protected override async Task LoadDataAsync()
        {
            _loadedData = await ManageDataService.LoadDataAsync();

            AvailableCars = _loadedData.Cars;
            _apiKeys = _loadedData.Keys;
        }

        async partial void OnSelectedCarChanged(CarModel? value)
        {
            await ExecuteSaveAsync(async () =>
            {
                Messages.Clear();
                Context.Clear();

                if (value != null)
                {
                    RecordsCount = "Проверка...";
                    await GetCarsCountFromNhtsa(value);
                }

                if (value != null && value.Messages != null && value.Messages.Count != 0)
                {
                    await LoadContext(value.Messages);
                    IsHistoryNotExist = false;
                }
                else
                {
                    IsHistoryNotExist = true;
                }

            });
        }

        [RelayCommand]
        private void SelectCar()
        {
            SelectCarDialogModel dialogResult =
                (SelectCarDialogModel)DialogManager.ShowDialog<SelectCarDialogViewModel>(new SelectCarDialogModel()
                {
                    Cars = AvailableCars.ToList(),
                    CurrentCar = SelectedCar
                });

            if (dialogResult.DialogResult)
            {
                SelectedCar = dialogResult.CurrentCar;
            }
        }

        [RelayCommand]
        private async Task Send()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (SelectedCar == null)
                    throw new InvalidOperationException("Вы не выбрали машину для диагностики!");

                if (SelectedCar.Messages == null)
                    SelectedCar.Messages = new();

                if (string.IsNullOrWhiteSpace(InputText))
                    throw new InvalidOperationException("Поле запроса пустое!");

                AIModels.TryGetValue(SelectedModel, out string? currentModel);

                if (string.IsNullOrWhiteSpace(currentModel))
                    throw new InvalidOperationException($"Модели {SelectedModel} нет!");

                if (!SelectedCar.Messages.Any(m => m.IsCurrentCarInfoString))
                    await AddCarInfoInContext();

                if (UseNhtsa)
                {
                    if (!SelectedCar.Messages.Any(m => m.IsNhtsaString))
                        await UseNhtsaApi();
                }
                else
                {
                    if (SelectedCar.Messages.Any(m => m.IsNhtsaString))
                        DeleteNhtsaStrings();
                }

                MessageDto userMessage = new MessageDto()
                {
                    Message = InputText,
                    IsUser = true,
                };

                Messages.Add(userMessage);
                Context.Add(CreateMessage(true, InputText));

                GeminiRequest request = new GeminiRequest()
                {
                    Contents = Context.ToList(),
                    SystemInstruction = new SystemInstruction()
                    {
                        Parts = new List<Part>()
                        {
                            new Part()
                            {
                                Text = _aiInstraction
                            }
                        }
                    }
                };

                InputText = string.Empty;

                GeminiResponse geminiResponse = await _apiService.GeminiPost(request, currentModel, _apiKeys[0]);

                string geminiAnswerSupposedJson = geminiResponse.Candidates[0].Content.Parts[0].Text;

                GeminiMalfunctionResponse geminiDiagnos = JsonSerializer.Deserialize<GeminiMalfunctionResponse>(geminiAnswerSupposedJson)
                    ?? throw new InvalidOperationException("Нейросеть прислала пустой объект!");

                MessageDto? modelMessage = null;

                if (geminiDiagnos.NeedClarification && geminiDiagnos.AdditionalQuestion != null)
                {
                    modelMessage = new MessageDto()
                    {
                        Message = geminiDiagnos.AdditionalQuestion,
                        IsUser = false,
                        ModelName = SelectedModel
                    };

                    Context.Add(CreateMessage(false, geminiDiagnos.AdditionalQuestion));
                }
                else
                {
                    string geminiDiagnosJson = JsonSerializer.Serialize(geminiDiagnos);

                    modelMessage = new MessageDto()
                    {
                        Message = geminiDiagnosJson,
                        IsUser = false,
                        ModelName = SelectedModel,
                        IsDiagnosString = true
                    };

                    Context.Add(CreateMessage(false, geminiDiagnosJson));
                }

                SelectedCar.Messages.Add(userMessage);
                SelectedCar.Messages.Add(modelMessage);
                Messages.Add(modelMessage);
                IsHistoryNotExist = false;

                await ManageDataService.SaveDataAsync(AvailableCars, _apiKeys);
            });
        }

        [RelayCommand]
        private async Task ShowDiagnoseWindow(MessageDto message)
        {
            await ExecuteSaveAsync(async () =>
            {
                if (SelectedCar == null)
                    throw new InvalidOperationException("Вы не выбрали машину для диагностики!");

                GeminiMalfunctionResponse allMalfunctions = JsonSerializer.Deserialize<GeminiMalfunctionResponse>(message.Message)
                    ?? throw new InvalidOperationException("Данные были пустыми!");

                ListOfMalfunctionsDialogModel model = new()
                {
                    DiagnosInfo = allMalfunctions,
                };

                if (DialogManager.ShowDialog<ListOfMalfunctionsDialogViewModel>(model).DialogResult)
                {
                    if (model.SelectedMalfunction == null)
                        throw new InvalidOperationException("Неисправность не была выбрана!");

                    if (SelectedCar.Malfunctions == null)
                        SelectedCar.Malfunctions = new();

                    SelectedCar.Malfunctions.Add(new MalfunctionModel()
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        Title = model.SelectedMalfunction.Title,
                        Description = model.SelectedMalfunction.MalfunctionDescription,
                        Danger = model.SelectedMalfunction.Danger,
                    });

                    await ManageDataService.SaveDataAsync(AvailableCars, _apiKeys);
                }
            });
        }

        [RelayCommand]
        private void ShowWhatNhtsaIs()
        {
            DialogManager.ShowDialog<SuccessDialogViewModel>(new WarningDialogModel()
            {
                Title = "Справка",
                ButtonContent = "OK",
                WarningContent = "NHTSA - это сторонняя база данных национального управления безопасности дорожного движения США." +
                " Если вы хотите более точно и подробно диагностировать проблему, то лучше поставить галочку напротив этого пункта," +
                " однако галочка бесполезна, если записей о вашей машине в базе нет." +
                " Стоит также учитывать, что использование этого пункта влечёт за собой увеличение потребления токенов ИИ агента" +
                " (тоесть более быстрый расход лимитов разговора с ИИ)!",
            });
        }

        [RelayCommand]
        private async Task ClearChat()
        {
            await ExecuteSaveAsync(async () =>
            {
                if (SelectedCar == null)
                    throw new InvalidOperationException("Вы не выбрали машину!");

                if (SelectedCar.Messages == null)
                    SelectedCar.Messages = new();

                if (SelectedCar.Messages.Count == 0)
                    throw new InvalidOperationException("История пуста!");

                if (DialogManager.ShowDialog<WarningDialogViewModel>(new WarningDialogModel()
                {
                    Title = "Внимание!",
                    ButtonContent = "Подтвердить",
                    WarningContent = "Вы точно желаете очистить историю диалога с ИИ? Контекст вернуть невозможно!"
                }).DialogResult)
                {
                    SelectedCar.Messages.Clear();
                    Messages.Clear();

                    IsHistoryNotExist = true;

                    await ManageDataService.SaveDataAsync(AvailableCars, _apiKeys);
                }
            });
        }

        [RelayCommand]
        private void BackHomePage() => NavigationService.NavigateTo<MenuViewModel>();

        private async Task AddCarInfoInContext()
        {
            if (SelectedCar == null)
                throw new InvalidOperationException("Машина не выбрана!");

            CarJsonDto selectedCarDto = new()
            {
                Model = SelectedCar.Model,
                Make = SelectedCar.Make,
                Year = SelectedCar.Year,
                Vin = SelectedCar.Vin,
                Mileage = SelectedCar.Mileage,
                EngineType = SelectedCar.EngineType,
                AdditionalInfo = SelectedCar.AdditionalInfo,
                MalfunctionList = SelectedCar.Malfunctions,
            };

            string carInfoJson = JsonSerializer.Serialize(selectedCarDto);

            List<MessageDto> carInfo = new List<MessageDto>()
            {
                new MessageDto()
                {
                    Message = $"Вот информация о моей машине в формате json-файла: {carInfoJson}",
                    IsCurrentCarInfoString = true,
                    IsUser = true,
                },
                new MessageDto()
                {
                    Message = "Теперь мне ясно с какой машиной имею дело.",
                    IsCurrentCarInfoString = true,
                    IsUser = false,
                }
            };

            List<Message> carInfoContext = new();

            foreach (MessageDto message in carInfo)
                carInfoContext.Add(CreateMessage(message.IsUser, message.Message));

            SelectedCar.Messages.InsertRange(_carInfoIndex, carInfo);
            Context.InsertRange(_carInfoIndex, carInfoContext);
        }

        private async Task UseNhtsaApi()
        {
            if (SelectedCar == null)
                throw new InvalidOperationException("Машина не выбрана!");

            NhtsaResponse nhtsaDataResponse = await _apiService.NhtsaGet(SelectedCar);

            if (nhtsaDataResponse.Count > 0)
            {
                string nhtsaDataJson = JsonSerializer.Serialize(nhtsaDataResponse);

                List<MessageDto> nhtsaData = new List<MessageDto>()
                {
                    new MessageDto()
                    {
                        Message = $"Вот данные с базы данных сайта NHTSA о поломках и неисправностях автомобиля " +
                        $"других автовладельцев со схожей маркой, моделью и годом выпуска моего автомобиля, " +
                        $"данные предоставлены в json-формате: {nhtsaDataJson}",
                        IsNhtsaString = true,
                        IsUser = true,
                    },
                    new MessageDto()
                    {
                        Message = "Понял, учту это во время диагностики!",
                        IsNhtsaString = true,
                        IsUser = false,
                    }
                };

                List<Message> nhtsaDataContext = new(2);

                foreach (MessageDto message in nhtsaData)
                    nhtsaDataContext.Add(CreateMessage(message.IsUser, message.Message));

                SelectedCar.Messages.InsertRange(_nhtsaDataIndex, nhtsaData);
                Context.InsertRange(_nhtsaDataIndex, nhtsaDataContext);
            }
        }

        private void DeleteNhtsaStrings()
        {
            if (SelectedCar == null)
                throw new InvalidOperationException("Машина не выбрана!");

            if (SelectedCar.Messages.Count > 0)
                Context.RemoveRange(_nhtsaDataIndex, 2);
        }

        private async Task GetCarsCountFromNhtsa(CarModel car)
        {
            await ExecuteSaveAsync(async () =>
            {
                NhtsaResponse nhtsaData = await _apiService.NhtsaGet(car);
                if (nhtsaData.Count == 0)
                    RecordsCount = "Записи не найдены.";
                else
                    RecordsCount = nhtsaData.Count.ToString();
            });
        }

        private async Task LoadContext(List<MessageDto> carHistoryContext)
        {
            foreach (MessageDto message in carHistoryContext)
            {
                Context.Add(CreateMessage(message.IsUser, message.Message));

                if (!message.IsNhtsaString && !message.IsCurrentCarInfoString)
                    Messages.Add(message);
            }
        }

        private Message CreateMessage(bool isUser, string message)
        {
            return new Message()
            {
                Role = isUser ? "user" : "model",
                Parts = new List<Part>
                    {
                        new Part()
                        {
                            Text = message
                        }
                    }
            };
        }
    }
}
