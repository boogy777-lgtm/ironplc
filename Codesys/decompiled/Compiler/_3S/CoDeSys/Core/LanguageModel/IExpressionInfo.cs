using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExpressionInfo
	{
		IExpression Expression { get; }

		IType Type { get; }
	}
}
