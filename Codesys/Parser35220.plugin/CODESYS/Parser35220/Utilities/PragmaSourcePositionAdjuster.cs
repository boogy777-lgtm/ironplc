using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x0200001E RID: 30
	internal class PragmaSourcePositionAdjuster : EmptyVisitor
	{
		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000228 RID: 552 RVA: 0x0000C692 File Offset: 0x0000A892
		// (set) Token: 0x06000229 RID: 553 RVA: 0x0000C69A File Offset: 0x0000A89A
		private ITokenFactory TokenFactory { get; set; }

		// Token: 0x0600022A RID: 554 RVA: 0x0000C6A3 File Offset: 0x0000A8A3
		internal PragmaSourcePositionAdjuster(long lPosition, int nPositionOffset, ITokenFactory tokenFactory)
		{
			this._lPosition = lPosition;
			this._nPositionOffset = nPositionOffset;
			this.TokenFactory = tokenFactory;
		}

		// Token: 0x0600022B RID: 555 RVA: 0x0000C6C0 File Offset: 0x0000A8C0
		internal void AdjustSourcePositions(_IExpression exp)
		{
			StandardTraverser standardTraverser = new StandardTraverser(this);
			exp.Accept(standardTraverser);
		}

		// Token: 0x0600022C RID: 556 RVA: 0x0000C6DB File Offset: 0x0000A8DB
		public override void visit(_IVariableExpression variable)
		{
			this.AdjustPosition(variable);
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000C6E4 File Offset: 0x0000A8E4
		private void AdjustPosition(_IExpression expr)
		{
			ISourcePosition position = expr.Position;
			if (position == null)
			{
				return;
			}
			_IToken position2 = this.TokenFactory.CreateToken(this._lPosition, position.PositionOffset + (short)this._nPositionOffset + 1, (int)position.Length);
			expr.SetPosition(position2);
		}

		// Token: 0x0400007D RID: 125
		private readonly long _lPosition;

		// Token: 0x0400007E RID: 126
		private readonly int _nPositionOffset;
	}
}
