using System.Text.Json;
using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.GisModels
{
    public class GisResponseBody
    {
        [JsonPropertyName("result")]
        public Result Result { get; set; } = new();
    }

    public class Result
    {
        [JsonPropertyName("items")]
        public List<CarServiceInfo> Items { get; set; } = new();
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class CarServiceInfo
    {
        [JsonPropertyName("address_name")]
        public string AddressName { get; set; } = string.Empty;
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("purpose_name")]
        public string PurposeName { get; set; } = string.Empty;
        [JsonPropertyName("point")]
        public Point Point { get; set; } = new();
        [JsonPropertyName("rubrics")]
        public List<Rubric> Rubrics { get; set; } = new();
        [JsonPropertyName("schedule")]
        public JsonElement? Schedule { get; set; }

        public Dictionary<string, DaySchedule> GetParsedSchedule()
        {
            var result = new Dictionary<string, DaySchedule>();
            var validDays = new HashSet<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            if (Schedule == null || Schedule.Value.ValueKind != JsonValueKind.Object)
                return result;

            foreach (var property in Schedule.Value.EnumerateObject())
            {
                if (!validDays.Contains(property.Name))
                    continue;

                try
                {
                    var daySchedule = JsonSerializer.Deserialize<DaySchedule>(property.Value.GetRawText());
                    if (daySchedule != null)
                        result[property.Name] = daySchedule;
                }
                catch
                { 
                }
            }

            return result;
        }
    }

    public class Point
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    public class Rubric
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; } = string.Empty;
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class DaySchedule
    {
        [JsonPropertyName("working_hours")]
        public List<WorkingHours> WorkingHours { get; set; } = new();
    }

    public class WorkingHours
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;
    }

    
    }
