using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodePiece3 : ICodePiece2, ICodePiece
	{
		void SetCode(byte[] bytes);
	}
}
