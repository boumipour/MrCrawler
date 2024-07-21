using System.Collections.Concurrent;

namespace Crawler.Core.Schedulers
{
    public class FIFO_QueueLinkScheduler : ILinkScheduler
    {
        private ConcurrentQueue<Link> _queue;
        private ConcurrentDictionary<string, bool> _scheduledUris;
        private bool disposedValue;

        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        public FIFO_QueueLinkScheduler()
        {
            _queue = new();
            _scheduledUris = new();
        }

        public bool Add(Link link)
        {
            string hashCode = link.GetHashCode().ToString();

            var added = _scheduledUris.TryAdd(hashCode, false);
            if (added)
            {
                _queue.Enqueue(link);
                return true;
            }

            return false;
        }

        public Link Get()
        {
            _queue.TryDequeue(out Link link);
            return link;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    // AnyClass.Dispose()
                }

                _queue.Clear();
                _queue = null;

                _scheduledUris.Clear();
                _scheduledUris = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}