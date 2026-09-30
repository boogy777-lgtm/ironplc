using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DD RID: 221
	internal class LanguageModelList : _ILanguageModelList, ILanguageModelList
	{
		// Token: 0x06000F8E RID: 3982 RVA: 0x0002AB30 File Offset: 0x00029B30
		public void AddLanguageModel(string stLanguageModelContent)
		{
			this.m_alLanguageModelList.Add(stLanguageModelContent);
		}

		// Token: 0x17000473 RID: 1139
		// (get) Token: 0x06000F8F RID: 3983 RVA: 0x0002AB3E File Offset: 0x00029B3E
		public IList<string> LanguageModels
		{
			get
			{
				return this.m_alLanguageModelList;
			}
		}

		// Token: 0x04000394 RID: 916
		private readonly LList<string> m_alLanguageModelList = new LList<string>();
	}
}
