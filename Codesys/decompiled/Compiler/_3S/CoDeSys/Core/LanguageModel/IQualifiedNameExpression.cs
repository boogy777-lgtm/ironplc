using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IQualifiedNameExpression : IExpression2, IExpression, IExprement
	{
		string Name { get; }

		string Namespace { get; }

		int SignatureId { get; }
	}
}
