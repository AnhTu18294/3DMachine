using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Lifetime.Clear;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace CT3DMachine.Notifications
{
    class NotificationManager
    {
        private Notifier mNotifier;

        public NotificationManager()
        {
            mNotifier = CreateNotifier(Corner.TopLeft, PositionProviderType.Window, NotificationLifetimeType.TimeBased);
            Application.Current.MainWindow.Closing += MainWindowOnClosing;
        }
        public bool? FreezeOnMouseEnter { get; set; } = true;
        public bool? ShowCloseButton { get; set; } = false;

        public bool? TopMost { get; set; } = true;
        
        internal void ShowWarning(string message)
        {
            mNotifier.ShowWarning(message, CreateOptions());
        }

        internal void ShowSuccess(string message)
        {
            mNotifier.ShowSuccess(message, CreateOptions());
        }

        public void ShowInformation(string message)
        {
            mNotifier.ShowInformation(message, CreateOptions());
        }

        public void ShowError(string message)
        {
            mNotifier.ShowError(message, CreateOptions());
        }

        public void clearAll()
        {
            mNotifier.ClearMessages(new ClearAll());
        }

        public void close()
        {
            mNotifier.Dispose();
        }

        private Notifier CreateNotifier(Corner corner, PositionProviderType relation, NotificationLifetimeType lifetime)
        {
            mNotifier?.Dispose();
            mNotifier = null;

            return new Notifier(cfg =>
            {
                cfg.PositionProvider = CreatePositionProvider(corner, relation);
                cfg.LifetimeSupervisor = CreateLifetimeSupervisor(lifetime);
                cfg.Dispatcher = Dispatcher.CurrentDispatcher;
                cfg.DisplayOptions.TopMost = TopMost.GetValueOrDefault();
            });
        }
                
        private static IPositionProvider CreatePositionProvider(Corner corner, PositionProviderType relation)
        {
            switch (relation)
            {
                case PositionProviderType.Window:
                {
                    return new WindowPositionProvider(Application.Current.MainWindow, corner, 5, 5);
                }
                case PositionProviderType.Screen:
                {
                    return new PrimaryScreenPositionProvider(corner, 5, 5);
                }
                case PositionProviderType.Control:
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    var glView3D = mainWindow?.glView3D;
                    return new ControlPositionProvider(mainWindow, glView3D, corner, 5, 5);
                }
            }

            throw new InvalidEnumArgumentException();
        }

        private static INotificationsLifetimeSupervisor CreateLifetimeSupervisor(NotificationLifetimeType lifetime)
        {
            if (lifetime == NotificationLifetimeType.Basic)
                return new CountBasedLifetimeSupervisor(MaximumNotificationCount.FromCount(5));

            return new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(3),
                MaximumNotificationCount.FromCount(10));
        }

        private MessageOptions CreateOptions()
        {
            return new MessageOptions
            {
                FreezeOnMouseEnter = FreezeOnMouseEnter.GetValueOrDefault(),
                ShowCloseButton = ShowCloseButton.GetValueOrDefault()
            };
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            mNotifier.Dispose();
        }
    }
}
