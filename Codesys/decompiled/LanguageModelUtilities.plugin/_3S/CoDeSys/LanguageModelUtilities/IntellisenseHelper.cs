using System.Collections.Generic;
using System.Text;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[TypeGuid("{DAE4B3B9-61C6-457C-83FB-6E5940D563A7}")]
	public class IntellisenseHelper : IIntellisenseHelper
	{
		public bool ContainsDeclaration(string stDeclarationSnippet)
		{
			List<HumanReadableToken> list = new List<HumanReadableToken>();
			IScanner scanner = null;
			ScanDeclarationSnippet(stDeclarationSnippet, list, out scanner);
			bool flag = false;
			int num = 0;
			int count = list.Count;
			while (num < count && !flag)
			{
				HumanReadableToken humanReadableToken = list[num];
				if (TokenType.Operator == humanReadableToken.Token.Type)
				{
					flag = Operator.Colon == scanner.GetOperator(humanReadableToken.Token);
				}
				if (!flag)
				{
					num++;
				}
			}
			return flag;
		}

		public string DetermineAccessPathInCaseOfStructureInitialization(string stDeclarationSnippet)
		{
			List<HumanReadableToken> tokenList = new List<HumanReadableToken>();
			IScanner scanner = null;
			ScanDeclarationSnippet(stDeclarationSnippet, tokenList, out scanner);
			string result = null;
			if (IsWithinStructureInitialization(tokenList, scanner))
			{
				List<IAccessPathToken> list = CollectAccessPathTokens(tokenList, scanner);
				if (0 < list.Count)
				{
					result = BuildAccessPath(list);
				}
			}
			return result;
		}

		private void ScanDeclarationSnippet(string stDeclarationSnippet, List<HumanReadableToken> tokenList, out IScanner scanner)
		{
			scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stDeclarationSnippet, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			IToken token = null;
			Operator @operator = Operator.None;
			TokenType next = scanner.GetNext(out token);
			while (TokenType.End != next)
			{
				HumanReadableToken item = new HumanReadableToken(token, scanner.GetTokenText(token));
				if (TokenType.Operator == token.Type)
				{
					@operator = scanner.GetOperator(token);
				}
				else
				{
					@operator = Operator.None;
				}
				tokenList.Add(item);
				next = scanner.GetNext(out token);
			}
		}

		private bool SkipFB_InitParamAssignments(List<HumanReadableToken> tokenList, IScanner scanner, ref int i)
		{
			int iIgnoreCounter = 0;
			EState eState = EState.Start;
			EState eState2 = EState.Start;
			Operator @operator = Operator.None;
			while (EState.End != eState && EState.Error != eState && i < tokenList.Count)
			{
				eState2 = EState.Error;
				HumanReadableToken humanReadableToken = tokenList[i];
				if (TokenType.Operator == humanReadableToken.Token.Type)
				{
					@operator = scanner.GetOperator(humanReadableToken.Token);
				}
				switch (eState)
				{
				case EState.Start:
					if (humanReadableToken.Token.Type == TokenType.Operator && @operator == Operator.LeftParenthesis)
					{
						eState2 = EState.IgnoreParenthesis;
						iIgnoreCounter++;
					}
					break;
				case EState.IgnoreParenthesis:
					eState2 = Ignore(humanReadableToken, scanner, ref iIgnoreCounter, Operator.RightParenthesis, Operator.LeftParenthesis, eState);
					if (iIgnoreCounter == 0)
					{
						eState2 = EState.End;
					}
					break;
				}
				if (EState.End != eState2)
				{
					i++;
				}
				eState = eState2;
			}
			return EState.End == eState;
		}

		private bool IsWithinStructureInitialization(List<HumanReadableToken> tokenList, IScanner scanner)
		{
			EState eState = EState.Start;
			EState eState2 = EState.Start;
			int i = 0;
			while (EState.End != eState && EState.Error != eState && i < tokenList.Count)
			{
				eState2 = EState.Error;
				HumanReadableToken humanReadableToken = tokenList[i];
				switch (eState)
				{
				case EState.Start:
					eState2 = CheckIdentifierIgnoringComment(humanReadableToken, EState.IdentifierFound, eState);
					break;
				case EState.IdentifierFound:
					eState2 = CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.Colon, EState.ColonFound, eState);
					break;
				case EState.ColonFound:
					eState2 = CheckIdentifierIgnoringComment(humanReadableToken, EState.TypeFound, eState);
					break;
				case EState.TypeFound:
					eState2 = CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.Period, EState.NamspaceFound, eState);
					if (EState.Error == eState2)
					{
						eState2 = CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.Assign, EState.AssignFound, eState);
						if (EState.Error == eState2 && EState.LeftParenthesisFound == CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.LeftParenthesis, EState.LeftParenthesisFound, eState) && SkipFB_InitParamAssignments(tokenList, scanner, ref i))
						{
							eState2 = EState.TypeFound;
						}
					}
					break;
				case EState.NamspaceFound:
					if (humanReadableToken.Token.Type == TokenType.Identifier)
					{
						eState2 = EState.TypeFound;
					}
					break;
				case EState.CommentAfterTypeFound:
					eState2 = CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.Assign, EState.AssignFound, eState);
					break;
				case EState.AssignFound:
					eState2 = CheckOperatorIgnoringComment(humanReadableToken, scanner, Operator.LeftParenthesis, EState.End, eState);
					break;
				}
				i++;
				eState = eState2;
			}
			return EState.End == eState;
		}

		private EState CheckIdentifierIgnoringComment(HumanReadableToken current, EState eStateNextInCaseOfSuccess, EState eStateCurr)
		{
			EState result = EState.Error;
			switch (current.Token.Type)
			{
			case TokenType.Identifier:
				result = eStateNextInCaseOfSuccess;
				break;
			case TokenType.Comment:
				result = eStateCurr;
				break;
			}
			return result;
		}

		private EState CheckOperatorIgnoringComment(HumanReadableToken current, IScanner scanner, Operator eExpectedOperator, EState eStateNextInCaseOfSuccess, EState eStateCurr)
		{
			EState result = EState.Error;
			switch (current.Token.Type)
			{
			case TokenType.Operator:
				if (eExpectedOperator == scanner.GetOperator(current.Token))
				{
					result = eStateNextInCaseOfSuccess;
				}
				break;
			case TokenType.Comment:
				result = eStateCurr;
				break;
			}
			return result;
		}

		private EState Ignore(HumanReadableToken current, IScanner scanner, ref int iIgnoreCounter, Operator eDecreasingOperator, Operator eIncreasingOperator, EState eStateCurr)
		{
			EState result = eStateCurr;
			if (TokenType.Operator == current.Token.Type)
			{
				Operator @operator = scanner.GetOperator(current.Token);
				if (eDecreasingOperator == @operator)
				{
					iIgnoreCounter--;
					if (iIgnoreCounter == 0)
					{
						result = EState.Start;
					}
				}
				else if (eIncreasingOperator == @operator)
				{
					iIgnoreCounter++;
				}
			}
			return result;
		}

		private Operator SkipPreviousInitialization(List<HumanReadableToken> tokenList, IScanner scanner, ref int i, bool bStopBeforeIdentifier)
		{
			int iIgnoreCounter = 0;
			int iIgnoreCounter2 = 0;
			EState eState = EState.Start;
			EState eState2 = EState.Start;
			Operator @operator = Operator.None;
			while (EState.End != eState && EState.Error != eState && 0 < i)
			{
				eState2 = EState.Error;
				HumanReadableToken humanReadableToken = tokenList[i];
				if (TokenType.Operator == humanReadableToken.Token.Type)
				{
					@operator = scanner.GetOperator(humanReadableToken.Token);
				}
				switch (eState)
				{
				case EState.Start:
					switch (humanReadableToken.Token.Type)
					{
					case TokenType.Operator:
						switch (@operator)
						{
						case Operator.RightParenthesis:
							eState2 = EState.IgnoreParenthesis;
							iIgnoreCounter++;
							break;
						case Operator.RightBracket:
							eState2 = EState.IgnoreBrackets;
							iIgnoreCounter2++;
							break;
						}
						break;
					case TokenType.Comment:
						eState2 = eState;
						break;
					case TokenType.Boolean:
					case TokenType.Date:
					case TokenType.DateAndTime:
					case TokenType.DoubleByteString:
					case TokenType.Duration:
					case TokenType.LDuration:
					case TokenType.Identifier:
					case TokenType.Integer:
					case TokenType.Real:
					case TokenType.SingleByteString:
					case TokenType.TimeOfDay:
					case TokenType.XByteString:
						eState2 = EState.InitializationSkipped;
						break;
					}
					break;
				case EState.IgnoreParenthesis:
					eState2 = Ignore(humanReadableToken, scanner, ref iIgnoreCounter, Operator.LeftParenthesis, Operator.RightParenthesis, eState);
					if (iIgnoreCounter == 0)
					{
						eState2 = (bStopBeforeIdentifier ? EState.End : EState.InitializationSkipped);
					}
					break;
				case EState.IgnoreBrackets:
					eState2 = Ignore(humanReadableToken, scanner, ref iIgnoreCounter2, Operator.LeftBracket, Operator.RightBracket, eState);
					if (iIgnoreCounter2 == 0)
					{
						eState2 = EState.InitializationSkipped;
					}
					break;
				case EState.InitializationSkipped:
					switch (humanReadableToken.Token.Type)
					{
					case TokenType.Operator:
						eState2 = EState.End;
						break;
					case TokenType.Comment:
						eState2 = eState;
						break;
					case TokenType.Integer:
						eState2 = eState;
						break;
					}
					break;
				}
				if (EState.End != eState2)
				{
					i--;
				}
				eState = eState2;
			}
			return @operator;
		}

		private List<IAccessPathToken> CollectAccessPathTokens(List<HumanReadableToken> tokenList, IScanner scanner)
		{
			List<IAccessPathToken> list = new List<IAccessPathToken>();
			Operator @operator = Operator.None;
			int num = 0;
			EState eState = EState.Start;
			EState eState2 = EState.Start;
			int i = tokenList.Count - 1;
			while (EState.End != eState && EState.Error != eState && 0 < i)
			{
				eState2 = EState.Error;
				HumanReadableToken humanReadableToken = tokenList[i];
				if (TokenType.Operator == humanReadableToken.Token.Type)
				{
					@operator = scanner.GetOperator(humanReadableToken.Token);
				}
				switch (eState)
				{
				case EState.Start:
					switch (humanReadableToken.Token.Type)
					{
					case TokenType.Operator:
						switch (@operator)
						{
						case Operator.LeftParenthesis:
							eState2 = eState;
							break;
						case Operator.LeftBracket:
							if (num == 0)
							{
								eState2 = EState.End;
								break;
							}
							eState2 = eState;
							list.Add(new IndexAccessAccessPathToken());
							break;
						case Operator.Assign:
							eState2 = EState.ExpectStructMemberInitialization;
							break;
						case Operator.Comma:
							eState2 = EState.SkipInitialization;
							break;
						}
						break;
					case TokenType.Identifier:
						if (num == 0)
						{
							_ = humanReadableToken.TokenText;
							eState2 = eState;
						}
						else
						{
							eState2 = EState.CalleeFound;
							list.Add(new VariableAccessPathToken(humanReadableToken.TokenText));
						}
						break;
					case TokenType.Integer:
						eState2 = eState;
						break;
					case TokenType.Comment:
						eState2 = eState;
						break;
					}
					num++;
					break;
				case EState.CalleeFound:
					eState2 = EState.End;
					break;
				case EState.ExpectStructMemberInitialization:
					switch (humanReadableToken.Token.Type)
					{
					case TokenType.Identifier:
						list.Add(new VariableAccessPathToken(humanReadableToken.TokenText));
						eState2 = EState.IdentifierFound;
						break;
					case TokenType.Operator:
						if (@operator == Operator.RightParenthesis)
						{
							@operator = SkipPreviousInitialization(tokenList, scanner, ref i, bStopBeforeIdentifier: true);
							eState2 = eState;
						}
						break;
					}
					break;
				case EState.IdentifierFound:
					if (humanReadableToken.Token.Type == TokenType.Operator)
					{
						switch (@operator)
						{
						case Operator.LeftParenthesis:
							eState2 = EState.Start;
							break;
						case Operator.LeftBracket:
							eState2 = EState.Start;
							list.Add(new IndexAccessAccessPathToken());
							break;
						case Operator.Comma:
							eState2 = EState.SkipInitialization;
							break;
						case Operator.Period:
							eState2 = EState.DotFound;
							break;
						}
					}
					break;
				case EState.SkipInitialization:
					@operator = SkipPreviousInitialization(tokenList, scanner, ref i, bStopBeforeIdentifier: false);
					switch (@operator)
					{
					case Operator.Assign:
						eState2 = EState.SkipPreviousStructMemberInitialization;
						break;
					case Operator.Comma:
						eState2 = eState;
						break;
					case Operator.LeftBracket:
						eState2 = EState.Start;
						list.Add(new IndexAccessAccessPathToken());
						break;
					}
					break;
				case EState.SkipPreviousStructMemberInitialization:
					if (humanReadableToken.Token.Type == TokenType.Identifier)
					{
						eState2 = EState.StructMemberInitializationSkipped;
					}
					break;
				case EState.StructMemberInitializationSkipped:
					switch (@operator)
					{
					case Operator.Comma:
						eState2 = EState.SkipInitialization;
						break;
					case Operator.LeftBracket:
						eState2 = EState.Start;
						list.Add(new IndexAccessAccessPathToken());
						break;
					case Operator.LeftParenthesis:
						eState2 = EState.Start;
						break;
					}
					break;
				case EState.DotFound:
					if (humanReadableToken.Token.Type == TokenType.Identifier)
					{
						list.Add(new VariableAccessPathToken(humanReadableToken.TokenText));
						eState2 = ((1 < i) ? EState.ExpectDotOrColon : EState.End);
					}
					break;
				case EState.ExpectDotOrColon:
					if (TokenType.Operator == humanReadableToken.Token.Type)
					{
						switch (@operator)
						{
						case Operator.Period:
							eState2 = EState.DotFound;
							break;
						case Operator.Colon:
							eState2 = EState.End;
							break;
						}
					}
					break;
				}
				i--;
				eState = eState2;
			}
			return list;
		}

		private string BuildAccessPath(List<IAccessPathToken> lstIdentifiers)
		{
			lstIdentifiers.Reverse();
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < lstIdentifiers.Count; i++)
			{
				if (0 < i && lstIdentifiers[i].NeedsSeparator)
				{
					stringBuilder.Append(".");
				}
				stringBuilder.Append(lstIdentifiers[i].StringRepresentation);
			}
			return stringBuilder.ToString();
		}
	}
}
