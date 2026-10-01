using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Statements;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220
{
	// Token: 0x02000007 RID: 7
	internal class InternalParser : IInternalParser2, IInternalParser
	{
		// Token: 0x06000009 RID: 9 RVA: 0x0000233E File Offset: 0x0000053E
		private void SetInDeclaration(bool bValue)
		{
			this.StatementParser.InDeclaration = bValue;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000234C File Offset: 0x0000054C
		private void SetInLibrary(bool bValue)
		{
			this.DeclarationParser.Library = bValue;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000B RID: 11 RVA: 0x0000235A File Offset: 0x0000055A
		private bool Implicit
		{
			get
			{
				return this.StatementParser.Implicit;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002367 File Offset: 0x00000567
		// (set) Token: 0x0600000D RID: 13 RVA: 0x00002374 File Offset: 0x00000574
		public bool ImplicitAnyway
		{
			get
			{
				return this.StatementParser.ImplicitAnyway;
			}
			set
			{
				this.StatementParser.ImplicitAnyway = value;
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002382 File Offset: 0x00000582
		public InternalParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000F RID: 15 RVA: 0x00002398 File Offset: 0x00000598
		private DeclarationParser DeclarationParser
		{
			get
			{
				return this.Context.DeclarationParser;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000023A5 File Offset: 0x000005A5
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000011 RID: 17 RVA: 0x000023B2 File Offset: 0x000005B2
		private ParserContext Context { get; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000023BA File Offset: 0x000005BA
		private _ILanguageModelBuilder7 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000013 RID: 19 RVA: 0x000023C7 File Offset: 0x000005C7
		private ITokenFactory TokenFactory
		{
			get
			{
				return this.Context.TokenFactory;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000023D4 File Offset: 0x000005D4
		// (set) Token: 0x06000015 RID: 21 RVA: 0x000023E1 File Offset: 0x000005E1
		public IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
			set
			{
				this.Context.Scanner = value;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000023EF File Offset: 0x000005EF
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000017 RID: 23 RVA: 0x000023FC File Offset: 0x000005FC
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002404 File Offset: 0x00000604
		public Guid MessageGuid { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000019 RID: 25 RVA: 0x0000240D File Offset: 0x0000060D
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000241A File Offset: 0x0000061A
		private void SetInSTCode()
		{
			this.ExpressionParser.SetInSTCode();
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002427 File Offset: 0x00000627
		public _IStatement ParsePragma(out bool bError, IMinimalPosition errorpos, string stPragma)
		{
			return this.Context.PragmaStatementParser.ParsePragmaIntern(out bError, errorpos, stPragma, this.TokenFactory.Empty());
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002447 File Offset: 0x00000647
		private void Next(out IToken token)
		{
			this.Scanner.Next(out token);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002456 File Offset: 0x00000656
		public TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002466 File Offset: 0x00000666
		private _IStatement ParseST()
		{
			return this.ParseST(false);
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002470 File Offset: 0x00000670
		public _IStatement ParseST(bool bLibrary)
		{
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(this.TokenFactory.Empty());
			isequenceStatement.SetFlag(8L, bLibrary);
			this.MessageGuid = Guid.Empty;
			this.SetInLibrary(bLibrary);
			for (;;)
			{
				try
				{
					_IStatement istatement = this.NextStatement();
					if (istatement != null)
					{
						if (this.m_bFirstStatement)
						{
							_IPragmaStatement ipragmaStatement = istatement as _IPragmaStatement;
							if (ipragmaStatement != null && ipragmaStatement.Text == "ST_IMPLEMENTATION")
							{
								this.SetInSTCode();
							}
						}
						this.m_bFirstStatement = false;
						isequenceStatement.Add(istatement);
						continue;
					}
				}
				catch (MaximumNestingDepthExceededException)
				{
					_IErrorStatement ierrorStatement = this.Context.LMItemFactory.CreateErrorStatement();
					ierrorStatement.SetPositionIntern(this.Context.LMItemFactory.CreateMinimalPosition(0L, 0));
					this.Context.ErrorHandler.AddErrorST(ierrorStatement, 584, Array.Empty<object>());
					isequenceStatement._StatementList.Add(ierrorStatement);
				}
				break;
			}
			if (!this.StatementParser.InDeclaration)
			{
				CodeStatementChecker.CheckForUnexpectedStatementsInImplementation(isequenceStatement, this.Context);
			}
			return isequenceStatement;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002578 File Offset: 0x00000778
		public _IExpression[] ParseSTSnippet()
		{
			ArrayList arrayList = new ArrayList();
			bool flag = false;
			do
			{
				bool flag2;
				_IExpression iexpression = this.ParseAssignExp(out flag2);
				if (iexpression != null && !flag2)
				{
					if (flag && iexpression.ToString() == "ERROR")
					{
						flag = false;
					}
					else
					{
						arrayList.Add(iexpression);
					}
				}
				else
				{
					IToken token;
					this.Next(out token);
					if (this.Scanner.GetTokenText(token) == "!")
					{
						flag = true;
					}
				}
			}
			while (this.Scanner.CurrentToken.Type != 21);
			_IExpression[] array = new _IExpression[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000260B File Offset: 0x0000080B
		public void ExtendForLoop(_IForStatement forstatement)
		{
			ForLoopExtender.ExtendForLoop(forstatement, this.Context.LMItemFactory);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002620 File Offset: 0x00000820
		private _IStatement NextStatement()
		{
			bool flag;
			return this.ParseSTStatement(out flag, true);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002636 File Offset: 0x00000836
		public _IExpression ParseAssignExp(out bool bError)
		{
			if (this.Implicit || this.ImplicitAnyway)
			{
				return this.ParseInitialisationExp(out bError);
			}
			return this.ExpressionParser.ParseAssignment(out bError);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000265C File Offset: 0x0000085C
		private _IExpression ParseInitialisationExp(out bool bError)
		{
			return this.ExpressionParser.ParseInitialisationExp(out bError);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x0000266A File Offset: 0x0000086A
		public _IExpression ParseSTOperand(out bool bError)
		{
			return this.ExpressionParser.ParseSTOperand(out bError);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002678 File Offset: 0x00000878
		public _IExpression ParseInitialisation()
		{
			return this.ExpressionParser.ParseInitialisation();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002685 File Offset: 0x00000885
		internal _IStatement ParseSTStatement(out bool bError)
		{
			return this.ParseSTStatement(out bError, false);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x0000268F File Offset: 0x0000088F
		private _IStatement ParseSTStatement(out bool bError, bool bTopLevel)
		{
			return this.StatementParser.ParseSTStatement(out bError, bTopLevel);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x0000269E File Offset: 0x0000089E
		public Operator MatchOperator(params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, null, true, ops);
		}

		// Token: 0x0600002A RID: 42 RVA: 0x000026B4 File Offset: 0x000008B4
		public _IType ParseType()
		{
			return this.Context.TypeParser.ParseType(true, false, this.TokenFactory.Empty());
		}

		// Token: 0x0600002B RID: 43 RVA: 0x000026D3 File Offset: 0x000008D3
		public _IStatement ParseInterfaceStatement()
		{
			this.SetInDeclaration(true);
			_IStatement istatement = this.ParseST();
			this.SetInDeclaration(false);
			DeclarationStatementChecker.PreventAnyCodeAfterTypeDeclaration(istatement, this.Context);
			return istatement;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x000026F5 File Offset: 0x000008F5
		public IPOUSyntax[] ParsePOUs()
		{
			return POUSyntaxParser.ParsePOUSyntax(this.Context);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002704 File Offset: 0x00000904
		public IPOUSyntax[] ParsePOUs(out IEnumerable<IMessage> parserErrors)
		{
			IPOUSyntax[] array = POUSyntaxParser.ParsePOUSyntax(this.Context);
			IErrorHandler2 errorHandler = this.Context.ErrorHandler as IErrorHandler2;
			if (errorHandler != null)
			{
				parserErrors = errorHandler.GetMessages(array);
			}
			else
			{
				parserErrors = Array.Empty<IMessage>();
			}
			return array;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002744 File Offset: 0x00000944
		public _ISequenceStatement ParseRawST()
		{
			this.Context.Scanner.IncludePragmas = true;
			this.Context.Scanner.IncludeComments = true;
			this.Context.Scanner.AutoIncrementPositionOnLineBreaks = true;
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(this.TokenFactory.Empty());
			this.MessageGuid = Guid.Empty;
			for (;;)
			{
				_IStatement istatement = this.NextStatement();
				if (istatement == null)
				{
					break;
				}
				InternalParser.AddStatementAndHandleDeclarations(istatement, isequenceStatement);
			}
			return isequenceStatement;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x000027BC File Offset: 0x000009BC
		private static void AddStatementAndHandleDeclarations(_IStatement statement, _ISequenceStatement sequenceStatement)
		{
			sequenceStatement._StatementList.Add(statement);
			int count = sequenceStatement._StatementList.Count;
			_IPOUDeclarationStatement ipoudeclarationStatement = statement as _IPOUDeclarationStatement;
			if (ipoudeclarationStatement != null)
			{
				_ISequenceStatement isequenceStatement = ipoudeclarationStatement.Declarations as _ISequenceStatement;
				if (isequenceStatement != null && isequenceStatement._StatementList.Any<_IStatement>())
				{
					while (isequenceStatement._StatementList.Any<_IStatement>() && !(isequenceStatement._StatementList[isequenceStatement._StatementList.Count - 1] is _IVariableDeclarationListStatement))
					{
						_IStatement istatement = isequenceStatement._StatementList[isequenceStatement._StatementList.Count - 1];
						isequenceStatement._StatementList.RemoveAt(isequenceStatement._StatementList.Count - 1);
						sequenceStatement.InsertStatement(count, istatement);
						_IPOUDeclarationStatement ipoudeclarationStatement2 = ipoudeclarationStatement;
						ipoudeclarationStatement2.LengthIntern -= istatement.LengthIntern;
					}
				}
			}
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002888 File Offset: 0x00000A88
		public void CreateLanguageModelOfRawST(IPOUSyntax[] pouSyntax, Guid sourceObjectGuid, Guid parentGuid, ILanguageModel dstLanguageModel)
		{
			LanguageModelOfRawST.CreateLanguageModelOfRawST(this.Context, pouSyntax, sourceObjectGuid, parentGuid, dstLanguageModel);
		}

		// Token: 0x04000002 RID: 2
		private bool m_bFirstStatement = true;
	}
}
