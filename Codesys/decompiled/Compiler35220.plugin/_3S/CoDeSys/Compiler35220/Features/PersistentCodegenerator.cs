using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using \u0003;
using \u000F;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001F1 RID: 497
	public class PersistentCodegenerator
	{
		// Token: 0x1700065F RID: 1631
		// (get) Token: 0x060021BC RID: 8636 RVA: 0x000741B0 File Offset: 0x000723B0
		private ICompileContext CompileContext { get; }

		// Token: 0x17000660 RID: 1632
		// (get) Token: 0x060021BD RID: 8637 RVA: 0x000741B8 File Offset: 0x000723B8
		private ILanguageModelList LMList { get; }

		// Token: 0x17000661 RID: 1633
		// (get) Token: 0x060021BE RID: 8638 RVA: 0x000741C0 File Offset: 0x000723C0
		private \u001E.\u0008 PersistenceInformation { get; }

		// Token: 0x060021BF RID: 8639 RVA: 0x000741C8 File Offset: 0x000723C8
		private PersistentCodegenerator(Guid applicationGuid, ILanguageModelList lmlist)
		{
			this.LMList = lmlist;
			this.CompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(applicationGuid);
			this.PersistenceInformation = new \u001E.\u0008();
		}

		// Token: 0x060021C0 RID: 8640 RVA: 0x000741F8 File Offset: 0x000723F8
		private PersistentCodegenerator(ICompileContext comcon)
		{
			this.LMList = null;
			this.CompileContext = comcon;
			this.PersistenceInformation = new \u001E.\u0008();
		}

		// Token: 0x060021C1 RID: 8641 RVA: 0x0007421C File Offset: 0x0007241C
		public static bool DoGeneratePersistentInFBCode(Guid applicationGuid, ILanguageModelList lmlist)
		{
			return new PersistentCodegenerator(applicationGuid, lmlist).\u0001();
		}

		// Token: 0x060021C2 RID: 8642 RVA: 0x0007422C File Offset: 0x0007242C
		internal static void \u0001(ICompileContext \u0002)
		{
			new PersistentCodegenerator(\u0002).\u0002();
		}

		// Token: 0x060021C3 RID: 8643 RVA: 0x0007423C File Offset: 0x0007243C
		private void \u0001()
		{
			new global::\u000F.\u000F(this.CompileContext).\u0002(this.PersistenceInformation);
		}

		// Token: 0x060021C4 RID: 8644 RVA: 0x00074254 File Offset: 0x00072454
		private void \u0002()
		{
			this.\u0001();
			foreach (global::\u0019.\u0005 u in this.PersistenceInformation.Instances)
			{
				foreach (global::\u0003.\u000E u2 in u.Instances)
				{
					this.\u0001(u2);
				}
			}
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x000742E0 File Offset: 0x000724E0
		private bool \u0001()
		{
			this.\u0001();
			ITaskInfo[] allTasks = this.CompileContext.AllTasks;
			for (int i = 0; i < allTasks.Length; i++)
			{
				global::\u0019.\u0005 u = this.PersistenceInformation.\u0001((byte)i);
				if (u.Count != 0)
				{
					ITaskInfo taskInfo = allTasks[i];
					if (u.Count != 0)
					{
						string text = string.Format("__var__persistent__write__{0}", taskInfo.TaskName);
						LStringBuilder lstringBuilder = new LStringBuilder();
						lstringBuilder.Append("{implicit on}");
						lstringBuilder.AppendLine("FUNCTION " + text);
						lstringBuilder.Append("VAR_INPUT\r\n\tptaskinfo: POINTER TO _IMPLICIT_TASK_INFO;\r\n\tpapplicationinfo: POINTER TO _IMPLICIT_APPLICATION_INFO;\r\nEND_VAR\r\n");
						lstringBuilder.Append("{implicit off}");
						LStringBuilder lstringBuilder2 = new LStringBuilder();
						lstringBuilder2.Append("{implicit on}");
						lstringBuilder2.Append("{nobp}");
						lstringBuilder2.Append("{noflow}");
						for (int j = 0; j < u.Count; j++)
						{
							global::\u0003.\u000E u000E = u[j];
							string text2 = this.PersistenceInformation.\u0001(u000E);
							if (text2 != null)
							{
								lstringBuilder2.Append(string.Format("{{nobp}}{{messageguid '{0}'}}", this.PersistenceInformation.Signature.MessageGuid));
								IVariable variable = this.PersistenceInformation.Signature[text2];
								string text3 = string.Format("{{p {0}}}", variable.SourcePosition.PositionCombination);
								lstringBuilder2.AppendLine(string.Concat(new string[]
								{
									text3,
									this.PersistenceInformation.Signature.OrgName,
									".",
									text2,
									" := ",
									u000E.InstancePath,
									";"
								}));
							}
						}
						lstringBuilder2.Append("{bp}");
						lstringBuilder2.Append("{flow}");
						lstringBuilder2.Append("{implicit off}");
						StringWriter stringWriter = new StringWriter();
						XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
						xmlTextWriter.WriteStartElement("language-model");
						xmlTextWriter.WriteStartElement("pou");
						xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
						xmlTextWriter.WriteAttributeString("task-id", XmlConvert.ToString(taskInfo.TaskGuid));
						xmlTextWriter.WriteAttributeString("slot", XmlConvert.ToString(40000));
						xmlTextWriter.WriteAttributeString("name", text);
						xmlTextWriter.WriteElementString("interface", lstringBuilder.ToString());
						xmlTextWriter.WriteElementString("body", lstringBuilder2.ToString());
						xmlTextWriter.WriteEndElement();
						xmlTextWriter.WriteEndElement();
						xmlTextWriter.Close();
						this.LMList.AddLanguageModel(stringWriter.ToString());
					}
				}
			}
			return true;
		}

		// Token: 0x060021C6 RID: 8646 RVA: 0x00074584 File Offset: 0x00072784
		private void \u0001(global::\u0003.\u000E \u0002)
		{
			if (this.PersistenceInformation.\u0001(\u0002) == null)
			{
				MessageId messageId;
				string u;
				if (\u0002.IsStackVariable)
				{
					messageId = MessageId.Wrn_PersistentVariableOnStack;
					u = global::\u0003.\u0006.\u0001(messageId, new object[]
					{
						\u0002.InstancePath
					});
				}
				else
				{
					messageId = MessageId.Wrn_MissingInstancePathForPersistent;
					u = global::\u0003.\u0006.\u0001(messageId, new object[]
					{
						\u0002.InstancePath
					});
					if (this.PersistenceInformation.Signature == null)
					{
						u = global::\u0003.\u0006.\u0001(MessageId.Wrn_MissingObjectForPersistent, new object[]
						{
							\u0002.InstancePath
						});
					}
				}
				Severity u2 = Severity.Warning;
				if (\u0002.Variable != null)
				{
					u2 = Messages.\u0001((_IVariable)\u0002.Variable, messageId);
				}
				\u0002.Signature.AddError(global::\u0019.\u0003.\u0001(\u0002.Variable.SourcePosition, u, u2, messageId));
			}
		}

		// Token: 0x040005CA RID: 1482
		[CompilerGenerated]
		private readonly ICompileContext \u0001;

		// Token: 0x040005CB RID: 1483
		[CompilerGenerated]
		private readonly ILanguageModelList \u0001;

		// Token: 0x040005CC RID: 1484
		[CompilerGenerated]
		private readonly \u001E.\u0008 \u0001;
	}
}
