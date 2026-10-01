using System;
using System.Runtime.CompilerServices;
using \u0011;
using \u0013;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0003
{
	// Token: 0x020003B2 RID: 946
	internal sealed class \u0017 : EmptyVisitor351900
	{
		// Token: 0x170008DD RID: 2269
		// (get) Token: 0x06003680 RID: 13952 RVA: 0x000DCB40 File Offset: 0x000DAD40
		// (set) Token: 0x06003681 RID: 13953 RVA: 0x000DCB48 File Offset: 0x000DAD48
		public bool IsInsideCompo { get; set; }

		// Token: 0x170008DE RID: 2270
		// (get) Token: 0x06003682 RID: 13954 RVA: 0x000DCB54 File Offset: 0x000DAD54
		// (set) Token: 0x06003683 RID: 13955 RVA: 0x000DCB5C File Offset: 0x000DAD5C
		public bool IsInsideDeref { get; set; }

		// Token: 0x06003684 RID: 13956 RVA: 0x000DCB68 File Offset: 0x000DAD68
		internal \u0017(_ICompileContext \u0001\u0002, _ISignature \u001C\u0002)
		{
			this.\u0001 = \u001C\u0002;
			this.\u0001 = \u0001\u0002;
			this.\u0001 = new \u0013.\u0010(this);
		}

		// Token: 0x06003685 RID: 13957 RVA: 0x000DCB8C File Offset: 0x000DAD8C
		internal string \u0001(string \u0002, IExpression \u0003)
		{
			this.\u0001.\u0001();
			this.IsInsideDeref = false;
			this.IsInsideCompo = false;
			this.\u0001 = \u0002;
			\u0003 = this.\u0001.\u0001((_IExpression)\u0003);
			this.\u0001.\u0002((_IExpression)\u0003);
			return \u0003.ToString();
		}

		// Token: 0x06003686 RID: 13958 RVA: 0x000DCBE4 File Offset: 0x000DADE4
		private bool \u0001(IVariableExpression \u0002)
		{
			return this.\u0001(\u0002.Name, this.\u0001);
		}

		// Token: 0x06003687 RID: 13959 RVA: 0x000DCBF8 File Offset: 0x000DADF8
		private bool \u0001(string \u0002, _ISignature \u0003)
		{
			if (\u0003[\u0002] != null)
			{
				return true;
			}
			if (Helper.InvalidId == \u0003.BaseSignatureId)
			{
				return false;
			}
			_ISignature isignature = this.\u0001.GetSignatureById(\u0003.BaseSignatureId) as _ISignature;
			return isignature != null && this.\u0001(\u0002, isignature);
		}

		// Token: 0x06003688 RID: 13960 RVA: 0x000DCC44 File Offset: 0x000DAE44
		private static _IExpression \u0001(string \u0002)
		{
			return (_IExpression)new \u0011.\u0006(\u0002, true).\u0002();
		}

		// Token: 0x06003689 RID: 13961 RVA: 0x000DCC58 File Offset: 0x000DAE58
		public override void visit(_IThisExpression thisexp)
		{
			if (!this.IsInsideDeref)
			{
				_IExpression u = \u0017.\u0001("ADR(" + this.\u0001 + ")");
				this.\u0001.\u0001(u);
				this.\u0001.\u0003();
			}
		}

		// Token: 0x0600368A RID: 13962 RVA: 0x000DCCA0 File Offset: 0x000DAEA0
		public override void visit(_IDeRefAccessExpression deref)
		{
			if (deref._Base is _IThisExpression)
			{
				_IExpression u = \u0017.\u0001(this.\u0001);
				this.\u0001.\u0001(u);
				this.\u0001.\u0003();
			}
		}

		// Token: 0x0600368B RID: 13963 RVA: 0x000DCCE0 File Offset: 0x000DAEE0
		public override void visit(_IVariableExpression varExp)
		{
			if (this.IsInsideCompo)
			{
				return;
			}
			if (this.\u0001(varExp))
			{
				_IExpression u = \u0017.\u0001(string.Format("{0}.{1}", this.\u0001, varExp));
				this.\u0001.\u0001(u);
				this.\u0001.\u0003();
			}
		}

		// Token: 0x0600368C RID: 13964 RVA: 0x000DCD30 File Offset: 0x000DAF30
		public override void visit(_ICompoAccessExpression compoExp)
		{
			IVariableExpression variableExpression = compoExp.Left as IVariableExpression;
			if (variableExpression != null && this.\u0001(variableExpression))
			{
				_IExpression u = \u0017.\u0001(string.Format("{0}.{1}", this.\u0001, compoExp));
				this.\u0001.\u0001(u);
				this.\u0001.\u0003();
			}
		}

		// Token: 0x04000A9E RID: 2718
		private readonly _ISignature \u0001;

		// Token: 0x04000A9F RID: 2719
		private readonly _ICompileContext \u0001;

		// Token: 0x04000AA0 RID: 2720
		private readonly \u0013.\u0010 \u0001;

		// Token: 0x04000AA1 RID: 2721
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000AA2 RID: 2722
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000AA3 RID: 2723
		private string \u0001;
	}
}
