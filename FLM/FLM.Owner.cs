using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace flamingo_contract_staking
{
    partial class FLM
    {

        [DisplayName("AddAuthor")]
        public static event Action<UInt160> OnAddAuthor;

        [DisplayName("RemoveAuthor")]
        public static event Action<UInt160> OnRemoveAuthor;

        private static bool IsOwner() => Runtime.CheckWitness(GetOwner());

        [Safe]
        public static UInt160 GetOwner()
        {
            return OwnerStorage.Get();
        }

        public static bool SetOwner(UInt160 owner)
        {
            Assert(Runtime.CheckWitness(GetOwner()), "SetOwner: CheckWitness failed, owner-".ToByteArray().Concat(owner).ToByteString());
            Assert(CheckAddrVaild(owner), "SetOwner: invalid owner-".ToByteArray().Concat(owner).ToByteString());
            OwnerStorage.Put(owner);
            return true;
        }

        [Safe]
        public static bool IsAuthor(UInt160 usr)
        {
            return AuthorStorage.Get(usr) || usr.Equals(GetOwner());
        }

        [Safe]
        public static BigInteger GetAuthorCount()
        {
            return AuthorStorage.Count();
        }

        [Safe]
        public static UInt160[] GetAllAuthor()
        {
            BigInteger count = GetAuthorCount();
            return AuthorStorage.Find(count);
        }

        public static bool AddAuthor(UInt160 newAuthor)
        {
            Assert(CheckAddrVaild(newAuthor), "addAuthor: invalid newAuthor, newAuthor-".ToByteArray().Concat(newAuthor).ToByteString());
            Assert(IsOwner() && newAuthor != GetOwner(), "addAuthor: CheckWitness failed, only owner can add other author");
            Assert(!IsAuthor(newAuthor), "addAuthor: newAuthor-".ToByteArray().Concat(newAuthor).Concat(" is already a author".ToByteArray()).ToByteString());
            AuthorStorage.Put(newAuthor);
            OnAddAuthor(newAuthor);
            return true;
        }

        public static bool RemoveAuthor(UInt160 author)
        {
            Assert(CheckAddrVaild(author), "removeAuthor: invalid author, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsOwner() && author != GetOwner(), "removeAuthor: CheckWitness failed, only first pika can remove other author");
            Assert(IsAuthor(author), "removeAuthor: author-".ToByteArray().Concat(author).Concat(" is not a author".ToByteArray()).ToByteString());
            AuthorStorage.Delete(author);
            OnRemoveAuthor(author);
            return true;
        }

        public static bool Mint(UInt160 minter, UInt160 receiver, BigInteger amount)
        {
            Assert(CheckAddrVaild(minter, receiver), "approve: invalid minter or receiver, usr-".ToByteArray().Concat(minter).Concat("and receiver-".ToByteArray()).Concat(receiver).ToByteString());
            Assert(amount >= 0, "mint:invalid amount-".ToByteArray().Concat(amount.ToByteArray()).ToByteString());
            Assert(IsAuthor(minter), "mint: author-".ToByteArray().Concat(minter).Concat(" is not a real author".ToByteArray()).ToByteString());
            Assert(Runtime.CheckWitness(minter) || minter.Equals(Runtime.CallingScriptHash), "mint: CheckWitness failed, author-".ToByteArray().Concat(minter).ToByteString());

            amount = amount / ConvertDecimal;
            TransferInternal(UInt160.Zero, UInt160.Zero, receiver, amount);
            return true;
        }

        public static void Update(ByteString nefFile, string manifest, object data)
        {
            Assert(IsOwner(), "upgrade: Only allowed to be called by owner.");
            ContractManagement.Update(nefFile, manifest, data);
        }
    }
}
