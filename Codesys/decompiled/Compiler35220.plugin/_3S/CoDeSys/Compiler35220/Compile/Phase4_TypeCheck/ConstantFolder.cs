using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0010;
using \u0018;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck
{
	// Token: 0x020002FF RID: 767
	public class ConstantFolder : EmptyVisitor351900
	{
		// Token: 0x170007D4 RID: 2004
		// (get) Token: 0x06002ECE RID: 11982 RVA: 0x000B0364 File Offset: 0x000AE564
		private IScope5 Scope { get; }

		// Token: 0x170007D5 RID: 2005
		// (get) Token: 0x06002ECF RID: 11983 RVA: 0x000B036C File Offset: 0x000AE56C
		private _ICompileContext CompileContext { get; }

		// Token: 0x170007D6 RID: 2006
		// (get) Token: 0x06002ED0 RID: 11984 RVA: 0x000B0374 File Offset: 0x000AE574
		// (set) Token: 0x06002ED1 RID: 11985 RVA: 0x000B037C File Offset: 0x000AE57C
		private global::\u0010.\u0007 ConstantFolderTraverser { get; set; }

		// Token: 0x170007D7 RID: 2007
		// (get) Token: 0x06002ED2 RID: 11986 RVA: 0x000B0388 File Offset: 0x000AE588
		private _ICompiledPOU CompiledPOU { get; }

		// Token: 0x06002ED3 RID: 11987 RVA: 0x000B0390 File Offset: 0x000AE590
		private ConstantFolder(_ICompileContext compileContext, IScope5 scope, _ICompiledPOU compiledPOU)
		{
			this.CompileContext = compileContext;
			this.Scope = scope;
			this.CompiledPOU = compiledPOU;
		}

		// Token: 0x06002ED4 RID: 11988 RVA: 0x000B03B0 File Offset: 0x000AE5B0
		internal void \u0001(_IOperatorExpression \u0002)
		{
			new ExpressionTypifierWithSpecialTasks(this.Scope, this.CompileContext, this.CompiledPOU)
			{
				InterpretPragmas = false,
				NoCrossReferences = true
			}.\u0001(\u0002);
		}

		// Token: 0x06002ED5 RID: 11989 RVA: 0x000B03E0 File Offset: 0x000AE5E0
		public static _IExpression ReplaceFoldedConstantsInPlace(_IExpression expression, _ICompileContext compileContext, IScope5 scope, _ICompiledPOU compiledPOU)
		{
			ConstantFolder constantFolder = new ConstantFolder(compileContext, scope, compiledPOU);
			global::\u0010.\u0007 u = new global::\u0010.\u0007(constantFolder);
			constantFolder.ConstantFolderTraverser = u;
			_IExpression iexpression = u.\u0001(expression);
			iexpression.Accept(u);
			return iexpression;
		}

		// Token: 0x06002ED6 RID: 11990 RVA: 0x000B0410 File Offset: 0x000AE610
		public static void ReplaceInnerFoldedConstants(_IExpression expression, _ICompileContext compileContext, IScope5 scope, _ICompiledPOU compiledPOU)
		{
			ConstantFolder constantFolder = new ConstantFolder(compileContext, scope, compiledPOU);
			global::\u0010.\u0007 u = new global::\u0010.\u0007(constantFolder);
			constantFolder.ConstantFolderTraverser = u;
			expression.Accept(u);
		}

		// Token: 0x06002ED7 RID: 11991 RVA: 0x000B043C File Offset: 0x000AE63C
		public static void ReplaceFoldedConstants(_IStatement parsetree, _ICompileContext compileContext, IScope5 scope, _ICompiledPOU compiledPOU)
		{
			ConstantFolder constantFolder = new ConstantFolder(compileContext, scope, compiledPOU);
			global::\u0010.\u0007 u = new global::\u0010.\u0007(constantFolder);
			constantFolder.ConstantFolderTraverser = u;
			parsetree.Accept(u);
		}

		// Token: 0x06002ED8 RID: 11992 RVA: 0x000B0468 File Offset: 0x000AE668
		public override void visit(_IOperatorExpression op)
		{
			_ILiteralExpression iliteralExpression = this.\u0001(op);
			if (iliteralExpression != null)
			{
				this.ConstantFolderTraverser.\u0001(iliteralExpression);
			}
		}

		// Token: 0x06002ED9 RID: 11993 RVA: 0x000B048C File Offset: 0x000AE68C
		private _ILiteralExpression \u0001(_IOperatorExpression \u0002)
		{
			bool u = \u001D.\u0005.\u0001(\u0002);
			if (!\u0002.IsConstant(this.Scope, false))
			{
				return null;
			}
			ILiteralValue literalValue = \u0002.Literal(this.Scope);
			if (literalValue == null)
			{
				return null;
			}
			\u0018.\u000E.\u0001(\u0002, this.CompileContext);
			_ILiteralExpression iliteralExpression = ConstantFoldingHelper.\u0001(literalValue);
			if (iliteralExpression != null)
			{
				iliteralExpression._Position = \u0002._Position;
				this.\u0001(\u0002, iliteralExpression);
				this.\u0001(\u0002, u, iliteralExpression);
			}
			if (iliteralExpression != null && this.\u0001(iliteralExpression))
			{
				return iliteralExpression;
			}
			return null;
		}

		// Token: 0x06002EDA RID: 11994 RVA: 0x000B0508 File Offset: 0x000AE708
		private bool \u0001(_ILiteralExpression \u0002)
		{
			return \u0002.Type != null && (!TypeTable.IsInteger(\u0002.Type.Class) || TypeTable.IsSigned(\u0002.Type.Class) || !\u0002.Negative);
		}

		// Token: 0x06002EDB RID: 11995 RVA: 0x000B0544 File Offset: 0x000AE744
		private void \u0001(_IOperatorExpression \u0002, _ILiteralExpression \u0003)
		{
			if (\u0002.Code == Operator.SizeOf && TypeTable.GetSize(\u0002.Type.Class, this.Scope) <= 2)
			{
				ulong num = 0UL;
				if (\u0003.LiteralValue.KindOf == KindOfLiteral.SignedInteger)
				{
					bool flag;
					num = (ulong)\u0003.LiteralValue.GetSignedLong(out flag);
				}
				else if (\u0003.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger)
				{
					bool flag;
					num = \u0003.LiteralValue.GetUnsignedLong(out flag);
				}
				if (num > 65535UL)
				{
					string u = \u0018.\u0001(MessageId.Inf_UseXSizeOfOperator);
					_ICompilerMessage cm = \u0019.\u0003.\u0001(\u0002.Position, u, Severity.Information, MessageId.Inf_UseXSizeOfOperator);
					\u0003.AddMessage(cm);
				}
			}
		}

		// Token: 0x06002EDC RID: 11996 RVA: 0x000B05E4 File Offset: 0x000AE7E4
		private void \u0001(_IOperatorExpression \u0002, bool \u0003, _ILiteralExpression \u0004)
		{
			if (\u0002.Type != null && \u0002.Type.Class != TypeClass.Bool && !TypeTable.IsTimeOrDateType(\u0002.Type.Class))
			{
				\u0004.Type = Helper.\u0001(\u0004, this.CompileContext.TypeIsSupported(TypeClass.LReal), this.CompileContext.TreatLRealAsReal, this.CompileContext.TypeIsSupported(TypeClass.LInt), this.CompileContext.TreatInt64AsInt32, this.CompileContext.IsDefined("NO_UNICODE_SUPPORT"), this.CompileContext.HasByteSupport());
				_IExpression[] u = new _IExpression[]
				{
					\u0002,
					\u0004
				};
				_IType itype = \u001D.\u0005.\u0001(\u0002.Code, \u0003, 0, u, \u0002._CompiledType, this.Scope, this.CompileContext);
				if (TypeTable.IsReal(itype.Class) == TypeTable.IsReal(\u0004.Type.Class))
				{
					\u0004.Type = itype;
					\u0004.ConstantType = \u0004.Type.Class;
				}
				else
				{
					\u0004.Type = Helper.\u0001(\u0004, this.CompileContext.TypeIsSupported(TypeClass.LReal), this.CompileContext.TreatLRealAsReal, this.CompileContext.TypeIsSupported(TypeClass.LInt), this.CompileContext.TreatInt64AsInt32, this.CompileContext.IsDefined("NO_UNICODE_SUPPORT"), this.CompileContext.HasByteSupport());
				}
				\u0002.Type = itype;
				return;
			}
			\u0004.Type = \u0002.Type;
		}

		// Token: 0x040008E9 RID: 2281
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x040008EA RID: 2282
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040008EB RID: 2283
		[CompilerGenerated]
		private global::\u0010.\u0007 \u0001;

		// Token: 0x040008EC RID: 2284
		[CompilerGenerated]
		private readonly _ICompiledPOU \u0001;
	}
}
