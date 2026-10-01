using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities11 : IPreCompileUtilities10, IPreCompileUtilities9, IPreCompileUtilities8, IPreCompileUtilities7, IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		ILocalCodeWriter CreateLocalCodeWriter(IPreCompileContext localContext);

		IEnumerable<IAttributedString> GetSignatureDeclarationTokens(ISignature sign, Guid appGuid, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope);

		IEnumerable<IAttributedString> GetTypeDeclarationTokens(IType type, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope);

		IEnumerable<IAttributedString> GetExpressionTokens(IExpression expr, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope);

		IEnumerable<IAttributedString> GetPropertyDeclarationTokens(IVariable var, ISignature signGetter, ISignature signSetter, ILMPreCompileSet preCompileSet, ISignature signatureForCreatingScope);

		string GetPOUTypeString(ISignature signature);
	}
}
