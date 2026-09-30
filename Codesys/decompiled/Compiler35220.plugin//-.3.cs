using System;
using System.Collections.Generic;
using \u0007;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x020000B5 RID: 181
	internal static class \u0003
	{
		// Token: 0x06000E3B RID: 3643 RVA: 0x0002670C File Offset: 0x0002490C
		internal static void \u0001(_ICompileContext \u0002)
		{
			IEnumerable<_ISignature> pousignatures = \u0002.POUSignatures;
			IScope5 u = \u0005.\u0001(\u0002);
			foreach (_ISignature isignature in pousignatures)
			{
				if (isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Interface)
				{
					if (isignature.POUType == Operator.Interface)
					{
						_ISignature isignature2 = \u0002[CompilerServicesInternal.\u0001(isignature, \u0002)];
						if (isignature2 != null && isignature2.GetFlag(SignatureFlag.TopLevel))
						{
							isignature.SetFlag(SignatureFlag.TopLevel, true);
						}
					}
					\u0003.\u0002(\u0002, isignature, u);
					\u0003.\u0001(\u0002, isignature, u);
					\u0003.\u0001(\u0002, isignature);
				}
			}
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x000267C0 File Offset: 0x000249C0
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			if (\u0003.POUType == Operator.FunctionBlock && \u0003.GetFlag(SignatureFlag.TopLevel))
			{
				foreach (object obj in \u0003._SubSignatures)
				{
					_ISignature u = (_ISignature)obj;
					\u0003.\u0002(\u0002, u);
				}
			}
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x00026834 File Offset: 0x00024A34
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			if (\u0003.BaseSignatureId != Helper.InvalidId)
			{
				ISignature signature = \u0004[\u0003.BaseSignatureId];
				if (signature.GetFlag(SignatureFlag.TopLevel))
				{
					\u0003.SetFlag(SignatureFlag.TopLevel, true);
				}
				IScope5 scope = \u0005.\u0001(\u0002, signature.Id);
				foreach (object obj in \u0003._SubSignatures)
				{
					_ISignature isignature = (_ISignature)obj;
					_ISignature isignature2 = scope.FindSignatureLocal(isignature.Name) as _ISignature;
					if (isignature2 != null && isignature2.HasFlag(SignatureFlag.TopLevel))
					{
						\u0003.\u0002(\u0002, isignature);
					}
				}
			}
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x000268FC File Offset: 0x00024AFC
		private static bool \u0001(_ISignature \u0002)
		{
			return \u0002.GetFlag(SignatureFlag.TopLevel);
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x0002690C File Offset: 0x00024B0C
		private static void \u0002(_ICompileContext \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			\u0004 u = new \u0004(\u0003, \u0004, new Func<_ISignature, bool>(\u0003.\u0001));
			u.\u0001();
			using (IEnumerator<_ISignature> enumerator = u.Interfaces.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.GetFlag(SignatureFlag.TopLevel))
					{
						\u0003.SetFlag(SignatureFlag.TopLevel, true);
					}
				}
			}
			foreach (_ISignature isignature in u.InterfaceMethods)
			{
				if (isignature.HasFlag(SignatureFlag.TopLevel))
				{
					_ISignature isignature2 = \u0003.GetSubSignature(isignature.Name) as _ISignature;
					if (isignature2 != null)
					{
						\u0003.\u0002(\u0002, isignature2);
					}
				}
			}
		}

		// Token: 0x06000E40 RID: 3648 RVA: 0x000269E4 File Offset: 0x00024BE4
		private static void \u0002(_ICompileContext \u0002, _ISignature \u0003)
		{
			\u0003.SetFlag(SignatureFlag.TopLevel, true);
			_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0003.Id);
			if (icompiledPOU == null)
			{
				return;
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
		}
	}
}
