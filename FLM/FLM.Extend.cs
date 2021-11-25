using System;
using Neo;

namespace flamingo_contract_staking
{
    partial class FLM
    {
        private static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                throw new InvalidOperationException(msg);
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
