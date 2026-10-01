using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IHasAttributes
	{
		string[] Attributes { get; }

		bool HasAttribute(string stAttribute);

		string GetAttributeValue(string stAttribute);
	}
}
