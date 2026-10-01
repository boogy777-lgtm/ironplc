using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services.UpToDateChecks
{
	// Token: 0x0200011F RID: 287
	internal sealed class ChangedSignatureChecker : global::\u000F.\u0006
	{
		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x0600149D RID: 5277 RVA: 0x0003CD70 File Offset: 0x0003AF70
		internal static global::\u000F.\u0006 Instance { get; } = new ChangedSignatureChecker();

		// Token: 0x0600149E RID: 5278 RVA: 0x0003CD78 File Offset: 0x0003AF78
		private ChangedSignatureChecker()
		{
		}

		// Token: 0x0600149F RID: 5279 RVA: 0x0003CD80 File Offset: 0x0003AF80
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !ChangedSignatureChecker.\u0001(\u0002) || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x060014A0 RID: 5280 RVA: 0x0003CD98 File Offset: 0x0003AF98
		private static bool \u0001(global::\u0014.\u0003 \u0002)
		{
			foreach (_ISignature u in \u0002.CompileContext.GetAllSignaturesFlatEx().OfType<_ISignature>())
			{
				\u0002.Strategy.CheckNextPOU();
				if (ChangedSignatureChecker.\u0001(u, \u0002))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060014A1 RID: 5281 RVA: 0x0003CE04 File Offset: 0x0003B004
		private static bool \u0001(_ISignature \u0002, global::\u0014.\u0003 \u0003)
		{
			bool flag = \u0002.GetFlag(SignatureFlag.SuperGlobal) && !\u0002.GetFlag(SignatureFlag.InhibitOnlineChange);
			if (flag && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
			{
				flag = false;
			}
			if (\u0002.ObjectGuid == Guid.Empty || \u0002.GetFlag(SignatureFlag.Generated) || flag)
			{
				return false;
			}
			ISignature[] array = null;
			_IPreCompileContext ipreCompileContext = null;
			_ISignature u = \u0019.\u0001.\u0001(\u0003.CompileContext, \u0002, \u0003.PreCompileContext, \u0003.PreCompileContextPool, out array, out ipreCompileContext);
			if (ChangedSignatureChecker.\u0001(\u0002, u, \u0003))
			{
				return true;
			}
			bool result;
			if (ChangedSignatureChecker.\u0001(\u0002, u, \u0003, out result))
			{
				return result;
			}
			int num = ChangedSignatureChecker.\u0001(\u0002);
			IEnumerable<ISignature> enumerable;
			if (array == null)
			{
				enumerable = null;
			}
			else
			{
				enumerable = array.Where(new Func<ISignature, bool>(ChangedSignatureChecker.<>c.<>9.\u0001));
			}
			IEnumerable<ISignature> enumerable2 = enumerable;
			return enumerable2 != null && enumerable2.Count<ISignature>() > num && \u0003.Strategy.SubSignaturesChanged(\u0002);
		}

		// Token: 0x060014A2 RID: 5282 RVA: 0x0003CEFC File Offset: 0x0003B0FC
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, global::\u0014.\u0003 \u0004, out bool \u0005)
		{
			\u0005 = false;
			if (global::\u0014.\u0004.\u0001(\u0003, \u0002))
			{
				bool flag2;
				bool flag = ChangedSignatureChecker.\u0001(\u0002, \u0003, \u0004.Strategy, out flag2);
				if (flag2)
				{
					\u0005 = flag;
					return true;
				}
				if (\u0004.Strategy.SignatureChanged(\u0002, \u0003))
				{
					\u0005 = true;
					return true;
				}
			}
			return false;
		}

		// Token: 0x060014A3 RID: 5283 RVA: 0x0003CF44 File Offset: 0x0003B144
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, _IIsUpTopDateStrategy \u0004, out bool \u0005)
		{
			\u0005 = true;
			if (\u0003 != null && \u0002.GetFlag(SignatureFlag.InterfaceLibraryObject) && \u0002.POUType == Operator.VarGlobal)
			{
				return false;
			}
			if (\u0003 != null && \u0002.GetFlag(SignatureFlag.InterfaceLibraryObject))
			{
				return false;
			}
			if (\u0002.HasAttribute("omit_uptodate_check"))
			{
				return ChangedSignatureChecker.\u0001(\u0002, \u0003, \u0004);
			}
			\u0005 = false;
			return false;
		}

		// Token: 0x060014A4 RID: 5284 RVA: 0x0003CFA4 File Offset: 0x0003B1A4
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, global::\u0014.\u0003 \u0004)
		{
			return !\u0004.CompileContext.SimulationMode && \u0003 != null && \u0003.GetFlag(SignatureFlag.External) != \u0002.GetFlag(SignatureFlag.External) && \u0004.Strategy.ExternalSignatureFlagChanged();
		}

		// Token: 0x060014A5 RID: 5285 RVA: 0x0003CFDC File Offset: 0x0003B1DC
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, _IIsUpTopDateStrategy \u0004)
		{
			return \u0003 != null && \u0002.HasAttribute("uptodate_check_checksum") && \u0003.HasAttribute("uptodate_check_checksum") && \u0002.GetAttributeValue("uptodate_check_checksum") != \u0003.GetAttributeValue("uptodate_check_checksum") && \u0004.SignatureChangedByChecksumAttribute(\u0002, \u0003);
		}

		// Token: 0x060014A6 RID: 5286 RVA: 0x0003D030 File Offset: 0x0003B230
		private static int \u0001(_ISignature \u0002)
		{
			int num = 0;
			foreach (object obj in \u0002._SubSignatures)
			{
				ISignature signature = (ISignature)obj;
				if (!(signature.ObjectGuid == Guid.Empty) && !signature.GetFlag(SignatureFlag.Generated))
				{
					num++;
				}
			}
			return num;
		}

		// Token: 0x04000393 RID: 915
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
