namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	internal sealed class Attribute
	{
		private string _stAttribute;

		private string _stAttributeValue;

		internal string AttributeName => _stAttribute;

		internal string AttributeValue => _stAttributeValue;

		internal Attribute(string stAttribute, string stAttributeValue)
		{
			_stAttribute = stAttribute;
			_stAttributeValue = stAttributeValue;
		}
	}
}
