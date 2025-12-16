using DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Interfaces
{
    public interface INavigationService
    {
        void NavigateToLogin();
        void NavigateToRegister();
        void NavigateToMain();
        void ShowMessage(string message, string title = "Info");
        void ShowError(string message, string title = "Error");
        bool? ShowEditGameDialog(GameDTOWithId game, bool isNew);
    }
}
