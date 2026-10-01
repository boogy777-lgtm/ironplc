using System;
using \u0007;
using \u0011;
using \u0016;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace \u0004
{
	// Token: 0x020003CB RID: 971
	internal static class \u0019
	{
		// Token: 0x060036EA RID: 14058 RVA: 0x000E0060 File Offset: 0x000DE260
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004)
		{
			if (CompilerServicesInternal.\u0001(\u0002.GetTargetSettings()))
			{
				return;
			}
			if (\u0002.IsDefined("global_init_in_cycle"))
			{
				Locator.\u0001(\u0019.\u0001(\u0002, \u0003, true), null, \u0002, \u0003);
			}
			string text = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(\u0002.ApplicationGuid, \u0002.SimulationMode);
			int num = text.IndexOf('.');
			if (num >= 0)
			{
				text = text.Remove(0, num + 1);
			}
			for (int i = 0; i < \u0002.TaskList.Count; i++)
			{
				ITaskInfo taskInfo = \u0002.TaskList[i];
				_ISignature isignature = \u0002[IdentifierConstants.GetCycleCode(taskInfo.TaskName)];
				_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(isignature.Id);
				int[] slotArray = \u0002.SlotPOUs.GetSlotArray(taskInfo.TaskGuid);
				ITargetSettings targetSettings = \u0002.GetTargetSettings();
				bool boolValue = global::\u0016.\u0004.CycleControlInIec.GetBoolValue(targetSettings);
				bool flag = Helper.\u0001(targetSettings);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("{implicit on}");
				lstringBuilder.Append("{nobp}");
				lstringBuilder.AppendLine(string.Format("__TaskSpecificInfoGVL.taskSpecificInfos[{0}].TaskIndex := INT#{1};", i, i));
				lstringBuilder.AppendLine(string.Format("__TaskSpecificInfoGVL.taskSpecificInfos[{0}].pTaskInfo := ptaskinfo;", i));
				lstringBuilder.AppendLine(string.Format("__TaskSpecificInfoGVL.taskEntryAddress[{0}] := ADR(ptaskinfo);", i));
				if (\u0002.IsDefined("global_init_in_cycle"))
				{
					lstringBuilder.AppendLine("global__init__x(true, pTaskInfo, pApplicationInfo);");
				}
				for (int j = 0; j < slotArray.Length; j++)
				{
					_ICompiledPOU[] taskSlotPOUs = \u0002.SlotPOUs.GetTaskSlotPOUs(taskInfo.TaskGuid, slotArray[j], \u0002);
					int num2 = slotArray[j];
					if (j == 0)
					{
						if (flag)
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle__2(0, {0}, hTaskInfo);", new object[]
							{
								num2 - 1
							});
						}
						else if (\u0004)
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle(0, {0}, '{1}', '{2}');", new object[]
							{
								num2 - 1,
								taskInfo.TaskName,
								text
							});
						}
						else
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle(0, {0}, \"{1}\", \"{2}\");", new object[]
							{
								num2 - 1,
								taskInfo.TaskName,
								text
							});
						}
					}
					else
					{
						int num3 = slotArray[j - 1];
						if (flag)
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle__2({0}, {1}, hTaskInfo);", new object[]
							{
								num3,
								num2 - 1
							});
						}
						else if (\u0004)
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle({0}, {1}, '{2}', '{3}');", new object[]
							{
								num3 + 1,
								num2 - 1,
								taskInfo.TaskName,
								text
							});
						}
						else
						{
							lstringBuilder.AppendFormat("__sys__rts__cycle({0}, {1}, \"{2}\", \"{3}\");", new object[]
							{
								num3 + 1,
								num2 - 1,
								taskInfo.TaskName,
								text
							});
						}
					}
					foreach (_ICompiledPOU icompiledPOU2 in taskSlotPOUs)
					{
						if (icompiledPOU2 != null)
						{
							if (!boolValue)
							{
								lstringBuilder.AppendFormat("{0}();", new object[]
								{
									icompiledPOU2.Name
								});
							}
							else
							{
								ISignature signature = \u0002[icompiledPOU2.SignatureId];
								if (signature.Inputs.Length == 2 && signature.Inputs[0].Name == "PTASKINFO" && signature.Inputs[1].Name == "PAPPLICATIONINFO")
								{
									lstringBuilder.AppendLine(icompiledPOU2.Name + "(pTaskInfo := pTaskInfo, pApplicationInfo := pApplicationInfo);");
								}
								else
								{
									lstringBuilder.AppendLine("if pApplicationInfo^.udState <> 2 then");
									lstringBuilder.AppendLine(icompiledPOU2.Name + "();");
									lstringBuilder.AppendLine("end_if");
								}
							}
						}
					}
				}
				int num4 = slotArray[slotArray.Length - 1];
				if (flag)
				{
					lstringBuilder.AppendFormat("__sys__rts__cycle__2({0}, {1}, hTaskInfo);", new object[]
					{
						num4,
						int.MaxValue
					});
				}
				else if (\u0004)
				{
					lstringBuilder.AppendFormat("__sys__rts__cycle({0}, {1}, '{2}', '{3}');", new object[]
					{
						num4 + 1,
						int.MaxValue,
						taskInfo.TaskName,
						text
					});
				}
				else
				{
					lstringBuilder.AppendFormat("__sys__rts__cycle({0}, {1}, \"{2}\", \"{3}\");", new object[]
					{
						num4 + 1,
						int.MaxValue,
						taskInfo.TaskName,
						text
					});
				}
				lstringBuilder.Append("{bp}");
				lstringBuilder.Append("{implicit off}");
				IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
				_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
				icompiledPOU.SetParseTree(istatement);
				ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, true, icompiledPOU);
				istatement.Accept(ivisit);
				TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002, true);
				istatement.Accept(ivisit2);
				ErrorVisitor errorVisitor = new ErrorVisitor();
				istatement.Accept(errorVisitor);
				Debug.\u0001(errorVisitor.MessageList.Count == 0);
				icompiledPOU.UpdateChecksum();
				icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			}
		}
	}
}
