using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u000F;
using \u0014;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x02000121 RID: 289
	internal sealed class \u0003 : global::\u000F.\u0006
	{
		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x060014AB RID: 5291 RVA: 0x0003D0E0 File Offset: 0x0003B2E0
		internal static global::\u000F.\u0006 Instance { get; } = new \u007F.\u0003();

		// Token: 0x060014AC RID: 5292 RVA: 0x0003D0E8 File Offset: 0x0003B2E8
		private \u0003()
		{
		}

		// Token: 0x060014AD RID: 5293 RVA: 0x0003D0F0 File Offset: 0x0003B2F0
		public bool \u0001(global::\u0014.\u0003 \u0002)
		{
			return !\u007F.\u0003.\u0001(\u0002) || \u0002.Strategy.IsUpToDate;
		}

		// Token: 0x060014AE RID: 5294 RVA: 0x0003D108 File Offset: 0x0003B308
		private static bool \u0001(global::\u0014.\u0003 \u0002)
		{
			using (IEnumerator<_ICompiledPOU> enumerator = \u0002.CompileContext.CompiledPOUList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (\u007F.\u0003.\u0001(enumerator.Current, \u0002))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060014AF RID: 5295 RVA: 0x0003D164 File Offset: 0x0003B364
		private static bool \u0001(_ICompiledPOU \u0002, global::\u0014.\u0003 \u0003)
		{
			if (\u0002.ObjectGuid == Guid.Empty)
			{
				return false;
			}
			if (\u0002.GetFlag(CompiledPOUFlags.NotForUpToDate))
			{
				return false;
			}
			_ICompiledPOU icompiledPOU = \u0019.\u0001.\u0001(\u0003.CompileContext, \u0002, \u0003.PreCompileContext, \u0003.PreCompileContextPool);
			if (!\u007F.\u0003.\u0001(icompiledPOU, \u0002))
			{
				return false;
			}
			ISignature signature = \u0003.CompileContext[\u0002.SignatureId];
			if (signature != null && signature.GetFlag(SignatureFlag.SuperGlobal) && !signature.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL))
			{
				return false;
			}
			ISignature signature2;
			if (signature == null || !(signature.Name == IdentifierConstants.MainSignatureName))
			{
				signature2 = signature;
			}
			else
			{
				ISignature signature3 = \u0003.CompileContext[signature.ParentSignatureId];
				signature2 = signature3;
			}
			ISignature signature4 = signature2;
			return (signature4 == null || !signature4.HasAttribute("omit_uptodate_check")) && \u0003.Strategy.CompiledPouChanged(signature as _ISignature, \u0002, icompiledPOU);
		}

		// Token: 0x060014B0 RID: 5296 RVA: 0x0003D238 File Offset: 0x0003B438
		private static bool \u0001(_ICompiledPOU \u0002, _ICompiledPOU \u0003)
		{
			return \u0002 == null || (\u0003.Checksum != 0U && \u0002.Checksum != \u0003.Checksum);
		}

		// Token: 0x04000396 RID: 918
		[CompilerGenerated]
		private static readonly global::\u000F.\u0006 \u0001;
	}
}
