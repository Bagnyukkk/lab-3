using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace Lab3_OOP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windowsLifecycleBuilder =>
                    {
                        windowsLifecycleBuilder.OnWindowCreated(window =>
                        {        
                            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);      
                            appWindow.Closing += async (s, e) => 
                            {
                                e.Cancel = true;
                                bool result = await App.Current.MainPage.DisplayAlert(
                                    "Підтвердження закриття", 
                                    "Чи дійсно ви хочете закрити програму?", 
                                    "Так", 
                                    "Скасувати");

                                if (result)
                                {
                                    App.Current.Quit();
                                }
                            };
                        });
                    });
#endif

                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}