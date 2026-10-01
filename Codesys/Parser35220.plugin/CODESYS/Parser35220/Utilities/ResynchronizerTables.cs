using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x0200001F RID: 31
	internal static class ResynchronizerTables
	{
		// Token: 0x0600022E RID: 558 RVA: 0x0000C72C File Offset: 0x0000A92C
		static ResynchronizerTables()
		{
			ResynchronizerTables.InitResynchronizationTables();
		}

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x0600022F RID: 559 RVA: 0x0000C733 File Offset: 0x0000A933
		// (set) Token: 0x06000230 RID: 560 RVA: 0x0000C73A File Offset: 0x0000A93A
		internal static Operator[] ReSyncTableST { get; private set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000231 RID: 561 RVA: 0x0000C742 File Offset: 0x0000A942
		// (set) Token: 0x06000232 RID: 562 RVA: 0x0000C749 File Offset: 0x0000A949
		internal static Operator[] ReSyncTableIF { get; private set; }

		// Token: 0x06000233 RID: 563 RVA: 0x0000C754 File Offset: 0x0000A954
		private static void InitResynchronizationTables()
		{
			ResynchronizerTables.ReSyncTableST = new Operator[23];
			ResynchronizerTables.ReSyncTableST[0] = 89;
			ResynchronizerTables.ReSyncTableST[1] = 101;
			ResynchronizerTables.ReSyncTableST[2] = 68;
			ResynchronizerTables.ReSyncTableST[3] = 69;
			ResynchronizerTables.ReSyncTableST[4] = 75;
			ResynchronizerTables.ReSyncTableST[5] = 172;
			ResynchronizerTables.ReSyncTableST[6] = 85;
			ResynchronizerTables.ReSyncTableST[7] = 102;
			ResynchronizerTables.ReSyncTableST[8] = 64;
			ResynchronizerTables.ReSyncTableST[9] = 67;
			ResynchronizerTables.ReSyncTableST[10] = 72;
			ResynchronizerTables.ReSyncTableST[11] = 65;
			ResynchronizerTables.ReSyncTableST[12] = 90;
			ResynchronizerTables.ReSyncTableST[13] = 71;
			ResynchronizerTables.ReSyncTableST[14] = 115;
			ResynchronizerTables.ReSyncTableST[15] = 82;
			ResynchronizerTables.ReSyncTableST[16] = 96;
			ResynchronizerTables.ReSyncTableST[17] = 77;
			ResynchronizerTables.ReSyncTableST[18] = 104;
			ResynchronizerTables.ReSyncTableST[19] = 163;
			ResynchronizerTables.ReSyncTableST[20] = 83;
			ResynchronizerTables.ReSyncTableST[21] = 98;
			ResynchronizerTables.ReSyncTableST[22] = 164;
			ResynchronizerTables.ReSyncTableIF = new Operator[12];
			ResynchronizerTables.ReSyncTableIF[0] = 172;
			ResynchronizerTables.ReSyncTableIF[1] = 81;
			ResynchronizerTables.ReSyncTableIF[2] = 105;
			ResynchronizerTables.ReSyncTableIF[3] = 106;
			ResynchronizerTables.ReSyncTableIF[4] = 107;
			ResynchronizerTables.ReSyncTableIF[5] = 108;
			ResynchronizerTables.ReSyncTableIF[6] = 109;
			ResynchronizerTables.ReSyncTableIF[7] = 111;
			ResynchronizerTables.ReSyncTableIF[8] = 110;
			ResynchronizerTables.ReSyncTableIF[9] = 112;
			ResynchronizerTables.ReSyncTableIF[10] = 114;
			ResynchronizerTables.ReSyncTableIF[11] = 244;
		}
	}
}
