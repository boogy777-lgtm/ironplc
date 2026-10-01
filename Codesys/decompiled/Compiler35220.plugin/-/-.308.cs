using System;
using System.Runtime.CompilerServices;
using \u0014;
using \u001A;
using \u001F;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000E
{
	// Token: 0x02000343 RID: 835
	internal sealed class \u0017
	{
		// Token: 0x17000832 RID: 2098
		// (get) Token: 0x0600327D RID: 12925 RVA: 0x000C2718 File Offset: 0x000C0918
		// (set) Token: 0x0600327E RID: 12926 RVA: 0x000C2720 File Offset: 0x000C0920
		internal IScope Scope { get; private set; }

		// Token: 0x17000833 RID: 2099
		// (get) Token: 0x0600327F RID: 12927 RVA: 0x000C272C File Offset: 0x000C092C
		// (set) Token: 0x06003280 RID: 12928 RVA: 0x000C2734 File Offset: 0x000C0934
		internal _ICompileContext Comcon { get; private set; }

		// Token: 0x17000834 RID: 2100
		// (get) Token: 0x06003281 RID: 12929 RVA: 0x000C2740 File Offset: 0x000C0940
		// (set) Token: 0x06003282 RID: 12930 RVA: 0x000C2748 File Offset: 0x000C0948
		internal \u001F.\u0007 Typechecker { get; private set; }

		// Token: 0x17000835 RID: 2101
		// (get) Token: 0x06003283 RID: 12931 RVA: 0x000C2754 File Offset: 0x000C0954
		// (set) Token: 0x06003284 RID: 12932 RVA: 0x000C275C File Offset: 0x000C095C
		internal _IExpressionTypifier Expressiontypifier { get; private set; }

		// Token: 0x17000836 RID: 2102
		// (get) Token: 0x06003285 RID: 12933 RVA: 0x000C2768 File Offset: 0x000C0968
		// (set) Token: 0x06003286 RID: 12934 RVA: 0x000C2770 File Offset: 0x000C0970
		internal IErrorVisitor Errorvisitor { get; set; }

		// Token: 0x17000837 RID: 2103
		// (get) Token: 0x06003287 RID: 12935 RVA: 0x000C277C File Offset: 0x000C097C
		// (set) Token: 0x06003288 RID: 12936 RVA: 0x000C2784 File Offset: 0x000C0984
		internal ISourcePosition SourcePos { get; private set; }

		// Token: 0x17000838 RID: 2104
		// (get) Token: 0x06003289 RID: 12937 RVA: 0x000C2790 File Offset: 0x000C0990
		// (set) Token: 0x0600328A RID: 12938 RVA: 0x000C2798 File Offset: 0x000C0998
		internal _ISignature SignDecl { get; private set; }

		// Token: 0x17000839 RID: 2105
		// (get) Token: 0x0600328B RID: 12939 RVA: 0x000C27A4 File Offset: 0x000C09A4
		// (set) Token: 0x0600328C RID: 12940 RVA: 0x000C27AC File Offset: 0x000C09AC
		internal _IVariable VarWithType { get; private set; }

		// Token: 0x1700083A RID: 2106
		// (get) Token: 0x0600328D RID: 12941 RVA: 0x000C27B8 File Offset: 0x000C09B8
		// (set) Token: 0x0600328E RID: 12942 RVA: 0x000C27C0 File Offset: 0x000C09C0
		internal \u001A.\u0013 InterfaceCompiler { get; private set; }

		// Token: 0x0600328F RID: 12943 RVA: 0x000C27CC File Offset: 0x000C09CC
		private \u0017()
		{
		}

		// Token: 0x06003290 RID: 12944 RVA: 0x000C27D4 File Offset: 0x000C09D4
		internal static \u0017 \u0001(IScope \u0002, _ICompileContext \u0003, ISourcePosition \u0004, _ISignature \u0005, _IVariable \u0006)
		{
			\u001F.\u0007 u = new global::\u0014.\u0012(\u0002 as _IScope2, \u0003, null);
			TypifierAndCrossReferenceCollector u2 = new TypifierAndCrossReferenceCollector(\u0002 as IScope5, \u0003, null);
			ErrorVisitor u3 = new ErrorVisitor();
			return new \u0017
			{
				Scope = \u0002,
				Comcon = \u0003,
				SourcePos = \u0004,
				SignDecl = \u0005,
				Typechecker = u,
				Expressiontypifier = u2,
				Errorvisitor = u3,
				VarWithType = \u0006,
				InterfaceCompiler = null
			};
		}

		// Token: 0x06003291 RID: 12945 RVA: 0x000C2848 File Offset: 0x000C0A48
		internal static \u0017 \u0001(IScope \u0002, _ICompileContext \u0003, \u001F.\u0007 \u0004, _IExpressionTypifier \u0005, ISourcePosition \u0006)
		{
			ErrorVisitor u = new ErrorVisitor();
			return new \u0017
			{
				Scope = \u0002,
				Comcon = \u0003,
				SourcePos = \u0006,
				SignDecl = null,
				Typechecker = \u0004,
				Expressiontypifier = \u0005,
				Errorvisitor = u,
				VarWithType = null,
				InterfaceCompiler = null
			};
		}

		// Token: 0x06003292 RID: 12946 RVA: 0x000C28A0 File Offset: 0x000C0AA0
		internal static \u0017 \u0001(IScope \u0002, _ICompileContext \u0003, \u001F.\u0007 \u0004, _IExpressionTypifier \u0005, ISourcePosition \u0006, _ISignature \u0007, _IVariable \u0008)
		{
			ErrorVisitor u = new ErrorVisitor();
			return new \u0017
			{
				Scope = \u0002,
				Comcon = \u0003,
				SourcePos = \u0006,
				SignDecl = \u0007,
				Typechecker = \u0004,
				Expressiontypifier = \u0005,
				Errorvisitor = u,
				VarWithType = \u0008,
				InterfaceCompiler = null
			};
		}

		// Token: 0x06003293 RID: 12947 RVA: 0x000C28FC File Offset: 0x000C0AFC
		internal static \u0017 \u0001(IScope \u0002, \u001B \u0003, \u001F.\u0007 \u0004, _IExpressionTypifier \u0005, ISourcePosition \u0006, _ISignature \u0007, _IVariable \u0008)
		{
			ErrorVisitor u = new ErrorVisitor();
			return new \u0017
			{
				Scope = \u0002,
				Comcon = \u0003.ComconNew,
				SourcePos = \u0006,
				SignDecl = \u0007,
				Typechecker = \u0004,
				Expressiontypifier = \u0005,
				Errorvisitor = u,
				VarWithType = \u0008,
				InterfaceCompiler = \u0003.InterfaceCompiler
			};
		}

		// Token: 0x04000979 RID: 2425
		[CompilerGenerated]
		private IScope \u0001;

		// Token: 0x0400097A RID: 2426
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x0400097B RID: 2427
		[CompilerGenerated]
		private \u001F.\u0007 \u0001;

		// Token: 0x0400097C RID: 2428
		[CompilerGenerated]
		private _IExpressionTypifier \u0001;

		// Token: 0x0400097D RID: 2429
		[CompilerGenerated]
		private IErrorVisitor \u0001;

		// Token: 0x0400097E RID: 2430
		[CompilerGenerated]
		private ISourcePosition \u0001;

		// Token: 0x0400097F RID: 2431
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x04000980 RID: 2432
		[CompilerGenerated]
		private _IVariable \u0001;

		// Token: 0x04000981 RID: 2433
		[CompilerGenerated]
		private \u001A.\u0013 \u0001;
	}
}
