using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000258 RID: 600
	[TypeGuid("{8ec925c2-9c5b-4d91-b581-8ec7c2f47ca9}")]
	[StorageVersion("3.3.0.0")]
	public class FunctionPointerEntry : GenericObject2, _IFunctionPointerEntry, IVFTableEntry2, IVFTableEntry
	{
		// Token: 0x06002814 RID: 10260 RVA: 0x0006489F File Offset: 0x0006389F
		public FunctionPointerEntry()
		{
		}

		// Token: 0x06002815 RID: 10261 RVA: 0x000648B9 File Offset: 0x000638B9
		public FunctionPointerEntry(string stFunctionName, int nFunctionId)
		{
			this.Name = stFunctionName;
			this.Id = nFunctionId;
		}

		// Token: 0x17000B29 RID: 2857
		// (get) Token: 0x06002816 RID: 10262 RVA: 0x000648E1 File Offset: 0x000638E1
		// (set) Token: 0x06002817 RID: 10263 RVA: 0x000648E9 File Offset: 0x000638E9
		public string Name
		{
			get
			{
				return this._stFunctionName;
			}
			set
			{
				this._stFunctionName = value;
			}
		}

		// Token: 0x17000B2A RID: 2858
		// (get) Token: 0x06002818 RID: 10264 RVA: 0x000648F2 File Offset: 0x000638F2
		// (set) Token: 0x06002819 RID: 10265 RVA: 0x000648FA File Offset: 0x000638FA
		public int Id
		{
			get
			{
				return this._nFunctionId;
			}
			set
			{
				this._nFunctionId = value;
			}
		}

		// Token: 0x17000B2B RID: 2859
		// (get) Token: 0x0600281A RID: 10266 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool IsFunctionPointerEntry
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600281B RID: 10267 RVA: 0x00064903 File Offset: 0x00063903
		public IVFTableEntry Duplicate()
		{
			return new FunctionPointerEntry(this.Name, this.Id);
		}

		// Token: 0x0600281C RID: 10268 RVA: 0x00064918 File Offset: 0x00063918
		public bool IsEqual(IVFTableEntry vftableIn)
		{
			FunctionPointerEntry functionPointerEntry = vftableIn as FunctionPointerEntry;
			return functionPointerEntry != null && this.Name == functionPointerEntry.Name && this.Id == functionPointerEntry.Id;
		}

		// Token: 0x04000797 RID: 1943
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stFunctionName = string.Empty;

		// Token: 0x04000798 RID: 1944
		[DefaultSerialization("Id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int _nFunctionId = -1;
	}
}
