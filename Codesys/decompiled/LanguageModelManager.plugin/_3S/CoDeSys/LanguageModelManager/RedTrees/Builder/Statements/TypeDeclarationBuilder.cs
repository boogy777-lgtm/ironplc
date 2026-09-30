using System;
using _3S.CoDeSys.Compiler.LanguageModelBuilder;
using _3S.CoDeSys.Compiler.LanguageModelBuilder.Statements;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.RedTrees.Builder.Statements
{
	// Token: 0x0200026C RID: 620
	public class TypeDeclarationBuilder : ITypeDeclarationStatementBuilder, ITypeOptionalPosStep, ILmbPositional<ITypeBuilderChoose>, ITypeBuilderChoose, ITypeBuilderAlias, ITypeBuilderName, ILmbExprementBuilder<ITypeDeclarationStatement>, ITypeBuilderOptionalExtend, ITypeBuilderBaseType, ITypeBuilderDeclarations, ITypeBuilderOptionalInitialValue, ITypeBuilderInitialValue, ITypeBuilderFlag
	{
		// Token: 0x06002A17 RID: 10775 RVA: 0x0006B1E4 File Offset: 0x0006A1E4
		public ITypeDeclarationStatement Build()
		{
			if (this._pos == null)
			{
				this._pos = new ExprementPosition(0L, 0);
			}
			TypeDeclarationStatement typeDeclarationStatement = new TypeDeclarationStatement();
			typeDeclarationStatement.SetPositionIntern(new MinimalPositionBase(this._pos.Position, this._pos.PositionOffset));
			typeDeclarationStatement.Name = this._name;
			typeDeclarationStatement.Declarations = this._declarations;
			typeDeclarationStatement.Extends = this._extendsOrBaseType;
			typeDeclarationStatement.Initial = this._initialValue;
			typeDeclarationStatement.Flags = this._flags;
			typeDeclarationStatement.Type = this._alias;
			return typeDeclarationStatement;
		}

		// Token: 0x06002A18 RID: 10776 RVA: 0x0006B27A File Offset: 0x0006A27A
		public ITypeBuilderDeclarations Extends(IExpression type)
		{
			this._extendsOrBaseType = (type as _IExpression);
			return this;
		}

		// Token: 0x06002A19 RID: 10777 RVA: 0x0006B289 File Offset: 0x0006A289
		public ITypeBuilderOptionalInitialValue StructOrUnion(IVariableDeclarationListStatement value)
		{
			this._declarations = (value as _IStatement);
			return this;
		}

		// Token: 0x06002A1A RID: 10778 RVA: 0x0006B289 File Offset: 0x0006A289
		public ITypeBuilderOptionalInitialValue Enum(IEnumDeclarationListStatement value)
		{
			this._declarations = (value as _IStatement);
			return this;
		}

		// Token: 0x06002A1B RID: 10779 RVA: 0x0006B298 File Offset: 0x0006A298
		public ILmbExprementBuilder<ITypeDeclarationStatement> Flags(SignatureFlag flags)
		{
			this._flags = flags;
			return this;
		}

		// Token: 0x06002A1C RID: 10780 RVA: 0x0006B2A2 File Offset: 0x0006A2A2
		public ITypeBuilderFlag InitialValue(IExpression value)
		{
			this._initialValue = (value as _IExpression);
			return this;
		}

		// Token: 0x06002A1D RID: 10781 RVA: 0x0006B2B1 File Offset: 0x0006A2B1
		public ITypeBuilderChoose At(IExprementPosition position)
		{
			this._pos = position;
			return this;
		}

		// Token: 0x06002A1E RID: 10782 RVA: 0x0006B2BB File Offset: 0x0006A2BB
		public ITypeBuilderFlag Alias(IType type)
		{
			this._alias = (type as _IType);
			return this;
		}

		// Token: 0x06002A1F RID: 10783 RVA: 0x0006B2CA File Offset: 0x0006A2CA
		public ITypeBuilderOptionalExtend Name(string name)
		{
			this._name = name;
			return this;
		}

		// Token: 0x06002A20 RID: 10784 RVA: 0x0006B2D4 File Offset: 0x0006A2D4
		public static ITypeOptionalPosStep Init()
		{
			return new TypeDeclarationBuilder();
		}

		// Token: 0x040007F1 RID: 2033
		private _IType _alias;

		// Token: 0x040007F2 RID: 2034
		private _IStatement _declarations;

		// Token: 0x040007F3 RID: 2035
		private _IExpression _extendsOrBaseType;

		// Token: 0x040007F4 RID: 2036
		private SignatureFlag _flags;

		// Token: 0x040007F5 RID: 2037
		private _IExpression _initialValue;

		// Token: 0x040007F6 RID: 2038
		private string _name;

		// Token: 0x040007F7 RID: 2039
		private IExprementPosition _pos;
	}
}
