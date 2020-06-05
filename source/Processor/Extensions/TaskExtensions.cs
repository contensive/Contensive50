using System.Threading.Tasks;

namespace Contensive.Processor.Extensions
{
    internal static class TaskExtensions
    {
        public static T WaitSynchronously<T>(this Task<T> task)
        {
            return Task.Run(async () => await task).Result;
        }

        public static void WaitSynchronously(this Task task)
        {
            Task.Run(async () => await task).Wait();
        }
    }
}
