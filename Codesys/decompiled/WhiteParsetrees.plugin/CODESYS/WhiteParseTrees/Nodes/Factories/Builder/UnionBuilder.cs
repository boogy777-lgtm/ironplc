using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class UnionBuilder : IUnionBuilder, IStatementBuilder<IWhiteUnionDeclarationStatement>, IUnionNameStep, IUnionDeclarationsStep
	{
		private ITypeToken _typeOp { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression _nameExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IColonToken _colonOp { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteVariableDeclarationListStatement _declaration
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IEndTypeToken _endTypeOp { get; set; }

		public UnionBuilder()
		{
			_typeOp = TokenFactory<ITypeToken>.Create("TYPE");
			_colonOp = TokenFactory<IColonToken>.Create(":");
			_endTypeOp = TokenFactory<IEndTypeToken>.Create("END_TYPE");
		}

		public IWhiteUnionDeclarationStatement Build()
		{
			if (_nameExpression == null)
			{
				throw new BuilderException("Name Expression not assigned");
			}
			if (_declaration == null)
			{
				throw new BuilderException("Declarations not assigned");
			}
			return new WhiteUnionDeclarationStatement(_typeOp, new List<IAccessSpecifierToken>(), _nameExpression, _colonOp, _declaration, _endTypeOp);
		}

		public IUnionDeclarationsStep WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		public IUnionBuilder WithDeclarations(IWhiteVariableDeclarationListStatement declarations)
		{
			_declaration = declarations;
			return this;
		}
	}
}
