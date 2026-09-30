using System;
using System.Runtime.CompilerServices;
using \u0003;
using \u0004;
using \u0006;
using \u0007;
using \u000E;
using \u0010;
using \u0013;
using \u0014;
using \u0018;
using \u001A;
using \u001B;
using \u001C;
using \u001D;
using \u001E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0081;
using \u0084;

namespace \u0008
{
	// Token: 0x0200023F RID: 575
	internal sealed class \u000E
	{
		// Token: 0x060025B9 RID: 9657 RVA: 0x00082B30 File Offset: 0x00080D30
		public \u000E(IScope5 \u009B\u0002, _IDataManager \u0006\u0004, _ICompileContext \u0001\u0002, ICodegenerator \u0007\u0004, Codegeneration \u000E\u0002) : this(\u009B\u0002, null, \u0006\u0004, \u0001\u0002, \u0007\u0004, \u000E\u0002)
		{
		}

		// Token: 0x060025BA RID: 9658 RVA: 0x00082B40 File Offset: 0x00080D40
		public \u000E(IScope5 \u009B\u0002, IScope5 \u0015\u0004, _IDataManager \u0006\u0004, _ICompileContext \u0001\u0002, ICodegenerator \u0007\u0004, Codegeneration \u000E\u0002)
		{
			this.Scope = \u009B\u0002;
			this.ScopeRef = \u0015\u0004;
			this.\u0001(\u009B\u0002, \u0001\u0002, \u0007\u0004, \u000E\u0002, \u0006\u0004);
		}

		// Token: 0x060025BB RID: 9659 RVA: 0x00082B64 File Offset: 0x00080D64
		private static ReplacerController \u0001(IReplacer \u0002)
		{
			return new ReplacerController(\u0002, null);
		}

		// Token: 0x060025BC RID: 9660 RVA: 0x00082B70 File Offset: 0x00080D70
		private void \u0001(IScope5 \u0002, _ICompileContext \u0003, ICodegenerator \u0004, Codegeneration \u0005, _IDataManager \u0006)
		{
			LateCodeGenerator u0019_u = LateCodeGenerator.CreateNormalGenerator(\u0003);
			LateCodeGenerator u0019_u2 = LateCodeGenerator.CreateGeneratorAfterInterfaceReplacement(\u0003);
			LateCodeGenerator u0019_u3 = LateCodeGenerator.CreateGeneratorAfterInterfaceAndReferenceReplacement(\u0003);
			global::\u000E.\u0011 u = new global::\u000E.\u0011(\u0002, u0019_u, \u0003, \u0005, \u0004);
			global::\u000E.\u0011 u2 = new global::\u000E.\u0011(\u0002, u0019_u2, \u0003, \u0005, \u0004);
			global::\u000E.\u0011 u3 = new global::\u000E.\u0011(\u0002, u0019_u3, \u0003, \u0005, \u0004);
			InstanceVarReplacer instanceVarReplacer = new InstanceVarReplacer(u);
			OperatorReplacer operatorReplacer = new OperatorReplacer(u);
			IExprementReplacer[] u0011_u = new IExprementReplacer[]
			{
				operatorReplacer,
				instanceVarReplacer
			};
			this.\u0001 = new ReplacerController[]
			{
				global::\u0008.\u000E.\u0001(new PropertyReturnValueInserter(u)),
				global::\u0007.\u000F.\u0001(u),
				\u001E.\u0012.\u0001(u),
				global::\u001A.\u0007.\u0001(u, \u0006),
				global::\u0008.\u000E.\u0001(new OptionalInputsProvider(u)),
				global::\u0008.\u000E.\u0001(new CheckFunctionReplacer(u)),
				TaskLocalAccessReplacer.\u0001(u),
				AssignmentStatementReplacer.\u0001(u, global::\u0008.\u000E.\u0001, global::\u0008.\u000E.\u0001),
				SpecialOperationsReplacer.\u0001(u),
				global::\u0008.\u000E.\u0001(operatorReplacer),
				\u001C.\u0010.\u0001(u),
				StructAndArrayInitReplacer.\u0001(u),
				CallReplacer.\u0001(u, global::\u0008.\u000E.\u0001, global::\u0008.\u000E.\u0001),
				global::\u0008.\u000E.\u0001(instanceVarReplacer),
				CallReplacer.\u0001(u, global::\u0008.\u000E.\u0002, global::\u0008.\u000E.\u0002),
				AssignmentStatementReplacer.\u0001(u, global::\u0008.\u000E.\u0002, global::\u0008.\u000E.\u0002),
				AssignmentStatementReplacer.\u0001(u, global::\u0008.\u000E.\u0003, global::\u0008.\u000E.\u0003),
				\u001D.\u0008.\u0001(u),
				\u0081.\u0012.\u0001(u),
				global::\u0008.\u000E.\u0001(new ThisAndBaseReplacer(u)),
				\u0081.\u0013.\u0001(u),
				global::\u0007.\u0011.\u0001(u),
				global::\u0008.\u000E.\u0001(new ConversionReplacer(u)),
				InterfaceComparisonReplacer.\u0001(u2),
				global::\u0008.\u000E.\u0001(\u0081.\u0010.\u0001(new global::\u0018.\u0006(u2), u2)),
				global::\u0008.\u000E.\u0001(new ReferenceReplacer(u3)),
				global::\u0008.\u000E.\u0001(\u0081.\u0010.\u0001(new global::\u0014.\u0010(u3), u3)),
				global::\u0008.\u000E.\u0001(new LateOperationReplacer(u3)),
				global::\u0008.\u000E.\u0001(new CheckPointerReplacer(\u0002, \u0003, \u0005.\u0001, u3))
			};
			this.\u0001 = new global::\u0004.\u000E(\u0002, \u0006, u0011_u, u3);
		}

