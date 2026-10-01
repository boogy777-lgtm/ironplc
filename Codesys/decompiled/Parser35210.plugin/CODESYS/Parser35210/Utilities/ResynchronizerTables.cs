using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35210.Utilities
{
	internal static class ResynchronizerTables
	{
		internal static Operator[] ReSyncTableST { get; private set; }

		internal static Operator[] ReSyncTableIF { get; private set; }

		static ResynchronizerTables()
		{
			InitResynchronizationTables();
		}

		private static void InitResynchronizationTables()
		{
			ReSyncTableST = new Operator[23];
			ReSyncTableST[0] = Operator.If;
			ReSyncTableST[1] = Operator.Then;
			ReSyncTableST[2] = Operator.Else;
			ReSyncTableST[3] = Operator.Elsif;
			ReSyncTableST[4] = Operator.EndIf;
			ReSyncTableST[5] = Operator.Semicolon;
			ReSyncTableST[6] = Operator.For;
			ReSyncTableST[7] = Operator.To;
			ReSyncTableST[8] = Operator.By;
			ReSyncTableST[9] = Operator.Do;
			ReSyncTableST[10] = Operator.EndFor;
			ReSyncTableST[11] = Operator.Case;
			ReSyncTableST[12] = Operator.Of;
			ReSyncTableST[13] = Operator.EndCase;
			ReSyncTableST[14] = Operator.While;
			ReSyncTableST[15] = Operator.EndWhile;
			ReSyncTableST[16] = Operator.Repeat;
			ReSyncTableST[17] = Operator.EndRepeat;
			ReSyncTableST[18] = Operator.Until;
			ReSyncTableST[19] = Operator.Colon;
			ReSyncTableST[20] = Operator.Exit;
			ReSyncTableST[21] = Operator.Return;
			ReSyncTableST[22] = Operator.Assign;
			ReSyncTableIF = new Operator[12];
			ReSyncTableIF[0] = Operator.Semicolon;
			ReSyncTableIF[1] = Operator.EndVar;
			ReSyncTableIF[2] = Operator.Var;
			ReSyncTableIF[3] = Operator.VarAccess;
			ReSyncTableIF[4] = Operator.VarConfig;
			ReSyncTableIF[5] = Operator.VarExternal;
			ReSyncTableIF[6] = Operator.VarGlobal;
			ReSyncTableIF[7] = Operator.VarInOut;
			ReSyncTableIF[8] = Operator.VarInput;
			ReSyncTableIF[9] = Operator.VarOutput;
			ReSyncTableIF[10] = Operator.VarStat;
			ReSyncTableIF[11] = Operator.VarInst;
		}
	}
}
