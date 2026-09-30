using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarRef3 : IVarRef2, IVarRef
	{
		IExpression InstancePathExpression { get; }
	}
}
