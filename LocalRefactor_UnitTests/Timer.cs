using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace HotspotDevelopments.LocalRefactor
{
    public class Timer : IDisposable
    {
        string logMessage;
        Stopwatch timer;
        TextWriter stream;

        public Timer(String logMessage, TextWriter stream = null)
        {
            if (stream == null)
            {
                stream = Console.Out;
            }
            this.stream = stream;
            this.logMessage = logMessage;
            timer = new Stopwatch();
            timer.Start();
        }

        public void Dispose()
        {
            
            if (timer != null)
            {
                timer.Stop();
                stream.WriteLine(logMessage + timer.Elapsed.ToString());
                stream.Flush();
            }
        }
    }
}
