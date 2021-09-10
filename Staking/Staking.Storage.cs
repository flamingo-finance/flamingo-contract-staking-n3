using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        public static class OwnerStorage
        {
            private static readonly byte[] ownerPrefix = new byte[] { 0x03, 0x02 };

            internal static void Put(UInt160 usr)
            {
                StorageMap map = new(Storage.CurrentContext, ownerPrefix);
                map.Put("owner", usr);
            }

            internal static UInt160 Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, ownerPrefix);
                byte[] v = (byte[])map.Get("owner");
                if(v is null)
                {
                    return InitialOwner;
                }
                else if (v.Length != 20)
                {
                    return InitialOwner;
                }
                else
                {
                    return (UInt160)v;
                }
            }

            internal static void Delete()
            {
                StorageMap map = new(Storage.CurrentContext, ownerPrefix);
                map.Delete("owner");
            }
        }

        public static class AuthorStorage
        {
            private static readonly byte[] AuthorPrefix = new byte[] { 0x03, 0x01 };

            internal static void Put(UInt160 usr)
            {
                StorageMap map = new(Storage.CurrentContext, AuthorPrefix);
                map.Put(usr, 1);
            }

            internal static void Delete(UInt160 usr)
            {
                StorageMap map = new(Storage.CurrentContext, AuthorPrefix);
                map.Delete(usr);
            }

            internal static bool Get(UInt160 usr)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                return (BigInteger)map.Get(usr) == 1;
            }

            internal static BigInteger Count()
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                var iterator = authorMap.Find();
                BigInteger count = 0;
                while (iterator.Next())
                {
                    count++;
                }
                return count;
            }

            internal static UInt160[] Find(BigInteger count)
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                var iterator = authorMap.Find(FindOptions.RemovePrefix | FindOptions.KeysOnly);
                UInt160[] addrs = new UInt160[(uint)count];
                uint i = 0;
                while (iterator.Next())
                {
                    addrs[i] = (UInt160)iterator.Value;
                    i++;
                }
                return addrs;
            }
        }

        public static class PauseStorage
        {
            private static readonly byte[] PausePrefix = new byte[] { 0x09, 0x03 };

            internal static void Put(BigInteger ispause)
            {
                StorageMap map = new(Storage.CurrentContext, PausePrefix);
                map.Put("PausePrefix", ispause);
            }

            internal static bool Get()
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, PausePrefix);
                return (BigInteger)authorMap.Get("PausePrefix") == 1;
            }
        }

        public static class PauseStakingStorage
        {
            private static readonly byte[] PauseStakingPrefix = new byte[] { 0x09, 0x01 };

            internal static void Put(BigInteger ispause)
            {
                StorageMap map = new(Storage.CurrentContext, PauseStakingPrefix);
                map.Put("PauseStakingPrefix", ispause);
            }

            internal static bool Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, PauseStakingPrefix);
                return (BigInteger)map.Get("PauseStakingPrefix") == 1;
            }
        }

        public static class PauseRefundStorage
        {
            private static readonly byte[] PauseRefundPrefix = new byte[] { 0x09, 0x02 };

            internal static void Put(BigInteger ispause)
            {
                StorageMap map = new(Storage.CurrentContext, PauseRefundPrefix);
                map.Put("PauseRefundPrefix", ispause);
            }

            internal static bool Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, PauseRefundPrefix);
                return (BigInteger)map.Get("PauseRefundPrefix") == 1;
            }
        }

        public static class FLMAddressStorage
        {
            private static readonly byte[] FLMPrefix = new byte[] { 0x07, 0x01 };

            internal static void Put(UInt160 addr)
            {
                StorageMap map = new(Storage.CurrentContext, FLMPrefix);
                map.Put("FLMPrefix", addr);
            }

            internal static UInt160 Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, FLMPrefix);
                return (UInt160)map.Get("FLMPrefix");
            }
        }

        public static class AssetStorage
        {
            private static readonly byte[] AssetPrefix = new byte[] { 0x04, 0x01 };

            internal static void Put(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentContext, AssetPrefix);
                map.Put(asset, 1);
            }

            internal static void Delete(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentContext, AssetPrefix);
                map.Delete(asset);
            }

            internal static bool Get(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, AssetPrefix);
                return (BigInteger)map.Get(asset) == 1;
            }

            internal static BigInteger Count()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, AssetPrefix);
                var iterator = map.Find();
                BigInteger count = 0;
                while (iterator.Next())
                {
                    count++;
                }
                return count;
            }

            internal static UInt160[] Find(BigInteger count)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, AssetPrefix);
                var iterator = map.Find(FindOptions.RemovePrefix | FindOptions.KeysOnly);
                UInt160[] addrs = new UInt160[(uint)count];
                uint i = 0;
                while (iterator.Next())
                {
                    addrs[i] = (UInt160)iterator.Value;
                    i++;
                }
                return addrs;
            }
        }

        public static class HistoryStackProfitSumStorage
        {
            private static readonly byte[] HistoryUintStackProfitSumPrefix = new byte[] { 0x02, 0x01 };

            internal static void Put(UInt160 asset, BigInteger timestamp, BigInteger amount)
            {
                StorageMap map = new(Storage.CurrentContext, HistoryUintStackProfitSumPrefix);
                byte[] key = ((byte[])asset).Concat(timestamp.ToByteArray());
                map.Put(key, amount);
            }

            internal static BigInteger Get(UInt160 asset, BigInteger timestamp)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, HistoryUintStackProfitSumPrefix);
                byte[] key = ((byte[])asset).Concat(timestamp.ToByteArray());
                return (BigInteger)map.Get(key);
            }
        }

        public static class CurrentRateTimestampStorage
        {
            private static readonly byte[] CurrentRateTimeStampPrefix = new byte[] { 0x01, 0x01 };

            internal static void Put(UInt160 asset, BigInteger timestamp)
            {
                StorageMap map = new(Storage.CurrentContext, CurrentRateTimeStampPrefix);
                map.Put(asset, timestamp);
            }

            internal static BigInteger Get(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, CurrentRateTimeStampPrefix);
                return (BigInteger)map.Get(asset);
            }
        }

        public static class CurrentStackProfitStorage
        {
            private static readonly byte[] CurrentUintStackProfitPrefix = new byte[] { 0x01, 0x02 };

            internal static void Put(UInt160 asset, BigInteger profit)
            {
                StorageMap map = new(Storage.CurrentContext, CurrentUintStackProfitPrefix);
                map.Put(asset, profit);
            }

            internal static BigInteger Get(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, CurrentUintStackProfitPrefix);
                return map.Get(asset) is null ? 0 : (BigInteger)map.Get(asset);
            }
        }

        public static class CurrentShareAmountStorage
        {
            private static readonly byte[] CurrentShareAmountPrefix = new byte[] { 0x06, 0x01 };

            internal static void Put(UInt160 asset, BigInteger amount)
            {
                StorageMap map = new(Storage.CurrentContext, CurrentShareAmountPrefix);
                map.Put(asset, amount);
            }

            internal static BigInteger Get(UInt160 asset)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, CurrentShareAmountPrefix);
                return (BigInteger)map.Get(asset);
            }
        }

        public static class UpgradeTimeLockStorage
        {
            private static readonly byte[] UpgradeTimelockPrefix = new byte[] { 0x08, 0x01 };

            internal static void Put(BigInteger timestamp)
            {
                StorageMap map = new(Storage.CurrentContext, UpgradeTimelockPrefix);
                map.Put("UpgradeTimelockPrefix", timestamp);
            }

            internal static BigInteger Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, UpgradeTimelockPrefix);
                return (BigInteger)map.Get("UpgradeTimelockPrefix");
            }
        }

        public static class UserStakingStorage
        {
            private static readonly byte[] UserStakingPrefix = new byte[] { 0xa0,0x01};

            internal static void Put(UInt160 fromAddress, BigInteger amount, UInt160 asset, BigInteger timestamp, BigInteger profit)
            {
                byte[] key = ((byte[])asset).Concat((byte[])fromAddress);
                StorageMap map = new(Storage.CurrentContext, UserStakingPrefix);
                StakingReocrd record = new StakingReocrd
                {
                    timeStamp = timestamp,
                    fromAddress = fromAddress,
                    amount = amount,
                    assetId = asset,
                    Profit = profit
                };
                map.Put(key, StdLib.Serialize(record));
            }

            internal static StakingReocrd Get(UInt160 fromAddress, UInt160 asset)
            {
                byte[] key = ((byte[])asset).Concat((byte[])fromAddress);
                StorageMap map = new(Storage.CurrentReadOnlyContext, UserStakingPrefix);
                var r = map.Get(key);
                if(r is null)
                {
                    return new StakingReocrd
                    {
                        timeStamp = 0,
                        fromAddress = UInt160.Zero,
                        amount = 0,
                        assetId = UInt160.Zero,
                        Profit = 0
                    };
                }
                return (StakingReocrd)StdLib.Deserialize(r);
            }
        }
    }
}
