using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodeAdapter5 : ICodeAdapter4, ICodeAdapter3, ICodeAdapter2, ICodeAdapter
	{
		byte[] GetStringBytes(string st, TypeClass tc, bool MotorolaByteOrder, int nAlignment);
	}
}
