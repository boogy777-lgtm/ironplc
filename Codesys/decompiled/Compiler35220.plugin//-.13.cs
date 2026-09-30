using System;
using System.Runtime.CompilerServices;
using \u0001;
using \u000E;
using \u0014;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x020002BD RID: 701
	internal sealed class \u0012 : AbstractReplacer, IReplacer
	{
		// Token: 0x17000770 RID: 1904
		// (get) Token: 0x06002AE0 RID: 10976 RVA: 0x00096CD0 File Offset: 0x00094ED0
		private \u0081.\u0010 Visitor { get; }

		// Token: 0x17000771 RID: 1905
		// (get) Token: 0x06002AE1 RID: 10977 RVA: 0x00096CD8 File Offset: 0x00094ED8
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x06002AE2 RID: 10978 RVA: 0x00096CE0 File Offset: 0x00094EE0
		private \u0012(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
			this.Visitor = \u0081.\u0010.\u0001(this, \u0083\u0005);
		}

		// Token: 0x06002AE3 RID: 10979 RVA: 0x00096CFC File Offset: 0x00094EFC
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new \u0081.\u0012(\u0002), new global::\u0001.\u000F(\u0002));
		}

		// Token: 0x06002AE4 RID: 10980 RVA: 0x00096D10 File Offset: 0x00094F10
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x00096D20 File Offset: 0x00094F20
		public override _IExpression ReplaceVariableExpression(_IVariableExpression variableExpression, bool bReadAccess)
		{
			if (bReadAccess)
			{
				return this.\u0001(variableExpression);
			}
			return variableExpression;
		}

		// Token: 0x06002AE6 RID: 10982 RVA: 0x00096D30 File Offset: 0x00094F30
		private static _IVariable \u0001(_IVariableExpression \u0002, IScope5 \u0003)
		{
			_IVariable ivariable = \u0002.GetVariable(\u0003) as _IVariable;
			if (ivariable != null)
			{
				return ivariable;
			}
			return null;
		}

		// Token: 0x06002AE7 RID: 10983 RVA: 0x00096D50 File Offset: 0x00094F50
		private _IExpression \u0001(_IVariableExpression \u0002)
		{
			ISignature signature = this.Context._Scope[\u0002.SignatureId];
			_IVariable ivariable = \u0081.\u0012.\u0001(\u0002, this.Context._Scope);
			if (ivariable != null && ivariable.IsProperty && signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.VarGlobal))
			{
				string text;
				if (ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER))
				{
					text = global::\u0014.\u0013.\u0001(ivariable, -1);
				}
				else if (ivariable.HasAttribute(CompileAttributes.GET_ACCESS))
				{
					text = ivariable.GetAttributeValue(CompileAttributes.GET_ACCESS);
					text = Helper.\u0001(text, \u0002);
				}
				else if (signature.POUType == Operator.VarGlobal)
				{
					text = signature.Name + ".__get" + ivariable.OrgName + "()";
				}
				else if (signature.POUType == Operator.Program)
				{
					text = "__get" + ivariable.OrgName + "()";
				}
				else
				{
					text = signature.Name + ".__get" + ivariable.OrgName + "()";
				}
				_IExpression iexpression = this.Context.Generator.GenerateExpression(text, this.Context._Scope, this.Context.CompiledPOU);
				this.Context.Generator.CopyPositionAndMessages(\u0002, iexpression);
				return iexpression;
			}
			return \u0002;
		}

		// Token: 0x0400081C RID: 2076
		[CompilerGenerated]
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x0400081D RID: 2077
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;
	}
}
