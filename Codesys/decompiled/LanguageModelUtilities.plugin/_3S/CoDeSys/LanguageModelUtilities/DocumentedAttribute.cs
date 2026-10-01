using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class DocumentedAttribute : ICheckedAttribute, IAttribute
	{
		private string _name;

		private string _description;

		public string Description => _description;

		public Version RequiredCompilerVersion => null;

		public AttributeScope Scope => AttributeScope.All;

		public string Name => _name;

		public DocumentedAttribute(string name, string description)
		{
			_name = name;
			_description = description;
		}

		public bool CheckValue(string value, AttributeScope scope, ISignature signature, IVariable variable, out string error)
		{
			error = "";
			return true;
		}
	}
}
