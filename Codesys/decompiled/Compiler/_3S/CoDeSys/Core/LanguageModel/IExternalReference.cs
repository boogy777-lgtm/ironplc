using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExternalReference
	{
		string Name { get; }

		IDataLocation PointerLocation { get; }
	}
}
