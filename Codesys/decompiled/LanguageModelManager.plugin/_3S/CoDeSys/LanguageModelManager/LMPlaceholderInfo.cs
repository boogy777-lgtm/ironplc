using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000118 RID: 280
	internal class LMPlaceholderInfo : ILMPlaceholderInfo
	{
		// Token: 0x060014E5 RID: 5349 RVA: 0x0003C91B File Offset: 0x0003B91B
		internal LMPlaceholderInfo(LibraryPlaceholder libplaceholder)
		{
			this._libplaceholder = libplaceholder;
		}

		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x060014E6 RID: 5350 RVA: 0x0003C92A File Offset: 0x0003B92A
		public ILibraryPlaceholder PlaceholderInfo
		{
			get
			{
				return this._libplaceholder;
			}
		}

		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x060014E7 RID: 5351 RVA: 0x0003C932 File Offset: 0x0003B932
		// (set) Token: 0x060014E8 RID: 5352 RVA: 0x0003C93A File Offset: 0x0003B93A
		public ILibParameterTable ParamTable
		{
			get
			{
				return this._paramtable;
			}
			set
			{
				if (((value != null) ? value.ParameterTable : null) != null)
				{
					this._paramtable = value;
				}
			}
		}

		// Token: 0x040004DB RID: 1243
		private readonly LibraryPlaceholder _libplaceholder;

		// Token: 0x040004DC RID: 1244
		private ILibParameterTable _paramtable;
	}
}
