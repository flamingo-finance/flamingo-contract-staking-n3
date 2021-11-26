using System;
using System.ComponentModel;
using Neo;
using Neo.SmartContract.Framework;

namespace FLMStaking
{
    partial class FLMStaking
    {
        /// <summary>
        /// params: message, extend data
        /// </summary>
        [DisplayName("Fault")]
        public static event FaultEvent onFault;
        public delegate void FaultEvent(string message, params object[] paras);

        private static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                onFault(msg);
                ExecutionEngine.Assert(false);
            }
        }

        static bool CheckAddrValid(bool checkZero, params UInt160[] addrs)
        {
            foreach (UInt160 addr in addrs)
            {
                if (!addr.IsValid || (checkZero && addr.IsZero)) return false;
            }
            return true;
        }
    }
}
