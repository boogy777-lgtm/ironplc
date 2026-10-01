using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILiteralExpression2 : ILiteralExpression, IExpression2, IExpression, IExprement
	{
		TypeClass OriginalType { get; }
	}
}
