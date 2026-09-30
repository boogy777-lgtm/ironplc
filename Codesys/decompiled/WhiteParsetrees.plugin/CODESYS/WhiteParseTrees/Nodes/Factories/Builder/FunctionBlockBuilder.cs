using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class FunctionBlockBuilder : IFunctionBlockBuilder, IStatementBuilder<IWhiteFunctionBlockDeclarationStatement>, IFunctionBlockNameStep, IFunctionBlockOptionalsStep, IFunctionBlockImplementsStep, IFunctionBlockExtendsStep, IFunctionBlockAccessStep
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		private IFunctionBlockToken _class
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		private IWhiteExpression _nameExpression { get; set; }

		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		private IWhiteSequenceStatement _genericDeclarations
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

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

		private IExtendsToken _extendsOp { get; set; }

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteExpression> _extends
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		private IImplementsToken _implementsOp { get; set; }

		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteExpression> _implements
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		public FunctionBlockBuilder()
		{
			_class = TokenFactory<IFunctionBlockToken>.Create("FUNCTION_BLOCK");
			_extendsOp = TokenFactory<IExtendsToken>.Create("EXTENDS");
			_implementsOp = TokenFactory<IImplementsToken>.Create("IMPLEMENTS");
			_genericDeclarations = new WhiteSequenceStatement();
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IWhiteFunctionBlockDeclarationStatement Build()
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
				IEnumerable<IAccessSpecifierToken> enumerable2 = (_access = new List<IAccessSpecifierToken>());
			}
			if (_extends == null)
			{
				_extends = new List<IWhiteExpression>();
				_extendsOp = null;
			}
			if (_implements == null)
			{
				_implements = new List<IWhiteExpression>();
				_implementsOp = null;
			}
			return new WhiteFunctionBlockDeclarationStatement(_class, _access, _nameExpression, _genericDeclarations, _extendsOp, _extends, _implementsOp, _implements, _declarations);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockOptionalsStep WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockOptionalsStep WithGenericDeclarations(IWhiteSequenceStatement declarations)
		{
			_genericDeclarations = declarations;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockOptionalsStep WithAccess(IEnumerable<IAccessSpecifierToken> accesses)
		{
			_access = accesses;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockBuilder WithDeclarations(IWhiteSequenceStatement declarations)
		{
			_declarations = declarations;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockOptionalsStep Implements(IEnumerable<IWhiteExpression> implements)
		{
			_implements = implements;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IFunctionBlockOptionalsStep Extends(IEnumerable<IWhiteExpression> extends)
		{
			_extends = extends;
			return this;
		}
	}
}
