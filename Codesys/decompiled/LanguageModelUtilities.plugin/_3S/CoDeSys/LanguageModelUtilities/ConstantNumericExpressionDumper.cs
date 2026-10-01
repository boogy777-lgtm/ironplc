using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes", Justification = "A class implementing the visitor pattern normally violates the class coupling metric")]
	internal class ConstantNumericExpressionDumper : IExprVisitor
	{
		private readonly ICompileContext _comcon;

		private readonly bool _bUseExpressionsAsIs;

		private StringBuilder _sb;

		internal ConstantNumericExpressionDumper(ICompileContext comcon, bool bUseExpressionsAsIs)
		{
			_comcon = comcon;
			_bUseExpressionsAsIs = bUseExpressionsAsIs;
		}

		internal string DumpConstantExpression(IExpression expr)
		{
			_sb = new StringBuilder();
			expr.AcceptVisitor(this);
			return _sb.ToString();
		}

		public void visit(IExitStatement exit)
		{
		}

		public void visit(IContinueStatement cont)
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

		public void visit(IEmptyStatement empty)
		{
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

		public void visit(IPragmaIfStatement pifst)
		{
		}

		public void visit(IDefineStatement defstate)
		{
		}

		public void visit(ISequenceStatement seq)
		{
		}

		public void visit(IIfStatement ifst)
		{
		}

		public void visit(IForStatement forloop)
		{
		}

		public void visit(IWhileStatement whilst)
		{
		}

		public void visit(IRepeatStatement repeat)
		{
		}

		public void visit(IExpressionStatement expstat)
		{
		}

		public void visit(IAssignmentExpression assign)
		{
		}

		public void visit(IAddressExpression address)
		{
		}

		public void visit(ICaseRangeExpression caserange)
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

		public void visit(IBaseExpression baseexp)
		{
		}

		public void visit(IThisExpression thisexp)
		{
		}

		public void visit(ICallExpression call)
		{
		}

		public void visit(IConversionExpression conv)
		{
		}

		public void visit(IIndexAccessExpression indexaccess)
		{
		}

		public void visit(IDeRefAccessExpression deref)
		{
		}

		public void visit(IGlobalScopeExpression globexp)
		{
		}

		public void visit(ICompoAccessExpression compo)
		{
			IEnumType2 enumType = compo.Type as IEnumType2;
			bool flag = enumType != null;
			if (flag)
			{
				ISignature signatureById = _comcon.GetSignatureById(enumType.SignatureId);
				if (signatureById != null && !signatureById.HasAttribute("m4export_enum-no-prefix"))
				{
					_sb.Append(enumType.Name + "_");
				}
			}
			if (_bUseExpressionsAsIs && !flag)
			{
				_sb.Append(compo.ToString().ToUpper());
				return;
			}
			string[] array = Common.SplitAtDot(compo.ToString().ToUpper());
			string value = array[array.Length - 1].Replace("(", "").Replace(")", "");
			_sb.Append(value);
		}

		public void visit(IOperatorExpression op)
		{
			switch (op.Code)
			{
			case Operator.Plus:
				op.Operands[0].AcceptVisitor(this);
				_sb.Append(" + ");
				op.Operands[1].AcceptVisitor(this);
				break;
			case Operator.Minus:
				op.Operands[0].AcceptVisitor(this);
				_sb.Append(" - ");
				op.Operands[1].AcceptVisitor(this);
				break;
			case Operator.Times:
				op.Operands[0].AcceptVisitor(this);
				_sb.Append(" * ");
				op.Operands[1].AcceptVisitor(this);
				break;
			case Operator.Divide:
				op.Operands[0].AcceptVisitor(this);
				_sb.Append(" / ");
				op.Operands[1].AcceptVisitor(this);
				break;
			case Operator.Power:
				break;
			}
		}

		public void visit(IVariableExpression variable)
		{
			_sb.Append(variable.ToString().ToUpper());
		}

		public void visit(ILiteralExpression literal)
		{
			ILiteralValue literalValue = literal.LiteralValue;
			switch (literalValue.KindOf)
			{
			case KindOfLiteral.Bool:
				_sb.Append(literalValue.Bool.ToString().ToUpperInvariant());
				break;
			case KindOfLiteral.Float:
				_sb.Append(literalValue.Float.ToString(CultureInfo.InvariantCulture));
				break;
			case KindOfLiteral.SignedInteger:
				_sb.AppendFormat("0x{0:X}", literalValue.SignedLong);
				break;
			case KindOfLiteral.UnsignedInteger:
				_sb.AppendFormat("0x{0:X}", literalValue.UnsignedLong);
				break;
			case KindOfLiteral.String:
				break;
			}
		}
	}
}
