using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000210 RID: 528
	internal sealed class CodeLocationInfo
	{
		// Token: 0x0600230A RID: 8970 RVA: 0x00078590 File Offset: 0x00076790
		private string \u0001()
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("[");
			bool flag = true;
			foreach (__CodeLocationInfoStruct u in this.\u0001)
			{
				lstringBuilder.Append(flag ? "(" : ",(");
				flag = false;
				this.\u0001(u, lstringBuilder);
				lstringBuilder.Append(")");
			}
			lstringBuilder.Append("]");
			return lstringBuilder.ToString();
		}

		// Token: 0x0600230B RID: 8971 RVA: 0x00078628 File Offset: 0x00076828
		private void \u0001(__CodeLocationInfoStruct \u0002, LStringBuilder \u0003)
		{
			\u0003.AppendFormat("\r\nusiAreaCodeLocation := {0},\r\nudiOffsetCodeLocation := {1},\r\nudiCodeSize := {2}\r\n", new object[]
			{
				\u0002.AreaCodeLocation,
				\u0002.OffsetCodeLocation,
				\u0002.CodeSize
			});
		}

		// Token: 0x0600230C RID: 8972 RVA: 0x00078668 File Offset: 0x00076868
		internal bool \u0001(_ICompileContext \u0002)
		{
			LDictionary<int, LList<__CodeLocationInfoStruct>> ldictionary = new LDictionary<int, LList<__CodeLocationInfoStruct>>();
			_IDataManager dataManager = \u0002.DataManager;
			CodeLocationInfo.\u0001(\u0002.GetAllCompiledPOUsEx(), dataManager, ldictionary);
			this.\u0001(ldictionary);
			this.\u0001.Sort(new Comparison<__CodeLocationInfoStruct>(CodeLocationInfo.<>c.<>9.\u0001));
			return this.\u0001.Count > 0;
		}

		// Token: 0x0600230D RID: 8973 RVA: 0x000786D0 File Offset: 0x000768D0
		private void \u0001(LDictionary<int, LList<__CodeLocationInfoStruct>> \u0002)
		{
			foreach (KeyValuePair<int, LList<__CodeLocationInfoStruct>> keyValuePair in \u0002)
			{
				keyValuePair.Value.Sort(new Comparison<__CodeLocationInfoStruct>(CodeLocationInfo.<>c.<>9.\u0002));
				__CodeLocationInfoStruct _CodeLocationInfoStruct = keyValuePair.Value[0];
				for (int i = 1; i < keyValuePair.Value.Count; i++)
				{
					__CodeLocationInfoStruct _CodeLocationInfoStruct2 = keyValuePair.Value[i];
					if (_CodeLocationInfoStruct2.OffsetCodeLocation - (_CodeLocationInfoStruct.OffsetCodeLocation + _CodeLocationInfoStruct.CodeSize) == 0U)
					{
						_CodeLocationInfoStruct.CodeSize += _CodeLocationInfoStruct2.CodeSize;
					}
					else
					{
						this.\u0001.Add(_CodeLocationInfoStruct);
						_CodeLocationInfoStruct = _CodeLocationInfoStruct2;
					}
				}
				this.\u0001.Add(_CodeLocationInfoStruct);
			}
		}

		// Token: 0x0600230E RID: 8974 RVA: 0x000787C4 File Offset: 0x000769C4
		private static void \u0001(IList<ICompiledPOU4> \u0002, _IDataManager \u0003, LDictionary<int, LList<__CodeLocationInfoStruct>> \u0004)
		{
			foreach (ICompiledPOU4 compiledPOU in \u0002)
			{
				if (!compiledPOU.GetFlag(CompiledPOUFlags.Blob) && !compiledPOU.GetFlag(CompiledPOUFlags.ConstBlob) && compiledPOU.CompiledCode != null && compiledPOU.CompiledCode.Location != null)
				{
					ICompiledCode compiledCode = compiledPOU.CompiledCode;
					if (\u0003.GetAreaByIndex((int)compiledCode.Location.Area).DataSegmentFlags != DataSegmentFlags.Code)
					{
						if (!\u0004.ContainsKey((int)compiledCode.Location.Area))
						{
							\u0004[(int)compiledCode.Location.Area] = new LList<__CodeLocationInfoStruct>();
						}
						__CodeLocationInfoStruct _CodeLocationInfoStruct = new __CodeLocationInfoStruct
						{
							AreaCodeLocation = compiledCode.Location.Area,
							OffsetCodeLocation = (uint)compiledCode.Location.Offset,
							CodeSize = (uint)compiledCode.CodeSize
						};
						\u0004[(int)compiledCode.Location.Area].Add(_CodeLocationInfoStruct);
					}
				}
			}
		}

		// Token: 0x0600230F RID: 8975 RVA: 0x000788DC File Offset: 0x00076ADC
		internal string \u0002()
		{
			return "\r\n{attribute 'hide'}\r\n{attribute 'qualified_only'}\r\nVAR_GLOBAL CONSTANT\r\n\t{attribute 'no_init'}" + string.Format("\r\n\tappCodeLocations : CODE_LOCATION := (udiInfoVersion := 0\r\n\t\t\t\t\t, udiNumPOUs := {0}\r\n\t\t\t\t\t, arPOUs := {1}\r\n\t\t\t\t\t, pPOUs := ADR(appCodeLocations.arPOUs)\r\n\t\t\t\t\t);\r\nEND_VAR\r\n", this.\u0001.Count, this.\u0001());
		}

		// Token: 0x06002310 RID: 8976 RVA: 0x00078908 File Offset: 0x00076B08
		internal string \u0003()
		{
			return string.Format("\r\nTYPE CODE_LOCATION :\r\nSTRUCT\r\n\tudiInfoVersion: UDINT;\r\n\tudiNumPOUs: UDINT;\r\n\r\n\tpPOUs : POINTER TO __SYSTEM.__CodeLocationStruct;\r\n\tarPOUs : ARRAY [0..{0}-1] OF __SYSTEM.__CodeLocationStruct;\r\nEND_STRUCT\r\nEND_TYPE\r\n", this.\u0001.Count);
		}

		// Token: 0x04000620 RID: 1568
		private readonly LList<__CodeLocationInfoStruct> \u0001 = new LList<__CodeLocationInfoStruct>();

		// Token: 0x04000621 RID: 1569
		private const string \u0001 = "\r\n{attribute 'hide'}\r\n{attribute 'qualified_only'}\r\nVAR_GLOBAL CONSTANT\r\n\t{attribute 'no_init'}";

		// Token: 0x04000622 RID: 1570
		private const string \u0002 = "\r\nTYPE CODE_LOCATION :\r\nSTRUCT\r\n\tudiInfoVersion: UDINT;\r\n\tudiNumPOUs: UDINT;\r\n\r\n\tpPOUs : POINTER TO __SYSTEM.__CodeLocationStruct;\r\n\tarPOUs : ARRAY [0..{0}-1] OF __SYSTEM.__CodeLocationStruct;\r\nEND_STRUCT\r\nEND_TYPE\r\n";

		// Token: 0x04000623 RID: 1571
		private const string \u0003 = "\r\n\tappCodeLocations : CODE_LOCATION := (udiInfoVersion := 0\r\n\t\t\t\t\t, udiNumPOUs := {0}\r\n\t\t\t\t\t, arPOUs := {1}\r\n\t\t\t\t\t, pPOUs := ADR(appCodeLocations.arPOUs)\r\n\t\t\t\t\t);\r\nEND_VAR\r\n";

		// Token: 0x04000624 RID: 1572
		private const string \u0004 = "\r\nusiAreaCodeLocation := {0},\r\nudiOffsetCodeLocation := {1},\r\nudiCodeSize := {2}\r\n";
	}
}
