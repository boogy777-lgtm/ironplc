using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using \u0003;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Features.RetainsInCycle
{
	// Token: 0x0200020E RID: 526
	public class RetainCycleCodegenerator
	{
		// Token: 0x1700067A RID: 1658
		// (get) Token: 0x060022F3 RID: 8947 RVA: 0x00077964 File Offset: 0x00075B64
		private ICompileContext CompileContext { get; }

		// Token: 0x1700067B RID: 1659
		// (get) Token: 0x060022F4 RID: 8948 RVA: 0x0007796C File Offset: 0x00075B6C
		private Guid ApplicationGuid { get; }

		// Token: 0x1700067C RID: 1660
		// (get) Token: 0x060022F5 RID: 8949 RVA: 0x00077974 File Offset: 0x00075B74
		private ILanguageModelList LMList { get; }

		// Token: 0x1700067D RID: 1661
		// (get) Token: 0x060022F6 RID: 8950 RVA: 0x0007797C File Offset: 0x00075B7C
		private ITaskInfo[] Taskinfos { get; }

		// Token: 0x1700067E RID: 1662
		// (get) Token: 0x060022F7 RID: 8951 RVA: 0x00077984 File Offset: 0x00075B84
		private LList<LList<\u0080.\u0011>> RetainInstances { get; }

		// Token: 0x1700067F RID: 1663
		// (get) Token: 0x060022F8 RID: 8952 RVA: 0x0007798C File Offset: 0x00075B8C
		private LStringBuilder ErrorMessages { get; }

		// Token: 0x060022F9 RID: 8953 RVA: 0x00077994 File Offset: 0x00075B94
		private RetainCycleCodegenerator(Guid applicationGuid, ILanguageModelList lmlist)
		{
			this.CompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(applicationGuid);
			this.ApplicationGuid = applicationGuid;
			this.LMList = lmlist;
			this.Taskinfos = this.CompileContext.AllTasks;
			this.RetainInstances = new LList<LList<\u0080.\u0011>>(this.Taskinfos.Length);
			for (int i = 0; i < this.Taskinfos.Length; i++)
			{
				this.RetainInstances.Add(new LList<\u0080.\u0011>());
			}
			this.ErrorMessages = new LStringBuilder();
		}

		// Token: 0x060022FA RID: 8954 RVA: 0x00077A20 File Offset: 0x00075C20
		private static string \u0001(string \u0002)
		{
			return \u0002.Replace(".", "__").Replace("[", "__").Replace("]", "__").Replace("#", "__");
		}

		// Token: 0x060022FB RID: 8955 RVA: 0x00077A60 File Offset: 0x00075C60
		public static void DoGenerateRetainInFBCode(Guid applicationGuid, ILanguageModelList lmlist)
		{
			new RetainCycleCodegenerator(applicationGuid, lmlist).\u0001();
		}

		// Token: 0x060022FC RID: 8956 RVA: 0x00077A70 File Offset: 0x00075C70
		private void \u0001()
		{
			this.\u0002();
			LDictionary<string, string> u = new LDictionary<string, string>();
			this.\u0001(u);
			this.\u0001(u);
		}

		// Token: 0x060022FD RID: 8957 RVA: 0x00077A98 File Offset: 0x00075C98
		private void \u0001(IDictionary<string, string> \u0002)
		{
			for (int i = 0; i < this.Taskinfos.Length; i++)
			{
				LList<\u0080.\u0011> llist = this.RetainInstances[i];
				if (llist.Count != 0)
				{
					ITaskInfo u = this.Taskinfos[i];
					this.\u0002(\u0002, llist, u);
					this.\u0001(\u0002, llist, u);
				}
			}
		}

		// Token: 0x060022FE RID: 8958 RVA: 0x00077AE8 File Offset: 0x00075CE8
		private void \u0001(IDictionary<string, string> \u0002, IList<\u0080.\u0011> \u0003, ITaskInfo \u0004)
		{
			if (\u0003.Count == 0)
			{
				return;
			}
			string text = string.Format("__var__retain__read__{0}", \u0004.TaskName);
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{attribute 'signature_flag' := '4294967296'}");
			lstringBuilder.Append("{attribute 'call_after_global_init_slot' := '199'}");
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.AppendLine("FUNCTION " + text);
			lstringBuilder.AppendFormat("VAR_INPUT bInitRetains : BOOL; END_VAR", new object[]
			{
				text
			});
			lstringBuilder.Append("{implicit off}");
			LStringBuilder lstringBuilder2 = new LStringBuilder();
			lstringBuilder2.Append("{implicit on}");
			lstringBuilder2.Append("{nobp}");
			lstringBuilder2.Append("{noflow}");
			lstringBuilder2.AppendLine("IF bInitRetains THEN __var__retain__write__" + \u0004.TaskName + "(0, 0); RETURN; END_IF");
			for (int i = 0; i < \u0003.Count; i++)
			{
				string text2 = \u0003[i].Instance;
				string text3 = \u0002[text2];
				lstringBuilder2.AppendFormat("{0} := {1};", new object[]
				{
					text2,
					text3
				});
			}
			lstringBuilder2.Append("{bp}");
			lstringBuilder2.Append("{flow}");
			lstringBuilder2.Append("{implicit off}");
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.WriteStartElement("language-model");
			xmlTextWriter.WriteStartElement("pou");
			xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
			xmlTextWriter.WriteAttributeString("task-id", XmlConvert.ToString(\u0004.TaskGuid));
			xmlTextWriter.WriteAttributeString("name", text);
			xmlTextWriter.WriteElementString("interface", lstringBuilder.ToString());
			xmlTextWriter.WriteElementString("body", lstringBuilder2.ToString());
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
			this.LMList.AddLanguageModel(stringWriter.ToString());
		}

		// Token: 0x060022FF RID: 8959 RVA: 0x00077CC4 File Offset: 0x00075EC4
		private void \u0002(IDictionary<string, string> \u0002, IList<\u0080.\u0011> \u0003, ITaskInfo \u0004)
		{
			if (\u0003.Count == 0)
			{
				return;
			}
			string text = "__var__retain__write__" + \u0004.TaskName;
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.Append("{implicit on}");
			lstringBuilder.AppendLine("FUNCTION " + text);
			lstringBuilder.Append("VAR_INPUT\r\n\tptaskinfo: POINTER TO _IMPLICIT_TASK_INFO;\r\n\tpapplicationinfo: POINTER TO _IMPLICIT_APPLICATION_INFO;\r\nEND_VAR\r\n");
			lstringBuilder.Append("{implicit off}");
			LStringBuilder lstringBuilder2 = new LStringBuilder();
			lstringBuilder2.Append("{implicit on}");
			lstringBuilder2.Append("{nobp}");
			lstringBuilder2.Append("{noflow}");
			if (this.ErrorMessages.Length > 0)
			{
				lstringBuilder2.Append(this.ErrorMessages.ToString());
			}
			foreach (\u0080.\u0011 u in \u0003)
			{
				string text2 = u.Instance;
				string text3 = \u0002[text2];
				lstringBuilder2.AppendFormat("{0} := {1};", new object[]
				{
					text3,
					text2
				});
			}
			lstringBuilder2.Append("{implicit off}");
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.WriteStartElement("language-model");
			xmlTextWriter.WriteStartElement("pou");
			xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
			xmlTextWriter.WriteAttributeString("task-id", XmlConvert.ToString(\u0004.TaskGuid));
			xmlTextWriter.WriteAttributeString("slot", XmlConvert.ToString(40000));
			xmlTextWriter.WriteAttributeString("name", text);
			xmlTextWriter.WriteElementString("interface", lstringBuilder.ToString());
			xmlTextWriter.WriteElementString("body", lstringBuilder2.ToString());
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.Close();
			this.LMList.AddLanguageModel(stringWriter.ToString());
		}

		// Token: 0x06002300 RID: 8960 RVA: 0x00077E98 File Offset: 0x00076098
		private void \u0001(LDictionary<string, string> \u0002)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("{implicit on}");
			lstringBuilder.AppendLine("{attribute 'do_retain'}");
			lstringBuilder.AppendLine("VAR_GLOBAL RETAIN");
			for (int i = 0; i < this.Taskinfos.Length; i++)
			{
				LList<\u0080.\u0011> llist = this.RetainInstances[i];
				for (int j = 0; j < llist.Count; j++)
				{
					if (!\u0002.ContainsKey(llist[j].Instance))
					{
						string text = RetainCycleCodegenerator.\u0001(llist[j].Instance);
						_IVariable ivariable = llist[j].Variable as _IVariable;
						ivariable.AddAttribute("retain_implicit_variable", text);
						ISignature signature = llist[j].Signature;
						IScope u = this.CompileContext.CreateIScope(signature.Id);
						\u0002[llist[j].Instance] = text;
						string str = RetainCycleCodegenerator.\u0001(ivariable, signature, u);
						lstringBuilder.AppendLine(text + ": " + str + ";");
					}
				}
			}
			lstringBuilder.Append("END_VAR");
			lstringBuilder.Append("{implicit off}");
			if (\u0002.Keys.Count > 0)
			{
				string value = "__var__retain__definitions";
				StringWriter stringWriter = new StringWriter();
				XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
				xmlTextWriter.WriteStartElement("language-model");
				xmlTextWriter.WriteStartElement("global-interface");
				xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
				xmlTextWriter.WriteAttributeString("name", value);
				xmlTextWriter.WriteElementString("interface", lstringBuilder.ToString());
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.Close();
				this.LMList.AddLanguageModel(stringWriter.ToString());
			}
		}

		// Token: 0x06002301 RID: 8961 RVA: 0x00078070 File Offset: 0x00076270
		private static string \u0001(_IVariable \u0002, ISignature \u0003, IScope \u0004)
		{
			if (\u0002._Type is IEnumType)
			{
				_IVariable ivariable = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(\u0003).GetSignature(\u0003.ObjectGuid)[\u0002.Name] as _IVariable;
				if (ivariable != null)
				{
					IUserdefType userdefType = ivariable._Type as IUserdefType;
					if (userdefType != null)
					{
						return userdefType.ToString();
					}
				}
			}
			return \u0002._Type.GetConstantString(\u0004);
		}

		// Token: 0x06002302 RID: 8962 RVA: 0x000780E0 File Offset: 0x000762E0
		private void \u0002()
		{
			foreach (_ISignature isignature in this.CompileContext.AllSignatures.OfType<_ISignature>())
			{
				if (isignature.GetFlag(SignatureFlag.ContainsRetain) || isignature.POUType == Operator.VarGlobal)
				{
					IEnumerable<IVariable> allVariables = isignature.AllVariables;
					IVariable[] u;
					ISignature[] u2;
					string[] array = (this.CompileContext as _ICompileContext).InstancePaths(isignature, out u, out u2, true);
					foreach (IVariable variable in allVariables)
					{
						if (variable.GetFlag(VarFlag.Retain) && !variable.GetFlag(VarFlag.Persistent))
						{
							this.\u0001(isignature, array, variable);
							this.\u0001(isignature, u, u2, array, variable);
						}
					}
				}
			}
		}

		// Token: 0x06002303 RID: 8963 RVA: 0x000781DC File Offset: 0x000763DC
		private void \u0001(_ISignature \u0002, IVariable[] \u0003, ISignature[] \u0004, string[] \u0005, IVariable \u0006)
		{
			for (int i = 0; i < \u0005.Length; i++)
			{
				byte[] array;
				if (\u0003.Length == 0 && \u0004.Length == 0)
				{
					array = StaticSignatureTaskReferenceDetector.\u0001(\u0006, \u0002, true, this.CompileContext);
				}
				else
				{
					array = StaticSignatureTaskReferenceDetector.\u0001(\u0003[i], \u0004[i], true, this.CompileContext);
				}
				string u = \u0005[i] + "." + \u0006.OrgName;
				if (array.Length == 0)
				{
					ITaskInfo[] allTasks = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(this.ApplicationGuid).AllTasks;
					if (allTasks.Length != 0)
					{
						array = new byte[]
						{
							0
						};
					}
					this.\u0001(array, allTasks);
				}
				foreach (byte b in array)
				{
					this.RetainInstances[(int)b].Add(new \u0080.\u0011
					{
						Instance = u,
						Signature = \u0002,
						Variable = \u0006
					});
				}
			}
		}

		// Token: 0x06002304 RID: 8964 RVA: 0x000782CC File Offset: 0x000764CC
		private void \u0001(byte[] \u0002, ITaskInfo[] \u0003)
		{
			string retainCycleTask = (this.CompileContext as _ICompileContext).RetainCycleTask;
			if (retainCycleTask != string.Empty)
			{
				try
				{
					for (int i = 0; i < \u0003.Length; i++)
					{
						if (string.Compare(\u0003[i].TaskName, retainCycleTask, StringComparison.OrdinalIgnoreCase) == 0)
						{
							\u0002[0] = (byte)i;
						}
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06002305 RID: 8965 RVA: 0x00078330 File Offset: 0x00076530
		private void \u0001(_ISignature \u0002, string[] \u0003, IVariable \u0004)
		{
			if (!string.IsNullOrEmpty(\u0002.LibraryPath) && \u0003.Length != 0 && \u0002.POUType == Operator.VarGlobal)
			{
				IScope u = this.CompileContext.CreateGlobalIScope();
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0002.LibraryPath);
				if (libraryContext != null)
				{
					string text = Helper.\u0001(this.CompileContext as _ICompileContext, libraryContext);
					if (text != null)
					{
						text = text.Split(new char[]
						{
							'.'
						})[0];
					}
					this.\u0001(\u0003, \u0004, u, text);
				}
			}
		}

		// Token: 0x06002306 RID: 8966 RVA: 0x000783B0 File Offset: 0x000765B0
		private void \u0001(string[] \u0002, IVariable \u0003, IScope \u0004, string \u0005)
		{
			if (!string.IsNullOrEmpty(\u0005))
			{
				IVariable[] array;
				ISignature[] array2;
				IScope scope;
				\u0004.FindDeclaration(\u0005, out array, out array2, out scope);
				if (array != null && array.Length != 0 && array2 != null && array2.Length != 0)
				{
					this.\u0001(\u0002, \u0003, array, array2);
					return;
				}
				if (array2 != null && array2.Length != 0)
				{
					this.\u0001(\u0002, \u0003, array2);
				}
			}
		}

		// Token: 0x06002307 RID: 8967 RVA: 0x00078400 File Offset: 0x00076600
		private void \u0001(string[] \u0002, IVariable \u0003, ISignature[] \u0004)
		{
			string str = string.Format("messageguid '{0}'", \u0004[0].ObjectGuid.ToString());
			this.ErrorMessages.AppendLine("{" + str + "}");
			this.ErrorMessages.Append("{error '");
			string text = global::\u0003.\u0006.\u0001(MessageId.Err_RetainNotAccessiblePOU, new object[]
			{
				\u0004[0].Name,
				\u0002[0] + "." + \u0003.OrgName
			});
			this.ErrorMessages.Append(text);
			this.ErrorMessages.Append("'}");
		}

		// Token: 0x06002308 RID: 8968 RVA: 0x000784AC File Offset: 0x000766AC
		private void \u0001(string[] \u0002, IVariable \u0003, IVariable[] \u0004, ISignature[] \u0005)
		{
			string str = string.Format("messageguid '{0}'", \u0005[0].ObjectGuid.ToString());
			this.ErrorMessages.AppendLine("{" + str + "}");
			this.ErrorMessages.AppendLine("{p " + \u0004[0].SourcePosition.Position.ToString() + "}");
			this.ErrorMessages.Append("{error '");
			string text = global::\u0003.\u0006.\u0001(MessageId.Err_RetainNotAccessibleVariable, new object[]
			{
				\u0004[0].Name,
				\u0002[0] + "." + \u0003.OrgName
			});
			this.ErrorMessages.Append(text);
			this.ErrorMessages.Append("'}");
		}

		// Token: 0x0400061A RID: 1562
		[CompilerGenerated]
		private readonly ICompileContext \u0001;

		// Token: 0x0400061B RID: 1563
		[CompilerGenerated]
		private readonly Guid \u0001;

		// Token: 0x0400061C RID: 1564
		[CompilerGenerated]
		private readonly ILanguageModelList \u0001;

		// Token: 0x0400061D RID: 1565
		[CompilerGenerated]
		private readonly ITaskInfo[] \u0001;

		// Token: 0x0400061E RID: 1566
		[CompilerGenerated]
		private readonly LList<LList<\u0080.\u0011>> \u0001;

		// Token: 0x0400061F RID: 1567
		[CompilerGenerated]
		private readonly LStringBuilder \u0001;
	}
}
