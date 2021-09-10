using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        private static bool MintFLM(UInt160 receiver, BigInteger amount, UInt160 callingScript)
        {
            object[] @params = new object[]
            {
                callingScript,
                receiver,
                amount
            };
            UInt160 flmAddress = FLMAddressStorage.Get();
            return (bool)Contract.Call(flmAddress, "mint", CallFlags.All, @params);
        }

        public static bool SetFLMAddress(UInt160 flm, UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "SetFLMAddress: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "SetFLMAddress: not author-".ToByteArray().Concat(author).ToByteString());
            Assert(flm.IsValid, "SetFLMAddress: address valid-".ToByteArray().Concat(flm).ToByteString());
            FLMAddressStorage.Put(flm);
            return true;
        }

        [Safe]
        public static UInt160 GetFLMAddress()
        {
            return FLMAddressStorage.Get();
        }
    }
}
