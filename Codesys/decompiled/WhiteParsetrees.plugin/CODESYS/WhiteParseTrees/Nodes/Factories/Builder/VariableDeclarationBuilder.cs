using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(2)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class VariableDeclarationBuilder : IVariableDeclarationBuilder, IStatementBuilder<IWhiteVariableDeclarationStatement>, IVarDeclNameStep, IVarDeclInitializationStep, IVarDeclTypeStep, IVarDeclAtStep
	{
		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IList<IWhiteExpression> _variableNames
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		private IAtToken _at { get; set; }

		private IWhiteDirectVariableExpression _addressLocation { get; set; }

		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		private IColonToken _colon
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		private IWhiteTypeExpression _declaredType { get; set; }

		private IAnyAssignmentToken _assignment { get; set; }

		private IWhiteExpression _initializationExpression { get; set; }

		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		private ISemicolonToken _semicolon
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
			[System.Runtime.CompilerServices.NullableContext(1)]
			set;
		}

		public VariableDeclarationBuilder()
		{
			_at = TokenFactory<IAtToken>.Create("AT");
			_colon = TokenFactory<IColonToken>.Create(":");
			_assignment = TokenFactory<IAnyAssignmentToken>.Create(":=");
			_semicolon = TokenFactory<ISemicolonToken>.Create(";");
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IWhiteVariableDeclarationStatement Build()
		{
			if (_variableNames == null)
			{
				throw new BuilderException("VariableNames not assigned");
			}
			if (_addressLocation == null)
			{
				_at = null;
			}
			if (_declaredType == null)
			{
				throw new BuilderException("DeclaredType not assigned");
			}
			if (_initializationExpression == null)
			{
				_assignment = null;
			}
			return new WhiteVariableDeclarationStatement(_variableNames, _at, _addressLocation, _colon, _declaredType, _assignment, _initializationExpression, _semicolon);
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVarDeclTypeStep WithName(IEnumerable<IWhiteExpression> VariableNames)
		{
			_variableNames = VariableNames.ToList();
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVarDeclTypeStep WithName(IWhiteExpression VariableName)
		{
			_variableNames = new List<IWhiteExpression> { VariableName };
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVarDeclTypeStep WithAt(IWhiteDirectVariableExpression addressLocation)
		{
			_addressLocation = addressLocation;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVarDeclInitializationStep WithType(IWhiteTypeExpression type)
		{
			_declaredType = type;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVariableDeclarationBuilder WithInitialization(IWhiteExpression initialization)
		{
			_initializationExpression = initialization;
			return this;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IVariableDeclarationBuilder Finish()
		{
			return this;
		}
	}
}
