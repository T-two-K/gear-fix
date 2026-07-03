using GearFix.Models.ApiModels.GisModels;
using System.Windows;

namespace GearFix.Interfaces
{
    public interface IMapService
    {
        public Task ShowUserLocation(double lat, double lon, int radiusKm);
        public Task DrawRaiusCircle(double lat, double lon, int radiusKm);
        public Task PlaceCarServices(ICollection<CarServiceInfo> services);

        public Task ExecuteGettingUserLocation(int radius);

        public Task ClearAllMap();
    }
}
