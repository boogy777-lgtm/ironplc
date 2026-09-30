using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0084
{
	// Token: 0x0200017F RID: 383
	internal sealed class \u0008 : IStatementVisitorNoTraversion
	{
		// Token: 0x06001A19 RID: 6681 RVA: 0x00053810 File Offset: 0x00051A10
		internal \u0008()
		{
		}

		// Token: 0x1700059D RID: 1437
		// (get) Token: 0x06001A1A RID: 6682 RVA: 0x00053828 File Offset: 0x00051A28
		// (set) Token: 0x06001A1B RID: 6683 RVA: 0x00053830 File Offset: 0x00051A30
		public IStatementTraverser Traverser { get; set; }

		// Token: 0x06001A1C RID: 6684 RVA: 0x0005383C File Offset: 0x00051A3C
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x06001A1D RID: 6685 RVA: 0x00053840 File Offset: 0x00051A40
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A1E RID: 6686 RVA: 0x00053860 File Offset: 0x00051A60
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A1F RID: 6687 RVA: 0x00053880 File Offset: 0x00051A80
		public void \u0001(_IForStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A20 RID: 6688 RVA: 0x000538A0 File Offset: 0x00051AA0
		public void \u0001(_IExitStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A21 RID: 6689 RVA: 0x000538C0 File Offset: 0x00051AC0
		public void \u0001(_IContinueStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A22 RID: 6690 RVA: 0x000538E0 File Offset: 0x00051AE0
		public void \u0001(_ISequenceStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A23 RID: 6691 RVA: 0x00053900 File Offset: 0x00051B00
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
		}

		// Token: 0x06001A24 RID: 6692 RVA: 0x00053920 File Offset: 0x00051B20
		public void \u0001(_IReturnStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A25 RID: 6693 RVA: 0x00053940 File Offset: 0x00051B40
		public void \u0001(_IJumpStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A26 RID: 6694 RVA: 0x00053960 File Offset: 0x00051B60
		public void \u0001(_ILabelStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A27 RID: 6695 RVA: 0x00053980 File Offset: 0x00051B80
		public void \u0001(_ICommentStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A28 RID: 6696 RVA: 0x000539A0 File Offset: 0x00051BA0
		public void \u0001(_IPragmaStatement \u0002)
		{
			string text = \u0002.Text;
			_IScanner5 iscanner = Scanner.\u0001();
			iscanner.Initialize(text);
			iscanner.AllowMultipleUnderlines = true;
			IPragmaToken pragmaToken;
			if (\u0005.Singleton.Create(iscanner).GetNext(out pragmaToken) == PragmaTokenType.Operator)
			{
				switch (pragmaToken.Operator)
				{
				case PragmaOperator.flow:
					this.\u0002 = true;
					break;
				case PragmaOperator.noflow:
					this.\u0002 = false;
					break;
				case PragmaOperator.bp:
					this.\u0001 = true;
					break;
				case PragmaOperator.nobp:
					this.\u0001 = false;
					break;
				}
			}
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A29 RID: 6697 RVA: 0x00053A3C File Offset: 0x00051C3C
		public void \u0001(_IExpressionStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2A RID: 6698 RVA: 0x00053A5C File Offset: 0x00051C5C
		public void \u0001(_IEmptyStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2B RID: 6699 RVA: 0x00053A7C File Offset: 0x00051C7C
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2C RID: 6700 RVA: 0x00053A9C File Offset: 0x00051C9C
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2D RID: 6701 RVA: 0x00053ABC File Offset: 0x00051CBC
		public void \u0001(_IErrorStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2E RID: 6702 RVA: 0x00053ADC File Offset: 0x00051CDC
		public void \u0001(_INullStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A2F RID: 6703 RVA: 0x00053AFC File Offset: 0x00051CFC
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A30 RID: 6704 RVA: 0x00053B1C File Offset: 0x00051D1C
		public void \u0001(_IBreakPointStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A31 RID: 6705 RVA: 0x00053B3C File Offset: 0x00051D3C
		public void \u0001(_IDefineStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A32 RID: 6706 RVA: 0x00053B5C File Offset: 0x00051D5C
		public void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x06001A33 RID: 6707 RVA: 0x00053B7C File Offset: 0x00051D7C
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.SetFlag(StatementFlag.GenerateFlow, this.\u0002);
			\u0002.SetFlag(StatementFlag.GenerateBP, this.\u0001);
		}

		// Token: 0x04000485 RID: 1157
		private bool \u0001 = true;

		// Token: 0x04000486 RID: 1158
		private bool \u0002 = true;

		// Token: 0x04000487 RID: 1159
		[CompilerGenerated]
		private IStatementTraverser \u0001;
	}
}
