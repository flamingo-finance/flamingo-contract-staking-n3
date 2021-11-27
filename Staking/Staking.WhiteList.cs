using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace FLMStaking
{
    partial class FLMStaking
    {
        [Safe]
        public static bool IsInWhiteList(UInt160 asset)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, asset), "IsInWhiteList: invald params");
            return AssetStorage.Get(asset);
        }

        [Safe]
        public static BigInteger GetAssetCount()
        {
            return AssetStorage.Count();
        }

        [Safe]
        public static UInt160[] GetAllAsset()
        {
            BigInteger count =  AssetStorage.Count();
            return AssetStorage.Find(count);
        }

        public static bool AddAsset(UInt160 asset, UInt160 author)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, asset, author), "AddAsset: invald params");
            ExecutionEngine.Assert(Runtime.CheckWitness(author), "AddAsset: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            ExecutionEngine.Assert(IsAuthor(author), "AddAsset: not author-".ToByteArray().Concat(author).ToByteString());
            AssetStorage.Put(asset);
            return true;
        }

        public static bool RemoveAsset(UInt160 asset, UInt160 author)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(author), "RemoveAsset: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            ExecutionEngine.Assert(IsAuthor(author), "RemoveAsset: not author-".ToByteArray().Concat(author).ToByteString());
            ExecutionEngine.Assert(IsInWhiteList(asset), "RemoveAsset: not whitelist-".ToByteArray().Concat(asset).ToByteString());
            AssetStorage.Delete(asset);
            return true;
        }
    }
}
