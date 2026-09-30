using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class MethodBuilder : IMethodBuilder, IStatementBuilder<IWhiteMethodDeclarationStatement>, IMethodNameStep, IMethodAccessStep, IMethodReturnTypeStep, IMethodDeclarationsStep
	{
		private IColonToken _colon { get; set; }

		private IWhiteTypeExpression _returnType { get; set; }

		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		private IMethodToken _class
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		private IWhiteExpression _nameExpression { get; set; }

		private IWhiteSequenceStatement _declarations { get; set; }

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IAccessSpecifierToken> _access
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		public MethodBuilder()
		{
			_class = TokenFactory<IMethodToken>.Create("METHOD");
			_colon = TokenFactory<IColonToken>.Create(":");
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IWhiteMethodDeclarationStatement Build()
		{
			if (_nameExpression == null)
			{
				throw new BuilderException("NameExpression not assigned");
			}
			if (_declarations == null)
			{
				throw new BuilderException("Declarations not assigned");
			}
			if (_access == null)
			{
				throw new BuilderException("Access not assigned");
			}
			return new WhiteMethodDeclarationStatement(_class, _access, _nameExpression, _colon, _returnType, _declarations);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IMethodAccessStep WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IMethodReturnTypeStep WithAccess(IEnumerable<IAccessSpecifierToken> accesses)
		{
			_access = accesses;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IMethodDeclarationsStep WithReturnType(IWhiteTypeExpression returnType)
		{
			_returnType = returnType;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IMethodBuilder WithDeclarations(IWhiteSequenceStatement declarations)
		{
			_declarations = declarations;
			return this;
		}
	}
}
