using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[ExcludeFromCodeCoverage]
	internal abstract class Ignorable
	{
		public bool visit(IWhiteMultipleIndexInitialization expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteNamespaceAccessExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhitePointerTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteReferenceTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteArrayInitializationExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteArrayTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(ISubRangeTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IStringTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IEnumerationTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IVectorTypeExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IEmptyExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteStructureInitialization expression, IExpression context)
		{
			return false;
		}
	}
}
