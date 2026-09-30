using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterFromIEC2 : IConverterFromIEC
	{
		void GetLiteralValue(string stIEC, out object value, out TypeClass typeClass, TypeClass typeClassExpected);
	}
}
