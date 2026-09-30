using System;
using System.Collections;
using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C7 RID: 455
	[TypeGuid("{35E108DD-922C-4765-9F66-6C7ED83F4AF7}")]
	[StorageVersion("3.3.0.0")]
	public class CompiledCodeDataReloc : CompiledCodeData, _ICompiledCodeStub, ICompiledCodeDataRelocSerializable, ICompiledCodeDataSerializable, ICompiledCodeSerializable
	{
		// Token: 0x1700086A RID: 2154
		// (get) Token: 0x06002030 RID: 8240 RVA: 0x00059506 File Offset: 0x00058506
		// (set) Token: 0x06002031 RID: 8241 RVA: 0x0005950E File Offset: 0x0005850E
		[DefaultSerialization("MotorolaByteOrder")]
		[StorageVersion("3.3.0.0")]
		public bool MotorolaByteOrder { get; set; }

		// Token: 0x06002032 RID: 8242 RVA: 0x00059517 File Offset: 0x00058517
		public CompiledCodeDataReloc()
		{
		}

		// Token: 0x06002033 RID: 8243 RVA: 0x0005951F File Offset: 0x0005851F
		[Obsolete("Nix Use")]
		public CompiledCodeDataReloc(byte[] abyCode, bool bMotorolaByteOrder) : base(abyCode, -1)
		{
			this.MotorolaByteOrder = bMotorolaByteOrder;
		}

		// Token: 0x06002034 RID: 8244 RVA: 0x00059530 File Offset: 0x00058530
		internal CompiledCodeDataReloc(Stream memstream, bool bMotorolaByteOrder)
		{
			this.m_msCodeStream = memstream;
			this.MotorolaByteOrder = bMotorolaByteOrder;
		}

		// Token: 0x06002035 RID: 8245 RVA: 0x00059548 File Offset: 0x00058548
		public override bool GenerateDisassembly(TextWriter writer)
		{
			Swapper swapper = new Swapper(this.MotorolaByteOrder);
			this.m_msCodeStream.Position = 0L;
			BinaryReader binaryReader = new BinaryReader(this.m_msCodeStream);
			ArrayList arrayList = new ArrayList();
			try
			{
				while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
				{
					ushort num = swapper.Swap(binaryReader.ReadUInt16());
					ushort num2 = swapper.Swap(binaryReader.ReadUInt16());
					for (int i = 0; i < (int)num2; i++)
					{
						ushort num3 = swapper.Swap(binaryReader.ReadUInt16());
						arrayList.Add((uint)new IntegerUnion
						{
							m_short0 = (short)num3,
							m_short1 = (short)num
						}.m_int0);
					}
				}
			}
			catch
			{
				return false;
			}
			writer.WriteLine("Num of addresses to relocate: {0}", arrayList.Count);
			int num4 = 0;
			foreach (object obj in arrayList)
			{
				uint num5 = (uint)obj;
				writer.WriteLine("{0}: 0x{1:X}", num4++, num5);
			}
			return true;
		}
	}
}
