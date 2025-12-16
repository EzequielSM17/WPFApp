using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Interfaces
{
    public interface IDispatcherService
    {
        void Invoke(Action action);
    }
}
