using System;
using System.Threading;
using System.Threading.Tasks;

namespace StorageServices
{
    public static class Retry
    {

        public async static Task ExecuteAsync(Action retryOperation)
        {
            await ExecuteAsync(retryOperation, TimeSpan.FromMilliseconds(2000), 3);

        }

        public async static Task ExecuteAsync(Action retryOperation, TimeSpan deltaBackoff, int maxRetries)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);

            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    await Task.Run(retryOperation);
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(delayMilliseconds);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

        public static void Execute(Action retryOperation)
        {
            Execute(retryOperation, TimeSpan.FromMilliseconds(2000), 3);
        }

        public static void Execute(Action retryOperation, TimeSpan deltaBackoff, int maxRetries)
        {
            int delayMilliseconds = Convert.ToInt32(deltaBackoff.TotalMilliseconds);
            if (maxRetries < 1)
            {
                throw new ArgumentOutOfRangeException("Retry maxRetries must be >= 1.");
            }

            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    retryOperation();
                    return;
                }
                catch
                {
                    if (attempt == maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(delayMilliseconds);
                    attempt++;
                }
            }

            throw new OperationCanceledException("Operation cancelled due to retry failure.");
        }

    }
}
