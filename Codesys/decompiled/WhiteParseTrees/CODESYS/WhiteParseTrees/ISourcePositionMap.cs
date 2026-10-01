using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface ISourcePositionMap
	{
		void GetPositionOffsetForTextOffset(int sourceTextOffset, out long position, out short offset);

		bool GetTextOffsetForPositionOffset(long position, short offset, out int textOffset);
	}
}
