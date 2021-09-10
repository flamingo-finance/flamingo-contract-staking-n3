using System.Numerics;
using Neo;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        private static BigInteger GetHistoryUintStackProfitSum(UInt160 assetId, BigInteger timestamp)
        {
            return HistoryStackProfitSumStorage.Get(assetId, timestamp);
        }

        private static BigInteger GetCurrentTimestamp()
        {
            return Runtime.Time / 1000;
        }

        private static BigInteger GetCurrentRecordTimestamp(UInt160 assetId)
        {
            return CurrentRateTimestampStorage.Get(assetId);
        }

        private static void UpdateCurrentRecordTimestamp(UInt160 assetId)
        {
            CurrentRateTimestampStorage.Put(assetId, GetCurrentTimestamp());
        }

        private static BigInteger GetCurrentUintStackProfit(UInt160 assetId)
        {
            return CurrentStackProfitStorage.Get(assetId);
        }

        private static void UpdateCurrentUintStackProfit(UInt160 assetId, BigInteger profit)
        {
            CurrentStackProfitStorage.Put(assetId, profit);
        }

        public static BigInteger GetCurrentTotalAmount(UInt160 assetId)
        {
            UInt160 selfAddress = Runtime.ExecutingScriptHash;
            var @params = new object[] { selfAddress };
            BigInteger totalAmount = (BigInteger)Contract.Call(assetId, "balanceOf", CallFlags.All, @params);
            return totalAmount;
        }

        private static void UpdateHistoryUintStackProfitSum(UInt160 assetId, BigInteger currentTimestamp)
        {
            BigInteger recordTimestamp = GetCurrentRecordTimestamp(assetId);
            if (recordTimestamp >= currentTimestamp)
            {
                return;
            }
            else
            {
                var uintStackProfit = GetCurrentUintStackProfit(assetId);
                BigInteger increaseAmount = uintStackProfit * (currentTimestamp - recordTimestamp);
                HistoryStackProfitSumStorage.Put(assetId, currentTimestamp, increaseAmount + GetHistoryUintStackProfitSum(assetId, recordTimestamp));
            }
        }


        private static void UpdateStackRecord(UInt160 assetId, BigInteger currentTimestamp)
        {
            //清算历史每stack收益率
            UpdateHistoryUintStackProfitSum(assetId, currentTimestamp);
            //更新当前收益率记账高度    
            UpdateCurrentRecordTimestamp(assetId);
            //计算之后每stack收益率
            var currentTotalStakingAmount = GetCurrentTotalAmount(assetId);
            var currentShareAmount = GetCurrentShareAmount(assetId);
            BigInteger currentUintStackProfit = 0;
            //TODO: 做正负号检查
            if (currentTotalStakingAmount != 0)
            {
                currentUintStackProfit = currentShareAmount / currentTotalStakingAmount;
            }
            //更新当前每个stack收益率
            UpdateCurrentUintStackProfit(assetId, currentUintStackProfit);
        }
    }
}
