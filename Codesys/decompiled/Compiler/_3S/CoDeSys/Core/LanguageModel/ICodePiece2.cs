using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodePiece2 : ICodePiece
	{
		int Length { get; }

		bool GetFlag(CodePieceFlag flag);
	}
}
