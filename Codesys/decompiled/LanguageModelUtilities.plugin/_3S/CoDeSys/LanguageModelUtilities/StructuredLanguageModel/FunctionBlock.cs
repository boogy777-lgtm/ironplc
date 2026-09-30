using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{7130CE3C-9906-48AD-AA36-2139B1E6AF04}")]
	public class FunctionBlock : AbstractPOUWithMethods, IFunctionBlockBuilder2, IFunctionBlockBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		private List<IExpression> _lstExtends;

		private List<IExpression> _lstImplements;

		protected override Operator PouType => Operator.FunctionBlock;

		public void SetExtends(IExprementPosition pos, string stNamespace, string stBaseFunctionBlock)
		{
			_lstExtends = new List<IExpression>();
			IExpression item = _lmbuilder.CreateCompoAccessExpression(pos, _lmbuilder.ParseExpression(stNamespace), _lmbuilder.CreateVariableExpression(null, stBaseFunctionBlock));
			_lstExtends.Add(item);
		}

		public void AddImplements(IExprementPosition pos, string stNamespace, string stInterface)
		{
			if (_lstImplements == null)
			{
				_lstImplements = new List<IExpression>();
			}
			IExpression expression = null;
			expression = ((!string.IsNullOrEmpty(stNamespace)) ? ((IExpression2)_lmbuilder.CreateCompoAccessExpression(pos, _lmbuilder.CreateVariableExpression(null, stNamespace), _lmbuilder.CreateVariableExpression(null, stInterface))) : ((IExpression2)_lmbuilder.CreateVariableExpression(null, stInterface)));
			_lstImplements.Add(expression);
		}

		protected override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
			if (_lstExtends != null)
			{
				pouDeclaration.ExtendsList = _lstExtends;
			}
			if (_lstImplements != null)
			{
				pouDeclaration.ImplementsList = _lstImplements;
			}
			base.AddToLanguageModel(pos, languageModel, pou, pouDeclaration);
		}
	}
}
