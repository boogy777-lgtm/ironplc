using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class TypeTraverser : IExprVisitor
	{
		private StringBuilder _sb = new StringBuilder();

		private bool _bError;

		private void CheckOrVisitExpression(IExpression expr)
		{
			if (expr is IErrorExpression)
			{
				_bError = true;
			}
			else
			{
				expr.AcceptVisitor(this);
			}
		}

		internal void TraverseType(IType type, IPreCompileUtilities6 preCompileUtilities6, IEvaluationContext context)
		{
			if (type is IUserdefType)
			{
				string text = type.ToString();
				ISignature2 signature = preCompileUtilities6.FindTypeSignature2(context, text);
				if (signature != null && signature.GetFlag(SignatureFlag.Alias))
				{
					if (1 == signature.All.Length)
					{
						TraverseType(signature.All[0].Type, preCompileUtilities6, context);
					}
				}
				else
				{
					_sb.Append(text);
				}
			}
			else if (type is IArrayType)
			{
				IArrayType arrayType = (IArrayType)type;
				_sb.Append("ARRAY [");
				IArrayDimension[] dimensions = arrayType.Dimensions;
				for (int i = 0; i < dimensions.Length; i++)
				{
					if (0 < i)
					{
						_sb.Append(", ");
					}
					CheckOrVisitExpression(dimensions[i].LowerBorder);
					CheckOrVisitExpression(dimensions[i].UpperBorder);
					_sb.AppendFormat("{0}..{1}", dimensions[i].LowerBorder, dimensions[i].UpperBorder);
				}
				_sb.Append("] OF ");
				TraverseType(arrayType.Base, preCompileUtilities6, context);
			}
			else if (type is IPointerType)
			{
				IPointerType pointerType = (IPointerType)type;
				_sb.Append("POINTER TO ");
				TraverseType(pointerType.Base, preCompileUtilities6, context);
			}
			else
			{
				_sb.Append(type.ToString());
			}
		}

		internal string GetResolvedType()
		{
			if (_bError)
			{
				return null;
			}
			return _sb.ToString();
		}

		public void visit(IWhileStatement whilst)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
		}

		public void visit(IIfStatement ifst)
		{
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

		public void visit(IExpressionStatement expstat)
		{
		}

		public void visit(ICallExpression call)
		{
		}

		public void visit(IOperatorExpression op)
		{
			IExpression[] operands = op.Operands;
			foreach (IExpression expr in operands)
			{
				CheckOrVisitExpression(expr);
			}
		}

		public void visit(IConversionExpression conv)
		{
			CheckOrVisitExpression(conv.Exp);
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
			CheckOrVisitExpression(indexaccess.Var);
			IExpression[] accesses = indexaccess.Accesses;
			foreach (IExpression expr in accesses)
			{
				CheckOrVisitExpression(expr);
			}
		}

		public void visit(ICompoAccessExpression compo)
		{
			CheckOrVisitExpression(compo.Left);
			CheckOrVisitExpression(compo.Right);
		}

		public void visit(IDeRefAccessExpression deref)
		{
			CheckOrVisitExpression(deref.Base);
		}

		public void visit(IGlobalScopeExpression globexp)
		{
			CheckOrVisitExpression(globexp.Base);
		}

		public void visit(IEmptyStatement empty)
		{
		}

		public void visit(ICaseRangeExpression caserange)
		{
			CheckOrVisitExpression(caserange.Low);
			CheckOrVisitExpression(caserange.High);
		}

		public void visit(ICaseLabelStatement caselabel)
		{
		}

		public void visit(ICaseStatement casest)
		{
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

		public void visit(IPragmaAssertion assertion)
		{
		}
	}
}
