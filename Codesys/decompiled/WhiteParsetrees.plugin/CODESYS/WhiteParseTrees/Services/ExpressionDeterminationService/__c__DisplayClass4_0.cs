using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class ExpressionDeterminationService
	{
		private static void SplitPosition(long nPosition, out long nPositionId, out short nPositionOffset)
		{
			nPositionId = nPosition & 0xFFFFFFFFFFFFL;
			nPositionOffset = (short)(nPosition >> 48);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition)
		{
			return FindExpressionAtSourcePosition(treeInformation, sourcePosition.Position, sourcePosition.PositionOffset);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, long lEditorPosition)
		{
			SplitPosition(lEditorPosition, out var nPositionId, out var nPositionOffset);
			return FindExpressionAtSourcePosition(treeInformation, nPositionId, nPositionOffset);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteExprement FindExpressionAtSourcePosition(IWhiteTreeInformation treeInformation, long lPosition, short nOffset)
		{
			if (treeInformation.SourcePositionMap.GetTextOffsetForPositionOffset(lPosition, nOffset, out var textOffset))
			{
				return FindExpressionAtTextOffset(treeInformation.RootNode, textOffset);
			}
			return null;
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExprement FindExpressionAtTextOffset(INode root, int nOffsetOfToken)
		{
			IList<IWhiteToken> tokenList = TokenSerializer.GetTokenList(root);
			IDictionary<IWhiteToken, int> offsets = OffsetCalculator.CalculateOffsets(root);
			IWhiteToken tokenToFind = tokenList.First((IWhiteToken x) => offsets[x] >= nOffsetOfToken);
			return FindExprementWithToken(root, tokenToFind);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExprement FindSurroundingOperatorAtSourcePosition(IWhiteTreeInformation treeInformation, ISourcePosition sourcePosition, Operator operatorToMatch)
		{
			return FindSurroundingOperatorAtSourcePosition(treeInformation, sourcePosition.Position, sourcePosition.PositionOffset, operatorToMatch);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteExprement FindSurroundingOperatorAtSourcePosition(IWhiteTreeInformation treeInformation, long lPosition, short nOffset, Operator operatorToMatch)
		{
			if (treeInformation.SourcePositionMap.GetTextOffsetForPositionOffset(lPosition, nOffset, out var textOffset))
			{
				return FindSurroundingOperatorAtTextOffset(treeInformation.RootNode, textOffset, operatorToMatch);
			}
			return null;
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExpression FindMatchingWhiteExpressionForRedExpression(IWhiteTreeInformation treeInformation, IExpression expression)
		{
			if (expression.Position == null)
			{
				return null;
			}
			if (!treeInformation.SourcePositionMap.GetTextOffsetForPositionOffset(expression.Position.Position, expression.Position.PositionOffset, out var nOffsetOfToken))
			{
				return null;
			}
			IList<IWhiteToken> tokenList = TokenSerializer.GetTokenList(treeInformation.RootNode);
			IDictionary<IWhiteToken, int> offsets = OffsetCalculator.CalculateOffsets(treeInformation.RootNode);
			IWhiteToken tokenToFind = tokenList.First((IWhiteToken x) => offsets[x] >= nOffsetOfToken);
			return FindMatchingExpressionWithToken(treeInformation.RootNode, tokenToFind, expression);
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteExprement FindExprementWithToken(INode node, IWhiteToken tokenToFind)
		{
			if (node is IWhiteExprement result)
			{
				foreach (INode child in node.GetChildren())
				{
					if (child is IWhiteToken whiteToken)
					{
						if (whiteToken == tokenToFind)
						{
							return result;
						}
						continue;
					}
					IWhiteExprement whiteExprement = FindExprementWithToken(child, tokenToFind);
					if (whiteExprement != null)
					{
						return whiteExprement;
					}
				}
			}
			return null;
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteExpression FindMatchingExpressionWithToken(INode node, IWhiteToken tokenToFind, IExpression expression)
		{
			if (node is IWhiteExprement whiteExprement)
			{
				foreach (INode child in node.GetChildren())
				{
					IWhiteExpression whiteExpression = FindMatchingExpressionWithToken(child, tokenToFind, expression);
					if (whiteExpression != null)
					{
						return whiteExpression;
					}
				}
				if (TokenSerializer.GetTokenList(whiteExprement).Contains(tokenToFind) && whiteExprement is IWhiteExpression whiteExpression2 && ExpressionMatcher.ExpressionsMatch(whiteExpression2, expression))
				{
					return whiteExpression2;
				}
			}
			return null;
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		public static IWhiteExprement FindSurroundingOperatorAtTextOffset(INode root, int nOffsetOfToken, Operator operatorToMatch)
		{
			Operator endOperator = GetEndOperator(operatorToMatch);
			Stack<IWhiteToken> stack = new Stack<IWhiteToken>();
			IList<IWhiteToken> tokenList = TokenSerializer.GetTokenList(root);
			IDictionary<IWhiteToken, int> dictionary = OffsetCalculator.CalculateOffsets(root);
			foreach (IWhiteToken item in tokenList)
			{
				int num = dictionary[item];
				if (item is IWhiteOperatorToken whiteOperatorToken)
				{
					if (whiteOperatorToken.Operator == operatorToMatch)
					{
						stack.Push(item);
					}
					else if (whiteOperatorToken.Operator == endOperator)
					{
						stack.Pop();
					}
				}
				if (num >= nOffsetOfToken)
				{
					break;
				}
			}
			if (stack.Count > 0)
			{
				IWhiteToken tokenToFind = stack.Pop();
				return FindExpressionWithToken(root, tokenToFind);
			}
			return null;
		}

		private static Operator GetEndOperator(Operator @operator)
		{
			switch (@operator)
			{
			case Operator.Action:
				return Operator.EndAction;
			case Operator.Case:
				return Operator.EndCase;
			case Operator.For:
				return Operator.EndFor;
			case Operator.Function:
				return Operator.EndFunction;
			case Operator.FunctionBlock:
				return Operator.EndFunctionBlock;
			case Operator.If:
				return Operator.EndIf;
			case Operator.Program:
				return Operator.EndProgram;
			case Operator.Repeat:
				return Operator.EndRepeat;
			case Operator.Struct:
				return Operator.EndStruct;
			case Operator.Type:
				return Operator.EndType;
			case Operator.Union:
				return Operator.EndUnion;
			case Operator.Var:
				return Operator.EndVar;
			case Operator.While:
				return Operator.EndWhile;
			case Operator.__Try:
				return Operator.__EndTry;
			default:
				return Operator.None;
			}
		}

		[return: System.Runtime.CompilerServices.Nullable(2)]
		private static IWhiteExprement FindExpressionWithToken(INode node, IWhiteToken tokenToFind)
		{
			if (node is IWhiteExprement result)
			{
				foreach (INode child in node.GetChildren())
				{
					if (child is IWhiteToken whiteToken)
					{
						if (whiteToken == tokenToFind)
						{
							return result;
						}
						continue;
					}
					IWhiteExprement whiteExprement = FindExpressionWithToken(child, tokenToFind);
					if (whiteExprement != null)
					{
						return whiteExprement;
					}
				}
			}
			return null;
		}
	}
}
