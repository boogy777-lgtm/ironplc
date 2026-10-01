using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	public abstract class AbstractPOUWithAttributes : AbstractPOU
	{
		private List<Attribute> _lstAttributes = new List<Attribute>();

		public void AddAttribute(string stAttribute)
		{
			AddAttribute(stAttribute, null);
		}

		public void AddAttribute(string stAttribute, string stAttributeValue)
		{
			_lstAttributes.Add(new Attribute(stAttribute, stAttributeValue));
		}

		protected override void AddAttributes(IExprementPosition pos, ISequenceStatement2 pouInterface)
		{
			if (0 >= _lstAttributes.Count || !(_lmbuilder is ILanguageModelBuilder3 languageModelBuilder))
			{
				return;
			}
			foreach (Attribute lstAttribute in _lstAttributes)
			{
				pouInterface.AddStatement(languageModelBuilder.CreatePragmaAttributeStatement(pos, lstAttribute.AttributeName, lstAttribute.AttributeValue));
			}
		}
	}
}
