using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IBitWriteAccessSerializable
	{
		long PositionCombination { get; set; }

		int SignatureId { get; }

		IMinimalPosition Position { get; }

		string Symbol { get; }

		int Area { get; }

		int Offset { get; }

		byte BitNr { get; }
	}
}
