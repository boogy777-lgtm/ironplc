using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IExprementPosition
	{
		long Position { get; set; }

		short PositionOffset { get; set; }
	}
}
