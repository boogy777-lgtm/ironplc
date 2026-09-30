using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002D RID: 45
	[TypeGuid("{93F82D37-D1E7-4F44-B49A-EBB5A191C6D3}")]
	[StorageVersion("3.5.16.0")]
	public class TryCatchBreakpoint : Breakpoint
	{
		// Token: 0x060001F3 RID: 499 RVA: 0x00006CC1 File Offset: 0x00005CC1
		public TryCatchBreakpoint()
		{
			this._sTryCatchId = -1;
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x00006CD7 File Offset: 0x00005CD7
		internal TryCatchBreakpoint(int iOffset, IMinimalPosition sourcepos, short sLen, short sTryCatchId) : base(iOffset, sourcepos, sLen)
		{
			this._sTryCatchId = sTryCatchId;
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x060001F5 RID: 501 RVA: 0x00006CF1 File Offset: 0x00005CF1
		// (set) Token: 0x060001F6 RID: 502 RVA: 0x00006CF9 File Offset: 0x00005CF9
		public override short TryCatchId
		{
			get
			{
				return this._sTryCatchId;
			}
			set
			{
				this._sTryCatchId = value;
			}
		}

		// Token: 0x04000054 RID: 84
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[DefaultSerialization("TryCatchId")]
		[StorageVersion("3.5.16.0")]
		[StorageDefaultValue(-1)]
		[Obfuscation(Feature = "rename")]
		private short _sTryCatchId = -1;
	}
}
