using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledType4 : ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope);

		object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope);

		bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope);

		byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope);
	}
}
