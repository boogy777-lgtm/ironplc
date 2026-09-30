using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IProcessImageLocation
	{
		IDataLocation DataLocation { get; }

		int Size { get; }

		AccessFlag Access { get; }

		DirectVariableLocation AddressLocation { get; }
	}
}
