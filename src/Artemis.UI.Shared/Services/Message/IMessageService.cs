﻿using System;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     Providers messaging functionality
    /// </summary>
    public interface IMessageService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets the main snackbar message queue used by <see cref="ShowMessage(object)" /> and its overloads
        /// </summary>
        ISnackbarMessageQueue MainMessageQueue { get; }

        /// <summary>
        ///     Sets up the notification provider that shows desktop notifications
        /// </summary>
        /// <param name="notificationProvider">The notification provider that shows desktop notifications</param>
        void ConfigureNotificationProvider(INotificationProvider notificationProvider);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        void ShowMessage(object content);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        void ShowMessage(object content, object actionContent, Action actionHandler);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        /// <param name="actionArgument">Argument to pass to <paramref name="actionHandler" />.</param>
        void ShowMessage<TArgument>(
            object content,
            object actionContent,
            Action<TArgument> actionHandler,
            TArgument actionArgument);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="neverConsiderToBeDuplicate">
        ///     Subsequent, duplicate messages queued within a short time span will
        ///     be discarded. To override this behaviour and ensure the message always gets displayed set to <c>true</c>.
        /// </param>
        void ShowMessage(object content, bool neverConsiderToBeDuplicate);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        /// <param name="promote">The message will promoted to the front of the queue.</param>
        void ShowMessage(object content, object actionContent, Action actionHandler, bool promote);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        /// <param name="actionArgument">Argument to pass to <paramref name="actionHandler" />.</param>
        /// <param name="promote">The message will be promoted to the front of the queue and never considered to be a duplicate.</param>
        void ShowMessage<TArgument>(
            object content,
            object actionContent,
            Action<TArgument> actionHandler,
            TArgument actionArgument,
            bool promote);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        /// <param name="actionArgument">Argument to pass to <paramref name="actionHandler" />.</param>
        /// <param name="promote">The message will be promoted to the front of the queue.</param>
        /// <param name="neverConsiderToBeDuplicate">The message will never be considered a duplicate.</param>
        /// <param name="durationOverride">Message show duration override.</param>
        void ShowMessage<TArgument>(
            object content,
            object actionContent,
            Action<TArgument> actionHandler,
            TArgument actionArgument,
            bool promote,
            bool neverConsiderToBeDuplicate,
            TimeSpan? durationOverride = null);

        /// <summary>
        ///     Queues a notification message for display in a snackbar.
        /// </summary>
        /// <param name="content">Message.</param>
        /// <param name="actionContent">Content for the action button.</param>
        /// <param name="actionHandler">Call back to be executed if user clicks the action button.</param>
        /// <param name="actionArgument">Argument to pass to <paramref name="actionHandler" />.</param>
        /// <param name="promote">The message will promoted to the front of the queue.</param>
        /// <param name="neverConsiderToBeDuplicate">The message will never be considered a duplicate.</param>
        /// <param name="durationOverride">Message show duration override.</param>
        void ShowMessage(
            object content,
            object actionContent,
            Action<object> actionHandler,
            object actionArgument,
            bool promote,
            bool neverConsiderToBeDuplicate,
            TimeSpan? durationOverride = null);

        /// <summary>
        ///     Shows a desktop notification
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">The message of the notification</param>
        void ShowNotification(string title, string message);

        /// <summary>
        ///     Shows a desktop notification with a Material Design icon
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">The message of the notification</param>
        /// <param name="icon">The name of the icon</param>
        void ShowNotification(string title, string message, PackIconKind icon);

        /// <summary>
        ///     Shows a desktop notification with a Material Design icon
        /// </summary>
        /// <param name="title">The title of the notification</param>
        /// <param name="message">The message of the notification</param>
        /// <param name="icon">The name of the icon as a string</param>
        void ShowNotification(string title, string message, string icon);
    }
}