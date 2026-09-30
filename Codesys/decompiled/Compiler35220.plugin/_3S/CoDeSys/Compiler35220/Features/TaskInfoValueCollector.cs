using System;
using \u0017;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001F5 RID: 501
	public static class TaskInfoValueCollector
	{
		// Token: 0x060021E6 RID: 8678 RVA: 0x000755B8 File Offset: 0x000737B8
		internal static TaskInfoValues \u0001(ITaskInfo \u0002, ICompileContext \u0003)
		{
			TaskInfoValues result = default(TaskInfoValues);
			result.bProfilingTask = false;
			result.dwVersion = -1;
			result.pszName = null;
			result.nPriority = -1;
			result.KindOf = string.Empty;
			result.bWatchdog = false;
			result.dwEventFunctionPointer = string.Empty;
			result.pszExternalEvent = string.Empty;
			result.dwTaskEntryFunctionPointer = string.Empty;
			result.dwWatchdogTime = -1;
			result.dwWatchdogSensitivity = -1;
			result.dwInterval = -1;
			result.nCoreConfigured = -1;
			ISignature signature = \u0003.GetSignature(APEnvironmentFacade.Instance.LMServiceProvider.TaskLMService.GeneratedTaskConfigStruct);
			if (signature != null)
			{
				_IStructureInitialization istructureInitialization = signature[APEnvironmentFacade.Instance.LMServiceProvider.TaskLMService.GetTaskMemberName(\u0002.TaskName)].Initial as _IStructureInitialization;
				if (istructureInitialization != null)
				{
					TaskInfoValueCollector.\u0001(\u0003, ref result, istructureInitialization);
				}
			}
			return result;
		}

		// Token: 0x060021E7 RID: 8679 RVA: 0x0007569C File Offset: 0x0007389C
		private static void \u0001(ICompileContext \u0002, ref TaskInfoValues \u0003, _IStructureInitialization \u0004)
		{
			IScope scope = \u0002.CreateGlobalIScope();
			foreach (_IAssignmentExpression iassignmentExpression in \u0004._CompoInits)
			{
				string text = iassignmentExpression.LValue.ToString();
				if (!(text == "pszName") && !(text == "pszExternalEvent"))
				{
					if (!(text == "KindOf"))
					{
						if (!(text == "dwEventFunctionPointer"))
						{
							if (text == "dwTaskEntryFunctionPointer")
							{
								\u0003.dwTaskEntryFunctionPointer = iassignmentExpression.RValue.ToString();
							}
						}
						else
						{
							\u0003.dwEventFunctionPointer = iassignmentExpression.RValue.ToString();
						}
					}
					else
					{
						\u0003.KindOf = iassignmentExpression.RValue.ToString();
					}
				}
				ILiteralValue literalValue = iassignmentExpression.RValue.Literal(scope);
				if (literalValue != null)
				{
					uint num = \u0019.\u0001(text);
					if (num <= 1437945240U)
					{
						if (num <= 927833504U)
						{
							if (num != 162297094U)
							{
								if (num == 927833504U)
								{
									if (text == "dwVersion")
									{
										bool flag;
										\u0003.dwVersion = literalValue.GetInt(out flag);
									}
								}
							}
							else if (text == "dwWatchdogTime")
							{
								bool flag;
								\u0003.dwWatchdogTime = literalValue.GetInt(out flag);
							}
						}
						else if (num != 1186385085U)
						{
							if (num == 1437945240U)
							{
								if (text == "bProfilingTask")
								{
									bool flag;
									\u0003.bProfilingTask = literalValue.GetBool(out flag);
								}
							}
						}
						else if (text == "dwInterval")
						{
							bool flag;
							\u0003.dwInterval = literalValue.GetInt(out flag);
						}
					}
					else if (num <= 2526292866U)
					{
						if (num != 1459327554U)
						{
							if (num == 2526292866U)
							{
								if (text == "dwWatchdogSensitivity")
								{
									bool flag;
									\u0003.dwWatchdogSensitivity = literalValue.GetInt(out flag);
								}
							}
						}
						else if (text == "nCoreConfigured")
						{
							bool flag;
							\u0003.nCoreConfigured = literalValue.GetInt(out flag);
						}
					}
					else if (num != 3284948949U)
					{
						if (num == 3732354370U)
						{
							if (text == "bWatchdog")
							{
								bool flag;
								\u0003.bWatchdog = literalValue.GetBool(out flag);
							}
						}
					}
					else if (text == "nPriority")
					{
						bool flag;
						\u0003.nPriority = literalValue.GetInt(out flag);
					}
				}
			}
		}
	}
}
