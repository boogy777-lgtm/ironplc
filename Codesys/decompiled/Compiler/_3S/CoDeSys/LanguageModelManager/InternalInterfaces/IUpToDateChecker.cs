using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IUpToDateChecker
	{
		bool IsLmUpToDate(_ICompileContext comcon, _IPreCompileContext precomp, _IPreCompileContext precompPool, _IIsUpTopDateStrategy strategy);

		_IIsUpTopDateStrategy CreateFastIsUpToDateStrategy();

		_IIsUpTopDateStrategy CreateDetailedIsUpToDateStrategy(_ICompileContext comcon, _IPreCompileContext precomp, bool bCollectAllOnlineChangeProhibitingChanges);
	}
}
