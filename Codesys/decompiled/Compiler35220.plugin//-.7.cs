using System;
using System.Collections.Generic;
using \u0011;
using \u0014;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0084
{
	// Token: 0x02000152 RID: 338
	internal sealed class \u0007 : ICompiledSymbolTables
	{
		// Token: 0x0600179F RID: 6047 RVA: 0x00048BD8 File Offset: 0x00046DD8
		public \u0007(_ICompileContext \u0001\u0002)
		{
			this.\u0001 = \u0001\u0002;
			this.\u0001 = new LDictionary<string, global::\u0014.\u0005>();
		}

		// Token: 0x060017A0 RID: 6048 RVA: 0x00048C00 File Offset: 0x00046E00
		public IEnumerator<global::\u0014.\u0005> \u0001()
		{
			return this.\u0001.Values.GetEnumerator();
		}

		// Token: 0x060017A1 RID: 6049 RVA: 0x00048C18 File Offset: 0x00046E18
		public void \u0001(ISignature \u0002)
		{
			if (\u0002.GetFlag(SignatureFlag.SuperGlobal))
			{
				using (IEnumerator<global::\u0014.\u0005> enumerator = this.\u0001())
				{
					while (enumerator.MoveNext())
					{
						global::\u0014.\u0005 u = enumerator.Current;
						u.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.\u0001.ApplicationGuid), \u0002 as _ISignature, this.\u0001, global::\u0011.\u0005.\u0006);
					}
					return;
				}
			}
			this.\u0001(string.Empty).\u0001(APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.\u0001.ApplicationGuid), \u0002 as _ISignature, this.\u0001, global::\u0011.\u0005.\u0006);
		}

		// Token: 0x060017A2 RID: 6050 RVA: 0x00048CCC File Offset: 0x00046ECC
		public global::\u0014.\u0005 \u0001()
		{
			object u = this.\u0001;
			global::\u0014.\u0005 result;
			lock (u)
			{
				if (!this.\u0001.ContainsKey("@Pool"))
				{
					this.\u0001.Add("@Pool", new global::\u0014.\u0005(APEnvironmentFacade.Instance.LanguageModelMgr.Pool, this.\u0001));
				}
				result = this.\u0001["@Pool"];
			}
			return result;
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x00048D54 File Offset: 0x00046F54
		public global::\u0014.\u0005 \u0001(string \u0002)
		{
			object u = this.\u0001;
			global::\u0014.\u0005 result;
			lock (u)
			{
				if (!this.\u0001.ContainsKey(\u0002))
				{
					IPreCompileContext preCompileContext;
					if (string.IsNullOrEmpty(\u0002))
					{
						preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.\u0001.ApplicationGuid);
					}
					else
					{
						preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002);
					}
					if (preCompileContext == null)
					{
						return null;
					}
					this.\u0001.Add(\u0002, new global::\u0014.\u0005(preCompileContext as _IPreCompileContext, this.\u0001));
				}
				result = this.\u0001[\u0002];
			}
			return result;
		}

		// Token: 0x04000430 RID: 1072
		private readonly LDictionary<string, global::\u0014.\u0005> \u0001;

		// Token: 0x04000431 RID: 1073
		private readonly _ICompileContext \u0001;

		// Token: 0x04000432 RID: 1074
		private readonly object \u0001 = new object();
	}
}
