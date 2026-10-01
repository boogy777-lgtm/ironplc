using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class InterfaceBuilder : IInterfaceBuilder, IStatementBuilder<IWhiteInterfaceDeclarationStatement>, IInterfaceNameStep3, IInterfaceNameStep2, IInterfaceNameStep, IInterfaceOptionalsStep3, IInterfaceOptionalsStep2, IInterfaceOptionalsStep, IInterfaceExtendsStep
	{
		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IAccessSpecifierToken> _access
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IExtendsToken _extendsOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		} = TokenFactory<IExtendsToken>.Create("EXTENDS");


		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteExpression> _extends
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IImplementsToken _implementsOp
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		} = TokenFactory<IImplementsToken>.Create("IMPLEMENTS");


		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteExpression> _implements
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		private IInterfaceToken _class { get; set; } = TokenFactory<IInterfaceToken>.Create("INTERFACE");


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

		public IWhiteInterfaceDeclarationStatement Build()
		{
			if (_nameExpression == null)
			{
				throw new BuilderException("Name Expression not assigned");
			}
			if (_declarations == null)
			{
				throw new BuilderException("Declarations not assigned");
			}
			if (_extends == null)
			{
				_extendsOp = null;
				_extends = new List<IWhiteExpression>();
			}
			if (_implements == null)
			{
				_implementsOp = null;
				_implements = new List<IWhiteExpression>();
			}
			IAccessSpecifierToken[] access = _access?.ToArray() ?? Array.Empty<IAccessSpecifierToken>();
			return new WhiteInterfaceDeclarationStatement(_class, access, _nameExpression, _extendsOp, _extends, _implementsOp, _implements, _declarations);
		}

		IInterfaceOptionalsStep IInterfaceNameStep.WithName(IWhiteExpression nameExpression)
		{
			return WithName(nameExpression);
		}

		IInterfaceOptionalsStep2 IInterfaceNameStep2.WithName(IWhiteExpression nameExpression)
		{
			return WithName(nameExpression);
		}

		public IInterfaceOptionalsStep3 WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		IInterfaceOptionalsStep IInterfaceExtendsStep.Extends(IEnumerable<IWhiteExpression> extends)
		{
			return Extends(extends);
		}

		IInterfaceOptionalsStep2 IInterfaceOptionalsStep2.WithDeclarations(IWhiteSequenceStatement declarations)
		{
			return WithDeclarations(declarations);
		}

		IInterfaceOptionalsStep2 IInterfaceOptionalsStep2.WithAccess(IEnumerable<IAccessSpecifierToken> access)
		{
			return WithAccess(access);
		}

		public IInterfaceOptionalsStep3 Extends(IEnumerable<IWhiteExpression> extends)
		{
			_extends = extends;
			return this;
		}

		IInterfaceOptionalsStep2 IInterfaceOptionalsStep2.Extends(IEnumerable<IWhiteExpression> extends)
		{
			return Extends(extends);
		}

		public IInterfaceOptionalsStep3 Implements(IEnumerable<IWhiteExpression> implements)
		{
			_implements = implements;
			return this;
		}

		IInterfaceBuilder IInterfaceOptionalsStep.WithDeclarations(IWhiteSequenceStatement declarations)
		{
			_declarations = declarations;
			return this;
		}

		public IInterfaceOptionalsStep3 WithDeclarations(IWhiteSequenceStatement declarations)
		{
			_declarations = declarations;
			return this;
		}

		public IInterfaceOptionalsStep3 WithAccess(IEnumerable<IAccessSpecifierToken> access)
		{
			_access = access;
			return this;
		}
	}
}
