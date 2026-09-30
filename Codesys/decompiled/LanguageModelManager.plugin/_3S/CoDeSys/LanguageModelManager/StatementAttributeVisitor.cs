using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200011A RID: 282
	internal class StatementAttributeVisitor : IStatementVisitorNoTraversion
	{
		// Token: 0x06001709 RID: 5897 RVA: 0x0003F6E7 File Offset: 0x0003E6E7
		public StatementAttributeVisitor()
		{
			this._stillInHeader = true;
			this._attributes = new Dictionary<string, string>();
		}

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x0600170A RID: 5898 RVA: 0x0003F701 File Offset: 0x0003E701
		public IEnumerable<KeyValuePair<string, string>> Attributes
		{
			get
			{
				foreach (KeyValuePair<string, string> keyValuePair in this._attributes)
				{
					yield return new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value);
				}
				Dictionary<string, string>.Enumerator enumerator = default(Dictionary<string, string>.Enumerator);
				yield break;
				yield break;
			}
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IRepeatStatement repeat)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600170C RID: 5900 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IExitStatement exit)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ISequenceStatement seq)
		{
		}

		// Token: 0x0600170E RID: 5902 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IReturnStatement returnst)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_ILabelStatement label)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x0003F71C File Offset: 0x0003E71C
		public void visit(_IPragmaStatement pragma)
		{
			string key;
			string value;
			if (this._stillInHeader && CompilerProxy.ParseAttributePragma(pragma, out key, out value))
			{
				this._attributes[key] = value;
			}
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IEmptyStatement empty)
		{
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_ICaseStatement casest)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_INullStatement errorst)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IBreakPointStatement bpstate)
		{
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPragmaAssertion assertion)
		{
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_ITryCatchStatement trycatch)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IDefineStatement defstate)
		{
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IPragmaIfStatement pifst)
		{
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_IErrorStatement errorst)
		{
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_ICaseLabelStatement caselabel)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IExpressionStatement expstat)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600171C RID: 5916 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICommentStatement comment)
		{
		}

		// Token: 0x0600171D RID: 5917 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IJumpStatement gotost)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600171E RID: 5918 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IIfStatement ifst)
		{
			this._stillInHeader = false;
		}

		// Token: 0x0600171F RID: 5919 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IContinueStatement cont)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IForStatement forloop)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001721 RID: 5921 RVA: 0x0003F711 File Offset: 0x0003E711
		public void visit(_IWhileStatement whilst)
		{
			this._stillInHeader = false;
		}

		// Token: 0x06001722 RID: 5922 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void visit(_ICompiledPOU cpou)
		{
		}

		// Token: 0x040004DE RID: 1246
		private bool _stillInHeader;

		// Token: 0x040004DF RID: 1247
		private readonly Dictionary<string, string> _attributes;
	}
}
