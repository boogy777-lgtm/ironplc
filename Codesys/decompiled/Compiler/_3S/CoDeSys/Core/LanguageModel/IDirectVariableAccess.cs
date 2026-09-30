using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDirectVariableAccess
	{
		IDirectVariable DirectVariable { get; }

		IAccessInfo2 AccessInfo { get; }
	}
}
