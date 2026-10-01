using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x02000268 RID: 616
	public class EnumDeclarationListBuilder : IEnumDeclarationListBuilder, IEnumOptionalPosStep, ILmbPositional<IEnumBuilderAddValue>, IEnumBuilderAddValue, IEnumBuilderBaseType, ILmbExprementBuilder<IEnumDeclarationListStatement>
	{
		// Token: 0x060029EC RID: 10732 RVA: 0x0006AD21 File Offset: 0x00069D21
		public IEnumDeclarationListStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			return LanguageModelBuilder.Singleton.CreateEnumDeclarationListStatement(this._pos, this._baseType, this._enums);
		}

		// Token: 0x060029ED RID: 10733 RVA: 0x0006AD55 File Offset: 0x00069D55
		public IEnumBuilderAddValue At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x060029EE RID: 10734 RVA: 0x0006AD5F File Offset: 0x00069D5F
		public ILmbExprementBuilder<IEnumDeclarationListStatement> BaseType(ICompiledType type)
		{
			this._baseType = type;
			return this;
		}

		// Token: 0x060029EF RID: 10735 RVA: 0x0006AD6C File Offset: 0x00069D6C
		public IEnumBuilderAddValue Enumeration(string stName, IExpression expInit)
		{
			IEnumDeclarationStatement item = LanguageModelBuilder.Singleton.CreateEnumDeclarationStatement(null, stName, expInit);
			this._enums.Add(item);
			return this;
		}

		// Token: 0x060029F0 RID: 10736 RVA: 0x0006AD94 File Offset: 0x00069D94
		public IEnumBuilderAddValue Enumeration(IEnumDeclarationStatement enumDeclarationStatement)
		{
			this._enums.Add(enumDeclarationStatement);
			return this;
		}

		// Token: 0x060029F1 RID: 10737 RVA: 0x0006ADA3 File Offset: 0x00069DA3
		public static IEnumOptionalPosStep Init()
		{
			return new EnumDeclarationListBuilder();
		}

		// Token: 0x040007DB RID: 2011
		private readonly List<IEnumDeclarationStatement> _enums = new List<IEnumDeclarationStatement>();

		// Token: 0x040007DC RID: 2012
		private ICompiledType _baseType;

		// Token: 0x040007DD RID: 2013
		private IExprementPosition _pos;
	}
}
