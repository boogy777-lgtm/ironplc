using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IAddressCodePositionSerializable
	{
		long PositionToSave { get; set; }

		AccessFlag Access { get; set; }

		int TypeSize { get; set; }
	}
}
