using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPreCompileContext : ICompileContextCommon
	{
		string Namespace { get; }

		string LibraryPath { get; }

		IDeclarationInfo[] ParseForUnknownIdentifiers(string stCode, string stPOUName, string stSubObjectName);

		ISignature[] FindSignature(string stName);

		IIdentifierInfo[] FindSubelements(Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError);

		IIdentifierInfo[] GetIdentifierInfo(Guid guidSignature, string stAccessPath);

		IExpressionInfo GetExpressionInfo(Guid guidSignature, string stExpression);

		bool IsEmpty();

		IPrecompileScope CreatePrecompileScope(Guid guidSignature);
	}
}
