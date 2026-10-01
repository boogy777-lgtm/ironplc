using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder8 : ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		IEnumerable<KeyValuePair<string, string>> GetAttributesDefinedAtTopOfSequence(ISequenceStatement stmt);

		IMessage4 CreateCompilerMessage(IExprementPosition position, IExprement exp, string stMessage, Guid gdObject, short sLength, Severity severity, ShowAttribute showatt);
	}
}
