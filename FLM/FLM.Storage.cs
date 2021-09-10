using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace flamingo_contract_staking
{
    partial class FLM
    {
        public static class TotalSupplyStorage
        {
            private static readonly byte[] TotalSupplyPrefix = new byte[] { 0x01, 0x00 };

            private static readonly byte[] TotalSupplyKey = "totalSupply".ToByteArray();

            internal static void Put(BigInteger amount)
            {
                StorageMap balanceMap = new(Storage.CurrentContext, TotalSupplyPrefix);
                balanceMap.Put(TotalSupplyKey, amount);
            }

            internal static BigInteger Get()
            {
                StorageMap balanceMap = new(Storage.CurrentReadOnlyContext, TotalSupplyPrefix);
                return (BigInteger)balanceMap.Get(TotalSupplyKey);
            }

            internal static void Increase(BigInteger amount) => Put(Get() + amount);

            internal static void Reduce(BigInteger amount) => Put(Get() - amount);
        }

        public static class BalanceStorage
        {
            private static readonly byte[] BalancePrefix = new byte[] { 0x01, 0x01 };

            internal static void Put(UInt160 usr, BigInteger amount)
            {
                StorageMap balanceMap = new(Storage.CurrentContext, BalancePrefix);
                balanceMap.Put(usr, amount);
            }

            internal static BigInteger Get(UInt160 usr)
            {
                StorageMap balanceMap = new(Storage.CurrentReadOnlyContext, BalancePrefix);
                return (BigInteger)balanceMap.Get(usr);
            }

            internal static void Delete(UInt160 usr)
            {
                StorageMap balanceMap = new(Storage.CurrentContext, BalancePrefix);
                balanceMap.Delete(usr);
            }

            internal static void Increase(UInt160 usr, BigInteger amount) => Put(usr, Get(usr) + amount);

            internal static bool Reduce(UInt160 usr, BigInteger amount)
            {
                BigInteger balance = Get(usr);
                if (balance < amount)
                {
                    return false;
                }
                else if (balance == amount)
                {
                    Delete(usr);
                }
                else
                {
                    Put(usr, balance - amount);
                }
                return true;
            }
        }

        public static class AllowanceStorage
        {
            private static readonly byte[] AllowancePrefix = new byte[] { 0x01, 0x02 };

            internal static void Put(UInt160 usr, UInt160 spender, BigInteger amount)
            {
                StorageMap allowanceMap = new(Storage.CurrentContext, AllowancePrefix);
                allowanceMap.Put(usr + spender, amount);
            }

            internal static BigInteger Get(UInt160 usr, UInt160 spender)
            {
                StorageMap allowanceMap = new(Storage.CurrentReadOnlyContext, AllowancePrefix);
                return (BigInteger)allowanceMap.Get(usr + spender);
            }

            internal static void Delete(UInt160 usr, UInt160 spender)
            {
                StorageMap allowanceMap = new(Storage.CurrentContext, AllowancePrefix);
                allowanceMap.Delete(usr + spender);
            }

            internal static bool Reduce(UInt160 usr, UInt160 spender, BigInteger delta)
            {
                BigInteger allowance = Get(usr, spender);
                if (allowance < delta)
                {
                    return false;
                }
                else if (allowance == delta)
                {
                    Delete(usr, spender);
                }
                else
                {
                    Put(usr, spender, allowance - delta);
                }
                return true;
            }

        }

        public static class AuthorStorage
        {
            private static readonly byte[] AuthorPrefix = new byte[] { 0x01, 0x03 };

            internal static void Put(UInt160 usr)
            {
                StorageMap authorMap = new(Storage.CurrentContext, AuthorPrefix);
                authorMap.Put(usr, 1);
            }

            internal static void Delete(UInt160 usr)
            {
                StorageMap authorMap = new(Storage.CurrentContext, AuthorPrefix);
                authorMap.Delete(usr);
            }

            internal static bool Get(UInt160 usr)
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                return (BigInteger)authorMap.Get(usr) == 1;
            }

            internal static BigInteger Count()
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                var iterator = authorMap.Find();
                BigInteger count = 0;
                while (iterator.Next())
                {
                    count ++;
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

            internal static Iterator Find()
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, AuthorPrefix);
                return authorMap.Find();
            }
        }

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
                if (v is null)
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

    }
}
