using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0012;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0007
{
	// Token: 0x020002C5 RID: 709
	internal sealed class \u0011 : AbstractStatementReplacer, IReplacer
	{
		// Token: 0x17000779 RID: 1913
		// (get) Token: 0x06002B19 RID: 11033 RVA: 0x00097F90 File Offset: 0x00096190
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x1700077A RID: 1914
		// (get) Token: 0x06002B1A RID: 11034 RVA: 0x00097F98 File Offset: 0x00096198
		private StatementReplacerVisitor Visitor { get; }

		// Token: 0x1700077B RID: 1915
		// (get) Token: 0x06002B1B RID: 11035 RVA: 0x00097FA0 File Offset: 0x000961A0
		// (set) Token: 0x06002B1C RID: 11036 RVA: 0x00097FA8 File Offset: 0x000961A8
		private _ICompiledPOU CompiledPou { get; set; }

		// Token: 0x06002B1D RID: 11037 RVA: 0x00097FB4 File Offset: 0x000961B4
		internal \u0011(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Context = \u0083\u0005;
			this.Visitor = new StatementReplacerVisitor(this, \u0083\u0005);
		}

		// Token: 0x06002B1E RID: 11038 RVA: 0x00097FD0 File Offset: 0x000961D0
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new global::\u0007.\u0011(\u0002), new global::\u0012.\u0012());
		}

		// Token: 0x06002B1F RID: 11039 RVA: 0x00097FE4 File Offset: 0x000961E4
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.CompiledPou = cpou;
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x06002B20 RID: 11040 RVA: 0x00097FFC File Offset: 0x000961FC
		public override _IStatement visit(_IReturnStatement returnStatement)
		{
			_IJumpStatement ijumpStatement = \u0019.\u0003.\u0001(global::\u0016.\u0010.\u0001(this.CompiledPou), Token.Empty);
			ijumpStatement._Condition = returnStatement._Condition;
			_IJumpStatement ijumpStatement2 = this.Context.Generator.\u0001<_IJumpStatement>(ijumpStatement, this.Context._Scope, this.Context.CompiledPOU);
			ijumpStatement2.SetFlag(StatementFlag.GenerateBP, returnStatement.GetFlag(StatementFlag.GenerateBP));
			ijumpStatement2._Position = returnStatement._Position;
			((_IJumpStatement)ijumpStatement2).LengthIntern = returnStatement.LengthIntern;
			return ijumpStatement2;
		}

		// Token: 0x0400082C RID: 2092
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x0400082D RID: 2093
		[CompilerGenerated]
		private readonly StatementReplacerVisitor \u0001;

		// Token: 0x0400082E RID: 2094
		[CompilerGenerated]
		private _ICompiledPOU \u0001;
	}
}
