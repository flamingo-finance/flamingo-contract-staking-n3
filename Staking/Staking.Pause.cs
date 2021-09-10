using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;

namespace FLMStaking
{
    partial class FLMStaking
    {
        [Safe]
        public static bool IsPaused()
        {
            return PauseStorage.Get();
        }

        [Safe]
        public static bool IsStakingPaused()
        {
            return PauseStakingStorage.Get();
        }

        [Safe]
        public static bool IsRefundPaused()
        {
            return PauseRefundStorage.Get();
        }

        public static bool Pause(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "Pause: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "Pause: not author".ToByteArray().Concat(author).ToByteString());
            PauseStorage.Put(1);
            return true;
        }

        public static bool UnPause(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "Unpause: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "Unpause: not author".ToByteArray().Concat(author).ToByteString());
            PauseStorage.Put(0);
            return true;
        }

        public static bool PauseStaking(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "PauseStaking: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "PauseStaking: not author".ToByteArray().Concat(author).ToByteString());
            PauseStakingStorage.Put(1);
            return true;
        }

        public static bool UnPauseStaking(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "UnPauseStaking: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "UnPauseStaking: not author".ToByteArray().Concat(author).ToByteString());
            PauseStakingStorage.Put(0);
            return true;
        }

        public static bool PauseRefund(UInt160 author) 
        {
            Assert(Runtime.CheckWitness(author), "PauseRefund: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "PauseRefund: not author".ToByteArray().Concat(author).ToByteString());
            PauseRefundStorage.Put(1);
            return true;
        }

        public static bool UnPauseRefund(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "UnPauseRefund: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "UnPauseRefund: not author".ToByteArray().Concat(author).ToByteString());
            PauseRefundStorage.Put(0);
            return true;
        }
    }
}
