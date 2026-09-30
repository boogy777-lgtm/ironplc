using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IRecursionGuard
	{
		void Add(object obj);

		bool Has(object obj);

		IRecursionGuard Duplicate();
	}
}
