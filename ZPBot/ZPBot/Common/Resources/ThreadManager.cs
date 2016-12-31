using System.Threading;

namespace ZPBot.Common.Resources
{
    public abstract class ThreadManager
    {
        protected Thread Active;
        protected bool BActive;

        protected ThreadManager()
        {
            BActive = false;
        }

        ~ThreadManager()
        {
            Stop();
        }

        public void Start()
        {
            if (!BActive)
            {
                BActive = true;
                Active = new Thread(MyThread);
                Active.Start();
            }
        }
        public void Stop()
        {
            if (BActive)
            {
                BActive = false;
                Active.Abort();
            }
        }

        protected abstract void MyThread();
    }
}
