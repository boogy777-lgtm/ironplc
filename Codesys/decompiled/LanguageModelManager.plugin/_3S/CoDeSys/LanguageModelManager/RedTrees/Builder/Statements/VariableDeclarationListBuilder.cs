using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x0200026D RID: 621
	public class VariableDeclarationListBuilder : IVariableDeclarationListBuilder, IVariableDeclarationListOptionalPosStep, ILmbPositional<IVariableDeclarationBuilderAddDecls>, IVariableDeclarationBuilderAddDecls, IVariableDeclarationBuilderFlags, ILmbExprementBuilder<IVariableDeclarationListStatement>
	{
		// Token: 0x06002A22 RID: 10786 RVA: 0x0006B2DB File Offset: 0x0006A2DB
		public IVariableDeclarationListStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			return LanguageModelBuilder.Singleton.CreateVariableDeclarationListStatement(this._pos, this._flag, this._enums.IntoSequence());
		}

		// Token: 0x06002A23 RID: 10787 RVA: 0x0006B314 File Offset: 0x0006A314
		public IVariableDeclarationBuilderAddDecls At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x06002A24 RID: 10788 RVA: 0x0006B31E File Offset: 0x0006A31E
		public ILmbExprementBuilder<IVariableDeclarationListStatement> Flags(VarFlag flag)
		{
			this._flag = flag;
			return this;
		}

		// Token: 0x06002A25 RID: 10789 RVA: 0x0006B328 File Offset: 0x0006A328
		public IVariableDeclarationBuilderAddDecls Declaration(IEnumerable<string> names, ICompiledType type, IExpression initial)
		{
			IVariableDeclarationStatement declaration = LanguageModelBuilder.Singleton.CreateVariableDeclarationStatement(new ExprementPosition(0L, 0), names.ToList<string>(), type, initial);
			this.Declaration(declaration);
			return this;
		}

		// Token: 0x06002A26 RID: 10790 RVA: 0x0006B359 File Offset: 0x0006A359
		public IVariableDeclarationBuilderAddDecls Declaration(IVariableDeclarationStatement declaration)
		{
			this._enums.Add(declaration);
			return this;
		}

		// Token: 0x06002A27 RID: 10791 RVA: 0x0006B368 File Offset: 0x0006A368
		public static IVariableDeclarationListOptionalPosStep Init()
		{
			return new VariableDeclarationListBuilder();
		}

		// Token: 0x040007F8 RID: 2040
		private readonly List<IVariableDeclarationStatement> _enums = new List<IVariableDeclarationStatement>();

		// Token: 0x040007F9 RID: 2041
		private VarFlag _flag;

		// Token: 0x040007FA RID: 2042
		private IExprementPosition _pos;
	}
}
