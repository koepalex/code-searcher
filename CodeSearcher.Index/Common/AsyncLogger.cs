using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeSearcher.BusinessLogic.Common
{
    public static class AsyncLogger
    {
        private static BlockingCollection<string> m_Messages;
        static AsyncLogger()
        {
            m_Messages = new BlockingCollection<string>(1000);

            ThreadPool.QueueUserWorkItem((state) =>
            {
                string message;
                while (true)
                {

                    if (m_Messages.TryTake(out message))
                    {
                        Console.WriteLine(message);
                    }
                }  
            });
        }

        public static void WriteLine(String message)
        {
            m_Messages.Add(message);
        }
    }
}
