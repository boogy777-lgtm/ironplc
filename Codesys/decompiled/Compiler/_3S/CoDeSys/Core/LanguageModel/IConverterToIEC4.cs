using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterToIEC4 : IConverterToIEC3, IConverterToIEC2, IConverterToIEC
	{
		string GetLiteralTextForEnum(object value, TypeClass baseTypeClass);
	}
}
