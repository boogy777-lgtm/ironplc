using System;
using System.Runtime.CompilerServices;
using \u0001;
using \u000E;
using \u0014;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u001D
{
	// Token: 0x020002BB RID: 699
	internal sealed class \u0008 : AbstractReplacer, IReplacer
	{
		// Token: 0x1700076D RID: 1901
		// (get) Token: 0x06002AD4 RID: 10964 RVA: 0x00096A28 File Offset: 0x00094C28
		private \u0081.\u0010 Visitor { get; }

		// Token: 0x1700076E RID: 1902
		// (get) Token: 0x06002AD5 RID: 10965 RVA: 0x00096A30 File Offset: 0x00094C30
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002AD6 RID: 10966 RVA: 0x00096A38 File Offset: 0x00094C38
		private \u0008(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
			this.Visitor = \u0081.\u0010.\u0001(this, \u0083\u0005);
		}

		// Token: 0x06002AD7 RID: 10967 RVA: 0x00096A54 File Offset: 0x00094C54
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new \u001D.\u0008(\u0002), new global::\u0001.\u000F(\u0002));
		}

		// Token: 0x06002AD8 RID: 10968 RVA: 0x00096A68 File Offset: 0x00094C68
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x06002AD9 RID: 10969 RVA: 0x00096A78 File Offset: 0x00094C78
		public override _IExpression ReplaceCompoAccessExpression(_ICompoAccessExpression compoAccessExpression, bool bReadAccess)
		{
			if (bReadAccess)
			{
				return this.\u0001(compoAccessExpression);
			}
			return compoAccessExpression;
		}

		// Token: 0x06002ADA RID: 10970 RVA: 0x00096A88 File Offset: 0x00094C88
		private _IExpression \u0001(_ICompoAccessExpression \u0002)
		{
			_IVariable ivariable = \u0002._Right.GetVariable(this.Context._Scope) as _IVariable;
			string text = null;
			IVariable variable = \u0002._Left.GetVariable(this.Context._Scope);
			if (variable != null && variable.HasAttribute(CompileAttributes.GET_BITACCESS) && \u0002._Right.IsLiteral)
			{
				text = variable.GetAttributeValue(CompileAttributes.GET_BITACCESS);
				text = text.Replace("BITNR", \u0002.Right.ToString());
				text = Helper.\u0001(text, \u0002._Left);
			}
			else if (variable != null && variable.HasAttribute(CompileAttributes.DEVICE_PARAMETER) && \u0002._Right.IsLiteral)
			{
				bool flag;
				int @int = \u0002._Right.LiteralUnchecked(this.Context._Scope).GetInt(out flag);
				text = global::\u0014.\u0013.\u0001(variable as _IVariable, @int);
			}
			else if (ivariable != null && ivariable.IsProperty)
			{
				if (ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
				{
					text = global::\u0014.\u0013.\u0001(ivariable, -1);
				}
				else if (ivariable.HasAttribute(CompileAttributes.GET_ACCESS))
				{
					text = ivariable.GetAttributeValue(CompileAttributes.GET_ACCESS);
					text = Helper.\u0001(text, \u0002);
				}
				else
				{
					text = \u0002._Left.ToString() + ".__get" + ivariable.OrgName + "()";
				}
			}
			if (text != null)
			{
				_IExpression iexpression = this.Context.Generator.GenerateExpression(text, this.Context._Scope, this.Context.CompiledPOU);
				this.Context.Generator.CopyPositionAndMessages(\u0002, iexpression);
				return iexpression;
			}
			return \u0002;
		}

		// Token: 0x04000819 RID: 2073
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x0400081A RID: 2074
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
