using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVarRef4 : IVarRef3, IVarRef2, IVarRef
	{
		string DisplayExpression { get; set; }
	}
}
