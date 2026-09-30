using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities7 : IPreCompileUtilities6, IPreCompileUtilities5, IPreCompileUtilities4, IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		string AddDeviceApplicationPrefix(string stWatchExpression);

		int GetIntValue(IExpression expr, IPrecompileScope scope, out bool bValid);

		ISignature ResolveAlias(ISignature signAlias, IPrecompileScope6 scope);

		IIdentifierInfo[] FindSubelements(Guid guidSignature, IPreCompileContext11 precom, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError);

		string GetScopeDeclarationIdentifier(IVariable variable);

		bool ExpressionHasSideEffects(IExpression expression, ISignature4 signature, IPreCompileContext9 pcc);
	}
}
