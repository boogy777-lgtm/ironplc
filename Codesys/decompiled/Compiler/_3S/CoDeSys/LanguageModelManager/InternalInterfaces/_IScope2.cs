using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IScope2 : _IScope, IScope5, IScope4, IScope3, IScope2, IScope
	{
		_IScope CreateLibraryScope(string stLibPath);
	}
}
