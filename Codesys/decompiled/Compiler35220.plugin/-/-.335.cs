using System;
using System.Collections.Generic;
using System.Linq;
using \u0011;
using \u0018;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Compiler35220.OnlineChange.FastOnlinechange.Steps;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0002
{
	// Token: 0x0200037A RID: 890
	internal sealed class \u000F : \u001E
	{
		// Token: 0x06003450 RID: 13392 RVA: 0x000CDDAC File Offset: 0x000CBFAC
		private bool \u0001(\u0018.\u0010 \u0002, _ISignature \u0003)
		{
			_ISignature isignature = \u0002.ComconNew.GetSignatureById(\u0003.ParentSignatureId) as _ISignature;
			return \u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants) || (isignature != null && isignature.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants));
		}

		// Token: 0x06003451 RID: 13393 RVA: 0x000CDDF4 File Offset: 0x000CBFF4
		private bool \u0002(\u0018.\u0010 \u0002, _ISignature \u0003)
		{
			return LicenseCheckGenerator.\u0001(\u0003) || this.\u0001(\u0002, \u0003);
		}

		// Token: 0x06003452 RID: 13394 RVA: 0x000CDE08 File Offset: 0x000CC008
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			\u0002.compiledpous = new LList<_ICompiledPOU>();
			\u0002.changedSignatures = new LList<global::\u0011.\u0014>();
			foreach (_ISignature u in \u0002.ComconNew.GetAllSignaturesFlatEx().OfType<_ISignature>())
			{
				if (!this.\u0003(\u0002, u))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06003453 RID: 13395 RVA: 0x000CDE80 File Offset: 0x000CC080
		private bool \u0003(\u0018.\u0010 \u0002, _ISignature \u0003)
		{
			if (\u0003.ObjectGuid == Guid.Empty || (\u0003.GetFlag(SignatureFlag.Generated) && !this.\u0002(\u0002, \u0003)))
			{
				return true;
			}
			ISignature[] array;
			_IPreCompileContext ipreCompileContext;
			_ISignature isignature = Helper.\u0001(\u0002.ComconNew, \u0003, \u0002.Precomp, \u0002.PrecompPool, out array, out ipreCompileContext);
			if (isignature == null)
			{
				return true;
			}
			if (isignature.HasAttribute("delayed_languagemodel_provision"))
			{
				\u0002.ComconNew.CreateCompiledSignature(isignature, \u0003, ipreCompileContext, \u0002.ComconNew);
				isignature = ipreCompileContext[isignature.ObjectGuid];
			}
			if (\u0003.Checksum != isignature.Checksum)
			{
				if (global::\u0002.\u000F.\u0001(\u0003))
				{
					return false;
				}
				global::\u0011.\u0014 u = SignatureChangesInspector.\u0001(\u0002.ComconNew, \u0003, isignature);
				if (this.\u0001(\u0003, u))
				{
					return false;
				}
				\u0002.changedSignatures.Add(u);
			}
			return global::\u0002.\u000F.\u0001(\u0002, \u0003) ?? true;
		}

		// Token: 0x06003454 RID: 13396 RVA: 0x000CDF68 File Offset: 0x000CC168
		private static bool? \u0001(\u0018.\u0010 \u0002, _ISignature \u0003)
		{
			_ICompiledPOU icompiledPOU;
			_ICompiledPOU icompiledPOU2;
			if (!\u0002.\u0002(\u0003, out icompiledPOU, out icompiledPOU2))
			{
				return new bool?(true);
			}
			if (icompiledPOU2.Checksum != icompiledPOU.Checksum || LicenseCheckGenerator.\u0001(\u0003))
			{
				if (\u0003.POUType == Operator.Method && \u0003.Name == IdentifierConstants.InitMethodName)
				{
					return new bool?(false);
				}
				\u0002.\u0001(icompiledPOU2, icompiledPOU);
			}
			return null;
		}

		// Token: 0x06003455 RID: 13397 RVA: 0x000CDFD4 File Offset: 0x000CC1D4
		private bool \u0001(_ISignature \u0002, global::\u0011.\u0014 \u0003)
		{
			return \u0003.\u0012 || \u0003.\u0001 || \u0003.\u0013 || \u0003.\u0014 || \u0003.\u0002 || \u0003.\u0011 || \u0003.\u0015 || (\u0002.HasAttribute("subsequent") || \u0003.PrecompileSignature.HasAttribute("subsequent")) || (\u0002.POUType == Operator.VarGlobal && (\u0003.\u000F || \u0003.\u000E) && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY)) || global::\u0002.\u000F.\u0001(\u0002, \u0003) || global::\u0002.\u000F.\u0001(\u0003) || (\u0003.\u0007 && this.\u0001(\u0003));
		}

		// Token: 0x06003456 RID: 13398 RVA: 0x000CE098 File Offset: 0x000CC298
		private static bool \u0001(_ISignature \u0002, global::\u0011.\u0014 \u0003)
		{
			return (\u0003.VariableAdded || \u0003.VariableDeleted) && \u0002.Name.Equals("IoConfig_Globals_Mapping", StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x06003457 RID: 13399 RVA: 0x000CE0C0 File Offset: 0x000CC2C0
		private bool \u0001(global::\u0011.\u0014 \u0002)
		{
			using (IEnumerator<_IVariable> enumerator = \u0002.DeletedVariables.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.HasFlag(VarFlag.AllocateInInstance))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003458 RID: 13400 RVA: 0x000CE11C File Offset: 0x000CC31C
		private static bool \u0001(global::\u0011.\u0014 \u0002)
		{
			if (\u0002.\u000F || \u0002.\u0007)
			{
				foreach (_IVariable ivariable in \u0002.DeletedVariables)
				{
					ICompiledType compiledType = ivariable.CompiledType;
					if (compiledType.Class == TypeClass.Array)
					{
						compiledType = \u0084.\u0004.\u0001(compiledType);
					}
					if (compiledType.Class == TypeClass.Userdef)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06003459 RID: 13401 RVA: 0x000CE19C File Offset: 0x000CC39C
		private static bool \u0001(_ISignature \u0002)
		{
			return \u0002.POUType == Operator.Type || \u0002.GetFlag(SignatureFlag.ContainsVarConfig) || \u0002.GetFlag(SignatureFlag.Persistent);
		}

		// Token: 0x04000A2F RID: 2607
		internal const string \u0001 = "IoConfig_Globals_Mapping";
	}
}
