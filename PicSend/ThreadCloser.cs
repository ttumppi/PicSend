using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSend
{
    public sealed class ThreadCloser
    {
        List<IShutdown> _shutdownObjects;

        public ThreadCloser()
        {
            _shutdownObjects = new List<IShutdown>();
        }

        public void Add(IShutdown iShutdown)
        {
            _shutdownObjects.Add(iShutdown);
        }


        /// <summary>
        /// Starts shutting down shutdownable objects and invokes shutdown done delegate when done.
        /// </summary>
        /// <returns></returns>
        public void CloseAll(Action shutdownDone)
        {
            Task.Run(new Action(() => {
                shutdownObjects(shutdownDone);
                }));
        }

        private void shutdownObjects(Action shutdownDone)
        {
            foreach (IShutdown iShutdown in _shutdownObjects)
            {
                iShutdown.Shutdown();
            }

            shutdownDone();
        }

    }
}
