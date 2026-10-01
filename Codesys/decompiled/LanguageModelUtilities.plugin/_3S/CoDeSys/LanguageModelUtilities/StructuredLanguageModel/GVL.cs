using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{246F1999-6ACD-40EE-AA70-2A2CC6F5BC3C}")]
	public sealed class GVL : HasVarDeclaration, IGVLBuilder2, IGVLBuilder, IHasVarDeclaration, IHasAttributes
	{
		private string _stGlobalInitSlot;

		private SignatureFlag _signatureFlag;

		private List<GvlMessage> _lstMessages = new List<GvlMessage>();

		private List<Attribute> _lstAttributes = new List<Attribute>();

		public void SetGlobalInitSlot(string stGlobalInitSlot)
		{
			_stGlobalInitSlot = stGlobalInitSlot;
		}

		public void SetSignatureFlag(SignatureFlag signatureFlag)
		{
			_signatureFlag = signatureFlag;
		}

		public void AddErrorMessage(string stErrorMessage)
		{
			if (_lstMessages == null)
			{
				_lstMessages = new List<GvlMessage>();
			}
			_lstMessages.Add(new GvlMessage(stErrorMessage, Severity.Error, Guid.Empty, null, (short)stErrorMessage.Length));
		}

		public void AddMessage(string stErrorMessage, Severity severity, Guid gdObject, IExprementPosition exprementPosition, short sLength)
		{
			if (_lstMessages == null)
			{
				_lstMessages = new List<GvlMessage>();
			}
			_lstMessages.Add(new GvlMessage(stErrorMessage, severity, gdObject, exprementPosition, sLength));
		}

		public void AddAttribute(string stAttribute)
		{
			AddAttribute(stAttribute, null);
		}

		public void AddAttribute(string stAttribute, string stAttributeValue)
		{
			_lstAttributes.Add(new Attribute(stAttribute, stAttributeValue));
		}

		public override void AddToLanguageModel(IExprementPosition pos, ILanguageModel languageModel)
		{
			ILMGlobVarlist iLMGlobVarlist = _lmbuilder.CreateGlobVarlist(_stName, _gdObject);
			ISequenceStatement value = null;
			ISequenceStatement value2 = null;
			bool num = _htVarDeclarations.TryGetValue(VarFlag.Global, out value);
			bool flag = _htVarDeclarations.TryGetValue(VarFlag.Constant | VarFlag.Global, out value2);
			if (num || flag)
			{
				ISequenceStatement sequenceStatement = _lmbuilder.CreateSequenceStatement(pos);
				ILanguageModelBuilder3 languageModelBuilder = _lmbuilder as ILanguageModelBuilder3;
				if (_lmbuilder != null)
				{
					if (!string.IsNullOrEmpty(_stGlobalInitSlot))
					{
						sequenceStatement.AddStatement(languageModelBuilder.CreatePragmaAttributeStatement(pos, CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT, _stGlobalInitSlot));
					}
					if (_signatureFlag != SignatureFlag.None)
					{
						sequenceStatement.AddStatement(languageModelBuilder.CreatePragmaAttributeStatement(pos, CompileAttributes.ATTRIBUTE_SIGNATURE_FLAG, $"{(long)_signatureFlag}"));
					}
				}
				if (0 < _lstMessages.Count)
				{
					foreach (GvlMessage lstMessage in _lstMessages)
					{
						sequenceStatement.AddStatement(CreateMessageStatement(_lmbuilder, lstMessage));
					}
				}
				if (0 < _lstAttributes.Count)
				{
					foreach (Attribute lstAttribute in _lstAttributes)
					{
						sequenceStatement.AddStatement(languageModelBuilder.CreatePragmaAttributeStatement(pos, lstAttribute.AttributeName, lstAttribute.AttributeValue));
					}
				}
				if (value != null)
				{
					sequenceStatement.AddStatement(_lmbuilder.CreateVariableDeclarationListStatement(null, VarFlag.Global, value));
				}
				if (value2 != null)
				{
					sequenceStatement.AddStatement(_lmbuilder.CreateVariableDeclarationListStatement(null, VarFlag.Constant | VarFlag.Global, value2));
				}
				iLMGlobVarlist.Interface = sequenceStatement;
			}
			languageModel.AddGlobalVariableList(iLMGlobVarlist);
		}

		private IStatement CreateMessageStatement(ILanguageModelBuilder lmbuilder, GvlMessage msg)
		{
			IStatement statement = lmbuilder.CreateEmptyStatement(null);
			if (lmbuilder is ILanguageModelBuilder8 languageModelBuilder)
			{
				languageModelBuilder.CreateCompilerMessage(msg.Position, statement, msg.Message, msg.ObjectGuid, msg.Length, msg.Severity, ShowAttribute.Compile);
			}
			else
			{
				lmbuilder.CreateCompilerMessage(msg.Position, statement, msg.Message, msg.Severity, ShowAttribute.Compile);
			}
			return statement;
		}
	}
}
