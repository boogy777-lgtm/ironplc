using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileSmartCodingService
	{
		IEnumerable<IDeclarationInfo> ParseForUnknownIdentifiers(ILMPreCompileSet preCompileSet, string stCode, string stPOUName, string stSubObjectName);

		IEnumerable<IIdentifierInfo> FindSubelements(ILMPreCompileSet preCompileSet, Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError);

		IEnumerable<IIdentifierInfo> GetIdentifierInfo(ILMPreCompileSet preCompileSet, Guid guidSignature, string stAccessPath);

		IExpressionInfo GetExpressionInfo(ILMPreCompileSet preCompileSet, Guid guidSignature, string stExpression);

		IEnumerable<IIdentifierInfo> GetIdentifierInfoAtSourcePosition(string stName, ISourcePosition sourcepos, WhatToFind whattofind);

		IExprement FindExpressionAtSourcePosition(ISourcePosition sourcepos, WhatToFind whattofind, out ILMPreCompileSet precom);

		bool CheckPOUCode(ILMPreCompileSet preCompileSet, Guid guidObject, IList<IMessage4> compilermessages);

		bool CheckSignature(ILMPreCompileSet preCompileSet, Guid guidObject, IList<IMessage4> compilermessages);

		void DeriveAccessPathInformation(ILMPreCompileSet preCompileSet, Guid guidSignature, int nProjectHandle, string stAccessPathOrType, out bool bError, out IPrecompileScope derivedScope, out ISignature derivedSignature, out IVariable derivedVariable, out IType derivedType, out IPrecompileScope searchScope);
	}
}
