using System;
using System.Linq;
using \u0007;
using \u000E;
using \u0017;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0004
{
	// Token: 0x020002EC RID: 748
	internal static class \u0010
	{
		// Token: 0x06002D9A RID: 11674 RVA: 0x000A7C04 File Offset: 0x000A5E04
		internal static bool \u0001(_ICompileContext \u0002, bool \u0003)
		{
			bool flag = true;
			foreach (_ICompiledPOU icompiledPOU in Enumerable.ToReadOnlyCollectionWrapper<_ICompiledPOU>(\u0002._GetAllCompiledPOUs()))
			{
				if (icompiledPOU.GetFlag(CompiledPOUFlags.ContainsDirVarAccess))
				{
					global::\u0017.\u0015 u = new global::\u0017.\u0015(icompiledPOU.SignatureId, global::\u0007.\u0005.\u0001(\u0002, icompiledPOU.SignatureId), \u0002, \u0003, icompiledPOU.MessageGuid);
					u.\u0001(icompiledPOU);
					if (u.Errors)
					{
						flag = false;
					}
				}
			}
			foreach (_ISignature isignature in \u0002.GetAllSignaturesFlatEx().OfType<_ISignature>())
			{
				global::\u0017.\u0015 u2 = new global::\u0017.\u0015(isignature.Id, global::\u0007.\u0005.\u0001(\u0002, isignature.Id), \u0002, \u0003, isignature.MessageGuid);
				u2.\u0001(isignature);
				if (u2.Errors)
				{
					flag = false;
				}
			}
			if (flag)
			{
				flag = global::\u000E.\u0015.\u0001(\u0002);
			}
			return flag;
		}
	}
}
