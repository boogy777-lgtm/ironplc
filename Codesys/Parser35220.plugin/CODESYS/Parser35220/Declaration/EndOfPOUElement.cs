using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000055 RID: 85
	internal class EndOfPOUElement : SyntaxElement
	{
		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000577 RID: 1399 RVA: 0x000170B7 File Offset: 0x000152B7
		internal Operator EndOfPOUOperator { get; }

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000578 RID: 1400 RVA: 0x000170BF File Offset: 0x000152BF
		internal long Position { get; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000579 RID: 1401 RVA: 0x000170C7 File Offset: 0x000152C7
		internal short PositionOffset { get; }

		// Token: 0x0600057A RID: 1402 RVA: 0x000170CF File Offset: 0x000152CF
		internal EndOfPOUElement(Operator endOfPOUOperator, IToken token)
		{
			this.EndOfPOUOperator = endOfPOUOperator;
			this.Position = token.Position;
			this.PositionOffset = token.PositionOffset;
		}
	}
}
