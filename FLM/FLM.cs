using System;
using System.Numerics;
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace flamingo_contract_staking
{
    [ManifestExtra("Author", "")]
    [ManifestExtra("Email", "")]
    [ManifestExtra("Description", "")]
    [SupportedStandards("NEP-17")]
    [ContractPermission("*", "*")]
    public partial class FLM : SmartContract
    {
        [InitialValue("NaBUWGCLWFZTGK4V9f4pecuXmEijtGXMNX", ContractParameterType.Hash160)]
        private static readonly UInt160 InitialOwner;

        [InitialValue("00000040eaed7446d09c2c9f0c", ContractParameterType.ByteArray)]
        private static readonly BigInteger ConvertDecimal;

        [Safe]
        public static string Name() => "Flamingo";

        [Safe]
        public static string Symbol() => "FLM";

        [Safe]
        public static byte Decimals() => 8;

        public static bool Burn(UInt160 user, BigInteger amount)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(user), "Permission Invaid");
 
            return TransferInternal(user, UInt160.Zero, amount);
        }
    }
}
