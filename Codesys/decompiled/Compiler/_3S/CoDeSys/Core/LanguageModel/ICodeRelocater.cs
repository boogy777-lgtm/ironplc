using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeRelocater
	{
		void DoRelocation(Stream bytestream, int uiCodeOffset, int uiDataBaseAddress);
	}
}
