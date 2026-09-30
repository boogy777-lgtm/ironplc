using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArchiveAuxiliaryWriter
	{
		void SaveAuxStream(string stName, ILMItemSaver saver);
	}
}
