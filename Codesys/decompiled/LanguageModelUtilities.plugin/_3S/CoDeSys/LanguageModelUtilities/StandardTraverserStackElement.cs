using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class StandardTraverserStackElement
	{
		public AccessFlag _access;

		public HelperScope _scope;

		public IVariable _varResolved;

		public ISignature _signResolved;

		public HelperScope _scopeResolved;

		public IType _typeResolved;

		internal StandardTraverserStackElement(AccessFlag access, HelperScope scope)
		{
			_access = access;
			_scope = scope;
			_varResolved = null;
			_signResolved = null;
			_scopeResolved = null;
			_typeResolved = null;
		}

		internal void ClearResult()
		{
			_varResolved = null;
			_signResolved = null;
			_scopeResolved = null;
			_typeResolved = null;
		}
	}
}
