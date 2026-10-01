using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000051 RID: 81
	internal class ContextualOperatorHandler
	{
		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000560 RID: 1376 RVA: 0x00016DBF File Offset: 0x00014FBF
		private ParserContext Context { get; }

		// Token: 0x06000561 RID: 1377 RVA: 0x00016DC8 File Offset: 0x00014FC8
		internal void TryRecognizeContextualDeclarationOperator()
		{
			foreach (Operator @operator in ContextualOperatorHandler.s_ContextualOperators)
			{
				this.Context.StatementParser.RecognizeContextualDeclarationOperator(this.IsToStartDeclaration(@operator), @operator);
			}
		}

		// Token: 0x06000562 RID: 1378 RVA: 0x00016E08 File Offset: 0x00015008
		internal void TreatContextualDeclarationOperatorAsIdentifier()
		{
			foreach (Operator eWhichOperator in ContextualOperatorHandler.s_ContextualOperators)
			{
				this.Context.StatementParser.RecognizeContextualDeclarationOperator(false, eWhichOperator);
			}
		}

		// Token: 0x06000563 RID: 1379 RVA: 0x00016E3F File Offset: 0x0001503F
		internal ContextualOperatorHandler(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x00016E50 File Offset: 0x00015050
		private bool IsTokenWithOperatorText(IToken lookAhead, Operator eOperatorToCheck)
		{
			string tokenText = this.Context.Scanner.GetTokenText(lookAhead);
			return this.Context.Scanner.GetOperatorText(eOperatorToCheck).Equals(tokenText, StringComparison.InvariantCultureIgnoreCase);
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x00016E88 File Offset: 0x00015088
		private bool IsToStartDeclaration(Operator eOperatorToCheck)
		{
			IToken token;
			this.Context.Scanner.Next(out token, true, true);
			bool result = this.IsToStartDeclaration(token, eOperatorToCheck);
			this.Context.Scanner.SetPosition(token);
			return result;
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x00016EC4 File Offset: 0x000150C4
		private bool IsToStartDeclaration(IToken lookAhead1, Operator eOperatorToCheck)
		{
			if (!this.IsTokenWithOperatorText(lookAhead1, eOperatorToCheck))
			{
				return false;
			}
			IToken token;
			TokenType tokenType = this.Context.Scanner.Next(out token, true, true);
			if (token == null)
			{
				return false;
			}
			if (tokenType == 13)
			{
				return true;
			}
			if (tokenType != 15)
			{
				return false;
			}
			Operator @operator = this.Context.Scanner.GetOperator(token);
			return @operator == 224 || @operator - 226 <= 4;
		}

		// Token: 0x040000C7 RID: 199
		private static readonly Operator[] s_ContextualOperators = new Operator[]
		{
			287,
			290
		};
	}
}
