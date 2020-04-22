using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Interaction
{
    public class KeyHoldingAction
    {
        private static object syncLock = new object();
        private static int holdingReference = 0;
        
        /// <summary>
        /// Creates an asynchronous tasks which will only invoke the callback if this method was not
        /// called during the defined delay.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="delayMs"></param>
        public static void InvokeAsync(KeyInstance key, Action<KeyInstance> callback, int delayMs)
        {
            Debug.Assert(delayMs >= 0);
            int currentReference = ++holdingReference;
            Task.Run(() =>
            {
                Thread.Sleep(delayMs);
                lock (syncLock)
                {
                    if (currentReference == holdingReference)
                    {
                        callback?.Invoke(key);
                    }
                }
            });
        }
    }
}