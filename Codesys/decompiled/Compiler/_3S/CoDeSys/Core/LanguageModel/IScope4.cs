using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScope4 : IScope3, IScope2, IScope
	{
		ICompiledPOU GetCompiledPOUById(int nId);
	}
}
