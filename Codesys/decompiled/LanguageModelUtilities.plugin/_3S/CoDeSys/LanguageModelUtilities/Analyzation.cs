using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class Analyzation : IExprVisitor2, IExprVisitor
	{
		private readonly IScope _scope;

		private IStatement _changedStatement;

		private readonly ICompiledPOU _cpou;

		private readonly ICompileContext10 _comcon;

		private readonly LList<IMessage> _errorMessages = new LList<IMessage>();

		private Analyzation(ICompiledPOU cpou, ICompileContext10 comcon)
		{
			_comcon = comcon;
			_cpou = cpou;
			_scope = _comcon.CreateIScope(cpou.SignatureId);
		}

		public static void AnalyzeStatement(ICompiledPOU cpou, ICompileContext10 comcon)
		{
			Analyzation analyzation = new Analyzation(cpou, comcon);
			ISequenceStatement sequenceStatement = null;
			((!(comcon is ILMCompiledApplicationSetForInstrumentation)) ? (cpou.ParseTree as ISequenceStatement) : (comcon as ILMCompiledApplicationSetForInstrumentation).CreateParseTreeOfPOUForInstrumentation(cpou))?.AcceptVisitor(analyzation);
			analyzation.ReportErrorMessages();
		}

		public static string WriteExprement(IExprement exprement)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 0))
			{
				return ((ILMPreCompileSmartCodingService3)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileSmartCodingService).WriteExprement(exprement, WriteExprementFlags.MinimalParentheses);
			}
			return exprement.ToString();
		}

		private void ReportErrorMessages()
		{
			if (APEnvironmentFacade.Instance.CompilerMessageCategoryOrNull == null)
			{
				return;
			}
			foreach (IMessage errorMessage in _errorMessages)
			{
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.CompilerMessageCategoryOrNull, errorMessage);
			}
		}

		public void visit(ISequenceStatement seq)
		{
			if (!(seq is ISequenceStatement3 sequenceStatement))
			{
				return;
			}
			List<Tuple<int, IStatement>> list = new List<Tuple<int, IStatement>>();
			int num = 0;
			foreach (IStatement statement in sequenceStatement.StatementList)
			{
				_changedStatement = null;
				statement.AcceptVisitor(this);
				if (_changedStatement != null)
				{
					list.Add(new Tuple<int, IStatement>(num, _changedStatement));
					_changedStatement = null;
				}
				num++;
			}
			foreach (Tuple<int, IStatement> item in list)
			{
				sequenceStatement.InsertStatement(item.Item1, item.Item2);
				sequenceStatement.RemoveStatement(item.Item1 + 1);
			}
		}

		public void visit(IExpressionStatement expstat)
		{
			expstat.Expr.AcceptVisitor(this);
		}

		public void visit(ICallExpression call)
		{
			try
			{
				if (!(call is IExpression4))
				{
					return;
				}
				IExpression6 expression = call.Callee as IExpression6;
				IVariable variable = expression?.GetVariable(_scope);
				ICompiledType compiledType = variable?.CompiledType;
				bool flag = false;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 16, 0))
				{
					while (compiledType is IArrayType)
					{
						compiledType = (compiledType as IArrayType2).Base as ICompiledType;
						flag = true;
					}
				}
				if (!(compiledType is IUserdefType2))
				{
					return;
				}
				IUserdefType2 userdefType = compiledType as IUserdefType2;
				ISignature signature = _scope[userdefType.SignatureId];
				if (signature == null || !signature.HasAttribute("analyzation"))
				{
					return;
				}
				if (flag)
				{
					CheckForSideEffect(call, expression, variable);
				}
				ILanguageModelBuilder3 languageModelBuilder = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as ILanguageModelBuilder3;
				IExpressionTypifier4 expressionTypifier = APEnvironmentFacade.Instance.LanguageModelMgr.CreateTypifier(_comcon.ApplicationGuid, _cpou.SignatureId, bContributeToCompile: false, bInterpretPragmas: false) as IExpressionTypifier4;
				if (languageModelBuilder == null || expressionTypifier == null)
				{
					return;
				}
				IAssignmentExpression[] inputAssigns = call.InputAssigns;
				IExpression expression2 = null;
				IExpression expression3 = null;
				IAssignmentExpression[] array = inputAssigns;
				foreach (IAssignmentExpression assignmentExpression in array)
				{
					if (assignmentExpression.LValue is IVariableExpression && string.Equals((assignmentExpression.LValue as IVariableExpression).Name, "InputExp", StringComparison.OrdinalIgnoreCase))
					{
						expression2 = assignmentExpression.RValue;
					}
					if (assignmentExpression.LValue is IVariableExpression && string.Equals((assignmentExpression.LValue as IVariableExpression).Name, "DoAnalyze", StringComparison.OrdinalIgnoreCase))
					{
						expression3 = assignmentExpression.RValue;
					}
				}
				if (expression2 == null || expression3 == null)
				{
					return;
				}
				IExpression expInstance = languageModelBuilder.DuplicateExprement(expression) as IExpression;
				IExpression expToAnalyze = languageModelBuilder.DuplicateExprement(expression2) as IExpression;
				LList<IStatement> val = new LList<IStatement>();
				IStatement statement = languageModelBuilder.CreateExpressionStatement(call);
				val.Add(statement);
				val.Add(languageModelBuilder.CreatePragmaStatement2(null, "nobp"));
				val.Add(languageModelBuilder.CreatePragmaStatement2(null, "noflow"));
				LList<IStatement> val2 = new LList<IStatement>();
				if (signature["OutString"] != null)
				{
					AnalyzationStringBuilder.CreateStringBuilderStatement(expToAnalyze, expInstance, _comcon, val2);
				}
				if (signature["OutTable"] != null)
				{
					AnalyzationTableBuilder.CreateStringBuilderStatement(expToAnalyze, expInstance, _comcon, val2);
				}
				ISequenceStatement2 seqThen = languageModelBuilder.CreateSequenceStatementEx(null, (IEnumerable<IStatement>)val2);
				IExpression expression4 = null;
				expression4 = languageModelBuilder.DuplicateExprement(expression3) as IExpression;
				IIfStatement ifStatement = languageModelBuilder.CreateIfStatement(null, expression4, seqThen);
				val.Add((IStatement)ifStatement);
				val.Add(languageModelBuilder.CreatePragmaStatement2(null, "bp"));
				val.Add(languageModelBuilder.CreatePragmaStatement2(null, "flow"));
				array = call.OutputAssigns;
				foreach (IAssignmentExpression assignmentExpression2 in array)
				{
					if (assignmentExpression2.LValue is IVariableExpression2)
					{
						IExprementPosition pos = null;
						if (assignmentExpression2.Position != null)
						{
							pos = languageModelBuilder.CreateExprementPosition(assignmentExpression2.Position.PositionCombination);
						}
						IExpression expRight = languageModelBuilder.CreateCompoAccessExpression(null, languageModelBuilder.DuplicateExprement(call.Callee) as IExpression, assignmentExpression2.RValue as IVariableExpression2);
						IStatement statement2 = languageModelBuilder.CreateAssignmentStatement(pos, assignmentExpression2.LValue, expRight);
						val.Add(statement2);
					}
				}
				_changedStatement = languageModelBuilder.CreateSequenceStatementEx(null, (IEnumerable<IStatement>)val);
				((IExpressionTypifier5)expressionTypifier).TypifyAndCheckStatement(_changedStatement, out var messages);
				foreach (IMessage item in messages)
				{
					if (item.Severity == Severity.Error || item.Severity == Severity.FatalError)
					{
						_changedStatement = null;
						break;
					}
				}
			}
			catch
			{
				_changedStatement = null;
			}
		}

		private void CheckForSideEffect(ICallExpression call, IExpression6 expCallee, IVariable var)
		{
			ISignature signature = _scope[_cpou.SignatureId];
			IPreCompileContext9 preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signature) as IPreCompileContext9;
			ISignature4 signature2 = preCompileContext.GetSignature(signature.ObjectGuid) as ISignature4;
			if (((IPreCompileUtilities7)APEnvironmentFacade.Instance.LanguageModelUtilities.PreCompileUtils).ExpressionHasSideEffects(call.Callee, signature2, preCompileContext))
			{
				AnalyzationErrorMessage analyzationErrorMessage = new AnalyzationErrorMessage(expCallee.GetSignatureEx(_scope), expCallee, Strings.Err_AnalyzationCalleeNotSupported);
				_errorMessages.Add((IMessage)analyzationErrorMessage);
			}
		}

		public void visit(IWhileStatement whilst)
		{
			whilst.Controlled.AcceptVisitor(this);
		}

		public void visit(IRepeatStatement repeat)
		{
			repeat.Controlled.AcceptVisitor(this);
		}

		public void visit(IForStatement forloop)
		{
			forloop.Controlled.AcceptVisitor(this);
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
		}

		public void visit(IIfStatement ifst)
		{
			ifst.IfThen.AcceptVisitor(this);
			if (ifst.IfElse != null)
			{
				ifst.IfElse.AcceptVisitor(this);
			}
		}

		public void visit(IReturnStatement returnst)
		{
		}

		public void visit(IJumpStatement gotost)
		{
		}

		public void visit(ILabelStatement label)
		{
		}

		public void visit(ICommentStatement comment)
		{
		}

		public void visit(IPragmaStatement pragma)
		{
		}

		public void visit(IOperatorExpression op)
		{
		}

		public void visit(IConversionExpression conv)
		{
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(ILiteralExpression literal)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(IVariableExpression variable)
		{
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
		}

		public void visit(ICompoAccessExpression compo)
		{
		}

		public void visit(IDeRefAccessExpression deref)
		{
		}

		public void visit(IGlobalScopeExpression globexp)
		{
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(ICaseStatement casest)
		{
			ICase[] cases = casest.Cases;
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i].Controlled.AcceptVisitor(this);
			}
			if (casest.Else != null)
			{
				casest.Else.AcceptVisitor(this);
			}
		}

		public void visit(IBreakPointStatement bpstate)
		{
		}

		public void visit(IDefineReference defref)
		{
		}

		public void visit(IVariableReference varref)
		{
		}

		public void visit(ITypeReference typeref)
		{
		}

		public void visit(IPouReference pouref)
		{
		}

		public void visit(IDefinedExpression defexp)
		{
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
		}

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(IHasTypeExpression hastype)
		{
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
		}

		public void visit(IHasValueExpression hasvalue)
		{
		}

		public void visit(IHasConstantValueExpression hasvalue)
		{
		}

		public void visit(IPragmaAssertion assertion)
		{
		}
	}
}
