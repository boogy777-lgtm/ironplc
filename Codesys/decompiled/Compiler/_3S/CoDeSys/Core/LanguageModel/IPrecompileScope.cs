using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileScope
	{
		ISignature LocalSignature { get; }

		bool FindDeclaration(string stIdent, out IVariable variable, out ISignature signature, out IPrecompileScope scope);

		ISignature FindSignatureLocal(string stIdent);

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use FindSignatureGlobal in IPrecompileScope2 instead")]
		ISignature FindSignatureGlobal(IQualifiedNameExpression qne);

		ISignature FindSignatureGlobal(string stIdent);

		IPrecompileScope GlobalScope();

		IPrecompileScope NewLocalScope(string stName);

		IPrecompileScope NewLocalScope(ISignature sign);

		IIdentifierInfo[] GetAllDeclarations();
	}
}
