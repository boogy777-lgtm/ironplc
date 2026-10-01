using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class ProgramBuilder : IProgramBuilder, IStatementBuilder<IWhiteProgramDeclarationStatement>, IProgramNameStep, IProgramAccessStep, IProgramDeclarationsStep
	{
		private IProgramToken _class { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression _nameExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteSequenceStatement _declarations
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IAccessSpecifierToken> _access
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		public ProgramBuilder()
		{
			_class = TokenFactory<IProgramToken>.Create("PROGRAM");
		}

		public IWhiteProgramDeclarationStatement Build()
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
			return new WhiteProgramDeclarationStatement(_class, _access, _nameExpression, _declarations);
		}

		public IProgramAccessStep WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		public IProgramDeclarationsStep WithAccess(IEnumerable<IAccessSpecifierToken> accesses)
		{
			_access = accesses;
			return this;
		}

		public IProgramBuilder WithDeclarations(IWhiteSequenceStatement declarations)
		{
			_declarations = declarations;
			return this;
		}
	}
}
