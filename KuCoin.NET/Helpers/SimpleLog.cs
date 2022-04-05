using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace KuCoin.NET.Helpers
{

    public class SimpleLog : IDisposable
    {
        private object _lock = new object();

        public virtual bool IsOpened { get; protected set; }

        public virtual FileStream Stream { get; protected set; }

        public virtual string Filename { get; set; }

        public SimpleLog()
        {

        }
        public SimpleLog(string fileName, bool open = true)
        {
            Filename = fileName;
            if (open) OpenLog();
        }


        bool disposed = false;

        ~SimpleLog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (disposed) throw new ObjectDisposedException(nameof(SimpleLog));
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposed) throw new ObjectDisposedException(nameof(SimpleLog));

            try
            {
                Stream?.Close();
            }
            catch
            {

            }

            disposed = true;
            if (disposing) GC.SuppressFinalize(this);
        }

        public virtual void OpenLog(string fileName = null)
        {
            lock (_lock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(SimpleLog));

                if (fileName != null)
                {
                    Filename = fileName;
                }
                else if (Filename == null)
                {
                    throw new ArgumentNullException(nameof(fileName), "Must specify filename if property is not set.");
                }

                Stream = new FileStream(Filename, FileMode.Append, FileAccess.Write, FileShare.Read);
                IsOpened = true;
            }
        }

        public virtual void Close()
        {
            lock (_lock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(SimpleLog));

                Stream?.Close();
                Stream = null;
                IsOpened = false;
            }
        }

        public virtual void Log(string message)
        {
            lock (_lock)
            {
                if (disposed) throw new ObjectDisposedException(nameof(SimpleLog));

                try
                {
                    if (!IsOpened) return;
                    var data = Encoding.UTF8.GetBytes($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF")}]: {message}\r\n");
                    Stream.Write(data, 0, data.Length);
                    Stream.Flush();
                }
                catch
                {

                }
            }
        }

    }
}
