using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace flamingo_contract_staking
{
    partial class FLM
    {
        [DisplayName("Transfer")]
        public static event Action<UInt160, UInt160, BigInteger> OnTransfer;

        [Safe]
        public static BigInteger TotalSupply() => TotalSupplyStorage.Get();

        [Safe]
        public static BigInteger BalanceOf(UInt160 usr)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, usr), "BalanceOf: invalid usr, usr-".ToByteArray().Concat(usr).ToByteString());
            return BalanceStorage.Get(usr);
        }

        public static bool Transfer(UInt160 from, UInt160 to, BigInteger amount, object data = null)
        {
            ExecutionEngine.Assert(CheckAddrValid(true, from, to), "transfer: invalid from or to, owner-".ToByteArray().Concat(from).Concat("and to-".ToByteArray()).Concat(to).ToByteString());
            ExecutionEngine.Assert(Runtime.CheckWitness(from) || from.Equals(Runtime.CallingScriptHash), "transfer: CheckWitness failed, from-".ToByteArray().Concat(from).ToByteString());
            return TransferInternal(from, to, amount, data);
        }

        private static bool TransferInternal(UInt160 from, UInt160 to, BigInteger amount, object data = null)
        {
            ExecutionEngine.Assert(amount >= 0, "transferInternal: invalid amount-".ToByteArray().Concat(amount.ToByteArray()).ToByteString());

            if (from != UInt160.Zero && amount != 0)
            {
                if (!BalanceStorage.Reduce(from, amount))
                    return false;
            }
            else if (from == UInt160.Zero)
            { 
                TotalSupplyStorage.Increase(amount);
            }


            if (to != UInt160.Zero && amount != 0)
            {
                BalanceStorage.Increase(to, amount);
            }
            else if (to == UInt160.Zero)
            {
                TotalSupplyStorage.Reduce(amount);
            }

            // Validate payable
            if (ContractManagement.GetContract(to) != null)
                Contract.Call(to, "onNEP17Payment", CallFlags.All, new object[] { from, amount, data });

            OnTransfer(from, to, amount);
            return true;
        }
    }
}
