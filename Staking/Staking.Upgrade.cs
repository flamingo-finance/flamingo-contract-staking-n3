using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        public static bool UpgradeStart()
        {
            if (!Runtime.CheckWitness(GetOwner())) return false;
            var t = UpgradeTimeLockStorage.Get();
            if (t != 0) return false;
            UpgradeTimeLockStorage.Put(GetCurrentTimestamp() + 86400);
            return true;
        }

        public static void Update(ByteString nefFile, string manifest, object data)
        {
            Assert(Runtime.CheckWitness(GetOwner()), "SetOwner: CheckWitness failed, owner-".ToByteArray().Concat(GetOwner()).ToByteString());
            ContractManagement.Update(nefFile, manifest, data);
            UpgradeEnd();
        }

        private static void UpgradeEnd()
        {
            var t = UpgradeTimeLockStorage.Get();
            Assert(GetCurrentTimestamp()> t && t != 0, "UpgradeEnd: timelock wrong, t-".ToByteArray().Concat(t.ToByteArray()).ToByteString());
            UpgradeTimeLockStorage.Put(0);
        }
    }
}
