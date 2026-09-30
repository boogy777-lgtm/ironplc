using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "Close enough")]
	internal class CodeTranslator
	{
		private readonly ICompileContext _comcon;

		private readonly IExpressionTypifier6 _typifier;

		private readonly ILanguageModelBuilder4 _lmb;

		private readonly InterpreterCodeAdapter _adapter;

		internal IScope Scope { get; set; }

		internal CodeTranslator(ICompileContext comcon, IExpressionTypifier6 typifier, IScope scope)
		{
			_comcon = comcon;
			_typifier = typifier;
			Scope = scope;
			_lmb = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder() as ILanguageModelBuilder4;
			_adapter = new InterpreterCodeAdapter();
		}

		internal int GetStringLength(IExpression exp)
		{
			int num = exp.Type.Size(Scope);
			if (exp.Type.Class == TypeClass.String)
			{
				return num - 1;
			}
			if (exp.Type.Class == TypeClass.WString)
			{
				return num / 2 - 1;
			}
			return 0;
		}

		internal IExpression CreateStringIndexExpression(IExpression expString, int index)
		{
			if (expString is ILiteralExpression)
			{
				char c = expString.Literal(Scope).String[index];
				if (expString.Type.Class == TypeClass.String)
				{
					return _lmb.CreateLiteralExpression(null, c, TypeClass.Byte);
				}
				return _lmb.CreateLiteralExpression(null, c, TypeClass.Word);
			}
			ILiteralExpression expAccess = _lmb.CreateLiteralExpression(null, index);
			return _lmb.CreateIndexAccessExpression(null, expString, expAccess);
		}

		internal IOperatorExpression HandleStringComparison(IOperatorExpression op)
		{
			IExpression expression = op.Operands[0];
			IExpression expression2 = op.Operands[1];
			int num = Math.Min(GetStringLength(expression), GetStringLength(expression2));
			if (num == 0)
			{
				return op;
			}
			IOperatorExpression operatorExpression = null;
			Operator op2 = ((op.Code == Operator.Equal) ? Operator.And : Operator.Or);
			for (int i = 0; i < num; i++)
			{
				IExpression expOp = CreateStringIndexExpression(expression, i);
				IExpression expOp2 = CreateStringIndexExpression(expression2, i);
				IOperatorExpression operatorExpression2 = _lmb.CreateOperatorExpression(null, op.Code, expOp, expOp2);
				operatorExpression = ((operatorExpression != null) ? _lmb.CreateOperatorExpression(null, op2, operatorExpression, operatorExpression2) : operatorExpression2);
			}
			_typifier.TypifyAndCheckExpression(operatorExpression, out var _);
			return operatorExpression;
		}

		internal IOperatorExpression ReduceToPrimitiveLogicOperations(IOperatorExpression op)
		{
			IOperatorExpression operatorExpression = null;
			List<IExpression> list = op.Operands.ToList();
			if ((op.Code == Operator.Equal || op.Code == Operator.NotEqual) && op.Operands.Count() == 2 && (op.Operands[0].Type.Class == TypeClass.String || op.Operands[0].Type.Class == TypeClass.WString) && op.Operands[1].Type.Class == op.Operands[0].Type.Class)
			{
				return HandleStringComparison(op);
			}
			switch (op.Code)
			{
			case Operator.Greater:
			{
				IOperatorExpression expSingleOp3 = _lmb.CreateOperatorExpression(null, Operator.LessEqual, list);
				operatorExpression = _lmb.CreateOperatorExpression(null, Operator.Not, expSingleOp3);
				break;
			}
			case Operator.Less:
			{
				list.Reverse();
				IOperatorExpression expSingleOp2 = _lmb.CreateOperatorExpression(null, Operator.LessEqual, list);
				operatorExpression = _lmb.CreateOperatorExpression(null, Operator.Not, expSingleOp2);
				break;
			}
			case Operator.GreaterEqual:
				list.Reverse();
				operatorExpression = _lmb.CreateOperatorExpression(null, Operator.LessEqual, list);
				break;
			case Operator.Equal:
			{
				IOperatorExpression expSingleOp = _lmb.CreateOperatorExpression(null, Operator.NotEqual, list);
				operatorExpression = _lmb.CreateOperatorExpression(null, Operator.Not, expSingleOp);
				break;
			}
			case Operator.Not:
			{
				ILiteralExpression item = _lmb.CreateLiteralExpression(null, 1L);
				list.Insert(0, item);
				operatorExpression = _lmb.CreateOperatorExpression(null, Operator.Minus, list);
				break;
			}
			}
			if (operatorExpression != null)
			{
				_typifier.TypifyAndCheckExpression(operatorExpression, out var _);
			}
			return operatorExpression;
		}

		internal ICallExpression2 CreateExternalFuncCallForConversion(IConversionExpression conv)
		{
			ICallExpression2 result = null;
			if (_adapter.NeedsExternalFunctionCall(conv, out var stFunctionName, out var tcWithType))
			{
				result = CreateConversionFunctionCall(conv, stFunctionName, tcWithType);
			}
			return result;
		}

		private ICallExpression2 CreateConversionFunctionCall(IConversionExpression conv, string stFuncName, TypeClass tcWithType)
		{
			IVariableExpression2 expCallee = _lmb.CreateVariableExpression(null, stFuncName);
			List<IAssignmentExpression> list = new List<IAssignmentExpression>();
			List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
			ISignature[] array = Scope.FindSignature(stFuncName);
			if (array == null || array.Length == 0)
			{
				throw new UnsupportedInterpreterCodeException(string.Format(ExecutionpointStrings.ExternalFunctionForConversionUnavailable, conv.From.ToString(), conv.To.ToString()));
			}
			ISignature4 signature = array[0] as ISignature4;
			IAssignmentExpression item = _lmb.CreateAssignmentExpression(null, _lmb.CreateVariableExpression(null, signature.Inputs[0].Name), conv.Exp);
			list.Add(item);
			IAssignmentExpression item2 = _lmb.CreateAssignmentExpression(null, _lmb.CreateVariableExpression(null, signature.Inputs[1].Name), _lmb.CreateLiteralExpression(null, (long)tcWithType));
			list.Add(item2);
			ICallExpression2 callExpression = _lmb.CreateCallExpression(null, expCallee, null, null, list, outputassignments);
			_typifier.TypifyAndCheckExpression(callExpression, out var _);
			return callExpression;
		}

		[SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "Will be fixed with CDS-95535")]
		internal ICallExpression CreateCompleteCallStatement(ICallExpression call, out ISequenceStatement2 inputAssignSeq, out ISequenceStatement2 outputAssignSeq)
		{
			ICallExpression callExpression = null;
			inputAssignSeq = _lmb.CreateSequenceStatement(null);
			outputAssignSeq = _lmb.CreateSequenceStatement(null);
			ISignature signatureFromCallee = GetSignatureFromCallee(call.Callee);
			ISignature signature = ((signatureFromCallee.ParentSignatureId != -1) ? _comcon.GetSignatureById(signatureFromCallee.ParentSignatureId) : null);
			IEnumerable<IMessage> messages;
			if (signatureFromCallee.POUType == Operator.FunctionBlock)
			{
				ISignature[] subSignatures = signatureFromCallee.SubSignatures;
				for (int i = 0; i < subSignatures.Length && !(subSignatures[i].Name == "__MAIN"); i++)
				{
				}
				List<IAssignmentExpression> list = new List<IAssignmentExpression>();
				List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
				string stExpression = call.Callee.ToString() + ".__MAIN";
				IExpression expCallee = _lmb.ParseExpression(stExpression, allowImplicit: true);
				stExpression = "__INSTANCEPOINTER := ADR(" + call.Callee.ToString() + ")";
				IAssignmentExpression item = _lmb.ParseExpression(stExpression, allowImplicit: true) as IAssignmentExpression;
				list.Add(item);
				callExpression = _lmb.CreateCallExpression(null, expCallee, null, null, list, outputassignments);
				_typifier.TypifyAndCheckExpression(callExpression, out messages);
				ISequenceStatement2 sequenceStatement = _lmb.CreateSequenceStatement(null);
				IAssignmentExpression[] inputAssigns = call.InputAssigns;
				foreach (IAssignmentExpression assignmentExpression in inputAssigns)
				{
					ICompoAccessExpression expLeft = _lmb.CreateCompoAccessExpression(null, call.Callee, assignmentExpression.LValue as IVariableExpression2);
					IExpressionStatement expressionStatement = _lmb.CreateAssignmentStatement(null, expLeft, assignmentExpression.RValue);
					_typifier.TypifyAndCheckStatement(expressionStatement, out messages);
					sequenceStatement.AddStatement(expressionStatement);
				}
				inputAssignSeq = sequenceStatement;
				ISequenceStatement2 sequenceStatement2 = _lmb.CreateSequenceStatement(null);
				inputAssigns = call.OutputAssigns;
				foreach (IAssignmentExpression assignmentExpression2 in inputAssigns)
				{
					ICompoAccessExpression expRight = _lmb.CreateCompoAccessExpression(null, call.Callee, assignmentExpression2.RValue as IVariableExpression2);
					IExpressionStatement expressionStatement2 = _lmb.CreateAssignmentStatement(null, assignmentExpression2.LValue, expRight);
					_typifier.TypifyAndCheckStatement(expressionStatement2, out messages);
					sequenceStatement.AddStatement(expressionStatement2);
				}
				outputAssignSeq = sequenceStatement2;
			}
			else if (signatureFromCallee.POUType == Operator.Method)
			{
				List<IAssignmentExpression> list2 = call.InputAssigns.ToList();
				List<IAssignmentExpression> outputassignments2 = call.OutputAssigns.ToList();
				bool bInstanceCall = signature.POUType == Operator.FunctionBlock;
				IAssignmentExpression instancePointerExpression = GetInstancePointerExpression(call.Callee, bInstanceCall);
				if (list2.Count > 0)
				{
					list2.Insert(0, instancePointerExpression);
				}
				else
				{
					list2.Add(instancePointerExpression);
				}
				callExpression = _lmb.CreateCallExpression(null, call.Callee, null, call.Type, list2, outputassignments2);
				_typifier.TypifyAndCheckExpression(callExpression, out messages);
			}
			return callExpression;
		}

		internal ISignature GetSignatureFromCallee(IExpression callee)
		{
			if (callee is IExpression5)
			{
				return (callee as IExpression5).GetSignatureEx(Scope);
			}
			IVariableExpression2 variableExpression = ((!(callee is ICompoAccessExpression)) ? (callee as IVariableExpression2) : ((callee as ICompoAccessExpression).Right as IVariableExpression2));
			if (variableExpression.GetVariable(Scope) != null)
			{
				IUserdefType userdefType = callee.Type.DeRefType as IUserdefType;
				return _comcon.GetSignatureById(userdefType.SignatureId);
			}
			return variableExpression.GetSignature(Scope);
		}

		private IAssignmentExpression GetInstancePointerExpression(IExpression instanceExp, bool bInstanceCall)
		{
			string text = "0";
			if (bInstanceCall)
			{
				text = ((!(instanceExp is ICompoAccessExpression)) ? "THIS" : ("ADR(" + (instanceExp as ICompoAccessExpression).Left.ToString() + ")"));
			}
			string stExpression = "__INSTANCEPOINTER :=" + text;
			return _lmb.ParseExpression(stExpression, allowImplicit: true) as IAssignmentExpression;
		}

		internal ICallExpression CreatePropertyReadCall(IExpression propertyAccess)
		{
			ICallExpression callExpression = null;
			bool flag = false;
			string stExpression = string.Empty;
			if (propertyAccess is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = propertyAccess as ICompoAccessExpression;
				if (compoAccessExpression.Right is IVariableExpression variableExpression && variableExpression.GetVariable(Scope).HasAttribute("get"))
				{
					stExpression = compoAccessExpression.Left.ToString() + ".__get" + compoAccessExpression.Right.ToString();
					flag = true;
				}
			}
			else if (propertyAccess is IVariableExpression)
			{
				IVariable variable = (propertyAccess as IVariableExpression).GetVariable(Scope);
				if (variable != null && variable.HasAttribute("get"))
				{
					stExpression = "__get" + propertyAccess.ToString();
					flag = true;
				}
			}
			if (flag)
			{
				List<IAssignmentExpression> inputassignments = new List<IAssignmentExpression>();
				List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
				IExpression expCallee = _lmb.ParseExpression(stExpression, allowImplicit: true);
				callExpression = _lmb.CreateCallExpression(null, expCallee, null, null, inputassignments, outputassignments);
				_typifier.TypifyAndCheckExpression(callExpression, out var _);
			}
			if (callExpression != null && !IsPropertyCallable(callExpression, propertyAccess, out var stError))
			{
				throw new UnsupportedInterpreterCodeException(stError);
			}
			return callExpression;
		}

		internal ICallExpression CreatePropertyWriteCall(IExpression propertyAccess, IExpression value)
		{
			ICallExpression callExpression = null;
			bool flag = false;
			string text = string.Empty;
			string stExpression = string.Empty;
			if (propertyAccess is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = propertyAccess as ICompoAccessExpression;
				if (compoAccessExpression.Right is IVariableExpression variableExpression && variableExpression.GetVariable(Scope).HasAttribute("set"))
				{
					text = compoAccessExpression.Right.ToString();
					stExpression = compoAccessExpression.Left.ToString() + ".__set" + text;
					flag = true;
				}
			}
			else if (propertyAccess is IVariableExpression && (propertyAccess as IVariableExpression).GetVariable(Scope).HasAttribute("set"))
			{
				text = propertyAccess.ToString();
				stExpression = "__set" + text;
				flag = true;
			}
			if (flag)
			{
				List<IAssignmentExpression> list = new List<IAssignmentExpression>();
				List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
				string stExpression2 = text + " := " + value.ToString();
				IAssignmentExpression item = _lmb.ParseExpression(stExpression2, allowImplicit: true) as IAssignmentExpression;
				list.Add(item);
				IExpression expCallee = _lmb.ParseExpression(stExpression, allowImplicit: true);
				callExpression = _lmb.CreateCallExpression(null, expCallee, null, null, list, outputassignments);
				_typifier.TypifyAndCheckExpression(callExpression, out var _);
			}
			if (callExpression != null && !IsPropertyCallable(callExpression, propertyAccess, out var stError))
			{
				throw new UnsupportedInterpreterCodeException(stError);
			}
			return callExpression;
		}

		internal IExpression VarToInOut(IExpression varExp)
		{
			IExpression expression = _lmb.CreateOperatorExpression(null, Operator.Adr, varExp);
			_typifier.TypifyAndCheckExpression(expression, out var _);
			return expression;
		}

		internal bool IsPropertyCallable(ICallExpression expr, IExpression origExpression, out string stError)
		{
			stError = string.Empty;
			ISignature signatureFromCallee = GetSignatureFromCallee(expr.Callee);
			if (signatureFromCallee == null)
			{
				stError = string.Format(ExecutionpointStrings.EPCode_PropertyNotUsable, origExpression);
				return false;
			}
			if (!IsPOUCallable(signatureFromCallee, _comcon))
			{
				if (IsInterfaceCall(signatureFromCallee, _comcon))
				{
					stError = string.Format(ExecutionpointStrings.BPCodeInterfaceCallNotSupported, origExpression);
				}
				else
				{
					stError = string.Format(ExecutionpointStrings.EPCode_PropertyNotUsable, origExpression);
				}
				return false;
			}
			return true;
		}

		internal static bool IsPOUCallable(ISignature calleeSign, ICompileContext comcon)
		{
			return ((comcon.CreateGlobalIScope() as IScope4)?.GetCompiledPOUById(calleeSign.Id))?.GetFlag(CompiledPOUFlags.TopLevel) ?? false;
		}

		internal static bool IsInterfaceCall(ISignature calleeSign, ICompileContext comcon)
		{
			if (calleeSign.POUType == Operator.Interface)
			{
				return true;
			}
			if (calleeSign.ParentSignatureId != -1 && comcon.GetSignatureById(calleeSign.ParentSignatureId).POUType == Operator.Interface)
			{
				return true;
			}
			return false;
		}
	}
}
