using System;
using System.IO;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C8 RID: 456
	[TypeGuid("{18EE17DF-8EBB-4DDC-B9D6-CBCDC658FF9B}")]
	[StorageVersion("3.5.1.0")]
	internal class CompiledCodeDataPlaceholder : GenericObject2, ICompiledCode2, ICompiledCode, ICloneable, _ICompiledCodeStub, ICompiledCodeEmptyPlaceholder
	{
		// Token: 0x06002036 RID: 8246 RVA: 0x0000AC39 File Offset: 0x00009C39
		public CompiledCodeDataPlaceholder()
		{
		}

		// Token: 0x06002037 RID: 8247 RVA: 0x0005969C File Offset: 0x0005869C
		public CompiledCodeDataPlaceholder(int nSize, IDataLocation datloc)
		{
			this.CodeSize = nSize;
			this.Location = datloc;
		}

		// Token: 0x06002038 RID: 8248 RVA: 0x0000677E File Offset: 0x0000577E
		public Stream GetCode()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06002039 RID: 8249 RVA: 0x0000677E File Offset: 0x0000577E
		public void GetCode(BinaryWriter binwriter)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600203A RID: 8250 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool GenerateDisassembly(TextWriter writer)
		{
			return true;
		}

		// Token: 0x1700086B RID: 2155
		// (get) Token: 0x0600203B RID: 8251 RVA: 0x00005F0F File Offset: 0x00004F0F
		public IRelocationList RelocationList
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700086C RID: 2156
		// (get) Token: 0x0600203C RID: 8252 RVA: 0x000596B2 File Offset: 0x000586B2
		// (set) Token: 0x0600203D RID: 8253 RVA: 0x000596BA File Offset: 0x000586BA
		public IDataLocation Location { get; set; }

		// Token: 0x1700086D RID: 2157
		// (get) Token: 0x0600203E RID: 8254 RVA: 0x000596C3 File Offset: 0x000586C3
		// (set) Token: 0x0600203F RID: 8255 RVA: 0x000596CB File Offset: 0x000586CB
		public int CodeSize { get; set; }
	}
}
