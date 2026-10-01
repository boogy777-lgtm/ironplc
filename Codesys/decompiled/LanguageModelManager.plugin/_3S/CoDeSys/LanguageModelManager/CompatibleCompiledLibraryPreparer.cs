using System;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200002E RID: 46
	internal static class CompatibleCompiledLibraryPreparer
	{
		// Token: 0x060001F7 RID: 503 RVA: 0x00006D04 File Offset: 0x00005D04
		internal static void Prepare(_IPreCompileContext context)
		{
			ExprementPreparer remover = new ExprementPreparer();
			CompatibleCompiledLibraryPreparer.PrecompileCompiledPOUs(context, remover);
			CompatibleCompiledLibraryPreparer.PrepareSignatures(context, remover);
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x00006D28 File Offset: 0x00005D28
		private static void PrepareSignatures(_IPreCompileContext context, ExprementPreparer remover)
		{
			foreach (_ISignature isignature in context.AllFlat)
			{
				bool interfaceProperty;
				CompatibleCompiledLibraryPreparer.RemoveIgnorableVariablesFromInterfaceMethods(context, isignature, out interfaceProperty);
				foreach (_IVariable ivariable in from x in isignature.AllVariables
				where x.Type.Class == TypeClass.Pointer && x.GetFlag(VarFlag.Constant)
				select x)
				{
					ivariable.SetFlag(VarFlag.ReplacedConstant, true);
				}
				isignature.SetFlag(SignatureFlag.RawSTProperty, false);
				foreach (_ICompilerMessage icompilerMessage in isignature.Messages.OfType<_ICompilerMessage>())
				{
					if (icompilerMessage.Severity == Severity.SuppressedInformation)
					{
						icompilerMessage.Severity = Severity.Information;
					}
					if (icompilerMessage.Severity == Severity.SuppressedWarning)
					{
						icompilerMessage.Severity = Severity.Warning;
					}
				}
				if (isignature.RawDeclaration != null)
				{
					remover.InterfaceProperty = interfaceProperty;
					((_ISequenceStatement)isignature.RawDeclaration.Interface).Accept(remover);
					remover.InterfaceProperty = false;
				}
			}
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x00006EA4 File Offset: 0x00005EA4
		private static void RemoveIgnorableVariablesFromInterfaceMethods(_IPreCompileContext context, _ISignature sign, out bool bInterfaceProperty)
		{
			bInterfaceProperty = false;
			if (sign.GetFlag(SignatureFlag.RawSTProperty))
			{
				_ISignature isignature = context.GetSignature(sign.ParentObjectGuid) as _ISignature;
				if (isignature != null && isignature.POUType == Operator.Interface)
				{
					bInterfaceProperty = true;
					foreach (_IVariable var in (from x in sign.AllVariables
					where x.HasAttribute("ignore_in_interface")
					select x).ToArray<_IVariable>())
					{
						sign.RemoveVariable(var);
					}
				}
			}
		}

		// Token: 0x060001FA RID: 506 RVA: 0x00006F34 File Offset: 0x00005F34
		private static void PrecompileCompiledPOUs(_IPreCompileContext context, ExprementPreparer remover)
		{
			foreach (_ICompiledPOU icompiledPOU in context.AllCompiledPOUs)
			{
				_ISequenceStatement isequenceStatement = (_ISequenceStatement)icompiledPOU.GetParseTree();
				_ISignature isignature = (_ISignature)context.GetSignature(icompiledPOU.ObjectGuid);
				if (isignature.GetFlag(SignatureFlag.RawSTProperty))
				{
					CompatibleCompiledLibraryPreparer.AddImplicitReturnValueCode(isequenceStatement, isignature, context);
				}
				CompatibleCompiledLibraryPreparer.RemoveSuppressedMessages(icompiledPOU);
				isequenceStatement.Accept(remover);
				icompiledPOU.SetParseTree(isequenceStatement);
			}
		}

		// Token: 0x060001FB RID: 507 RVA: 0x00006FC8 File Offset: 0x00005FC8
		private static void RemoveSuppressedMessages(_ICompiledPOU cpou)
		{
			foreach (_ICompilerMessage icompilerMessage in cpou.Messages)
			{
				if (icompilerMessage.Severity == Severity.SuppressedInformation)
				{
					icompilerMessage.Severity = Severity.Information;
				}
				if (icompilerMessage.Severity == Severity.SuppressedWarning)
				{
					icompilerMessage.Severity = Severity.Warning;
				}
			}
			if (cpou.Messages != null && cpou.Messages.Any<_ICompilerMessage>())
			{
				cpou.SetMessages((from m in cpou.Messages
				where !m.IsSuppressed()
				select m).ToList<_ICompilerMessage>());
			}
			if (cpou.PrecompileMessages != null && cpou.PrecompileMessages.Any<_ICompilerMessage>())
			{
				cpou.PrecompileMessages = (from m in cpou.PrecompileMessages
				where !m.IsSuppressed()
				select m).ToArray<_ICompilerMessage>();
			}
		}

		// Token: 0x060001FC RID: 508 RVA: 0x000070A4 File Offset: 0x000060A4
		private static void AddImplicitReturnValueCode(_ISequenceStatement seq, _ISignature signature, _IPreCompileContext Context)
		{
			_ILanguageModelBuilder8 singleton = LanguageModelBuilder.Singleton;
			string attributeValue = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_PROPERTY);
			_IStatement monitoringVariableCode = CompatibleCompiledLibraryPreparer.GetMonitoringVariableCode(signature, Context, attributeValue);
			bool flag = signature.Name.StartsWith("__SET");
			if (monitoringVariableCode != null && flag)
			{
				seq.InsertStatement(0, singleton.CreatePragmaStatement2(null, "implicit off"));
				seq.InsertStatement(0, monitoringVariableCode);
				seq.InsertStatement(0, singleton.CreatePragmaStatement2(null, "implicit on"));
			}
			if (!signature.Name.StartsWith("__GET"))
			{
				return;
			}
			if (seq._StatementList.OfType<_IPragmaStatement>().FirstOrDefault((_IPragmaStatement x) => x.Text == "returnlabelposition") == null)
			{
				IStatement state = singleton.CreatePragmaStatement2(null, "returnlabelposition");
				seq.AddStatement(state);
			}
			seq.AddStatement(singleton.CreatePragmaStatement2(null, "implicit on"));
			if (monitoringVariableCode != null)
			{
				seq.AddStatement(monitoringVariableCode);
			}
			_IVariableExpression expLeft = singleton.CreateVariableExpression(signature.OrgName);
			_IVariableExpression expRight = singleton.CreateVariableExpression(attributeValue);
			IVariable variable = (_IVariable)signature[attributeValue];
			_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)singleton.CreateAssignmentExpression(null, expLeft, expRight);
			if (variable.Type.Class == TypeClass.Reference)
			{
				iassignmentExpression.KindOf = Operator.RefAssign;
			}
			IExpressionStatement state2 = singleton.CreateExpressionStatement(iassignmentExpression);
			seq.AddStatement(state2);
			seq.AddStatement(singleton.CreatePragmaStatement2(null, "implicit off"));
		}

		// Token: 0x060001FD RID: 509 RVA: 0x000071FC File Offset: 0x000061FC
		private static _IStatement GetMonitoringVariableCode(_ISignature signature, _IPreCompileContext Context, string stPropertyName)
		{
			_ILanguageModelBuilder8 singleton = LanguageModelBuilder.Singleton;
			if (signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
			{
				_ISignature isignature = (_ISignature)Context.GetSignature(signature.ParentObjectGuid);
				IVariable variable = (_IVariable)signature[stPropertyName];
				Operator kindof = Operator.Assign;
				if (variable.Type.Class == TypeClass.Reference)
				{
					kindof = Operator.RefAssign;
				}
				string stName = "__Watch_" + isignature.OrgName + "_" + stPropertyName;
				return (_IStatement)singleton.CreateExpressionStatement(singleton.CreateAssignmentExpression(null, singleton.CreateVariableExpression(null, stName), singleton.CreateVariableExpression(stPropertyName), kindof));
			}
			return null;
		}
	}
}
