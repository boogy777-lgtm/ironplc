using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal class ExpressionParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private OperandParser OperandParser => Context.OperandParser;

		private InitializationParser InitializationParser => Context.InitializationParser;

		internal ExpressionParser(ParserContext context)
		{
			Context = context;
		}

		internal void SetInSTCode()
		{
			InitializationParser.InSTCode = true;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		internal _IExpression ParseORExp(out bool bError)
		{
			return InfixOperationParser.ParseORExp(Context, out bError);
		}

		public _IExpression ParseInitialisation()
		{
			return InitializationParser.ParseInitialisation();
		}

		internal _IExpression ParseInitialisationExp(out bool bError)
		{
			return InitializationParser.ParseInitialisationExp(out bError);
		}

		internal IExpression ParseExpression()
		{
			bool bError;
			IExpression result = ParseAssignment(out bError);
			if (bError)
			{
				return null;
			}
			return result;
		}

		public _IExpression ParseAssignExp(out bool bError)
		{
			if (Context.StatementParser.ParseInitializationExpression)
			{
				return ParseInitialisationExp(out bError);
			}
			return ParseAssignment(out bError);
		}

		internal _IExpression ParseAssignment(out bool bError)
		{
			_IExpression iExpression = ParseORExp(out bError);
			IToken token;
			TokenType num = Next(out token);
			Operator @operator = Operator.None;
			if ((num == TokenType.Operator && (@operator = Scanner.GetOperator(token)) == Operator.Assign) || @operator == Operator.SetAssign || @operator == Operator.ResetAssign || @operator == Operator.RefAssign || @operator == Operator.FupAssign)
			{
				int sourceOffset = Scanner.SourceOffset;
				_IExpression rValue = ParseAssignment(out bError);
				_IAssignmentExpression iAssignmentExpression = LMItemFactory.CreateAssignmentExpression(iExpression, token);
				iAssignmentExpression._RValue = rValue;
				iAssignmentExpression.KindOf = @operator;
				iAssignmentExpression._Position = iExpression._Position;
				iExpression = iAssignmentExpression;
				int sourceOffset2 = Scanner.SourceOffset;
				iAssignmentExpression.PositionLength = (short)(sourceOffset2 - sourceOffset);
			}
			else
			{
				Scanner.SetPosition(token);
			}
			return iExpression;
		}

		internal _IExpression ParseVarAccess(_IExpression exp, out bool bError, _IToken startToken, ref _IToken endToken)
		{
			return OperandParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}

		internal IExpression ParseOperand()
		{
			return OperandParser.ParseOperand();
		}

		public _IExpression ParseSTOperand(out bool bError)
		{
			return OperandParser.ParseSTOperand(out bError);
		}

		internal _IExpression ParseSTOperandWithPosition(out bool bError, _IToken startToken, out _IToken endToken)
		{
			return OperandParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
		}

		internal _IExpression ParseSTPrefixOperator(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			endToken = token;
			lenghtOfExpWithoutParanthesis = 0;
			switch (op)
			{
			case Operator.Conversion:
				return ConversionExpressionParser.ParseStatic(Context, out bError, op, token, startToken, out endToken);
			case Operator.Plus:
			case Operator.Minus:
				return UnaryPlusMinusParser.Parse(Context, out bError, op, token, startToken, out endToken);
			case Operator.Not:
				return UnaryNotParser.Parse(Context, out bError, op, token, startToken, out endToken);
			case Operator.Min:
			case Operator.Max:
				return MinMaxOperatorParser.ParseStatic(Context, out bError, op, token, startToken, out endToken);
			case Operator.__Cast:
				return ImplicitCastOperatorParser.ParseStatic(Context, out bError, op, token, startToken, out endToken);
			case Operator.__New:
				return NewExpressionParser.Parse(Context, out bError, token, out endToken);
			case Operator.__Reloc:
			case Operator.Time:
			case Operator.LTime:
			case Operator.Adr:
			case Operator.BitAdr:
			case Operator.IndexOf:
			case Operator.SizeOf:
			case Operator.Ini:
			case Operator.Abs:
			case Operator.Limit:
			case Operator.Trunc:
			case Operator.Mux:
			case Operator.Sel:
			case Operator.Rol:
			case Operator.Ror:
			case Operator.Shl:
			case Operator.Shr:
			case Operator.Exp:
			case Operator.Expt:
			case Operator.Sqrt:
			case Operator.Ln:
			case Operator.Log:
			case Operator.Sin:
			case Operator.Cos:
			case Operator.Tan:
			case Operator.ASin:
			case Operator.ACos:
			case Operator.ATan:
			case Operator.Move:
			case Operator.TestAndSet:
			case Operator.TruncInt:
			case Operator.__LocalOffset:
			case Operator.__VarInfo:
			case Operator.__TypeOf:
			case Operator.__CRC:
			case Operator.__MaxOffset:
			case Operator.__Init:
			case Operator.__IsValidRef:
			case Operator.__QueryInterface:
			case Operator.__QueryPointer:
			case Operator.__Delete:
			case Operator.__AdrInst:
			case Operator.__RefAdr:
			case Operator.__Wait:
			case Operator.__BitOffset:
			case Operator.__FCall:
			case Operator.__PropertyInfo:
			case Operator.__MemorySet:
			case Operator.__GetLTick:
			case Operator.__Throw:
			case Operator.__CheckLicense:
			case Operator.__CallInitFunction:
			case Operator.__LateCompiledExpr:
			case Operator.LowerBound:
			case Operator.UpperBound:
			case Operator.__CheckLicenseBit:
			case Operator.__XAdd:
			case Operator.__MemoryBarrier:
			case Operator.__CompareAndSwap:
			case Operator.__vcSqrt:
			case Operator.__vcMin:
			case Operator.__vcMax:
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			case Operator.__vcStore:
			case Operator.XSizeOf:
			case Operator.__PouName:
			case Operator.__Position:
				return PrefixedOperatorParser.Parse(Context, out bError, op, token, startToken, out endToken);
			case Operator.LeftParenthesis:
				return ParenthesizedExpressionParser.ParseParenthesizedExpressionStatic(Context, out bError, token, out endToken, out lenghtOfExpWithoutParanthesis);
			case Operator.__Copy:
			case Operator.Period:
			case Operator.__SystemScope:
			case Operator.__PoolScope:
				return ScopeExpressionParser.ParseStatic(Context, out bError, op, token, startToken, out endToken);
			case Operator.__CurrentTask:
				return CurrentTaskExpressionParser.ParseStatic(Context, out bError, op, token, startToken, out endToken);
			case Operator.This:
			case Operator.Super:
				return ThisAndBaseExpressionParser.Parse(Context, out bError, op, token, startToken, out endToken);
			default:
				bError = true;
				return null;
			}
		}

		internal _IExpression ParseQualifiedNameExpression(_IExprement expForError)
		{
			return OperandParser.ParseQualifiedNameExpression(expForError);
		}
	}
}
