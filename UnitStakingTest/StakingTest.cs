extern alias scfx;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo;
using Neo.Compiler.CSharp.UnitTests.Utils;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using System.Linq;
using Neo.SmartContract;
using System.Numerics;

namespace UnitStakingTest
{
    [TestClass]
    public class StakingTest
    {
        private TestEngine engine;

        private byte[] owner = "d4772e063e0d9ad7b93e222475c539738879809c".HexToBytes().Reverse().ToArray();

        private byte[] addr1 = "bba58d0e5b015c8501a3a0e093d314d6f6b9fd69".HexToBytes().Reverse().ToArray();

        private byte[] addr2 = "67581314f0d57c5f3e083588025442e8f68da3bc".HexToBytes().Reverse().ToArray();

        private byte[] flmHash;
        private byte[] stakingHash;

        class DummyVerificable : IVerifiable
        {
            public Witness[] Witnesses { get; set; }

            public int Size => 0;

            public void Deserialize(BinaryReader reader) { }

            public void DeserializeUnsigned(BinaryReader reader) { }

            public UInt160 Addr { get; }

            public DummyVerificable(UInt160 addr)
            {
                Addr = addr;
            }

            public UInt160[] GetScriptHashesForVerifying(DataCache snapshot)
            {
                return new UInt160[]
                {
                    Addr
                };
            }

            public void Serialize(BinaryWriter writer) { }

            public void SerializeUnsigned(BinaryWriter writer) { }
        }

        [TestInitialize]
        public void Init()
        {
            TestDataCache dataCache = new TestDataCache();

            UInt160 defaultSender = new UInt160(owner);

            engine = new TestEngine(TriggerType.Application, new DummyVerificable(defaultSender), snapshot: dataCache, persistingBlock: new Block()
            {
                Header = new Header()
                {
                    Index = 123,
                    Timestamp = 3600 + 1601114400,
                    Witness = new Witness()
                    {
                        InvocationScript = System.Array.Empty<byte>(),
                        VerificationScript = System.Array.Empty<byte>()
                    },
                    NextConsensus = UInt160.Zero,
                    MerkleRoot = UInt256.Zero,
                    PrevHash = UInt256.Zero
                },

                Transactions = new Transaction[]
                {
                     new Transaction()
                     {
                          Attributes = System.Array.Empty<TransactionAttribute>(),
                          Signers = new Signer[]{ new Signer() { Account = defaultSender } },
                          Witnesses = System.Array.Empty<Witness>(),
                          Script = System.Array.Empty<byte>()
                     }
                }

            });

            string path = "../../../../Staking/";
            string file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            stakingHash = engine.Nef.Script.ToScriptHash().ToString().Substring(2).HexToBytes().Reverse().ToArray();
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = engine.Nef.Script.ToScriptHash(),
                Nef = engine.Nef,
                Manifest = Neo.SmartContract.Manifest.ContractManifest.FromJson(engine.Manifest)
            });

