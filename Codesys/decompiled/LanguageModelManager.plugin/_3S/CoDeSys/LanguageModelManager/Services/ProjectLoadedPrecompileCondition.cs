using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.Services
{
	// Token: 0x02000242 RID: 578
	public class ProjectLoadedPrecompileCondition : IPrecompilePrecondition
	{
		// Token: 0x06002676 RID: 9846 RVA: 0x00060322 File Offset: 0x0005F322
		private void LanguageModelMgrOnAfterLazyLibraryLoad(object sender, EventArgs e)
		{
			if (this.IsDone)
			{
				EventHandler whenDone = this.WhenDone;
				if (whenDone == null)
				{
					return;
				}
				whenDone(this, null);
			}
		}

		// Token: 0x06002677 RID: 9847 RVA: 0x0006033E File Offset: 0x0005F33E
		private void ObjectMgrOnProjectLoadFinished(object sender, ProjectLoadFinishedEventArgs e)
		{
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject && e.ProjectHandle == APEnvironmentFacade.Instance.PrimaryProjectHandle)
			{
				EventHandler whenDone = this.WhenDone;
				if (whenDone == null)
				{
					return;
				}
				whenDone(this, null);
			}
		}

		// Token: 0x17000AEE RID: 2798
		// (get) Token: 0x06002678 RID: 9848 RVA: 0x00060370 File Offset: 0x0005F370
		public bool IsDone
		{
			get
			{
				return !APEnvironmentFacade.Instance.ExistsPrimaryProject || APEnvironmentFacade.Instance.IsLoadProjectFinished(APEnvironmentFacade.Instance.PrimaryProjectHandle);
			}
		}

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x06002679 RID: 9849 RVA: 0x00060394 File Offset: 0x0005F394
		// (remove) Token: 0x0600267A RID: 9850 RVA: 0x000603CC File Offset: 0x0005F3CC
		public event EventHandler WhenDone;

		// Token: 0x0600267B RID: 9851 RVA: 0x00060401 File Offset: 0x0005F401
		public ProjectLoadedPrecompileCondition(LanguageModelManagerConsolidated lmm)
		{
			APEnvironmentFacade.Instance.ProjectLoadFinished += this.ObjectMgrOnProjectLoadFinished;
			lmm.AfterLazyLibraryLoad -= this.LanguageModelMgrOnAfterLazyLibraryLoad;
		}
	}
}
