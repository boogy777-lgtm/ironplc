using System;
using System.Xml;
using \u0004;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x0200015A RID: 346
	internal sealed class \u0007
	{
		// Token: 0x06001818 RID: 6168 RVA: 0x0004A8C4 File Offset: 0x00048AC4
		internal \u0007(_ICompileContext \u0001\u0002, ILanguageModelList \u0090\u0005, _ICompileContext \u0091\u0005)
		{
			this.\u0001 = \u0001\u0002;
			this.\u0001 = \u0090\u0005;
			this.\u0002 = \u0091\u0005;
		}

		// Token: 0x06001819 RID: 6169 RVA: 0x0004A8E4 File Offset: 0x00048AE4
		internal bool \u0001()
		{
			this.\u0001 = true;
			foreach (string xml in (this.\u0001 as _ILanguageModelList).LanguageModels)
			{
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.LoadXml(xml);
				}
				catch (Exception ex)
				{
					Debug.\u0001(false, ex.ToString());
					continue;
				}
				XmlNode xmlNode = xmlDocument["language-model"];
				if (xmlNode != null)
				{
					foreach (object obj in xmlNode.ChildNodes)
					{
						XmlNode xmlNode2 = (XmlNode)obj;
						int u = -1;
						int u2 = -1;
						int u3 = -1;
						Guid empty = Guid.Empty;
						_ISignature isignature = null;
						IScope5 scope = null;
						string name = xmlNode2.Name;
						if (!(name == "pou"))
						{
							if (!(name == "method") && !(name == "action"))
							{
								if (!(name == "global-interface"))
								{
									if (name == "data-type")
									{
										this.\u0002(xmlNode2, out isignature, out scope);
									}
								}
								else
								{
									this.\u0001(xmlNode2, out isignature, out scope);
								}
							}
							else
							{
								this.\u0001(xmlNode2, u, u2, u3, empty, ref isignature, ref scope);
							}
						}
						else
						{
							global::\u0017.\u0007.\u0001(xmlNode2, ref u, ref u2, ref u3, ref empty);
							this.\u0001(xmlNode2, u, u2, u3, empty, ref isignature, ref scope);
						}
					}
				}
			}
			return this.\u0001;
		}

		// Token: 0x0600181A RID: 6170 RVA: 0x0004AAB8 File Offset: 0x00048CB8
		private static void \u0001(XmlNode \u0002, ref int \u0003, ref int \u0004, ref int \u0005, ref Guid \u0006)
		{
			XmlAttribute xmlAttribute = \u0002.Attributes["slot"];
			if (xmlAttribute != null)
			{
				\u0003 = XmlConvert.ToInt32(xmlAttribute.InnerText);
			}
			XmlAttribute xmlAttribute2 = \u0002.Attributes["download_slot"];
			if (xmlAttribute2 != null)
			{
				\u0004 = XmlConvert.ToInt32(xmlAttribute2.InnerText);
			}
			XmlAttribute xmlAttribute3 = \u0002.Attributes["online_change_slot"];
			if (xmlAttribute3 != null)
			{
				\u0005 = XmlConvert.ToInt32(xmlAttribute3.InnerText);
			}
			XmlAttribute xmlAttribute4 = \u0002.Attributes["task-id"];
			if (xmlAttribute4 != null)
			{
				\u0006 = new Guid(xmlAttribute4.InnerText);
			}
		}

		// Token: 0x0600181B RID: 6171 RVA: 0x0004AB50 File Offset: 0x00048D50
		private void \u0001(XmlNode \u0002, int \u0003, int \u0004, int \u0005, Guid \u0006, ref _ISignature \u0007, ref IScope5 \u0008)
		{
			XmlAttribute xmlAttribute = \u0002.Attributes["name"];
			XmlNode xmlNode = \u0002["interface"];
			XmlNode xmlNode2 = \u0002["body"];
			if (xmlAttribute == null)
			{
				return;
			}
			string innerText = xmlAttribute.InnerText;
			XmlAttribute xmlAttribute2 = \u0002.Attributes["id"];
			XmlAttribute xmlAttribute3 = \u0002.Attributes["pou-id"];
			XmlAttribute xmlAttribute4 = \u0002.Attributes["external"];
			XmlAttribute xmlAttribute5 = \u0002.Attributes["enable-system-call"];
			Guid guid = (xmlAttribute2 == null) ? Guid.Empty : new Guid(xmlAttribute2.InnerText);
			Guid u = (xmlAttribute3 == null) ? Guid.Empty : new Guid(xmlAttribute3.InnerText);
			bool u2 = xmlAttribute4 != null && XmlConvert.ToBoolean(xmlAttribute4.InnerText);
			bool bSetTrue = xmlAttribute5 != null && XmlConvert.ToBoolean(xmlAttribute5.InnerText);
			_ISignature isignature = null;
			if (xmlNode != null)
			{
				this.\u0001(out \u0007, out \u0008, xmlNode, guid, u, u2, ref isignature);
			}
			else if (\u0002.Name == "action")
			{
				Debug.\u0001(false, "Actions not implemented yet for late lmm");
			}
			else
			{
				Debug.\u0001(false, "wrong language model");
			}
			if (xmlNode2 == null)
			{
				return;
			}
			_IParser iparser = new global::\u0011.\u0006(xmlNode2.InnerText);
			_IStatement istatement = iparser.ParseST();
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(innerText);
			icompiledPOU.MessageGuid = iparser.MessageGuid;
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.ObjectGuid = guid;
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, bSetTrue);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.NotForUpToDate, true);
			this.\u0001.AddCompiledPOU(icompiledPOU, \u0007, this.\u0002);
			if (\u0007.POUType == Operator.FunctionBlock)
			{
				ISignature subSignature = \u0007.GetSubSignature("__MAIN");
				icompiledPOU.SignatureId = subSignature.Id;
			}
			else
			{
				icompiledPOU.SignatureId = \u0007.Id;
			}
			CompilerServicesInternal.\u0001(istatement, \u0008, this.\u0001, null, true, true, icompiledPOU);
			CompilerServicesInternal.\u0001(istatement, \u0008, this.\u0001, true);
			IMessage[] array = CompilerServicesInternal.\u0001(istatement);
			IMessage[] u3 = array;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			this.\u0001(u3);
			if (\u0003 != -1 && \u0006 != Guid.Empty)
			{
				this.\u0001.SlotPOUs.Add(\u0006, \u0003, icompiledPOU.ObjectGuid);
				byte taskIndexByGuid = this.\u0001.TaskList.GetTaskIndexByGuid(\u0006);
				\u0007.AddTaskReference(taskIndexByGuid);
			}
			if (\u0004 != -1)
			{
				this.\u0001.SlotPOUs.AddDownloadSlot(\u0004, icompiledPOU.ObjectGuid);
			}
			if (\u0005 != -1)
			{
				this.\u0001.SlotPOUs.AddOnlineChangeSlot(\u0005, icompiledPOU.ObjectGuid);
			}
		}

		// Token: 0x0600181C RID: 6172 RVA: 0x0004ADF4 File Offset: 0x00048FF4
		private void \u0001(out _ISignature \u0002, out IScope5 \u0003, XmlNode \u0004, Guid \u0005, Guid \u0006, bool \u0007, ref _ISignature \u0008)
		{
			_IParser iparser = new global::\u0011.\u0006(\u0004.InnerText);
			\u0002 = iparser._ParseInterface(null, null);
			\u0002.ObjectGuid = \u0005;
			\u0002.ParentObjectGuid = \u0006;
			\u0002.SetFlag(SignatureFlag.Generated, true);
			\u0002.SetFlag(SignatureFlag.External, \u0007);
			if (this.\u0002 != null && \u0002.POUType != Operator.Method && \u0002.POUType != Operator.Action)
			{
				\u0008 = this.\u0002[\u0002.Name];
			}
			\u0002 = \u0002.CreateCompiledSignature(\u0008, this.\u0001, this.\u0002, this.\u0001.HasByteSupport());
			this.\u0001(\u0002);
			this.\u0001.AddSignature(\u0002, \u0008, this.\u0002, true);
			\u0018.\u0001(\u0002, this.\u0001, this.\u0002);
			\u0003 = global::\u0007.\u0005.\u0001(this.\u0001, \u0002.Id);
			global::\u0014.\u0013.\u0002(\u0002, \u0003, this.\u0001);
			\u0018.\u0001(\u0002, \u0003);
			this.\u0001(\u0002.Messages);
		}

		// Token: 0x0600181D RID: 6173 RVA: 0x0004AF08 File Offset: 0x00049108
		private void \u0001(XmlNode \u0002, out _ISignature \u0003, out IScope5 \u0004)
		{
			XmlAttribute xmlAttribute = \u0002.Attributes["name"];
			XmlNode xmlNode = \u0002["interface"];
			if (xmlAttribute == null || xmlNode == null)
			{
				\u0003 = null;
				\u0004 = null;
				return;
			}
			XmlAttribute xmlAttribute2 = \u0002.Attributes["id"];
			string innerText = xmlAttribute.InnerText;
			string innerText2 = xmlNode.InnerText;
			Guid objectGuid = (xmlAttribute2 == null) ? Guid.Empty : new Guid(xmlAttribute2.InnerText);
			\u0003 = ParserHelper.\u0001(innerText, innerText2, false);
			\u0003.ObjectGuid = objectGuid;
			_ISignature isignature = null;
			if (this.\u0002 != null)
			{
				isignature = this.\u0002[\u0003.Name];
			}
			\u0003 = \u0003.CreateCompiledSignature(isignature, this.\u0001, this.\u0002, this.\u0001.HasByteSupport());
			this.\u0001.AddSignature(\u0003, isignature, this.\u0002, true);
			\u0003.SetFlag(SignatureFlag.Generated, true);
			\u0004 = global::\u0007.\u0005.\u0001(this.\u0001, \u0003.Id);
			\u0004.SetLocalSignature(\u0003);
			global::\u0014.\u0013.\u0002(\u0003, \u0004, this.\u0001);
			this.\u0001(\u0003.Messages);
		}

		// Token: 0x0600181E RID: 6174 RVA: 0x0004B02C File Offset: 0x0004922C
		private void \u0002(XmlNode \u0002, out _ISignature \u0003, out IScope5 \u0004)
		{
			bool flag = \u0002.Attributes["name"] != null;
			XmlNode xmlNode = \u0002["interface"];
			if (!flag || xmlNode == null)
			{
				\u0003 = null;
				\u0004 = null;
				return;
			}
			string innerText = xmlNode.InnerText;
			XmlAttribute xmlAttribute = \u0002.Attributes["id"];
			Guid objectGuid = (xmlAttribute == null) ? Guid.Empty : new Guid(xmlAttribute.InnerText);
			_IParser iparser = new global::\u0011.\u0006(innerText);
			\u0003 = iparser._ParseInterface(null, null);
			\u0003.ObjectGuid = objectGuid;
			_ISignature isignature = null;
			if (this.\u0002 != null)
			{
				isignature = this.\u0002[\u0003.Name];
			}
			\u0003 = \u0003.CreateCompiledSignature(isignature, this.\u0001.HasByteSupport());
			this.\u0001(\u0003);
			this.\u0001.AddSignature(\u0003, isignature, this.\u0002, true);
			\u0003.SetFlag(SignatureFlag.Generated, true);
			XmlAttribute xmlAttribute2 = \u0002.Attributes["inhibit-online-change"];
			bool bSet = xmlAttribute2 != null && XmlConvert.ToBoolean(xmlAttribute2.InnerText);
			\u0003.SetFlag(SignatureFlag.InhibitOnlineChange, bSet);
			\u0004 = global::\u0007.\u0005.\u0001(this.\u0001, \u0003.Id);
			\u0004.SetLocalSignature(\u0003);
			global::\u0014.\u0013.\u0002(\u0003, \u0004, this.\u0001);
			this.\u0001(\u0003.Messages);
			\u0018.\u0001(\u0003, this.\u0001, this.\u0002);
		}

		// Token: 0x0600181F RID: 6175 RVA: 0x0004B18C File Offset: 0x0004938C
		private void \u0001(IMessage[] \u0002)
		{
			if (this.\u0001)
			{
				foreach (IMessage message in \u0002)
				{
					this.\u0001 = (message.Severity != Severity.Error && message.Severity != Severity.FatalError);
					if (!this.\u0001)
					{
						APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
						return;
					}
				}
			}
		}

		// Token: 0x06001820 RID: 6176 RVA: 0x0004B1F8 File Offset: 0x000493F8
		private void \u0001(_ISignature \u0002)
		{
			if (\u0002.ParentObjectGuid != Guid.Empty)
			{
				_ISignature isignature = this.\u0001.GetSignature(\u0002.ParentObjectGuid) as _ISignature;
				if (isignature != null)
				{
					\u0002.ParentSignatureId = isignature.Id;
					isignature.AddSubSignature(\u0002);
				}
			}
		}

		// Token: 0x04000442 RID: 1090
		private readonly _ICompileContext \u0001;

		// Token: 0x04000443 RID: 1091
		private readonly ILanguageModelList \u0001;

		// Token: 0x04000444 RID: 1092
		private readonly _ICompileContext \u0002;

		// Token: 0x04000445 RID: 1093
		private bool \u0001;
	}
}
