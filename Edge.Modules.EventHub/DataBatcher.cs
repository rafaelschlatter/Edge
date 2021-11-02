using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RaaLabs.Edge.Modules.EventHub
{

    public class DataBatcher<T>
    {
        private readonly Channel<T> _data;
        public event OnDataBatched<T> OnDataBatched;
        private readonly int _maxBatchSize;
        private readonly TimeSpan _timeSpan;

        public DataBatcher(TimeSpan timeSpan, int maxBatchSize = int.MaxValue)
        {
            _timeSpan = timeSpan;
            _maxBatchSize = maxBatchSize;
            _data = Channel.CreateUnbounded<T>();

            _ = RunBatcher();
        }

        public async Task Enqueue(T data)
        {
            await _data.Writer.WriteAsync(data);
        }

        private async Task RunBatcher()
        {
            List<T> batch = new();
            while (true)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(_timeSpan);

                try
                {
                    var data = await _data.Reader.ReadAsync(cts.Token);
                    batch.Add(data);

                    if (batch.Count >= _maxBatchSize)
                    {
                        await OnDataBatched(batch);
                        batch = new();
                    }
                }
                catch (OperationCanceledException)
                {
                    if (batch.Count > 0)
                    {
                        await OnDataBatched(batch);
                        batch = new();
                    }
                }

            }
        }
    }

    public delegate Task OnDataBatched<T>(List<T> batch);
}
