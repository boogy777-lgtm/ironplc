using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeRelocator64
	{
		void DoRelocation(Stream bytestream, int uiCodeOffset, ulong ulDataBaseAddress);

		void DoRelocation(Stream bytestream, IRelocation reloc, ulong ulDataBaseAddress);
	}
}
