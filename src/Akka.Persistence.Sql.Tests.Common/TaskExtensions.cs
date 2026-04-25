using System;
using System.Threading.Tasks;

namespace Akka.TestKit.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> ShouldCompleteWithin<T>(this Task<T> task, TimeSpan timeout)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeout)))
                throw new TimeoutException($"Task did not complete within {timeout}.");
            return await task;
        }

        public static async Task ShouldCompleteWithin(this Task task, TimeSpan timeout)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeout)))
                throw new TimeoutException($"Task did not complete within {timeout}.");
            await task;
        }
    }
}
