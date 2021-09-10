using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using System.Numerics;

namespace FLMStaking
{
    partial class FLMStaking
    {
        [Safe]
        public static bool IsInWhiteList(UInt160 asset)
        {
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
            Assert(Runtime.CheckWitness(author), "AddAsset: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "AddAsset: not author-".ToByteArray().Concat(author).ToByteString());
            AssetStorage.Put(asset);
            return true;
        }

        public static bool RemoveAsset(UInt160 asset, UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "RemoveAsset: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "RemoveAsset: not author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsInWhiteList(asset), "RemoveAsset: not whitelist-".ToByteArray().Concat(asset).ToByteString());
            AssetStorage.Delete(asset);
            return true;
        }
    }
}
