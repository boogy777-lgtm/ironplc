using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterToIEC5 : IConverterToIEC4, IConverterToIEC3, IConverterToIEC2, IConverterToIEC
	{
		string GetLiteralText(object value, TypeClass typeClass, int iCountDecimalPlaces);

		string GetReal(object value, TypeClass typeClass, int iCountDecimalPlaces);
	}
}
