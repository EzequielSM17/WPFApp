using Service.Interfaces;

using System.Windows;

namespace Service
{
    public class DispacherService : IDispatcherService
    {
        public DispacherService()
        {
        }

        public void Invoke(Action action)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                action();
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(action);
            }
        }
    }
}
