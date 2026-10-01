using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICaseStatement : IStatement, IExprement
	{
		IExpression Switch { get; }

		ICase[] Cases { get; }

		IStatement Else { get; }
	}
}
