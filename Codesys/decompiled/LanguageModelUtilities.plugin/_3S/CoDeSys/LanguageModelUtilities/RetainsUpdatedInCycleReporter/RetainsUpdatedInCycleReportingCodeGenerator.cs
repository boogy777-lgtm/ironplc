using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities.RetainsUpdatedInCycleReporter
{
	internal class RetainsUpdatedInCycleReportingCodeGenerator
	{
		private const string CYCLECODEVARS = "VAR_INPUT\r\n\tptaskinfo: POINTER TO _IMPLICIT_TASK_INFO;\r\n\tpapplicationinfo: POINTER TO _IMPLICIT_APPLICATION_INFO;\r\nEND_VAR\r\n";

		private ICompileContext21 CompileContext { get; }

		private Guid ApplicationGuid { get; }

		private ILanguageModelList LMList { get; }

		private ITaskInfo[] Taskinfos { get; }

		private LList<LList<InstanceInformation>> RetainInstances { get; }

		private LStringBuilder ErrorMessages { get; }

		internal RetainsUpdatedInCycleReportingCodeGenerator(Guid applicationGuid, ILanguageModelList lmlist)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			CompileContext = (ICompileContext21)APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(applicationGuid);
			ApplicationGuid = applicationGuid;
			LMList = lmlist;
			Taskinfos = CompileContext.AllTasks;
			RetainInstances = new LList<LList<InstanceInformation>>(Taskinfos.Length);
			for (int i = 0; i < Taskinfos.Length; i++)
			{
				RetainInstances.Add(new LList<InstanceInformation>());
			}
			ErrorMessages = new LStringBuilder();
		}

		internal void GenerateCode()
		{
			AddLmForExternalFunctions();
			CollectRelevantInstancePaths();
			GenerateCycleCode();
		}

		private void AddLmForExternalFunctions()
		{
			Stream manifestResourceStream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("_3S.CoDeSys.LanguageModelUtilities.RetainsUpdatedInCycleReporter.LibraryRetainUpdatedInCycle.xml");
			if (manifestResourceStream != null)
			{
				string stLanguageModelContent = new StreamReader(manifestResourceStream).ReadToEnd();
				LMList.AddLanguageModel(stLanguageModelContent);
			}
		}

		private void GenerateCycleCode()
		{
			for (int i = 0; i < Taskinfos.Length; i++)
			{
				LList<InstanceInformation> val = RetainInstances.get_Item(i);
				if (val.get_Count() != 0)
				{
					ITaskInfo taskinfo = Taskinfos[i];
					WriteAtCycleEnd((IList<InstanceInformation>)val, taskinfo, (uint)i);
				}
			}
		}

		private void WriteAtCycleEnd(IList<InstanceInformation> alRetainInstances, ITaskInfo taskinfo, uint taskId)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected O, but got Unknown
			if (alRetainInstances.Count != 0)
			{
				string text = "__var__retain_persistent__updated__" + taskinfo.TaskName;
				LStringBuilder val = new LStringBuilder();
				val.Append("{implicit on}");
				val.AppendFormat("FUNCTION {0}\r\n", new object[1] { text });
				val.Append("VAR_INPUT\r\n\tptaskinfo: POINTER TO _IMPLICIT_TASK_INFO;\r\n\tpapplicationinfo: POINTER TO _IMPLICIT_APPLICATION_INFO;\r\nEND_VAR\r\n");
				val.Append("{implicit off}");
				LStringBuilder val2 = new LStringBuilder();
				val2.Append("{implicit on}");
				val2.Append("{nobp}");
				val2.Append("{noflow}");
				if (ErrorMessages.get_Length() > 0)
				{
					val2.Append(((object)ErrorMessages).ToString());
				}
				WriteOutputUsingFunction(alRetainInstances, val2, taskId);
				val2.Append("{implicit off}");
				StringWriter stringWriter = new StringWriter();
				XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
				xmlTextWriter.WriteStartElement("language-model");
				xmlTextWriter.WriteStartElement("pou");
				xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
				xmlTextWriter.WriteAttributeString("task-id", XmlConvert.ToString(taskinfo.TaskGuid));
				xmlTextWriter.WriteAttributeString("slot", XmlConvert.ToString(41000));
				xmlTextWriter.WriteAttributeString("name", text);
				xmlTextWriter.WriteElementString("interface", ((object)val).ToString());
				xmlTextWriter.WriteElementString("body", ((object)val2).ToString());
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.Close();
				LMList.AddLanguageModel(stringWriter.ToString());
			}
		}

		private static void WriteOutputUsingFunction(IEnumerable<InstanceInformation> alRetainInstances, LStringBuilder stbRetainWrite, uint taskId)
		{
			foreach (InstanceInformation alRetainInstance in alRetainInstances)
			{
				string instance = alRetainInstance.Instance;
				stbRetainWrite.AppendLine($"Retain__Updated({taskId}, ADR({instance}), XSIZEOF({instance}));");
			}
			stbRetainWrite.AppendLine($"Retain__Commit({taskId});");
		}

		private void CollectRelevantInstancePaths()
		{
			foreach (_ISignature item in CompileContext.AllSignatures.OfType<_ISignature>())
			{
				if (!item.GetFlag(SignatureFlag.ContainsRetain) && !item.GetFlag(SignatureFlag.ContainsPersistent) && item.POUType != Operator.VarGlobal)
				{
					continue;
				}
				IList<_IVariable> allVariables = item.AllVariables;
				IVariable[] varInstances;
				ISignature[] declaringSignatures;
				string[] stInstancePaths = (CompileContext as _ICompileContext).InstancePaths(item, out varInstances, out declaringSignatures, bWithNamespace: true);
				foreach (IVariable item2 in (IEnumerable<IVariable>)allVariables)
				{
					if ((item2.GetFlag(VarFlag.Retain) || item2.GetFlag(VarFlag.Persistent)) && (!item2.HasAttribute("retain_implicit_variable") || !APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 21, 40)))
					{
						CollectInstancePath(item, varInstances, declaringSignatures, stInstancePaths, item2);
					}
				}
			}
		}

		private void CollectInstancePath(_ISignature sign, IVariable[] varInstances, ISignature[] declaringSignatures, string[] stInstancePaths, IVariable var)
		{
			for (int i = 0; i < stInstancePaths.Length; i++)
			{
				byte[] array = ((varInstances.Length != 0 || declaringSignatures.Length != 0) ? CompileContext.GetTaskIds(varInstances[i], declaringSignatures[i], bWriteOnly: true) : CompileContext.GetTaskIds(var, sign, bWriteOnly: true));
				string instance = stInstancePaths[i] + "." + var.OrgName;
				if (array.Length == 0 && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(ApplicationGuid).AllTasks.Length != 0)
				{
					array = new byte[1] { 0 };
				}
				byte[] array2 = array;
				foreach (byte b in array2)
				{
					RetainInstances.get_Item((int)b).Add(new InstanceInformation
					{
						Instance = instance,
						Signature = sign,
						Variable = var
					});
				}
			}
		}
	}
}
