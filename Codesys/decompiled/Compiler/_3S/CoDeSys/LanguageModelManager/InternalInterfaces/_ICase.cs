using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICase : ICase
	{
		_ICaseLabelStatement _Label { get; }

		_IStatement _Controlled { get; }
	}
}
