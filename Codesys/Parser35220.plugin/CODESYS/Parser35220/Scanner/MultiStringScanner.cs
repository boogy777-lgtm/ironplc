using System;
using System.Collections.Generic;
using CODESYS.Parser;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000010 RID: 16
	public class MultiStringScanner : InternalScanner, IMultiStringScanner, _IScanner5, _IScanner4, _IScanner3, _IScanner2, _IScanner, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner, IScanner7, IScanner8, IScanner9
	{
		// Token: 0x0600019D RID: 413 RVA: 0x000088C4 File Offset: 0x00006AC4
		public MultiStringScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService sos) : base(typeTable, overflowChecker, sos)
		{
		}

		// Token: 0x0600019E RID: 414 RVA: 0x000088CF File Offset: 0x00006ACF
		public void InitializeMulti(IList<string> strings)
		{
			this._strings = strings;
			this._nCurrentIndex = 0;
			base.Initialize(this._strings[this._nCurrentIndex]);
		}

		// Token: 0x0600019F RID: 415 RVA: 0x000088F8 File Offset: 0x00006AF8
		public override void SetPosition(IToken token)
		{
			MultiStringScanner.MultiStringToken multiStringToken = token as MultiStringScanner.MultiStringToken;
			if (multiStringToken != null && this._nCurrentIndex != multiStringToken.StringIndex)
			{
				this._nCurrentIndex = multiStringToken.StringIndex;
				base.Initialize(this._strings[this._nCurrentIndex]);
			}
			base.SetPosition(token);
		}

		// Token: 0x060001A0 RID: 416 RVA: 0x00008948 File Offset: 0x00006B48
		public override TokenType GetNext(out IToken token)
		{
			TokenType next = base.GetNext(out token);
			if (next == 21 && this._nCurrentIndex < this._strings.Count - 1)
			{
				this._nCurrentIndex++;
				base.Initialize(this._strings[this._nCurrentIndex]);
				next = this.GetNext(out token);
				if (token is Token)
				{
					MultiStringScanner.MultiStringToken multiStringToken = new MultiStringScanner.MultiStringToken(token);
					token = multiStringToken;
				}
				((MultiStringScanner.MultiStringToken)token).StringIndex = this._nCurrentIndex;
			}
			return next;
		}

		// Token: 0x04000031 RID: 49
		private IList<string> _strings;

		// Token: 0x04000032 RID: 50
		private int _nCurrentIndex;

		// Token: 0x02000062 RID: 98
		private class MultiStringToken : _IToken, IToken
		{
			// Token: 0x0600060B RID: 1547 RVA: 0x00019A4B File Offset: 0x00017C4B
			internal MultiStringToken(IToken token)
			{
				this._token = (Token)token;
			}

			// Token: 0x1700017A RID: 378
			// (get) Token: 0x0600060C RID: 1548 RVA: 0x00019A5F File Offset: 0x00017C5F
			// (set) Token: 0x0600060D RID: 1549 RVA: 0x00019A67 File Offset: 0x00017C67
			internal int StringIndex { get; set; }

			// Token: 0x1700017B RID: 379
			// (get) Token: 0x0600060E RID: 1550 RVA: 0x00019A70 File Offset: 0x00017C70
			public TokenType Type
			{
				get
				{
					return this._token.Type;
				}
			}

			// Token: 0x1700017C RID: 380
			// (get) Token: 0x0600060F RID: 1551 RVA: 0x00019A7D File Offset: 0x00017C7D
			public int SourceOffset
			{
				get
				{
					return this._token.SourceOffset;
				}
			}

			// Token: 0x1700017D RID: 381
			// (get) Token: 0x06000610 RID: 1552 RVA: 0x00019A8A File Offset: 0x00017C8A
			public long Position
			{
				get
				{
					return this._token.Position;
				}
			}

			// Token: 0x1700017E RID: 382
			// (get) Token: 0x06000611 RID: 1553 RVA: 0x00019A97 File Offset: 0x00017C97
			public short PositionOffset
			{
				get
				{
					return this._token.PositionOffset;
				}
			}

			// Token: 0x1700017F RID: 383
			// (get) Token: 0x06000612 RID: 1554 RVA: 0x00019AA4 File Offset: 0x00017CA4
			public int Length
			{
				get
				{
					return this._token.Length;
				}
			}

			// Token: 0x17000180 RID: 384
			// (get) Token: 0x06000613 RID: 1555 RVA: 0x00019AB1 File Offset: 0x00017CB1
			public int SourceLine
			{
				get
				{
					return this._token.SourceLine;
				}
			}

			// Token: 0x17000181 RID: 385
			// (get) Token: 0x06000614 RID: 1556 RVA: 0x00019ABE File Offset: 0x00017CBE
			public int SourceColumn
			{
				get
				{
					return this._token.SourceColumn;
				}
			}

			// Token: 0x17000182 RID: 386
			// (get) Token: 0x06000615 RID: 1557 RVA: 0x00019ACB File Offset: 0x00017CCB
			// (set) Token: 0x06000616 RID: 1558 RVA: 0x00019AD8 File Offset: 0x00017CD8
			public long CharactersToSkipSeen
			{
				get
				{
					return this._token.CharactersToSkipSeen;
				}
				set
				{
					this._token.CharactersToSkipSeen = value;
				}
			}

			// Token: 0x040000E8 RID: 232
			private Token _token;
		}
	}
}
