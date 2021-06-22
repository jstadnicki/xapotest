using Common;

namespace XapoWrappers
{
    public interface IXapoQueue
    {
        void Send<T>(T command, string queue);
    }
}