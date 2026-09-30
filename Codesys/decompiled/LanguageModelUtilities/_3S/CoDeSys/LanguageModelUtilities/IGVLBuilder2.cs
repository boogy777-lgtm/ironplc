using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IGVLBuilder2 : IGVLBuilder, IHasVarDeclaration, IHasAttributes
	{
		void AddMessage(string stMessage, Severity severity, Guid gdObject, IExprementPosition exprementPosition, short sLength);
	}
}
