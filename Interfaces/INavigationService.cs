using GearFix.Models;
using GearFix.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearFix.Interfaces
{
    public interface INavigationService
    {
        public event Action UpdatedViewModel;
        public BaseViewModel CurrentViewModel { get; set; }
        public Task NavigateTo<TViewModel>() where TViewModel : BaseViewModel;
    }
}
