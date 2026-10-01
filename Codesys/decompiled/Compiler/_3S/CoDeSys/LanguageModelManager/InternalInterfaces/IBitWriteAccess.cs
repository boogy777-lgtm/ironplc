using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IBitWriteAccess
	{
		int SignatureId { get; }

		IMinimalPosition Position { get; }

		string Symbol { get; }

		int Area { get; }

		int Offset { get; }

		byte BitNr { get; }
	}
}
