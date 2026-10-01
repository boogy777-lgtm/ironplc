using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IAddressCalculatorUnittestSupport
	{
		bool HasByteSupport { get; set; }

		void Initialize(ISourcePosition sp, ICompileContext compilecontext, IDirectVariable dirvar, IVariable2 var);

		IDataLocation CalculateAddressInternal();
	}
}
