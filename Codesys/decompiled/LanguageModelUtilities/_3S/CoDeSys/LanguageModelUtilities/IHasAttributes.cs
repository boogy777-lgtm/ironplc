using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IHasAttributes
	{
		void AddAttribute(string stAttribute);

		void AddAttribute(string stAttribute, string stAttributeValue);
	}
}
