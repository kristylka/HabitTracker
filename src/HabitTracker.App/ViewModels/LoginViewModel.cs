namespace HabitTracker.App.ViewModels;

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HabitTracker.Core.Interfaces;
using HabitTracker.Core.Messages;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(IAuthService authService, IMessenger messenger)
    {
        _authService = authService;
        _messenger = messenger;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            var (success, error, user) = await _authService.LoginAsync(Username, Password);

            if (success && user != null)
            {
                _messenger.Send(new UserLoggedInMessage(user));
            }
            else
            {
                ErrorMessage = error;
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void GoToRegister()
    {
        _messenger.Send(new NavigationMessage("Register"));
    }
}