using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000053 RID: 83
	[TypeGuid("{8de3e20a-33a6-4def-9755-b792364108bf}")]
	[StorageVersion("3.5.20.0")]
	public class ProjectDefinedExpression : PragmaExpression, _IProjectDefinedExpression, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x060004E5 RID: 1253 RVA: 0x0000C6EA File Offset: 0x0000B6EA
		public ProjectDefinedExpression()
		{
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0000C6F2 File Offset: 0x0000B6F2
		public ProjectDefinedExpression(IToken token) : base(token)
		{
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060004E7 RID: 1255 RVA: 0x0000E98B File Offset: 0x0000D98B
		// (set) Token: 0x060004E8 RID: 1256 RVA: 0x0000E993 File Offset: 0x0000D993
		[DefaultSerialization("Reference")]
		[StorageVersion("3.5.20.0")]
		[Obfuscation(Feature = "rename")]
		public _IDefineReference DefineReference { get; set; }

		// Token: 0x060004E9 RID: 1257 RVA: 0x0000E99C File Offset: 0x0000D99C
		public override void Accept(IExprementVisitor visitor)
		{
			IExprementVisitor352000 exprementVisitor = visitor as IExprementVisitor352000;
			if (exprementVisitor != null)
			{
				exprementVisitor.visit(this);
			}
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x0000E9BA File Offset: 0x0000D9BA
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x0000E9C4 File Offset: 0x0000D9C4
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			IExprVisitor9 exprVisitor = visitor as IExprVisitor9;
			if (exprVisitor != null)
			{
				exprVisitor.visit(this);
			}
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x060004ED RID: 1261 RVA: 0x0000E9E4 File Offset: 0x0000D9E4
		public override _IExprement Duplicate()
		{
			ProjectDefinedExpression projectDefinedExpression = new ProjectDefinedExpression();
			if (this.DefineReference != null)
			{
				projectDefinedExpression.DefineReference = (this.DefineReference.Duplicate() as _IDefineReference);
			}
			this.DuplicateCommon(projectDefinedExpression);
			return projectDefinedExpression;
		}
	}
}
