using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPartialAccessExpression : IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IExprement
	{
		IExpression Left { get; }

		DirectVariableSize PartSize { get; }

		int PartOffset { get; }
	}
}
