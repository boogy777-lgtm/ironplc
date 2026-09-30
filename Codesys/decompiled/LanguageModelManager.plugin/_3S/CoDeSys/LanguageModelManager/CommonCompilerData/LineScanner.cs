using System;
using System.Collections;
using System.Text;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.CommonCompilerData
{
	// Token: 0x020001C1 RID: 449
	[TypeGuid("{8E3106BE-2809-4180-9C92-A751D0AB7914}")]
	public class LineScanner : ILineScanner
	{
		// Token: 0x06001FD9 RID: 8153 RVA: 0x000580E0 File Offset: 0x000570E0
		public LineScanner()
		{
			if (APEnvironmentFacade.Instance.InjectionCompleted && APEnvironmentFacade.Instance.OEMCustomization.HasValue("LanguageModelManager", "NonCompliantIdentifiers"))
			{
				this.SupportNonCompliantIdentifiers = APEnvironmentFacade.Instance.OEMCustomization.GetBoolValue("LanguageModelManager", "NonCompliantIdentifiers");
			}
		}

		// Token: 0x1700084B RID: 2123
		// (get) Token: 0x06001FDA RID: 8154 RVA: 0x00058139 File Offset: 0x00057139
		// (set) Token: 0x06001FDB RID: 8155 RVA: 0x00058141 File Offset: 0x00057141
		public bool SupportNonCompliantIdentifiers { get; private set; }

		// Token: 0x06001FDC RID: 8156 RVA: 0x0005814C File Offset: 0x0005714C
		public ILineScannerContext ScanLine(IScanner scanner, string stLine, ILineScannerContext prevContext, bool bReturnTokens, out ILineScannerToken[] tokens)
		{
			if (scanner == null)
			{
				throw new ArgumentNullException("scanner");
			}
			if (stLine == null)
			{
				throw new ArgumentNullException("stLine");
			}
			bool includeComments = scanner.IncludeComments;
			bool includeEndOfLines = scanner.IncludeEndOfLines;
			bool includePragmas = scanner.IncludePragmas;
			bool includeWhitespaces = scanner.IncludeWhitespaces;
			scanner.IncludeComments = true;
			scanner.IncludeEndOfLines = true;
			scanner.IncludePragmas = true;
			scanner.IncludeWhitespaces = true;
			ILineScannerContext result;
			try
			{
				LineScannerContext lineScannerContext = new LineScannerContext();
				if (prevContext != null)
				{
					lineScannerContext.IncompleteTokenType = prevContext.IncompleteTokenType;
					lineScannerContext.Depth = prevContext.Depth;
				}
				this.CheckForIncompleteTokens(scanner, stLine, lineScannerContext);
				if (lineScannerContext.IncompleteTokenType == TokenType.DocComment)
				{
					lineScannerContext.IncompleteTokenType = TokenType.None;
					LineScannerContext lineScannerContext2 = lineScannerContext;
					int depth = lineScannerContext2.Depth;
					lineScannerContext2.Depth = depth - 1;
					Debug.Assert(lineScannerContext.Depth == 0);
				}
				if (bReturnTokens)
				{
					tokens = this.HandleReturnTokens(scanner, stLine, prevContext, includeComments, includeEndOfLines, includePragmas, includeWhitespaces, lineScannerContext);
				}
				else
				{
					tokens = null;
				}
				result = lineScannerContext;
			}
			finally
			{
				scanner.IncludeComments = includeComments;
				scanner.IncludeEndOfLines = includeEndOfLines;
				scanner.IncludePragmas = includePragmas;
				scanner.IncludeWhitespaces = includeWhitespaces;
			}
			return result;
		}

		// Token: 0x06001FDD RID: 8157 RVA: 0x00058260 File Offset: 0x00057260
		private ILineScannerToken[] HandleReturnTokens(IScanner scanner, string stLine, ILineScannerContext prevContext, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces, LineScannerContext context)
		{
			string text = string.Empty;
			int nPrefixLength = 0;
			int nSuffixLength = 0;
			if (prevContext != null)
			{
				string prefix = this.GetPrefix(prevContext);
				text += prefix;
				nPrefixLength = prefix.Length;
			}
			text += stLine;
			if (context != null)
			{
				string suffix = this.GetSuffix(context);
				text += suffix;
				nSuffixLength = suffix.Length;
			}
			scanner.Initialize(text);
			ArrayList arrayList = new ArrayList();
			for (;;)
			{
				IToken token;
				TokenType next = scanner.GetNext(out token);
				if (next == TokenType.End)
				{
					break;
				}
				LineScannerToken value = new LineScannerToken(next, token.SourceOffset, token.Length);
				arrayList.Add(value);
			}
			LineScanner.AdjustFirstAndLastToken(nPrefixLength, nSuffixLength, arrayList);
			LineScanner.FilterTokens(bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces, arrayList);
			ILineScannerToken[] array = new ILineScannerToken[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x06001FDE RID: 8158 RVA: 0x0005832C File Offset: 0x0005732C
		private static void FilterTokens(bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces, ArrayList tokenList)
		{
			for (int i = tokenList.Count - 1; i >= 0; i--)
			{
				TokenType type = ((LineScannerToken)tokenList[i]).Type;
				if (type <= TokenType.Pragma)
				{
					if (type - TokenType.Comment > 1)
					{
						if (type == TokenType.Pragma)
						{
							if (!bIncludePragmas)
							{
								tokenList.RemoveAt(i);
							}
						}
					}
					else if (!bIncludeComments)
					{
						tokenList.RemoveAt(i);
					}
				}
				else if (type != TokenType.EndOfLine)
				{
					if (type == TokenType.Whitespace)
					{
						if (!bIncludeWhitespaces)
						{
							tokenList.RemoveAt(i);
						}
					}
				}
				else if (!bIncludeEndOfLines)
				{
					tokenList.RemoveAt(i);
				}
			}
		}

		// Token: 0x06001FDF RID: 8159 RVA: 0x000583B0 File Offset: 0x000573B0
		private static void AdjustFirstAndLastToken(int nPrefixLength, int nSuffixLength, ArrayList tokenList)
		{
			if (tokenList.Count > 0)
			{
				((LineScannerToken)tokenList[0]).Length -= nPrefixLength;
				((LineScannerToken)tokenList[tokenList.Count - 1]).Length -= nSuffixLength;
				for (int i = 1; i < tokenList.Count; i++)
				{
					((LineScannerToken)tokenList[i]).LineOffset -= nPrefixLength;
				}
			}
		}

		// Token: 0x06001FE0 RID: 8160 RVA: 0x0005842C File Offset: 0x0005742C
		private void CheckForIncompleteTokens(IScanner scanner, string stLine, LineScannerContext context)
		{
			bool flag = false;
			for (int i = 0; i < stLine.Length; i++)
			{
				char c = stLine[i];
				if (c <= '/')
				{
					if (c != '"')
					{
						switch (c)
						{
						case '\'':
							if (!flag)
							{
								TokenType incompleteTokenType = context.IncompleteTokenType;
								if (incompleteTokenType != TokenType.None)
								{
									if (incompleteTokenType == TokenType.SingleByteString)
									{
										LineScanner.LeaveIncompleteToken(context);
									}
								}
								else
								{
									LineScanner.EnterIncompleteToken(context, TokenType.SingleByteString);
								}
							}
							break;
						case '(':
							if (i + 1 < stLine.Length && stLine[i + 1] == '*')
							{
								TokenType incompleteTokenType = context.IncompleteTokenType;
								if (incompleteTokenType != TokenType.None)
								{
									if (incompleteTokenType == TokenType.Comment)
									{
										Debug.Assert(context.Depth > 0);
										context.Depth = (scanner.AllowNestedComments ? (context.Depth + 1) : 1);
										i++;
									}
								}
								else
								{
									context.IncompleteTokenType = TokenType.Comment;
									Debug.Assert(context.Depth == 0);
									context.Depth = (scanner.AllowNestedComments ? (context.Depth + 1) : 1);
									i++;
								}
							}
							break;
						case ')':
							break;
						case '*':
							if (i + 1 < stLine.Length && stLine[i + 1] == ')' && context.IncompleteTokenType == TokenType.Comment)
							{
								Debug.Assert(context.Depth > 0);
								int depth = context.Depth;
								context.Depth = depth - 1;
								i++;
								if (context.Depth == 0)
								{
									context.IncompleteTokenType = TokenType.None;
								}
							}
							break;
						default:
							if (c == '/')
							{
								if (i + 1 < stLine.Length && stLine[i + 1] == '/' && context.IncompleteTokenType == TokenType.None)
								{
									context.IncompleteTokenType = TokenType.DocComment;
									Debug.Assert(context.Depth == 0);
									int depth = context.Depth;
									context.Depth = depth + 1;
									i++;
								}
							}
							break;
						}
					}
					else if (!flag)
					{
						TokenType incompleteTokenType = context.IncompleteTokenType;
						if (incompleteTokenType != TokenType.None)
						{
							if (incompleteTokenType == TokenType.DoubleByteString)
							{
								LineScanner.LeaveIncompleteToken(context);
							}
						}
						else
						{
							LineScanner.EnterIncompleteToken(context, TokenType.DoubleByteString);
						}
					}
				}
				else if (c != '`')
				{
					if (c != '{')
					{
						if (c == '}')
						{
							if (context.IncompleteTokenType == TokenType.Pragma)
							{
								LineScanner.LeaveIncompleteToken(context);
							}
						}
					}
					else if (context.IncompleteTokenType == TokenType.None)
					{
						LineScanner.EnterIncompleteToken(context, TokenType.Pragma);
					}
				}
				else if (this.SupportNonCompliantIdentifiers)
				{
					if (context.IncompleteTokenType == TokenType.None)
					{
						LineScanner.EnterIncompleteToken(context, TokenType.Identifier);
					}
					else if (context.IncompleteTokenType == TokenType.Identifier)
					{
						LineScanner.LeaveIncompleteToken(context);
					}
				}
				flag = ((!flag || stLine[i] != '$') && stLine[i] == '$');
			}
		}

		// Token: 0x06001FE1 RID: 8161 RVA: 0x000586C8 File Offset: 0x000576C8
		private static void EnterIncompleteToken(LineScannerContext context, TokenType type)
		{
			context.IncompleteTokenType = type;
			Debug.Assert(context.Depth == 0);
			int depth = context.Depth;
			context.Depth = depth + 1;
		}

		// Token: 0x06001FE2 RID: 8162 RVA: 0x000586FC File Offset: 0x000576FC
		private static void LeaveIncompleteToken(LineScannerContext context)
		{
			context.IncompleteTokenType = TokenType.None;
			int depth = context.Depth;
			context.Depth = depth - 1;
			Debug.Assert(context.Depth == 0);
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x00058730 File Offset: 0x00057730
		private string GetPrefix(ILineScannerContext context)
		{
			Debug.Assert(context != null);
			if (context == null)
			{
				return string.Empty;
			}
			TokenType incompleteTokenType = context.IncompleteTokenType;
			if (incompleteTokenType <= TokenType.DoubleByteString)
			{
				switch (incompleteTokenType)
				{
				case TokenType.None:
					return string.Empty;
				case TokenType.Boolean:
				case TokenType.DocComment:
					break;
				case TokenType.Comment:
				{
					Debug.Assert(context.Depth > 0);
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < context.Depth; i++)
					{
						stringBuilder.Append("(*");
					}
					return stringBuilder.ToString();
				}
				case TokenType.Pragma:
					Debug.Assert(context.Depth == 1);
					return "{";
				default:
					if (incompleteTokenType == TokenType.DoubleByteString)
					{
						Debug.Assert(context.Depth == 1);
						return "\"";
					}
					break;
				}
			}
			else if (incompleteTokenType != TokenType.Identifier)
			{
				if (incompleteTokenType == TokenType.SingleByteString)
				{
					Debug.Assert(context.Depth == 1);
					return "'";
				}
			}
			else
			{
				if (!this.SupportNonCompliantIdentifiers)
				{
					Debug.Fail("Token type cannot be incomplete: " + context.IncompleteTokenType.ToString());
					return string.Empty;
				}
				Debug.Assert(context.Depth == 1);
				return "`";
			}
			Debug.Fail("Token type cannot be incomplete: " + context.IncompleteTokenType.ToString());
			return string.Empty;
		}

		// Token: 0x06001FE4 RID: 8164 RVA: 0x00058878 File Offset: 0x00057878
		private string GetSuffix(ILineScannerContext context)
		{
			Debug.Assert(context != null);
			if (context == null)
			{
				return string.Empty;
			}
			TokenType incompleteTokenType = context.IncompleteTokenType;
			if (incompleteTokenType <= TokenType.DoubleByteString)
			{
				switch (incompleteTokenType)
				{
				case TokenType.None:
					return string.Empty;
				case TokenType.Boolean:
				case TokenType.DocComment:
					break;
				case TokenType.Comment:
				{
					Debug.Assert(context.Depth > 0);
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < context.Depth; i++)
					{
						stringBuilder.Append(" *)");
					}
					return stringBuilder.ToString();
				}
				case TokenType.Pragma:
					Debug.Assert(context.Depth == 1);
					return "}";
				default:
					if (incompleteTokenType == TokenType.DoubleByteString)
					{
						Debug.Assert(context.Depth == 1);
						return "\"";
					}
					break;
				}
			}
			else if (incompleteTokenType != TokenType.Identifier)
			{
				if (incompleteTokenType == TokenType.SingleByteString)
				{
					Debug.Assert(context.Depth == 1);
					return "'";
				}
			}
			else
			{
				if (!this.SupportNonCompliantIdentifiers)
				{
					Debug.Fail("Token type cannot be incomplete: " + context.IncompleteTokenType.ToString());
					return string.Empty;
				}
				Debug.Assert(context.Depth == 1);
				return "`";
			}
			Debug.Fail("Token type cannot be incomplete: " + context.IncompleteTokenType.ToString());
			return string.Empty;
		}
	}
}
