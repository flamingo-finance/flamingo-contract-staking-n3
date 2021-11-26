using System;
using System.ComponentModel;
using Neo;
using Neo.SmartContract.Framework;

namespace flamingo_contract_staking
{
    partial class FLM
    {
        private static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                ExecutionEngine.Assert(false, msg);
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
