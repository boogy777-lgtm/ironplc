using System;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Expressions;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions;
using _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder
{
	// Token: 0x02000265 RID: 613
	[TypeGuid("{A06288A8-B84B-433C-8EA7-1AE3C85E1A73}")]
	public class RedTreeBuilderFactory : IRedTreeBuilderFactory
	{
		// Token: 0x17000BCF RID: 3023
		// (get) Token: 0x060029D1 RID: 10705 RVA: 0x0006AB2F File Offset: 0x00069B2F
		public ICalleeBuilderOptionalPosStep CallBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions.CallBuilder.Init();
			}
		}

		// Token: 0x17000BD0 RID: 3024
		// (get) Token: 0x060029D2 RID: 10706 RVA: 0x0006AB36 File Offset: 0x00069B36
		public IOperatorOptionalPosStep OperatorBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Expressions.OperatorBuilder.Init();
			}
		}

		// Token: 0x17000BD1 RID: 3025
		// (get) Token: 0x060029D3 RID: 10707 RVA: 0x0006AB3D File Offset: 0x00069B3D
		public IIfOptionalPosStep IfBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.IfBuilder.Init();
			}
		}

		// Token: 0x17000BD2 RID: 3026
		// (get) Token: 0x060029D4 RID: 10708 RVA: 0x0006AB44 File Offset: 0x00069B44
		public IForOptionalPosStep ForBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.ForBuilder.Init();
			}
		}

		// Token: 0x17000BD3 RID: 3027
		// (get) Token: 0x060029D5 RID: 10709 RVA: 0x0006AB4B File Offset: 0x00069B4B
		public ICaseOptionalPosStep CaseBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.CaseBuilder.Init();
			}
		}

		// Token: 0x17000BD4 RID: 3028
		// (get) Token: 0x060029D6 RID: 10710 RVA: 0x0006AB52 File Offset: 0x00069B52
		public IWhileOptionalPosStep WhileBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.WhileBuilder.Init();
			}
		}

		// Token: 0x17000BD5 RID: 3029
		// (get) Token: 0x060029D7 RID: 10711 RVA: 0x0006AB59 File Offset: 0x00069B59
		public ITypeOptionalPosStep TypeDeclarationBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.TypeDeclarationBuilder.Init();
			}
		}

		// Token: 0x17000BD6 RID: 3030
		// (get) Token: 0x060029D8 RID: 10712 RVA: 0x0006AB60 File Offset: 0x00069B60
		public IElseIfOptionalPosStep ElseIfBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.ElseIfBuilder.Init();
			}
		}

		// Token: 0x17000BD7 RID: 3031
		// (get) Token: 0x060029D9 RID: 10713 RVA: 0x0006AB67 File Offset: 0x00069B67
		public IPouDeclarationBuilderOptionalPosStep PouDeclarationBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.PouDeclarationBuilder.Init();
			}
		}

		// Token: 0x17000BD8 RID: 3032
		// (get) Token: 0x060029DA RID: 10714 RVA: 0x0006AB6E File Offset: 0x00069B6E
		public IEnumOptionalPosStep EnumDeclarationListBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.EnumDeclarationListBuilder.Init();
			}
		}

		// Token: 0x17000BD9 RID: 3033
		// (get) Token: 0x060029DB RID: 10715 RVA: 0x0006AB75 File Offset: 0x00069B75
		public IVariableDeclarationListOptionalPosStep VariableDeclarationListBuilder
		{
			get
			{
				return _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements.VariableDeclarationListBuilder.Init();
			}
		}
	}
}
