using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	public abstract class AbstractPOUWithMethods : AbstractPOUWithAttributes
	{
		private List<IMethodBuilder> _lstMethods;

		public void AddMethod(IMethodBuilder method)
		{
			if (_lstMethods == null)
			{
				_lstMethods = new List<IMethodBuilder>();
			}
			if (method is Method)
			{
				((Method)method).OwningPouGuid = _gdObject;
			}
			_lstMethods.Add(method);
		}

		protected override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
			if (_lstMethods == null)
			{
				return;
			}
			foreach (Method lstMethod in _lstMethods)
			{
				lstMethod.AddToLanguageModel(pos, languageModel);
			}
		}
	}
}
