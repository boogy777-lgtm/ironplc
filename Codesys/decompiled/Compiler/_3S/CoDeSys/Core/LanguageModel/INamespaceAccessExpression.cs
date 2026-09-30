using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface INamespaceAccessExpression : IExpression, IExprement
	{
		IExpression Namespace { get; }

		IExpression Access { get; }
	}
}
