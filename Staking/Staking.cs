using System;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    struct StakingReocrd
    {
        public BigInteger timeStamp;
        public UInt160 fromAddress;
        public BigInteger amount;
        public UInt160 assetId;
        public BigInteger Profit;
    }

    [ManifestExtra("Author", "")]
    [ManifestExtra("Email", "")]
    [ManifestExtra("Description", "")]
    [ContractPermission("*", "*")]
    public partial class FLMStaking : SmartContract 
    {
        private static readonly uint StartStakingTimeStamp = 1601114400;
        private static readonly uint StartClaimTimeStamp = 1601269200;


        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            UInt160 asset = Runtime.CallingScriptHash;
            ExecutionEngine.Assert(!IsStakingPaused(), "OnNEP17Payment: IsStakingPaused");
            ExecutionEngine.Assert(IsInWhiteList(asset) && CheckAddrValid(true, from, asset) && !CheckWhetherSelf(from) && amount > 0, "OnNEP17Payment: invald params");
            BigInteger currentTimeStamp = GetCurrentTimestamp();
            ExecutionEngine.Assert(CheckIfStakingStart(currentTimeStamp), "OnNEP17Payment: Timeout");
            BigInteger currentProfit = 0;

            UpdateStackRecord(asset, currentTimeStamp);

            StakingReocrd stakingRecord = UserStakingStorage.Get(from, asset);

            if (stakingRecord.assetId != UInt160.Zero && stakingRecord.fromAddress != UInt160.Zero)
            {
                currentProfit = SettleProfit(stakingRecord.timeStamp, stakingRecord.amount, asset) + stakingRecord.Profit;
                amount += stakingRecord.amount;
            }
            UserStakingStorage.Put(from, amount, asset, currentTimeStamp, currentProfit);
        }

        [Safe]
        public static object GetUintProfit(UInt160 assetId)
        {
            if (!CheckAddrValid(true, assetId) || !IsInWhiteList(assetId))
            {
                return 0;
            }
            return GetCurrentUintStackProfit(assetId);
        }

        public static bool Refund(UInt160 fromAddress, BigInteger amount, UInt160 asset)
        {
            Transaction tran = (Transaction)Runtime.ScriptContainer;
            //检查是否存在reentered的情况
            ExecutionEngine.Assert(!EnteredStorage.IsSet(tran.Hash), "Re-entered");
            EnteredStorage.Set(tran.Hash);

            if (IsRefundPaused() || !Runtime.CheckWitness(fromAddress))
            {
                EnteredStorage.Delete(tran.Hash);
                return false;
            }
            BigInteger currentTimestamp = GetCurrentTimestamp();
            StakingReocrd stakingRecord = UserStakingStorage.Get(fromAddress, asset);
            if (stakingRecord.amount < amount || !(stakingRecord.fromAddress.Equals(fromAddress)) || !(stakingRecord.assetId.Equals(asset)))
            {
                EnteredStorage.Delete(tran.Hash);
                return false;
            }
            //Nep5转账
            object[] @params = new object[]
            {
                Runtime.ExecutingScriptHash,
                fromAddress,
                amount,
                new byte[0]
            };

            try
            {
                var result = (bool)Contract.Call(asset, "transfer", CallFlags.All, @params);
                ExecutionEngine.Assert(result, "Refund: transfer failed, ".ToByteArray().ToByteString());
            }
            catch (Exception)
            {
                ExecutionEngine.Assert(false, "Refund: transfer failed, ".ToByteArray().ToByteString());
            }

            BigInteger remainAmount = (stakingRecord.amount - amount);
            UpdateStackRecord(asset, currentTimestamp);
            //收益结算
            BigInteger currentProfit = SettleProfit(stakingRecord.timeStamp, stakingRecord.amount, asset) + stakingRecord.Profit;
            UserStakingStorage.Put(fromAddress, remainAmount, asset, currentTimestamp, currentProfit);
            EnteredStorage.Delete(tran.Hash);
            return true;
        }

        public static bool ClaimFLM(UInt160 fromAddress, UInt160 asset)
        {
            Transaction tran = (Transaction)Runtime.ScriptContainer;
            //检查是否存在reentered的情况
            ExecutionEngine.Assert(!EnteredStorage.IsSet(tran.Hash), "Re-entered");
            EnteredStorage.Set(tran.Hash);
            ExecutionEngine.Assert(CheckAddrValid(true, fromAddress, asset), "ClaimFLM: invald params");
            UInt160 selfAddress = Runtime.ExecutingScriptHash;
            var currentTimestamp = GetCurrentTimestamp();
            if (IsPaused() || !Runtime.CheckWitness(fromAddress) || !CheckIfRefundStart(currentTimestamp))
            {
                EnteredStorage.Delete(tran.Hash);
                return false;
            }
            StakingReocrd stakingRecord = UserStakingStorage.Get(fromAddress, asset);
            if (!stakingRecord.fromAddress.Equals(fromAddress))
            {
                EnteredStorage.Delete(tran.Hash);
                return false;
            }
            UpdateStackRecord(asset, currentTimestamp);
            BigInteger newProfit = SettleProfit(stakingRecord.timeStamp, stakingRecord.amount, asset);
            var profitAmount = stakingRecord.Profit + newProfit;
            if (profitAmount != 0)
            {
                UserStakingStorage.Put(fromAddress, stakingRecord.amount, stakingRecord.assetId, currentTimestamp, 0);
                ExecutionEngine.Assert(MintFLM(fromAddress, profitAmount, selfAddress), "ClaimFLM: mint failed");
            }
            EnteredStorage.Delete(tran.Hash);
            return true;
        }

        public static BigInteger CheckFLM(UInt160 fromAddress, UInt160 asset)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, fromAddress, asset), "CheckFLM: invald params");
            StakingReocrd stakingRecord = UserStakingStorage.Get(fromAddress, asset);
            UpdateStackRecord(asset, GetCurrentTimestamp());
            BigInteger newProfit = SettleProfit(stakingRecord.timeStamp, stakingRecord.amount, asset);
            var profitAmount = stakingRecord.Profit + newProfit;
            return profitAmount;
        }

        [Safe]
        public static BigInteger GetStakingAmount(UInt160 fromAddress, UInt160 asset)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, fromAddress, asset), "GetStakingAmount: invald params");
            return UserStakingStorage.Get(fromAddress, asset).amount;
        }

        private static BigInteger SettleProfit(BigInteger recordTimestamp, BigInteger amount, UInt160 asset)
        {
            BigInteger MinusProfit = GetHistoryUintStackProfitSum(asset, recordTimestamp);
            BigInteger SumProfit = GetHistoryUintStackProfitSum(asset, GetCurrentTimestamp());
            BigInteger currentProfit = (SumProfit - MinusProfit) * amount;
            return currentProfit;
        }

        private static bool CheckIfStakingStart(BigInteger currentTimeStamp)
        {
            if (currentTimeStamp >= StartStakingTimeStamp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckIfRefundStart(BigInteger currentTimeStamp)
        {
            if (currentTimeStamp >= StartClaimTimeStamp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckWhetherSelf(UInt160 fromAddress)
        {
            if (fromAddress.Equals(Runtime.ExecutingScriptHash)) return true;
            return false;
        }
    }
}
