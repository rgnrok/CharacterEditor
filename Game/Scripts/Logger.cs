using System;
using UnityEngine;

namespace CharacterEditor
{
    public static class Logger
    {
        static Logger()
        {
#if UNITY_EDITOR
            Debug.logger.logEnabled = true;
#else
            Debug.logger.logEnabled = false;
#endif
        }

        public static void Log(string str)
        {
            Debug.Log(PrepareMessage(str));
        }

        public static void LogWarning(string str)
        {
            Debug.LogWarning(PrepareMessage(str));
        }

        public static void LogError(string str) => 
            Debug.LogError(PrepareMessage(str));

        private static string PrepareMessage(string str)
        {
            var now = DateTime.Now;
            var msg = string.Format("[{1} {2} ({3}ms) {4}]: {0}", str, now.ToShortDateString(), now.ToLongTimeString(),
                now.Millisecond, Time.frameCount);
            return msg;
        }
    }
}