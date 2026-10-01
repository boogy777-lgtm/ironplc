using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodePiece
	{
		IDataLocation Destination { get; }

		byte[] Code { get; }
	}
}
