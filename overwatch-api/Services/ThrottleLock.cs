using System.Threading;

namespace overwatch_api.Services
{
    public class ThrottleLock<T>
    {
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
    }
}
