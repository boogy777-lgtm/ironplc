using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0014;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0018
{
	// Token: 0x02000313 RID: 787
	internal sealed class \u000F : \u001C.\u0011
	{
		// Token: 0x06002F5C RID: 12124 RVA: 0x000B238C File Offset: 0x000B058C
		internal \u000F(_IPreCompileContext \u0080\u0005, IList<ICompiledType> \u0003\u0008)
		{
			this.\u0001 = \u0080\u0005;
			this.\u0001 = \u0003\u0008;
		}

		// Token: 0x06002F5D RID: 12125 RVA: 0x000B23A4 File Offset: 0x000B05A4
		public _ISignature4 \u0001(_ISignature4 \u0002)
		{
			return APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(\u0002.PrecompileBaseSignatureId) as _ISignature4;
		}

		// Token: 0x06002F5E RID: 12126 RVA: 0x000B23C8 File Offset: 0x000B05C8
		public IEnumerable<string> \u0001(_ISignature4 \u0002)
		{
			_IPreCompileContext ipreCompileContext = this.\u0001;
			if (!string.IsNullOrEmpty(\u0002.LibraryId))
			{
				ipreCompileContext = (_IPreCompileContext)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(\u0002);
			}
			if (ipreCompileContext == null)
			{
				return Enumerable.Empty<string>();
			}
			IEnumerable<_ISignature> enumerable = ipreCompileContext._GetSubSignatures(\u0002.ObjectGuid);
			List<string> list = new List<string>();
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary = new CaseInsensitiveDictionary<bool>();
			foreach (_ISignature isignature in enumerable)
			{
				if (caseInsensitiveDictionary.ContainsKey(isignature.Name) || isignature.HasAttribute("overloaded"))
				{
					list.Add(isignature.Name);
				}
				caseInsensitiveDictionary[isignature.Name] = true;
			}
			return list;
		}

		// Token: 0x06002F5F RID: 12127 RVA: 0x000B2494 File Offset: 0x000B0694
		private void \u0001(HashSet<_ISignature> \u0002, IList<_ISignature> \u0003, _ISignature4 \u0004, string \u0005)
		{
			\u0018.\u000F.\u0001 u = new \u0018.\u000F.\u0001();
			u.\u0001 = \u0005;
			if (!\u0002.Add(\u0004))
			{
				return;
			}
			_IPreCompileContext ipreCompileContext = this.\u0001;
			if (!string.IsNullOrEmpty(\u0004.LibraryPath))
			{
				ipreCompileContext = (_IPreCompileContext)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetLibraryPrecompileSet(\u0004.LibraryPath);
			}
			else if (ipreCompileContext.GetSignature(\u0004.ObjectGuid) == null)
			{
				ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			}
			if (\u0004.PrecompileBaseSignatureId != Helper.InvalidId)
			{
				_ISignature4 isignature = (_ISignature4)APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0004.PrecompileBaseSignatureId);
				if (isignature != null)
				{
					this.\u0001(\u0002, \u0003, isignature, u.\u0001);
				}
			}
			Enumerable.AddRange<_ISignature>(\u0003, ipreCompileContext._GetSubSignatures(\u0004.ObjectGuid).Where(new Func<_ISignature, bool>(u.\u0001)));
		}

		// Token: 0x06002F60 RID: 12128 RVA: 0x000B256C File Offset: 0x000B076C
		public IList<_ISignature> \u0001(_ISignature4 \u0002, string \u0003)
		{
			List<_ISignature> list = new List<_ISignature>();
			HashSet<_ISignature> u = new HashSet<_ISignature>();
			this.\u0001(u, list, \u0002, \u0003);
			return list;
		}

		// Token: 0x06002F61 RID: 12129 RVA: 0x000B2590 File Offset: 0x000B0790
		public ICompiledType \u0001(_IExpression \u0002, int \u0003)
		{
			return this.\u0001[\u0003];
		}

		// Token: 0x06002F62 RID: 12130 RVA: 0x000B25A0 File Offset: 0x000B07A0
		public string \u0001(_ISignature \u0002)
		{
			if (\u0002.HasAttribute("search_name"))
			{
				return \u0002.GetAttributeValue("search_name");
			}
			if (\u0002.IsLibraryObject)
			{
				return global::\u0014.\u0002.\u0001(\u0002.LibraryPath) + "." + \u0002.OrgName;
			}
			if (\u0002.GetFlag(SignatureFlag.SystemNamespaceForced))
			{
				return "__SYSTEM." + \u0002.OrgName;
			}
			if (\u0002.GetFlag(SignatureFlag.PoolSignature))
			{
				return "@pool." + \u0002.OrgName;
			}
			return \u0002.Name;
		}

		// Token: 0x06002F63 RID: 12131 RVA: 0x000B2634 File Offset: 0x000B0834
		public bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			string a = NameManglingService.\u0001(\u0002);
			string b = NameManglingService.\u0001(\u0003);
			return a == b;
		}

		// Token: 0x04000903 RID: 2307
		private readonly _IPreCompileContext \u0001;

		// Token: 0x04000904 RID: 2308
		private readonly IList<ICompiledType> \u0001;

		// Token: 0x02000314 RID: 788
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06002F65 RID: 12133 RVA: 0x000B265C File Offset: 0x000B085C
			internal bool \u0001(_ISignature \u0002)
			{
				return \u0002.Name == this.\u0001;
			}

			// Token: 0x04000905 RID: 2309
			public string \u0001;
		}
	}
}
