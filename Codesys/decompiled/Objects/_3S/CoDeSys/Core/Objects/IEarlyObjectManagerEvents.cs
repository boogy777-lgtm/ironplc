using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F4 RID: 244
	[ReleasedInterface]
	public interface IEarlyObjectManagerEvents
	{
		// Token: 0x1400000B RID: 11
		// (add) Token: 0x060003BB RID: 955
		// (remove) Token: 0x060003BC RID: 956
		event ProjectLoadedEventHandler EProjectLoaded;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x060003BD RID: 957
		// (remove) Token: 0x060003BE RID: 958
		event ProjectSavedEventHandler EProjectSaved;

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060003BF RID: 959
		// (remove) Token: 0x060003C0 RID: 960
		event ProjectCreatedEventHandler EProjectCreated;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x060003C1 RID: 961
		// (remove) Token: 0x060003C2 RID: 962
		event ProjectClosedEventHandler EProjectClosed;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x060003C3 RID: 963
		// (remove) Token: 0x060003C4 RID: 964
		event ProjectDirtyChangedEventHandler EProjectDirtyChanged;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x060003C5 RID: 965
		// (remove) Token: 0x060003C6 RID: 966
		event ObjectEventHandler EObjectAdded;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x060003C7 RID: 967
		// (remove) Token: 0x060003C8 RID: 968
		event ObjectEventHandler EObjectLoaded;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060003C9 RID: 969
		// (remove) Token: 0x060003CA RID: 970
		event ObjectRemovedEventHandler EObjectRemoved;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060003CB RID: 971
		// (remove) Token: 0x060003CC RID: 972
		event ObjectModifiedEventHandler EObjectModified;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060003CD RID: 973
		// (remove) Token: 0x060003CE RID: 974
		event ObjectRenamedEventHandler EObjectRenamed;

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x060003CF RID: 975
		// (remove) Token: 0x060003D0 RID: 976
		event ObjectMovedEventHandler EObjectMoved;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x060003D1 RID: 977
		// (remove) Token: 0x060003D2 RID: 978
		event ObjectPropertyModifiedEventHandler EObjectPropertyModified;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x060003D3 RID: 979
		// (remove) Token: 0x060003D4 RID: 980
		event PromptEncryptionPasswordEventHandler EPromptEncryptionPassword;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x060003D5 RID: 981
		// (remove) Token: 0x060003D6 RID: 982
		event ObjectEventHandler EObjectExported;

		// Token: 0x14000019 RID: 25
		// (add) Token: 0x060003D7 RID: 983
		// (remove) Token: 0x060003D8 RID: 984
		event ProjectLoadFinishedEventHandler EProjectLoadFinished;

		// Token: 0x1400001A RID: 26
		// (add) Token: 0x060003D9 RID: 985
		// (remove) Token: 0x060003DA RID: 986
		event EventHandler<LossOfDataWarningEventArgs> ELossOfDataWarning;

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x060003DB RID: 987
		// (remove) Token: 0x060003DC RID: 988
		event EventHandler<UnserializableDataErrorEventArgs> EUnserializableDataError;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x060003DD RID: 989
		// (remove) Token: 0x060003DE RID: 990
		event PromptEncryptionPasswordEventHandler EProvideEncryptionPassword;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x060003DF RID: 991
		// (remove) Token: 0x060003E0 RID: 992
		event EventHandler<UnknownDataWarningEventArgs> EUnknownDataWarning;
	}
}
