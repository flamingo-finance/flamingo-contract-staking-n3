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

        private static bool CheckAddrVaild(params UInt160[] addrs)
        {
            bool vaild = true;

            foreach (UInt160 addr in addrs)
            {
                vaild = vaild && addr is not null && addr.IsValid;
                if (!vaild)
                    break;
            }

            return vaild;
        }
    }
}
