using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILanguageModelBuilder11 : ILanguageModelBuilder10, ILanguageModelBuilder9, ILanguageModelBuilder8, ILanguageModelBuilder7, ILanguageModelBuilder6, ILanguageModelBuilder5, ILanguageModelBuilder4, ILanguageModelBuilder3, ILanguageModelBuilder2, ILanguageModelBuilder
	{
		ISourcePosition CreateSourcePosition(int nProjectHandle, Guid objectGuid, long nPosition, short sPositionOffset, short nLength);

		IRuntimeVersionExpression CreateRuntimeVersionExpression(IExprementPosition pos, Version version, Operator opComparison);
	}
}
