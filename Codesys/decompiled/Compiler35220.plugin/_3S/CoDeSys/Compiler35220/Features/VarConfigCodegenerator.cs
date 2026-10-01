using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using \u0003;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Features
{
	// Token: 0x020001F2 RID: 498
	public class VarConfigCodegenerator : IVarConfigCodeGenerator
	{
		// Token: 0x17000662 RID: 1634
		// (get) Token: 0x060021C7 RID: 8647 RVA: 0x00074648 File Offset: 0x00072848
		// (set) Token: 0x060021C8 RID: 8648 RVA: 0x00074650 File Offset: 0x00072850
		private Guid ApplicationGuid { get; set; }

		// Token: 0x17000663 RID: 1635
		// (get) Token: 0x060021C9 RID: 8649 RVA: 0x0007465C File Offset: 0x0007285C
		// (set) Token: 0x060021CA RID: 8650 RVA: 0x00074664 File Offset: 0x00072864
		private ILanguageModelList LMList { get; set; }

		// Token: 0x17000664 RID: 1636
		// (get) Token: 0x060021CB RID: 8651 RVA: 0x00074670 File Offset: 0x00072870
		// (set) Token: 0x060021CC RID: 8652 RVA: 0x00074678 File Offset: 0x00072878
		private _ICompileContext CompileContext { get; set; }

		// Token: 0x17000665 RID: 1637
		// (get) Token: 0x060021CD RID: 8653 RVA: 0x00074684 File Offset: 0x00072884
		// (set) Token: 0x060021CE RID: 8654 RVA: 0x0007468C File Offset: 0x0007288C
		private ITaskInfo[] Taskinfos { get; set; }

		// Token: 0x17000666 RID: 1638
		// (get) Token: 0x060021CF RID: 8655 RVA: 0x00074698 File Offset: 0x00072898
		// (set) Token: 0x060021D0 RID: 8656 RVA: 0x000746A0 File Offset: 0x000728A0
		private LList<\u0080.\u0011>[] InputInstances { get; set; }

		// Token: 0x17000667 RID: 1639
		// (get) Token: 0x060021D1 RID: 8657 RVA: 0x000746AC File Offset: 0x000728AC
		// (set) Token: 0x060021D2 RID: 8658 RVA: 0x000746B4 File Offset: 0x000728B4
		private LList<\u0080.\u0011>[] MemoryInstances { get; set; }

		// Token: 0x17000668 RID: 1640
		// (get) Token: 0x060021D3 RID: 8659 RVA: 0x000746C0 File Offset: 0x000728C0
		// (set) Token: 0x060021D4 RID: 8660 RVA: 0x000746C8 File Offset: 0x000728C8
		private LList<\u0080.\u0011>[] OutputInstances { get; set; }

		// Token: 0x060021D5 RID: 8661 RVA: 0x000746D4 File Offset: 0x000728D4
		private IDirectVariable[] \u0001(LList<\u0080.\u0011> \u0002, ISignature[] \u0003)
		{
			IDirectVariable[] array = new IDirectVariable[\u0002.Count];
			for (int i = 0; i < \u0002.Count; i++)
			{
				string text = \u0002[i].Instance;
				ISignature u = \u0002[i].Signature;
				bool flag;
				string u2 = ((_IParser)new global::\u0011.\u0006(text)).ParseSTOperand(out flag).ToString().ToUpperInvariant();
				string u3 = null;
				LList<IVariable> llist;
				LList<ISignature> llist2;
				u3 = this.\u0001(u, u2, u3, out llist, out llist2);
				VarConfigCodegenerator.\u0001(\u0003, u2, u3, llist, llist2);
				if (llist.Count == 1)
				{
					array[i] = llist[0].Address;
				}
				else if (llist.Count == 0)
				{
					array[i] = null;
				}
				else
				{
					array[i] = llist[0].Address;
					for (int j = 0; j < llist.Count; j++)
					{
						_ISignature isignature = llist2[j] as _ISignature;
						_IVariable ivariable = llist[j] as _IVariable;
						string u4 = string.Format(global::\u0003.\u0006.\u0001(MessageId.Err_DuplicateVarConfig, new object[]
						{
							text
						}), Array.Empty<object>());
						isignature.AddError(\u0019.\u0003.\u0001(ivariable._SourcePosition, u4, Severity.Error, MessageId.Err_DuplicateVarConfig));
					}
				}
			}
			return array;
		}

		// Token: 0x060021D6 RID: 8662 RVA: 0x00074814 File Offset: 0x00072A14
		private static void \u0001(ISignature[] \u0002, string \u0003, string \u0004, LList<IVariable> \u0005, LList<ISignature> \u0006)
		{
			foreach (_ISignature isignature in \u0002.OfType<_ISignature>())
			{
				if (isignature.POUType == Operator.VarConfig)
				{
					foreach (_IVariable ivariable in isignature.AllVariables.OfType<_IVariable>())
					{
						if (ivariable.VersionedName.ToUpperInvariant() == \u0003 || ivariable.VersionedName.ToUpperInvariant() == \u0004)
						{
							\u0005.Add(ivariable);
							\u0006.Add(isignature);
						}
					}
				}
			}
		}

		// Token: 0x060021D7 RID: 8663 RVA: 0x000748D8 File Offset: 0x00072AD8
		private string \u0001(ISignature \u0002, string \u0003, string \u0004, out LList<IVariable> \u0005, out LList<ISignature> \u0006)
		{
			\u0005 = new LList<IVariable>();
			\u0006 = new LList<ISignature>();
			if (\u0002.POUType == Operator.VarGlobal)
			{
				\u0004 = \u0003.Substring(\u0003.IndexOf('.'));
			}
			else if (\u0002.POUType == Operator.FunctionBlock)
			{
				string stName = \u0003.Substring(0, \u0003.IndexOf('.'));
				ISignature[] array = this.CompileContext.CreateGlobalIScope().FindSignature(stName);
				if (array != null && array.Length == 1 && array[0].POUType == Operator.VarGlobal)
				{
					\u0004 = \u0003.Substring(\u0003.IndexOf('.'));
				}
			}
			return \u0004;
		}

		// Token: 0x060021D8 RID: 8664 RVA: 0x00074964 File Offset: 0x00072B64
		private static string \u0001(IDirectVariable \u0002, LDictionary<string, string> \u0003)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (\u0002 == null)
			{
				stringBuilder.Append("not_defined");
			}
			else
			{
				switch (\u0002.Location)
				{
				case DirectVariableLocation.Input:
					stringBuilder.Append('I');
					break;
				case DirectVariableLocation.Output:
					stringBuilder.Append('Q');
					break;
				case DirectVariableLocation.Memory:
					stringBuilder.Append('M');
					break;
				default:
					stringBuilder.Append("ERROR");
					break;
				}
				switch (\u0002.Size)
				{
				case DirectVariableSize.X:
					stringBuilder.Append('X');
					break;
				case DirectVariableSize.B:
					stringBuilder.Append('B');
					break;
				case DirectVariableSize.W:
					stringBuilder.Append('W');
					break;
				case DirectVariableSize.D:
					stringBuilder.Append('D');
					break;
				case DirectVariableSize.L:
					stringBuilder.Append('L');
					break;
				default:
					stringBuilder.Append("ERROR");
					break;
				}
				int[] components = \u0002.Components;
				for (int i = 0; i < components.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append("__");
					}
					stringBuilder.Append(components[i]);
				}
			}
			string text = stringBuilder.ToString();
			int num = 0;
			while (\u0003.ContainsKey(text))
			{
				text = string.Format("{0}_{1}", stringBuilder, num);
				num++;
			}
			\u0003[text] = text;
			return text;
		}

		// Token: 0x060021D9 RID: 8665 RVA: 0x00074AAC File Offset: 0x00072CAC
		public void GenerateVarConfigCode(Guid guidApplication, ILanguageModelList lmlist)
		{
			this.ApplicationGuid = guidApplication;
			this.LMList = lmlist;
			this.\u0001();
		}

		// Token: 0x060021DA RID: 8666 RVA: 0x00074AC4 File Offset: 0x00072CC4
		private bool \u0001()
		{
			this.CompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(this.ApplicationGuid) as _ICompileContext);
			if (this.CompileContext.IsDefined(CompilerDefines.IGNORE_VAR_CONFIG))
			{
				return true;
			}
			this.Taskinfos = this.CompileContext.AllTasks;
			this.InputInstances = new LList<\u0080.\u0011>[this.Taskinfos.Length];
			this.MemoryInstances = new LList<\u0080.\u0011>[this.Taskinfos.Length];
			this.OutputInstances = new LList<\u0080.\u0011>[this.Taskinfos.Length];
			for (int i = 0; i < this.Taskinfos.Length; i++)
			{
				this.InputInstances[i] = new LList<\u0080.\u0011>();
				this.MemoryInstances[i] = new LList<\u0080.\u0011>();
				this.OutputInstances[i] = new LList<\u0080.\u0011>();
			}
			this.\u0001();
			LList<ISignature> llist = new LList<ISignature>();
			foreach (ISignature signature in this.CompileContext._GVLSignatures)
			{
				if (signature.POUType == Operator.VarConfig)
				{
					llist.Add(signature);
				}
			}
			ISignature[] array = new ISignature[llist.Count];
			llist.CopyTo(array);
			this.\u0001(array);
			return true;
		}

		// Token: 0x060021DB RID: 8667 RVA: 0x00074C04 File Offset: 0x00072E04
		private void \u0001(ISignature[] \u0002)
		{
			for (int i = 0; i < this.Taskinfos.Length; i++)
			{
				LList<\u0080.\u0011> llist = this.InputInstances[i];
				LList<\u0080.\u0011> llist2 = this.OutputInstances[i];
				LList<\u0080.\u0011> llist3 = this.MemoryInstances[i];
				if (llist.Count != 0 || llist3.Count != 0 || llist2.Count != 0)
				{
					ITaskInfo u = this.Taskinfos[i];
					IDirectVariable[] array = this.\u0001(llist, \u0002);
					IDirectVariable[] array2 = this.\u0001(llist3, \u0002);
					IDirectVariable[] array3 = this.\u0001(llist2, \u0002);
					string[] array4 = new string[llist.Count];
					string[] array5 = new string[llist3.Count];
					string[] array6 = new string[llist2.Count];
					LDictionary<string, string> u2 = new LDictionary<string, string>();
					for (int j = 0; j < array.Length; j++)
					{
						array4[j] = VarConfigCodegenerator.\u0001(array[j], u2);
					}
					for (int k = 0; k < array2.Length; k++)
					{
						array5[k] = VarConfigCodegenerator.\u0001(array2[k], u2);
					}
					for (int l = 0; l < array3.Length; l++)
					{
						array6[l] = VarConfigCodegenerator.\u0001(array3[l], u2);
					}
					this.\u0002(llist, llist3, u, array, array2, array4, array5);
					this.\u0001(llist2, llist3, u, array2, array3, array5, array6);
				}
			}
		}

		// Token: 0x060021DC RID: 8668 RVA: 0x00074D48 File Offset: 0x00072F48
		private void \u0001(LList<\u0080.\u0011> \u0002, LList<\u0080.\u0011> \u0003, ITaskInfo \u0004, IDirectVariable[] \u0005, IDirectVariable[] \u0006, string[] \u0007, string[] \u0008)
		{
			if (\u0002.Count != 0 || \u0003.Count != 0)
			{
				string text = string.Format("__var__config__output__{0}", \u0004.TaskName);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("{implicit on}");
				lstringBuilder.AppendLine("FUNCTION " + text + " : BOOL");
				lstringBuilder.AppendLine("VAR");
				this.\u0001(\u0002, \u0006, \u0008, lstringBuilder);
				this.\u0001(\u0003, \u0005, \u0007, lstringBuilder);
				lstringBuilder.Append("END_VAR");
				lstringBuilder.Append("{implicit off}");
				_ISignature isignature = ParserHelper.\u0001(lstringBuilder.ToString(), true);
				isignature.SetFlag(SignatureFlag.Generated, true);
				isignature.CreateCompiledSignature(null, this.CompileContext, null, this.CompileContext.HasByteSupport()).ObjectGuid = Guid.NewGuid();
				LStringBuilder lstringBuilder2 = new LStringBuilder();
				lstringBuilder2.Append("{implicit on}");
				lstringBuilder2.Append("{nobp}");
				VarConfigCodegenerator.\u0002(\u0002, \u0006, \u0008, lstringBuilder2);
				VarConfigCodegenerator.\u0001(\u0003, \u0005, \u0007, lstringBuilder2);
				lstringBuilder2.Append("{bp}");
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
		}

		// Token: 0x060021DD RID: 8669 RVA: 0x00074F18 File Offset: 0x00073118
		private static void \u0001(LList<\u0080.\u0011> \u0002, IDirectVariable[] \u0003, string[] \u0004, LStringBuilder \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				string text = \u0002[i].Instance;
				if (\u0003[i] != null)
				{
					string text2 = \u0004[i];
					\u0005.AppendFormat("{0} := {1};", new object[]
					{
						text2,
						text
					});
				}
			}
		}

		// Token: 0x060021DE RID: 8670 RVA: 0x00074F6C File Offset: 0x0007316C
		private static void \u0002(LList<\u0080.\u0011> \u0002, IDirectVariable[] \u0003, string[] \u0004, LStringBuilder \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				string text = \u0002[i].Instance;
				if (\u0003[i] == null)
				{
					_ISignature isignature = \u0002[i].Signature as _ISignature;
					_IVariable ivariable = \u0002[i].Variable as _IVariable;
					string u = string.Format(global::\u0003.\u0006.\u0001(MessageId.Err_NoInstancePath, new object[]
					{
						text
					}), Array.Empty<object>());
					isignature.AddError(\u0019.\u0003.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_NoInstancePath));
				}
				else
				{
					string text2 = \u0004[i];
					\u0005.AppendFormat("{0} := {1};", new object[]
					{
						text2,
						text
					});
				}
			}
		}

		// Token: 0x060021DF RID: 8671 RVA: 0x0007502C File Offset: 0x0007322C
		private static void \u0003(LList<\u0080.\u0011> \u0002, IDirectVariable[] \u0003, string[] \u0004, LStringBuilder \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				string text = \u0002[i].Instance;
				if (\u0003[i] == null)
				{
					_ISignature isignature = \u0002[i].Signature as _ISignature;
					_IVariable ivariable = \u0002[i].Variable as _IVariable;
					string u = string.Format(global::\u0003.\u0006.\u0001(MessageId.Err_NoInstancePath, new object[]
					{
						text
					}), Array.Empty<object>());
					isignature.AddError(\u0019.\u0003.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_NoInstancePath));
				}
				else
				{
					string text2 = \u0004[i];
					\u0005.AppendFormat("{0} := {1};", new object[]
					{
						text,
						text2
					});
				}
			}
		}

		// Token: 0x060021E0 RID: 8672 RVA: 0x000750EC File Offset: 0x000732EC
		private static void \u0004(LList<\u0080.\u0011> \u0002, IDirectVariable[] \u0003, string[] \u0004, LStringBuilder \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				string text = \u0002[i].Instance;
				if (\u0003[i] == null)
				{
					_ISignature isignature = \u0002[i].Signature as _ISignature;
					_IVariable ivariable = \u0002[i].Variable as _IVariable;
					string u = string.Format(global::\u0003.\u0006.\u0001(MessageId.Err_NoInstancePath, new object[]
					{
						text
					}), Array.Empty<object>());
					isignature.AddError(\u0019.\u0003.\u0001(ivariable._SourcePosition, u, Severity.Error, MessageId.Err_NoInstancePath));
				}
				else
				{
					string text2 = \u0004[i];
					\u0005.AppendFormat("{0} := {1};", new object[]
					{
						text,
						text2
					});
				}
			}
		}

		// Token: 0x060021E1 RID: 8673 RVA: 0x000751AC File Offset: 0x000733AC
		private void \u0001(LList<\u0080.\u0011> \u0002, IDirectVariable[] \u0003, string[] \u0004, LStringBuilder \u0005)
		{
			for (int i = 0; i < \u0002.Count; i++)
			{
				_IVariable ivariable = (_IVariable)\u0002[i].Variable;
				_ISignature isignature = \u0002[i].Signature as _ISignature;
				IDirectVariable directVariable = \u0003[i];
				if (directVariable != null)
				{
					string arg = \u0004[i];
					IScope scope = this.CompileContext.CreateIScope(isignature.Id);
					\u0005.AppendLine(string.Format("{0} AT {1} : {2};", arg, directVariable, ivariable._Type.GetConstantString(scope)));
				}
			}
		}

		// Token: 0x060021E2 RID: 8674 RVA: 0x00075238 File Offset: 0x00073438
		private void \u0002(LList<\u0080.\u0011> \u0002, LList<\u0080.\u0011> \u0003, ITaskInfo \u0004, IDirectVariable[] \u0005, IDirectVariable[] \u0006, string[] \u0007, string[] \u0008)
		{
			if (\u0002.Count != 0 || \u0003.Count != 0)
			{
				string text = string.Format("__var__config__input__{0}", \u0004.TaskName);
				LStringBuilder lstringBuilder = new LStringBuilder();
				lstringBuilder.Append("{implicit on}");
				lstringBuilder.AppendLine("FUNCTION " + text + " : BOOL");
				lstringBuilder.AppendLine("VAR");
				this.\u0001(\u0002, \u0005, \u0007, lstringBuilder);
				this.\u0001(\u0003, \u0006, \u0008, lstringBuilder);
				lstringBuilder.Append("END_VAR");
				lstringBuilder.Append("{implicit off}");
				LStringBuilder lstringBuilder2 = new LStringBuilder();
				lstringBuilder2.Append("{implicit on}");
				lstringBuilder2.Append("{nobp}");
				VarConfigCodegenerator.\u0003(\u0002, \u0005, \u0007, lstringBuilder2);
				VarConfigCodegenerator.\u0004(\u0003, \u0006, \u0008, lstringBuilder2);
				lstringBuilder2.Append("{bp}");
				lstringBuilder2.Append("{implicit off}");
				StringWriter stringWriter = new StringWriter();
				XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
				xmlTextWriter.WriteStartElement("language-model");
				xmlTextWriter.WriteStartElement("pou");
				xmlTextWriter.WriteAttributeString("id", XmlConvert.ToString(Guid.NewGuid()));
				xmlTextWriter.WriteAttributeString("task-id", XmlConvert.ToString(\u0004.TaskGuid));
				xmlTextWriter.WriteAttributeString("slot", XmlConvert.ToString(20000));
				xmlTextWriter.WriteAttributeString("name", text);
				xmlTextWriter.WriteElementString("interface", lstringBuilder.ToString());
				xmlTextWriter.WriteElementString("body", lstringBuilder2.ToString());
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.Close();
				this.LMList.AddLanguageModel(stringWriter.ToString());
			}
		}

		// Token: 0x060021E3 RID: 8675 RVA: 0x000753CC File Offset: 0x000735CC
		private void \u0001()
		{
			foreach (_ISignature isignature in this.CompileContext.AllSignatures.OfType<_ISignature>())
			{
				if (isignature.GetFlag(SignatureFlag.ContainsVarConfig))
				{
					IEnumerable allVariables = isignature.AllVariables;
					IVariable[] u2;
					ISignature[] u3;
					string[] u = this.CompileContext.InstancePaths(isignature, out u2, out u3, true);
					foreach (_IVariable ivariable in allVariables.OfType<_IVariable>())
					{
						if (ivariable.Address != null && ivariable.Address.Incomplete)
						{
							this.\u0001(isignature, u2, u3, u, ivariable);
						}
					}
				}
			}
		}

		// Token: 0x060021E4 RID: 8676 RVA: 0x000754A8 File Offset: 0x000736A8
		private void \u0001(_ISignature \u0002, IVariable[] \u0003, ISignature[] \u0004, string[] \u0005, _IVariable \u0006)
		{
			for (int i = 0; i < \u0005.Length; i++)
			{
				byte[] array;
				if (\u0003.Length == 0 && \u0004.Length == 0)
				{
					array = StaticSignatureTaskReferenceDetector.\u0001(\u0006, \u0002, false, this.CompileContext);
				}
				else
				{
					array = StaticSignatureTaskReferenceDetector.\u0001(\u0003[i], \u0004[i], false, this.CompileContext);
				}
				string u = \u0005[i] + "." + \u0006.VersionedName;
				foreach (byte b in array)
				{
					\u0080.\u0011 u2 = new \u0080.\u0011
					{
						Instance = u,
						Signature = \u0002,
						Variable = \u0006
					};
					switch (\u0006.Address.Location)
					{
					case DirectVariableLocation.Input:
						this.InputInstances[(int)b].Add(u2);
						break;
					case DirectVariableLocation.Output:
						this.OutputInstances[(int)b].Add(u2);
						break;
					case DirectVariableLocation.Memory:
						this.MemoryInstances[(int)b].Add(u2);
						break;
					}
				}
			}
		}

		// Token: 0x040005CD RID: 1485
		[CompilerGenerated]
		private Guid \u0001;

		// Token: 0x040005CE RID: 1486
		[CompilerGenerated]
		private ILanguageModelList \u0001;

		// Token: 0x040005CF RID: 1487
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x040005D0 RID: 1488
		[CompilerGenerated]
		private ITaskInfo[] \u0001;

		// Token: 0x040005D1 RID: 1489
		[CompilerGenerated]
		private LList<\u0080.\u0011>[] \u0001;

		// Token: 0x040005D2 RID: 1490
		[CompilerGenerated]
		private LList<\u0080.\u0011>[] \u0002;

		// Token: 0x040005D3 RID: 1491
		[CompilerGenerated]
		private LList<\u0080.\u0011>[] \u0003;
	}
}
