using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AcrylicKeyboard.Layout;

namespace AcrylicKeyboard.Interaction
{
    public class KeyHoldingAction
    {
        private static readonly object syncLock = new object();
        private static int holdingReference;

        /// <summary>
        ///     Creates an asynchronous tasks which will only invoke the callback if this method was not
        ///     called during the defined delay.
        /// </summary>
        /// <param name="key">The key object.</param>
        /// <param name="callback">The callback which will be called after the delay.</param>
        /// <param name="delayMs">The delay in milliseconds.</param>
        public static void InvokeDebounceAsync(KeyInstance key, Action<KeyInstance> callback, int delayMs)
        {
            Debug.Assert(delayMs >= 0);
            var currentReference = ++holdingReference;
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