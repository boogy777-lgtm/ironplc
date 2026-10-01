using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataLocation2 : IDataLocation
	{
		bool GetFlag(DataLocationFlag dlFlag);
	}
}
