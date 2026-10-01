using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICallInstanceExpression : IExpression, IExprement
	{
		bool WriteAccess { get; }

		IIntermediateValueLocation IntermediateValueLocation { get; }
	}
}