		// Token: 0x060025BD RID: 9661 RVA: 0x00082D98 File Offset: 0x00080F98
		private void \u0001(_ICompiledPOU \u0002)
		{
			ReplacementCheckerVisitor.CheckForReplacements(this.\u0001, \u0002);
			ReplacerController[] u = this.\u0001;
			for (int i = 0; i < u.Length; i++)
			{
				u[i].ReplaceCode(\u0002);
			}
		}

		// Token: 0x060025BE RID: 9662 RVA: 0x00082DD0 File Offset: 0x00080FD0
		public void \u0001(_ICompiledPOU \u0002, _ISignature \u0003)
		{
			ISignature localSignature = this.Scope.LocalSignature;
			ISignature methodSignature = this.Scope.MethodSignature;
			ISignature signature = null;
			this.Scope.MethodSignature = \u0003;
			if (this.ScopeRef != null)
			{
				signature = this.ScopeRef.MethodSignature;
				this.ScopeRef.MethodSignature = \u0003;
			}
			this.\u0001(\u0002);
			this.\u0001.\u0002(\u0002);
			this.Scope.LocalSignature = localSignature;
			this.Scope.MethodSignature = (methodSignature as _ISignature);
			this.Scope.InitFriend();
			if (this.ScopeRef != null)
			{
				this.ScopeRef.MethodSignature = (signature as _ISignature);
			}
		}

		// Token: 0x060025BF RID: 9663 RVA: 0x00082E78 File Offset: 0x00081078
		public void \u0002(_ICompiledPOU \u0002)
		{
			this.Scope.InitFriend();
			this.\u0001(\u0002);
			this.\u0001.\u0002(\u0002);
			this.Scope.InitFriend();
		}

		// Token: 0x170006F5 RID: 1781
		// (get) Token: 0x060025C0 RID: 9664 RVA: 0x00082EA4 File Offset: 0x000810A4
		private IScope5 Scope { get; }

		// Token: 0x170006F6 RID: 1782
		// (get) Token: 0x060025C1 RID: 9665 RVA: 0x00082EAC File Offset: 0x000810AC
		private IScope5 ScopeRef { get; }

		// Token: 0x040006D4 RID: 1748
		private global::\u0004.\u000E \u0001;

		// Token: 0x040006D5 RID: 1749
		private ReplacerController[] \u0001;

		// Token: 0x040006D6 RID: 1750
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] \u0001 = new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(global::\u0003.\u0010.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(global::\u001B.\u0007.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(BitOffsetHandler.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(\u0080.\u0012.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(VarInfoHandler.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(global::\u0010.\u0005.\u0001)
		};

		// Token: 0x040006D7 RID: 1751
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[] \u0001 = new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(global::\u0003.\u0010.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(global::\u001B.\u0007.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(BitOffsetHandler.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(\u0080.\u0012.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(VarInfoHandler.\u0001),
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(global::\u0010.\u0005.\u0001)
		};

		// Token: 0x040006D8 RID: 1752
		private static readonly Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] \u0001 = new Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[]
		{
			new Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>(\u0081.\u0014.\u0001),
			new Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>(\u0084.\u0019.\u0001)
		};

		// Token: 0x040006D9 RID: 1753
		private static readonly Func<_ISignature, _ICallExpression, bool>[] \u0001 = new Func<_ISignature, _ICallExpression, bool>[]
		{
			new Func<_ISignature, _ICallExpression, bool>(\u0081.\u0014.\u0001),
			new Func<_ISignature, _ICallExpression, bool>(\u0084.\u0019.\u0001)
		};

		// Token: 0x040006DA RID: 1754
		private static readonly Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[] \u0002 = new Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>[]
		{
			new Func<_ISignature, _ICompiledPOU, _ICallExpression, global::\u000E.\u0011, _IExpression>(global::\u000E.\u0013.\u0001)
		};

		// Token: 0x040006DB RID: 1755
		private static readonly Func<_ISignature, _ICallExpression, bool>[] \u0002 = new Func<_ISignature, _ICallExpression, bool>[]
		{
			new Func<_ISignature, _ICallExpression, bool>(global::\u000E.\u0013.\u0001)
		};

		// Token: 0x040006DC RID: 1756
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] \u0002 = new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(global::\u0013.\u0007.\u0001)
		};

		// Token: 0x040006DD RID: 1757
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[] \u0002 = new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(global::\u0013.\u0007.\u0001)
		};

		// Token: 0x040006DE RID: 1758
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[] \u0003 = new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, _IStatement>(global::\u0006.\u0006.\u0001)
		};

		// Token: 0x040006DF RID: 1759
		private static readonly Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[] \u0003 = new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>[]
		{
			new Func<_IAssignmentExpression, global::\u000E.\u0011, bool>(global::\u0006.\u0006.\u0001)
		};

		// Token: 0x040006E0 RID: 1760
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040006E1 RID: 1761
		[CompilerGenerated]
		private readonly IScope5 \u0002;
	}
}
