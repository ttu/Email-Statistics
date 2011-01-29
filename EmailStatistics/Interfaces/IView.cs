using System;

namespace EmailStatistics
{
    public interface IView
    {
        event EventHandler Closed;

        void SetController(IController controller);
        void SetModel(IModel model);
        void ShowView(); 
        void SetConnectionOK();
        void SetConnectionError(string errorMessage);
        void SetDisconnectOK();
        void SetDisconnectError(string errorMessage);
        void DataReady();
    }
}
