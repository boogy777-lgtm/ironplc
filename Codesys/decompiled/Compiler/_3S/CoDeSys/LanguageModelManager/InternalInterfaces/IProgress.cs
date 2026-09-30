using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IProgress
	{
		IProgressCallback Callback { get; }

		void StartLengthyOperation();

		void EndLengthyOperation();

		void NotifyNextTask(bool bAbortable, string stTask, int nItemsTotal, string stItemUnit);

		void NotifyTaskProgress(string stItem, int nInc);

		void NotifyTaskProgress(string stItem);

		void NotifyNextTask(IProgressCallback callback, bool bAbortable, string stTask, int nItemsTotal, string stItemUnit);

		void NotifyTaskProgress(IProgressCallback callback, string stItem, int nInc);

		void NotifyTaskProgress(IProgressCallback callback, string stItem);
	}
}
