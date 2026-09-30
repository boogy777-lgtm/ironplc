using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x0200004F RID: 79
	internal readonly struct EnumListParser
	{
		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000532 RID: 1330 RVA: 0x00016330 File Offset: 0x00014530
		private ParserContext Context { get; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000533 RID: 1331 RVA: 0x00016338 File Offset: 0x00014538
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000534 RID: 1332 RVA: 0x00016345 File Offset: 0x00014545
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x06000535 RID: 1333 RVA: 0x00016352 File Offset: 0x00014552
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000536 RID: 1334 RVA: 0x0001635F File Offset: 0x0001455F
		private _ILanguageModelBuilder7 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000537 RID: 1335 RVA: 0x0001636C File Offset: 0x0001456C
		private ITypeTable3 TypeTable
		{
			get
			{
				return this.Context.TypeTable;
			}
		}

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000538 RID: 1336 RVA: 0x00016379 File Offset: 0x00014579
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000539 RID: 1337 RVA: 0x00016386 File Offset: 0x00014586
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600053A RID: 1338 RVA: 0x00016393 File Offset: 0x00014593
		private Operator MatchOperator(_IExprement exprement, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, exprement, ops);
		}

		// Token: 0x0600053B RID: 1339 RVA: 0x000163A8 File Offset: 0x000145A8
		private EnumListParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x0600053C RID: 1340 RVA: 0x000163B4 File Offset: 0x000145B4
		internal static _IEnumDeclarationListStatement ParseEnumList(ParserContext context, IToken tokenParenthesis, string stEnumName)
		{
			EnumListParser enumListParser = new EnumListParser(context);
			return enumListParser.ParseEnumList(tokenParenthesis, stEnumName);
		}

		// Token: 0x0600053D RID: 1341 RVA: 0x000163D4 File Offset: 0x000145D4
		private _IEnumDeclarationListStatement ParseEnumList(IToken tokenParenthesis, string stEnumName)
		{
			_IEnumDeclarationListStatement ienumDeclarationListStatement = this.LMItemFactory.CreateEnumDeclarationListStatement(tokenParenthesis);
			Operator @operator;
			do
			{
				_IExpression iexpression = null;
				_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement();
				IToken token;
				if (!this.GetIdentifierToken(isequenceStatement, ienumDeclarationListStatement, out token))
				{
					break;
				}
				string identifier = this.Scanner.GetIdentifier(token);
				IToken nextTokenAndCollectPragmaAndCommentStatements = this.GetNextTokenAndCollectPragmaAndCommentStatements(isequenceStatement);
				this.Scanner.SetPosition(nextTokenAndCollectPragmaAndCommentStatements);
				_IExprement exprement = ienumDeclarationListStatement;
				Operator[] array = new Operator[3];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.2FF501C2E3C3E147436F7C3445D51A91AE291FEC969F2D0F7C3EDAB4DB92F37B).FieldHandle);
				@operator = this.MatchOperator(exprement, array);
				if (@operator == 164)
				{
					IToken currentToken = this.Scanner.CurrentToken;
					bool flag;
					iexpression = this.ExpressionParser.ParseInitialisationExp(out flag);
					@operator = this.MatchOperator(ienumDeclarationListStatement, new Operator[]
					{
						171,
						168
					});
					if (@operator != null)
					{
						this.Scanner.SetPosition(currentToken);
						this.ParseEndOfDeclaration(stEnumName, isequenceStatement, ienumDeclarationListStatement, @operator);
					}
				}
				if (isequenceStatement._StatementList.Count < 1)
				{
					isequenceStatement = null;
				}
				ienumDeclarationListStatement.AddEnumDeclaration(identifier, iexpression, isequenceStatement, token);
			}
			while (@operator == 171);
			IToken token2;
			this.Scanner.Next(out token2);
			this.Scanner.SetPosition(token2);
			if (token2.Type == 15 && this.Scanner.GetOperator(token2) != 172 && this.Scanner.GetOperator(token2) != 164)
			{
				ienumDeclarationListStatement._BaseType = (this.TypeParser.ParseType() ?? this.TypeTable.Int);
			}
			return ienumDeclarationListStatement;
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0001653C File Offset: 0x0001473C
		private void ParseEndOfDeclaration(string stEnumName, _ISequenceStatement seqAttributesEtc, _IEnumDeclarationListStatement edls, Operator opMatch)
		{
			bool flag = true;
			int num = 0;
			TokenType tokenType = 0;
			bool flag3;
			do
			{
				bool flag2 = 4 == tokenType;
				IToken token;
				tokenType = this.Scanner.Next(out token, flag, true);
				if (flag2 && !flag)
				{
					flag = true;
				}
				flag3 = false;
				if (tokenType <= 4)
				{
					if (tokenType - 2 > 1)
					{
						if (tokenType == 4)
						{
							this.ParsePragma(stEnumName, edls, token);
							flag = false;
						}
					}
					else
					{
						this.ParseComment(seqAttributesEtc, token);
					}
				}
				else if (tokenType != 15)
				{
					if (tokenType == 21)
					{
						flag3 = true;
					}
				}
				else
				{
					Operator @operator = this.Scanner.GetOperator(token);
					if (@operator == opMatch && num == 0)
					{
						flag3 = true;
					}
					else
					{
						num = EnumListParser.CalculateParenthesisBalance(num, @operator);
					}
				}
			}
			while (!flag3);
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x000165CC File Offset: 0x000147CC
		private static int CalculateParenthesisBalance(int paranthesisBalance, Operator opTest)
		{
			if (opTest == 167)
			{
				return paranthesisBalance + 1;
			}
			if (opTest == 168)
			{
				return paranthesisBalance - 1;
			}
			return paranthesisBalance;
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x000165E8 File Offset: 0x000147E8
		private void ParsePragma(string stEnumName, _IEnumDeclarationListStatement edls, IToken token)
		{
			this.Scanner.SetPosition(token);
			bool flag;
			_IPragmaIfStatement ipragmaIfStatement = this.StatementParser.ParseSTStatement(out flag, false) as _IPragmaIfStatement;
			if (ipragmaIfStatement != null && ipragmaIfStatement.ConditionExpression is _IProjectDefinedExpression && stEnumName != null)
			{
				this.ErrorHandler.AddErrorSTWithToken(edls, token, 570, new object[]
				{
					stEnumName
				});
			}
			this.Scanner.SetPosition(token);
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00016650 File Offset: 0x00014850
		private void ParseComment(_ISequenceStatement seqAttributesEtc, IToken token)
		{
			this.Scanner.SetPosition(token);
			bool flag;
			_IStatement istatement = this.StatementParser.ParseSTStatement(out flag, false);
			if (flag)
			{
				this.Scanner.ParseReSyncIF();
			}
			seqAttributesEtc.Add(istatement);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00016690 File Offset: 0x00014890
		private bool GetIdentifierToken(_ISequenceStatement seqAttributesEtc, _IEnumDeclarationListStatement edls, out IToken identifierToken)
		{
			identifierToken = this.GetNextTokenAndCollectPragmaAndCommentStatements(seqAttributesEtc);
			if (identifierToken.Type != 13)
			{
				this.ErrorHandler.AddErrorSTWithToken(edls, identifierToken, 26, new object[]
				{
					this.Scanner.GetTokenText(identifierToken)
				});
				return false;
			}
			return true;
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x000166DC File Offset: 0x000148DC
		private IToken GetNextTokenAndCollectPragmaAndCommentStatements(_ISequenceStatement seqAttributesEtc)
		{
			IToken token;
			for (;;)
			{
				TokenType tokenType = this.Scanner.Next(out token, true, true);
				if (tokenType != 2 && tokenType != 3 && tokenType != 4)
				{
					break;
				}
				this.Scanner.SetPosition(token);
				bool flag;
				_IStatement istatement = this.StatementParser.ParseSTStatement(out flag, false);
				if (flag)
				{
					this.Scanner.ParseReSyncIF();
				}
				seqAttributesEtc.Add(istatement);
			}
			return token;
		}
	}
}
