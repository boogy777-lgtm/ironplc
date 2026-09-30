using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class AddressInfoCreator : IExprVisitor
	{
		private Func<string, IExpression, Pair<IAddressInfo, string>> m_createBaseAi;

		private string _stInstancePath;

		private List<StackEntry> m_stack;

		private StackEntry Top => m_stack[m_stack.Count - 1];

		public AddressInfoCreator(Func<string, IExpression, Pair<IAddressInfo, string>> createBaseAi, string stInstancePath)
		{
			if (createBaseAi == null)
			{
				throw new ArgumentNullException("createBaseAi");
			}
			m_createBaseAi = createBaseAi;
			m_stack = new List<StackEntry>();
			_stInstancePath = stInstancePath;
		}

		public IAddressInfo GetResult(out string stErrorMsg)
		{
			IAddressInfo result = ((m_stack.Count != 0) ? Top.AddressInfo : null);
			stErrorMsg = ((m_stack.Count != 0) ? Top.ErrorMsg : "");
			return result;
		}

		private void Pop()
		{
			m_stack.RemoveAt(m_stack.Count - 1);
		}

		private void Push(IAddressInfo ai)
		{
			m_stack.Add(new StackEntry(ai, ""));
		}

		private void PushError(string stErrorMsg)
		{
			m_stack.Add(new StackEntry(null, stErrorMsg));
		}

		public void visit(IOperatorExpression op)
		{
			List<IAddressInfo> list = new List<IAddressInfo>();
			IExpression[] operands = op.Operands;
			for (int i = 0; i < operands.Length; i++)
			{
				operands[i].AcceptVisitor(this);
				if (Top.AddressInfo == null)
				{
					return;
				}
				list.Add(Top.AddressInfo);
				Pop();
			}
			Push(APEnvironmentFacade.Instance.AddressInfoFactory.CreateOperator(Fun.ToArr<IAddressInfo>((IEnumerable<IAddressInfo>)list), op.Code, op.Type));
		}

		public void visit(ICallExpression call)
		{
			CreateBaseAi(call);
		}

		private void CreateBaseAi(IExpression exp)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			Pair<IAddressInfo, string> val = m_createBaseAi(_stInstancePath, exp);
			if (val.first == null)
			{
				PushError(val.second);
			}
			else
			{
				Push(val.first);
			}
		}

		public void visit(ILiteralExpression literal)
		{
			ILiteralValue literalValue = literal.LiteralValue;
			IExpression expression = literal as IExpression3;
			if (literalValue == null || expression == null)
			{
				PushError("Invalid literal value");
			}
			else
			{
				Push(APEnvironmentFacade.Instance.AddressInfoFactory.CreateLiteral(literalValue, expression.Type));
			}
		}

		public void visit(IAddressExpression address)
		{
			CreateBaseAi(address);
		}

		public void visit(IVariableExpression variable)
		{
			CreateBaseAi(variable);
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
			CreateBaseAi(indexaccess);
		}

		public void visit(ICompoAccessExpression compo)
		{
			CreateBaseAi(compo);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			CreateBaseAi(deref);
		}

		public void visit(IExpressionStatement expstat)
		{
			expstat.Expr.AcceptVisitor(this);
		}

		public void visit(IConversionExpression conv)
		{
			conv.Exp.AcceptVisitor(this);
			IAddressInfo addressInfo = Top.AddressInfo;
			if (addressInfo != null)
			{
				Push(APEnvironmentFacade.Instance.AddressInfoFactory.CreateConversion(conv.From, conv.To, addressInfo, conv.Implicit, conv.Type));
			}
		}

		private void PushUnsupported(IExprement exp)
		{
			PushError($"Unsupported expression type '{exp.GetType().FullName}'");
		}

		public void visit(IThisExpression thisexp)
		{
			PushUnsupported(thisexp);
		}

		public void visit(IBaseExpression baseexp)
		{
			PushUnsupported(baseexp);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			PushUnsupported(globexp);
		}

		public void visit(IWhileStatement whilst)
		{
			PushUnsupported(whilst);
		}

		public void visit(IRepeatStatement repeat)
		{
			PushUnsupported(repeat);
		}

		public void visit(IForStatement forloop)
		{
			PushUnsupported(forloop);
		}

		public void visit(IExitStatement exit)
		{
			PushUnsupported(exit);
		}

		public void visit(IContinueStatement cont)
		{
			PushUnsupported(cont);
		}

		public void visit(ISequenceStatement seq)
		{
			PushUnsupported(seq);
		}

		public void visit(IAssignmentExpression assign)
		{
			PushUnsupported(assign);
		}

		public void visit(IIfStatement ifst)
		{
			PushUnsupported(ifst);
		}

		public void visit(IReturnStatement returnst)
		{
			PushUnsupported(returnst);
		}

		public void visit(IJumpStatement gotost)
		{
			PushUnsupported(gotost);
		}

		public void visit(ILabelStatement label)
		{
			PushUnsupported(label);
		}

		public void visit(ICommentStatement comment)
		{
			PushUnsupported(comment);
		}

		public void visit(IPragmaStatement pragma)
		{
			PushUnsupported(pragma);
		}

		public void visit(IEmptyStatement empty)
		{
			PushUnsupported(empty);
		}

		public void visit(ICaseRangeExpression caserange)
		{
			PushUnsupported(caserange);
		}

		public void visit(ICaseLabelStatement caselabel)
		{
			PushUnsupported(caselabel);
		}

		public void visit(ICaseStatement casest)
		{
			PushUnsupported(casest);
		}

		public void visit(IBreakPointStatement bpstate)
		{
			PushUnsupported(bpstate);
		}

		public void visit(IDefineReference defref)
		{
			PushUnsupported(defref);
		}

		public void visit(IVariableReference varref)
		{
			PushUnsupported(varref);
		}

		public void visit(ITypeReference typeref)
		{
			PushUnsupported(typeref);
		}

		public void visit(IPouReference pouref)
		{
			PushUnsupported(pouref);
		}

		public void visit(IDefinedExpression defexp)
		{
			PushUnsupported(defexp);
		}

		public void visit(IPragmaOperatorExpression popexp)
		{
			PushUnsupported(popexp);
		}

		public void visit(IPragmaIfStatement pifst)
		{
			PushUnsupported(pifst);
		}

		public void visit(IDefineStatement defstate)
		{
			PushUnsupported(defstate);
		}

		public void visit(IHasTypeExpression hastype)
		{
			PushUnsupported(hastype);
		}

		public void visit(IHasAttributeExpression hasattribute)
		{
			PushUnsupported(hasattribute);
		}

		public void visit(IHasValueExpression hasvalue)
		{
			PushUnsupported(hasvalue);
		}

		public void visit(IPragmaAssertion assertion)
		{
			PushUnsupported(assertion);
		}
	}
}
