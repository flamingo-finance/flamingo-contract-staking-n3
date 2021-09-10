extern alias scfx;
using System.IO;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo;
using Neo.Compiler.CSharp.UnitTests.Utils;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using System.Collections.Generic;

namespace UnitFLMTest
{
    [TestClass]
    public class FLMTest
    {
        private TestEngine engine;

        private byte[] owner = "d4772e063e0d9ad7b93e222475c539738879809c".HexToBytes().Reverse().ToArray();

        private byte[] addr1 = "bba58d0e5b015c8501a3a0e093d314d6f6b9fd69".HexToBytes().Reverse().ToArray();

        private byte[] addr2 = "67581314f0d57c5f3e083588025442e8f68da3bc".HexToBytes().Reverse().ToArray();

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
                    Timestamp = 1234,
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

            string path = "../../../../FLM/";
            string file = Directory.GetFiles(path, "*.csproj").FirstOrDefault();
            engine.AddEntryScript_Project(file);
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = engine.EntryScriptHash,
                Nef = engine.Nef,
                Manifest = new Neo.SmartContract.Manifest.ContractManifest()
            });
        }


        #region flm
        [TestMethod]
        public void TestName()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("name");
            Assert.AreEqual(stack.Pop().GetString(), "Flamingo");
        }

        [TestMethod]
        public void TestSymbol()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("symbol");
            Assert.AreEqual(stack.Pop().GetString(), "FLM");
        }

        [TestMethod]
        public void TestDecimals()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("decimals");
            Assert.AreEqual(stack.Pop().GetInteger(), 8);
        }

        [TestMethod]
        public void TestVerify()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("verify");
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }
        #endregion

        #region flm.owner
        [TestMethod]
        public void TestSetOwner()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("setOwner", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getOwner");
            Assert.AreEqual(stack.Pop().GetSpan().ToArray().ToHexString(),addr1.ToHexString());
        }

        [TestMethod]
        public void TestIsAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("isAuthor", owner);
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
            stack = engine.ExecuteTestCaseStandard("addAuthor", addr2);
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("getAllAuthor");

            //engine.Reset();
            //stack = engine.ExecuteTestCaseStandard("addAuthor", addr2);
            //engine.Reset();
            //stack = engine.ExecuteTestCaseStandard("getAuthorCount");
            //Assert.AreEqual(stack.Pop().GetInteger(), 2);
        }

        [TestMethod]
        public void TestAddAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);

            //重新构造tx避免因为原始tx的sender为owner影响判断
            engine.Reset();
            engine = new TestEngine(TriggerType.Application, new DummyVerificable(new UInt160(addr1)), snapshot: engine.Snapshot, persistingBlock: new Block()
            {
                Header = new Header()
                {
                    Index = 123,
                    Timestamp = 1234,
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
                          Signers = new Signer[]{ new Signer() { Account = UInt160.Parse("bba58d0e5b015c8501a3a0e093d314d6f6b9fd69") } },
                          Witnesses = System.Array.Empty<Witness>(),
                          Script = System.Array.Empty<byte>()
                     }
                }

            });
            string path = "../../../../FLM/";
            string[] files = Directory.GetFiles(path, "*.cs");
            engine.AddEntryScript(files);
            stack = engine.ExecuteTestCaseStandard("isAuthor", addr1);
            Assert.AreEqual(stack.Pop().GetBoolean(), true);

            //反例
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("isAuthor", addr2);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestRemoveAuthor()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("addAuthor", addr1);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("removeAuthor", addr1);

            //重新构造tx避免因为原始tx的sender为owner影响判断
            engine.Reset();
            engine = new TestEngine(TriggerType.Application, new DummyVerificable(new UInt160(addr1)), snapshot: engine.Snapshot, persistingBlock: new Block()
            {
                Header = new Header()
                {
                    Index = 123,
                    Timestamp = 1234,
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
                          Signers = new Signer[]{ new Signer() { Account = new UInt160(addr1) } },
                          Witnesses = System.Array.Empty<Witness>(),
                          Script = System.Array.Empty<byte>()
                     }
                }

            });
            string path = "../../../../FLM/";
            string[] files = Directory.GetFiles(path, "*.cs");
            engine.AddEntryScript(files);
            stack = engine.ExecuteTestCaseStandard("isAuthor", addr1);
            Assert.AreEqual(stack.Pop().GetBoolean(), false);
        }

        [TestMethod]
        public void TestMint()
        {
            engine.Reset();
            BigInteger mintAmount = BigInteger.Parse("1000000000000000000000000000000");
            var stack = engine.ExecuteTestCaseStandard("mint", owner, owner, mintAmount);
            //发行总量为 1000000000000000000000000000000 / 1000000000000000000000000000000
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("totalSupply");
            Assert.AreEqual(stack.Pop().GetInteger(), 1);
            //owner的余额应该也为1
            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("balanceOf", owner);
            Assert.AreEqual(stack.Pop().GetInteger(), 1);
        }
        #endregion

        #region   flm.asset
        [TestMethod]
        public void TestBalanceOf()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("balanceOf", addr1);
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }

        [TestMethod]
        public void TestTotalSupply()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("totalSupply");
            Assert.AreEqual(stack.Pop().GetInteger(), 0);
        }

        [TestMethod]
        public void TestApprove()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("approve",owner,addr1,10086);
            Assert.AreEqual(stack.Pop().GetBoolean(), true);
        }

        [TestMethod]
        public void TestAllowance()
        {
            engine.Reset();
            var stack = engine.ExecuteTestCaseStandard("approve", owner, addr1, 10086);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("allowance", owner, addr1);
            Assert.AreEqual(stack.Pop().GetInteger(), 10086);
        }

        [TestMethod]
        public void Transfer()
        {
            engine.Reset();
            BigInteger mintAmount = BigInteger.Parse("1000000000000000000000000000000");
            var stack = engine.ExecuteTestCaseStandard("mint", owner, owner, mintAmount);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("transfer", owner, addr1, 1,new byte[] { });

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("balanceOf", addr1);
            Assert.AreEqual(stack.Pop().GetInteger(), 1);
        }

        [TestMethod]
        public void TransferFrom()
        {
            engine.Reset();
            BigInteger mintAmount = BigInteger.Parse("1000000000000000000000000000000");
            var stack = engine.ExecuteTestCaseStandard("mint", owner, owner, mintAmount);

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("approve", owner, addr1, 1);

            engine.Reset();
            engine = new TestEngine(TriggerType.Application, new DummyVerificable(new UInt160(addr1)), snapshot: engine.Snapshot, persistingBlock: new Block()
            {
                Header = new Header()
                {
                    Index = 123,
                    Timestamp = 1234,
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
                          Signers = new Signer[]{ new Signer() { Account = UInt160.Parse("bba58d0e5b015c8501a3a0e093d314d6f6b9fd69") } },
                          Witnesses = System.Array.Empty<Witness>(),
                          Script = System.Array.Empty<byte>()
                     }
    }

            });
            string path = "../../../../FLM/";
            string[] files = Directory.GetFiles(path, "*.cs");
            engine.AddEntryScript(files);
            stack = engine.ExecuteTestCaseStandard("transferFrom", addr1, owner, addr2, 1, new byte[] { });

            engine.Reset();
            stack = engine.ExecuteTestCaseStandard("balanceOf",addr2);
            Assert.AreEqual(stack.Pop().GetInteger(), 1);
        }
        #endregion



    }
}