            path = "../../../../FLM/";
            file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            var context = engine.CompileProject(file);
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = context.CreateExecutable().Script.ToScriptHash(),
                Nef = context.CreateExecutable(),
                Manifest = Neo.SmartContract.Manifest.ContractManifest.FromJson(context.CreateManifest())  
            });
            flmHash = context.CreateExecutable().Script.ToScriptHash().ToString().Substring(2).HexToBytes().Reverse().ToArray();
            engine.Snapshot.Commit();
        }

        #region staking.owner
        [TestMethod]
        public void TestGetOwner()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("getOwner");
            Assert.AreEqual(stack.Pop().GetSpan().ToArray().ToHexString(), owner.ToHexString());
        }

        [TestMethod]
        public void TestSetOwner()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("setOwner", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getOwner");
            Assert.AreEqual(stack.Pop().GetSpan().ToArray().ToHexString(), addr1.ToHexString());
        }

        [TestMethod]
        public void TestIsAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isAuthor", owner);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestAddAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isAuthor", addr1);
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }

        [TestMethod]
        public void TestGetAuthorCount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAuthorCount");
            Assert.AreEqual(stack.Pop().GetInteger(), 1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAuthor", addr2);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAuthorCount");
            Assert.AreEqual(stack.Pop().GetInteger(), 2);
        }

        [TestMethod]
        public void TestGetAllAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAllAuthor");

            //engine.Reset();
            //stack = engine.ExecuteTestCaseStandard("addAuthor", addr2);
            //engine.Reset();
            //stack = engine.ExecuteTestCaseStandard("getAuthorCount");
            //Assert.AreEqual(stack.Pop().GetInteger(), 2);
        }

        [TestMethod]
        public void TestRemoveAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("removeAuthor", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isAuthor", addr1);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }
        #endregion

        #region staking.pause
        [TestMethod]
        public void TestIsPause()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestIsStakingPaused()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isStakingPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestIsRefundPaused()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isRefundPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestPause()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pause", owner);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }


        [TestMethod]
        public void TestUnPause()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pause", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("unPause", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestPauseStaking()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pauseStaking", owner);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isStakingPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }

        [TestMethod]
        public void TestUnPauseStaking()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pauseStaking", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isStakingPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("unPauseStaking", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isStakingPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestPauseRefund()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pauseRefund", owner);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isRefundPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }

        [TestMethod]
        public void TestUnPauseRefund()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("pauseRefund", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isRefundPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("unPauseRefund", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isRefundPaused");
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }
        #endregion

        #region staking.record
        [TestMethod]
        public void TestGetCurrentTotalAmount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("getCurrentTotalAmount", flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }
        #endregion

        #region staking.whitelist
        [TestMethod]
        public void TestInWhiteList()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isInWhiteList", flmHash);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestAddAsset()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isInWhiteList", flmHash);
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }

        [TestMethod]
        public void TestRemoveAsset()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isInWhiteList", flmHash);
            Assert.AreEqual(stack.Pop().GetBoolean(), true);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("removeAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isInWhiteList", flmHash);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestGetAssetCount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAssetCount");
            Assert.AreEqual(stack.Pop().GetInteger(), 1);
        }

        [TestMethod]
        public void TestGetAllAsset()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAllAsset");
        }
        #endregion

        #region staking.share
        [TestMethod]
        public void TestGetCurrentShareAmount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("getCurrentShareAmount", flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }

        [TestMethod]
        public void TestSetCurrentShareAmount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("setCurrentShareAmount", flmHash, 100, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getCurrentShareAmount", flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 100);
        }
        #endregion

        #region staking.flm
        [TestMethod]
        public void TestGetFLMAddress()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("getFLMAddress");
            Assert.AreEqual(stack.Pop().IsNull, true);
        }

        [TestMethod]
        public void TestSetFLMAddress()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("setFLMAddress", flmHash, owner);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getFLMAddress");
            Assert.AreEqual(stack.Pop().IsNull, false);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getFLMAddress");
            Assert.AreEqual(stack.Pop().GetSpan().ToArray().ToHexString(), flmHash.ToHexString());
        }
        #endregion

        #region  staking
        [TestMethod]
        public void TestGetUintProfit()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getUintProfit", flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }

        [TestMethod]
        public void TestGetStakingAmount()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("getStakingAmount", owner, flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }

        [TestMethod]
        public void TestStaking()
        {
            var path = "../../../../Staking/";
            var file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);


            path = "../../../../FLM/";
            file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            flmHash = engine.Nef.Script.ToScriptHash().ToString().Substring(2).HexToBytes().Reverse().ToArray();
            engine.Reset();
            BigInteger mintAmount = BigInteger.Parse("100000000000000000000000000000000000000");
            stack = engine.ExecuteTestCaseStandard("mint", owner, owner, mintAmount);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("transfer", owner, stakingHash, 1000, new byte[] { });

            path = "../../../../Staking/";
            file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getStakingAmount", owner, flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 1000);
        }

        [TestMethod]
        public void TestRefund()
        {
            var path = "../../../../Staking/";
            var file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", owner);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("addAsset", flmHash, owner);


            path = "../../../../FLM/";
            file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            flmHash = engine.Nef.Script.ToScriptHash().ToString().Substring(2).HexToBytes().Reverse().ToArray();
            engine.Reset();
            BigInteger mintAmount = BigInteger.Parse("100000000000000000000000000000000000000");
            stack = engine.ExecuteTestCaseStandard("mint", owner, owner, mintAmount);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("transfer", owner, stakingHash, 1000, new byte[] { });

            path = "../../../../Staking/";
            file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getStakingAmount", owner, flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 1000);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("refund", owner, 1000, flmHash);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getStakingAmount", owner, flmHash);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }
        #endregion
    }
}
