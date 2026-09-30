using System.IO;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeRelocater2 : ICodeRelocater
	{
		void DoRelocation(Stream bytestream, IRelocation reloc, int uiDataBaseAddress);
	}
}
