using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class PropertyBuilder : IPropertyBuilder, IStatementBuilder<IWhitePropertyDeclarationStatement>, IPropertyNameStep, IPropertyAccessStep, IPropertyReturnTypeStep
	{
		private IPropertyToken _propertyOp { get; set; }

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
		private IWhiteExpression _nameExpression
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IColonToken _colon { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteTypeExpression _returnType
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public PropertyBuilder()
		{
			_propertyOp = TokenFactory<IPropertyToken>.Create("PROPERTY");
			_colon = TokenFactory<IColonToken>.Create(":");
		}

		public IWhitePropertyDeclarationStatement Build()
		{
			if (_nameExpression == null)
			{
				throw new BuilderException("NameExpression not assigned");
			}
			if (_access == null)
			{
				throw new BuilderException("Access not assigned");
			}
			if (_returnType == null)
			{
				throw new BuilderException("Return type not assigned");
			}
			return new WhitePropertyDeclarationStatement(_propertyOp, _access, _nameExpression, _colon, _returnType);
		}

		public IPropertyAccessStep WithName(IWhiteExpression nameExpression)
		{
			_nameExpression = nameExpression;
			return this;
		}

		public IPropertyReturnTypeStep WithAccess(IEnumerable<IAccessSpecifierToken> accesses)
		{
			_access = accesses;
			return this;
		}

		public IPropertyBuilder WithReturnType(IWhiteTypeExpression returnType)
		{
			_returnType = returnType;
			return this;
		}
	}
}
