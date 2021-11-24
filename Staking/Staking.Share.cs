using System.Numerics;
using Neo;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        public static bool SetCurrentShareAmount(UInt160 assetId, BigInteger amount, UInt160 adminAddress)
        {
            Assert(CheckAddrVaild(assetId, adminAddress), "SetCurrentShareAmount: invald params");
            if (IsInWhiteList(assetId) && IsAuthor(adminAddress) && Runtime.CheckWitness(adminAddress))
            {
                if (amount >= 0)
                {
                    CurrentShareAmountStorage.Put(assetId, amount);
                    UpdateStackRecord(assetId, GetCurrentTimestamp());
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static BigInteger GetCurrentShareAmount(UInt160 assetId)
        {
            Assert(CheckAddrVaild(assetId), "GetCurrentShareAmount: invald params");
            if (IsInWhiteList(assetId))
            {
                return CurrentShareAmountStorage.Get(assetId);
            }
            return 0;
        }
    }
}
