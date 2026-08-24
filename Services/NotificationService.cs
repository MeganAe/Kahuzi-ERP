using System;
using MaterialDesignThemes.Wpf;

namespace GestionCommerciale.Services;

public static class NotificationService
{
    public static ISnackbarMessageQueue? MessageQueue { get; set; }

    public static void AfficherMessage(string message, string? actionContent = null, Action? actionHandler = null)
    {
        if (MessageQueue is null) return;

        if (actionContent is not null && actionHandler is not null)
        {
            MessageQueue.Enqueue(message, actionContent, actionHandler);
        }
        else
        {
            MessageQueue.Enqueue(message);
        }
    }
}
