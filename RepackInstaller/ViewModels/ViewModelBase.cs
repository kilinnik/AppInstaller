using System;
using ReactiveUI;

namespace RepackInstaller.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel
{
    // Реализация активатора для поддержки активации и деактивации ViewModel
    public ViewModelActivator Activator { get; } = new();

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    // Эвент для обработки ошибок
    public event EventHandler<Exception>? ExceptionOccurred;

    // Метод для обработки исключений, которые могут возникнуть в ViewModel
    protected void HandleException(Exception exception)
    {
        ErrorMessage = exception.Message;
        ExceptionOccurred?.Invoke(this, exception);
    }
}