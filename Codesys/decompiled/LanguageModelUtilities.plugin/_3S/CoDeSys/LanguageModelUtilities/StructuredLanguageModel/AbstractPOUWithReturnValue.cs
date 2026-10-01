using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	public abstract class AbstractPOUWithReturnValue : AbstractPOUWithAttributes
	{
		private ICompiledType _returnType;

		public void SetReturnType(TypeClass typeClass)
		{
			_returnType = _lmbuilder.CreateSimpleType(typeClass);
		}

		public void SetComplexReturnType(string stComplexType)
		{
			IMessage message = null;
			_returnType = _lmbuilder.CreateComplexType(stComplexType, out message);
		}

		protected override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
			if (_returnType != null)
			{
				pouDeclaration.ReturnType = _returnType;
			}
		}
	}
}
