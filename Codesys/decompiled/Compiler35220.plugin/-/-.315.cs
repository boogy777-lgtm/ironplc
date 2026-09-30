using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000E;
using \u0018;
using \u001F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0017
{
	// Token: 0x02000353 RID: 851
	internal sealed class \u0016
	{
		// Token: 0x17000869 RID: 2153
		// (get) Token: 0x06003332 RID: 13106 RVA: 0x000C64B4 File Offset: 0x000C46B4
		internal global::\u000E.\u001B CompileInformation { get; }

		// Token: 0x06003333 RID: 13107 RVA: 0x000C64BC File Offset: 0x000C46BC
		internal \u0016(global::\u000E.\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x06003334 RID: 13108 RVA: 0x000C64CC File Offset: 0x000C46CC
		internal bool? \u0001(IMessageCategory \u0002, IProgressCallback \u0003, out IOnlineChangeDetails \u0004)
		{
			_ILanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			\u0004 = null;
			global::\u0018.\u0010 u = new global::\u0018.\u0010(this.CompileInformation, \u0002, \u0003);
			try
			{
				_ICompileContext u2 = this.\u0001(u);
				_ICompileContext u3 = u.ComconNew;
				u.ComconNew = u2;
				if (u.HasFullAbortError)
				{
					Messages.\u0001(u3, APEnvironmentFacade.Instance.MessageStorage, \u0002);
					return new bool?(false);
				}
				\u0004 = u.\u0001;
			}
			catch
			{
				languageModelMgr[u.ApplicationGuid] = null;
				u.ComconNew = null;
				return null;
			}
			if (u.ComconNew != null)
			{
				u.ComconNew.DataId = u.ComconOld.DataId;
				u.ComconNew.LastDataId = u.ComconOld.DataId;
				u.ComconNew.CodeId = Guid.NewGuid();
				u.ComconNew.LastCodeId = u.ComconOld.CodeId;
				languageModelMgr[u.ApplicationGuid] = u.ComconNew;
				return new bool?(true);
			}
			languageModelMgr[u.ApplicationGuid] = null;
			if (u.HasFullAbortError)
			{
				return new bool?(false);
			}
			return null;
		}

		// Token: 0x06003335 RID: 13109 RVA: 0x000C6610 File Offset: 0x000C4810
		private _ICompileContext \u0001(global::\u0018.\u0010 \u0002)
		{
			\u0002.changedpous = new Dictionary<int, _ICompiledPOU>();
			using (IEnumerator<\u001E> enumerator = \u001F.\u0012.Instance.FastOnlineChangeSteps.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.\u0001(\u0002))
					{
						return null;
					}
				}
			}
			\u0002.\u0001.ResetOnlineChangeFlags();
			return \u0002.ComconNew;
		}

		// Token: 0x040009B4 RID: 2484
		[CompilerGenerated]
		private readonly global::\u000E.\u001B \u0001;
	}
}
