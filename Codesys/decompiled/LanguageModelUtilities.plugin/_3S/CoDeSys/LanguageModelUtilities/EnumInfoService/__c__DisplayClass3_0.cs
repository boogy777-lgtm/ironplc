using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class EnumInfoService
	{
		private static ISignature GetFullyResolvedSignature(IPrecompileScope2 scope, IType type, out ISignature originalSignature)
		{
			HashSet<ISignature> hashSet = null;
			originalSignature = null;
			ISignature signature;
			while (true)
			{
				if (scope == null || type == null || type.Class != TypeClass.Userdef)
				{
					return null;
				}
				IUserdefType2 userdefType = (IUserdefType2)type;
				signature = scope.FindSignatureGlobal(userdefType.NameExpression);
				if (signature == null)
				{
					return null;
				}
				originalSignature = originalSignature ?? signature;
				if (!signature.GetFlag(SignatureFlag.Alias))
				{
					break;
				}
				hashSet = hashSet ?? new HashSet<ISignature>();
				if (!hashSet.Add(signature) || signature.All.Length == 0)
				{
					return null;
				}
				scope = scope.NewLocalScope(signature) as IPrecompileScope2;
				type = signature.All[0].Type;
			}
			return signature;
		}

		private static ISignature TryGetEnumSignature(IPrecompileScope2 scope, IType type, out ISignature originalSignature)
		{
			ISignature fullyResolvedSignature = GetFullyResolvedSignature(scope, type, out originalSignature);
			if (fullyResolvedSignature != null && fullyResolvedSignature.GetFlag(SignatureFlag.Enum))
			{
				return fullyResolvedSignature;
			}
			return null;
		}

		private static bool IsQualifiedOnly(ISignature sign, IPreCompileContext12 pcc, out string stNamespace)
		{
			stNamespace = null;
			if (pcc == null)
			{
				throw new ArgumentNullException("pcc");
			}
			if (sign == null)
			{
				throw new ArgumentNullException("sign");
			}
			if (pcc.LibraryTable is ILibraryTable4 libraryTable && !string.IsNullOrEmpty(sign.LibraryPath))
			{
				bool? qualifiedOnlyRecursive = libraryTable.GetQualifiedOnlyRecursive(pcc, sign.LibraryPath);
				if (qualifiedOnlyRecursive.HasValue && qualifiedOnlyRecursive.Value)
				{
					stNamespace = libraryTable.GetLocalLibraryNamespaceRecursive(pcc, sign.LibraryPath);
					return true;
				}
			}
			return false;
		}

		internal static bool TryGetEnumerationItems(ILMPreCompileSet rootPcc, IPrecompileScope2 typeScope, IType type, out IEnumerable<string> enumMemberNames)
		{
			enumMemberNames = null;
			if (rootPcc == null || type == null || typeScope == null)
			{
				return false;
			}
			ISignature originalSignature;
			ISignature signature = TryGetEnumSignature(typeScope, type, out originalSignature);
			if (signature == null)
			{
				return false;
			}
			string stType = type.ToString();
			if (IsQualifiedOnly(signature, rootPcc as IPreCompileContext12, out var stNamespace))
			{
				stType = stNamespace + "." + originalSignature.OrgName;
			}
			enumMemberNames = signature.All.Select((IVariable v) => stType + "." + v.OrgName);
			return true;
		}
	}
}
