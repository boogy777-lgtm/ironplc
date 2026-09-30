using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	public abstract class HasVarDeclaration : IHasVarDeclaration
	{
		protected ILanguageModelBuilder _lmbuilder;

		protected string _stName;

		protected Guid _gdObject;

		protected Guid _gdMessage;

		protected Dictionary<VarFlag, ISequenceStatement> _htVarDeclarations;

		public string Name
		{
			get
			{
				return _stName;
			}
			set
			{
				_stName = value;
			}
		}

		public void Initialize(ILanguageModelBuilder lmbuilder, Guid gdObject, Guid gdMessage)
		{
			_lmbuilder = lmbuilder;
			_gdObject = gdObject;
			_gdMessage = gdMessage;
			_htVarDeclarations = new Dictionary<VarFlag, ISequenceStatement>();
		}

		public void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, TypeClass typeClass)
		{
			AddVariableDeclaration(pos, eVarFlag, stVariableName, typeClass, null);
		}

		public void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, TypeClass typeClass, IExpression expInitial)
		{
			ICompiledType type = _lmbuilder.CreateSimpleType(typeClass);
			AddVariableDeclaration(pos, eVarFlag, stVariableName, type, expInitial);
		}

		public void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, string stComplexType)
		{
			AddVariableDeclaration(pos, eVarFlag, stVariableName, stComplexType, null);
		}

		public void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, string stComplexType, IExpression expInitial)
		{
			IMessage message = null;
			ICompiledType type = _lmbuilder.CreateComplexType(stComplexType, out message);
			AddVariableDeclaration(pos, eVarFlag, stVariableName, type, expInitial);
		}

		public void AddAttribute(IExprementPosition pos, VarFlag eVarFlag, string stAttribute)
		{
			AddAttribute(pos, eVarFlag, stAttribute, null);
		}

		public void AddAttribute(IExprementPosition pos, VarFlag eVarFlag, string stAttribute, string stAttributeValue)
		{
			ISequenceStatement value = null;
			if (!_htVarDeclarations.TryGetValue(eVarFlag, out value))
			{
				value = _lmbuilder.CreateSequenceStatement(pos);
				_htVarDeclarations.Add(eVarFlag, value);
			}
			ILanguageModelBuilder3 languageModelBuilder = _lmbuilder as ILanguageModelBuilder3;
			if (_lmbuilder != null)
			{
				value.AddStatement(languageModelBuilder.CreatePragmaAttributeStatement(pos, stAttribute, stAttributeValue));
			}
		}

		public abstract void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel);

		private void AddVariableDeclaration(IExprementPosition pos, VarFlag eVarFlag, string stVariableName, ICompiledType type, IExpression expInitial)
		{
			ISequenceStatement value = null;
			if (!_htVarDeclarations.TryGetValue(eVarFlag, out value))
			{
				value = _lmbuilder.CreateSequenceStatement(pos);
				_htVarDeclarations.Add(eVarFlag, value);
			}
			IVariableDeclarationStatement state = _lmbuilder.CreateVariableDeclarationStatement(pos, stVariableName, type, expInitial, null);
			value.AddStatement(state);
		}
	}
}
