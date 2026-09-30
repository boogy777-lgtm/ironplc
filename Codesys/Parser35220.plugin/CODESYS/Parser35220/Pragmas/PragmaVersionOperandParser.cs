using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Pragmas
{
	// Token: 0x0200003A RID: 58
	internal readonly struct PragmaVersionOperandParser
	{
		// Token: 0x06000416 RID: 1046 RVA: 0x000123BE File Offset: 0x000105BE
		private PragmaVersionOperandParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x000123C8 File Offset: 0x000105C8
		internal static _IExpression ParsePragmaVersionOperand(ParserContext context, _IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			PragmaVersionOperandParser pragmaVersionOperandParser = new PragmaVersionOperandParser(context);
			return pragmaVersionOperandParser.ParseVersionPragmaOperand(verexpr, bUseWarningInsteadOfError);
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x06000418 RID: 1048 RVA: 0x000123E6 File Offset: 0x000105E6
		private ParserContext Context { get; }

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000419 RID: 1049 RVA: 0x000123EE File Offset: 0x000105EE
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600041A RID: 1050 RVA: 0x000123FB File Offset: 0x000105FB
		private PragmaStatementParser PragmaParser
		{
			get
			{
				return this.Context.PragmaStatementParser;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600041B RID: 1051 RVA: 0x00012408 File Offset: 0x00010608
		private IPragmaScanner PragmaScanner
		{
			get
			{
				return this.PragmaParser.PragmaScanner;
			}
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x0600041C RID: 1052 RVA: 0x00012415 File Offset: 0x00010615
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00012422 File Offset: 0x00010622
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00012434 File Offset: 0x00010634
		private void AddErrorSTAndAdjustSourcePosition(_IExprement exp, MessageId nErrorId, IToken token, params object[] args)
		{
			IMinimalPosition positionIntern = this.LMItemFactory.CreateMinimalPosition(exp.PositionIntern.EditorPosition, token.PositionOffset + 1);
			exp.PositionIntern = positionIntern;
			exp.PositionLength = (short)token.Length;
			this.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00012480 File Offset: 0x00010680
		private void AddWarningSTAndAdjustSourcePosition(_IExprement exp, MessageId nErrorId, IToken token, params object[] args)
		{
			IMinimalPosition positionIntern = this.LMItemFactory.CreateMinimalPosition(exp.PositionIntern.EditorPosition, token.PositionOffset + 1);
			exp.PositionIntern = positionIntern;
			exp.PositionLength = (short)token.Length;
			this.ErrorHandler.AddWarningST(exp, nErrorId, args);
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x000124D0 File Offset: 0x000106D0
		private _IExpression ParseVersionPragmaOperand(_IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			IPragmaToken pragmaToken;
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 40)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 6, pragmaToken.OrgToken, new object[]
				{
					"(",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
				return verexpr;
			}
			Operator @operator = 0;
			bool flag = false;
			bool flag2 = false;
			IToken token;
			if (15 == this.PragmaScanner.OrgScanner.GetNext(ref token))
			{
				flag = true;
				@operator = this.PragmaScanner.OrgScanner.GetOperator(token);
				flag2 = this.IsComparisonOperator(@operator);
			}
			if (!flag2)
			{
				if (flag && bUseWarningInsteadOfError)
				{
					this.AddWarningSTAndAdjustSourcePosition(verexpr, 449, token, new object[]
					{
						this.PragmaScanner.OrgScanner.GetTokenText(token)
					});
				}
				else
				{
					this.AddErrorSTAndAdjustSourcePosition(verexpr, 449, token, new object[]
					{
						this.PragmaScanner.OrgScanner.GetTokenText(token)
					});
				}
				return verexpr;
			}
			verexpr.SetOpComparison(@operator);
			IPragmaToken pragmaToken2;
			if (this.CheckNextOperator(out pragmaToken2, 47))
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 6, pragmaToken2.OrgToken, new object[]
				{
					",",
					this.PragmaScanner.GetTokenText(pragmaToken2)
				});
				return verexpr;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken2) != 4)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 450, pragmaToken2.OrgToken, new object[]
				{
					this.PragmaScanner.GetTokenText(pragmaToken2)
				});
				return verexpr;
			}
			if (this.SetVersionAndReturnError(verexpr, pragmaToken2))
			{
				return verexpr;
			}
			if (this.PragmaScanner.GetNext(ref pragmaToken) != 3 || pragmaToken.Operator != 41)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 6, pragmaToken.OrgToken, new object[]
				{
					")",
					this.PragmaScanner.GetTokenText(pragmaToken)
				});
			}
			return verexpr;
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x0001268C File Offset: 0x0001088C
		private bool SetVersionAndReturnError(_IVersionComparisonSupportingExpression verexpr, IPragmaToken ptoken)
		{
			try
			{
				verexpr.SetVersionToTest(new Version(ptoken.String));
			}
			catch (ArgumentOutOfRangeException)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 452, ptoken.OrgToken, new object[]
				{
					this.PragmaScanner.GetTokenText(ptoken)
				});
				return true;
			}
			catch (ArgumentException)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 453, ptoken.OrgToken, new object[]
				{
					this.PragmaScanner.GetTokenText(ptoken)
				});
				return true;
			}
			catch (OverflowException)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 451, ptoken.OrgToken, new object[]
				{
					this.PragmaScanner.GetTokenText(ptoken)
				});
				return true;
			}
			catch (FormatException)
			{
				this.AddErrorSTAndAdjustSourcePosition(verexpr, 453, ptoken.OrgToken, new object[]
				{
					this.PragmaScanner.GetTokenText(ptoken)
				});
				return true;
			}
			return false;
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x0001279C File Offset: 0x0001099C
		private bool CheckNextOperator(out IPragmaToken ptoken, PragmaOperator op)
		{
			return this.PragmaScanner.GetNext(ref ptoken) != 3 || ptoken.Operator != op;
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x000127BC File Offset: 0x000109BC
		private bool IsComparisonOperator(Operator operatorToCheck)
		{
			return operatorToCheck - 134 <= 5 || operatorToCheck - 175 <= 5;
		}
	}
}
