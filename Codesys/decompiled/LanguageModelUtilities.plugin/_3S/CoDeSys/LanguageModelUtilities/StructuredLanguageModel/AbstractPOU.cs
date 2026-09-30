using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	public abstract class AbstractPOU : HasVarDeclaration, IPouBuilder, IHasVarDeclaration
	{
		protected ISequenceStatement _body;

		private bool _bInhibitOnlineChange;

		protected abstract Operator PouType { get; }

		public void AddStatement(IExprementPosition pos, IStatement statement)
		{
			if (_body == null)
			{
				_body = _lmbuilder.CreateSequenceStatement(pos);
			}
			_body.AddStatement(statement);
		}

		public void SetInhibitOnlineChange(bool bInhibitOnlineChange)
		{
			_bInhibitOnlineChange = bInhibitOnlineChange;
		}

		public override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel)
		{
			ILMPOU iLMPOU = _lmbuilder.CreatePou(_stName, _gdObject);
			iLMPOU.MessageGuid = _gdMessage;
			iLMPOU.InhibitOnlineChange = _bInhibitOnlineChange;
			IPOUDeclarationStatement iPOUDeclarationStatement = _lmbuilder.CreatePOUDeclarationStatement(null, PouType, _stName);
			ISequenceStatement2 sequenceStatement = _lmbuilder.CreateSequenceStatement(pos);
			AddAttributes(pos, sequenceStatement);
			sequenceStatement.AddStatement(iPOUDeclarationStatement);
			iLMPOU.Interface = sequenceStatement;
			if (0 < _htVarDeclarations.Count)
			{
				List<IVariableDeclarationListStatement> list = new List<IVariableDeclarationListStatement>();
				foreach (KeyValuePair<VarFlag, ISequenceStatement> htVarDeclaration in _htVarDeclarations)
				{
					IVariableDeclarationListStatement item = _lmbuilder.CreateVariableDeclarationListStatement(pos, htVarDeclaration.Key, htVarDeclaration.Value);
					list.Add(item);
				}
				iPOUDeclarationStatement.DeclarationLists = list;
			}
			AddToLanguageModel(pos, languageModel, iLMPOU, iPOUDeclarationStatement);
			if (_body != null)
			{
				iLMPOU.Body = _body;
			}
			languageModel.AddPou(iLMPOU);
		}

		protected virtual void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel, ILMPOU pou, IPOUDeclarationStatement pouDeclaration)
		{
		}

		protected virtual void AddAttributes(IExprementPosition pos, ISequenceStatement2 pouInterface)
		{
		}

		private void AddVariableDeclaration(VarFlag eVarFlag, string stVariableName, ICompiledType type, IExpression expInitial)
		{
			ISequenceStatement value = null;
			if (!_htVarDeclarations.TryGetValue(eVarFlag, out value))
			{
				value = _lmbuilder.CreateSequenceStatement(null);
				_htVarDeclarations.Add(eVarFlag, value);
			}
			IVariableDeclarationStatement state = _lmbuilder.CreateVariableDeclarationStatement(null, stVariableName, type, expInitial, null);
			value.AddStatement(state);
		}
	}
}
