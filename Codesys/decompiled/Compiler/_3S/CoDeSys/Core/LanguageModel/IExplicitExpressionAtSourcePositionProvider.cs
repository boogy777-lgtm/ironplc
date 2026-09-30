using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExplicitExpressionAtSourcePositionProvider
	{
		IExprement GetExpressionAtSourcePosition(ISourcePosition position, WhatToFind whatToFind, out IPreCompileContext preCompileContext);
	}
}
