using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0011;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.Phase2_AfterTypification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Services.UpToDateChecks
{
	// Token: 0x02000115 RID: 277
	public class DetailedIsUpToDateStrategy : _IIsUpTopDateStrategy
	{
		// Token: 0x06001444 RID: 5188 RVA: 0x0003B4D4 File Offset: 0x000396D4
		public DetailedIsUpToDateStrategy(_ICompileContext comcon, _IPreCompileContext precomp, bool bCollectAllOnlineChangeProhibitingChanges)
		{
			this.\u0001 = comcon;
			this.\u0001 = precomp;
			this.\u0001 = precomp.IsDefined("uptodate_in_message_out");
			_ICompileContext u = this.\u0001;
			this.\u0001 = (((u != null) ? u.CreateGlobalIScope() : null) as _IScope);
			this.\u0001 = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			this.\u0001 = APEnvironmentFacade.Instance.MessageStorage;
			this.\u0002 = bCollectAllOnlineChangeProhibitingChanges;
			this.\u0005 = true;
			this.\u0001(EPouSetChange.Undefined);
			this.FastOnlineChangePossible = true;
			this.IsUpToDate = true;
		}

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x06001445 RID: 5189 RVA: 0x0003B578 File Offset: 0x00039778
		// (set) Token: 0x06001446 RID: 5190 RVA: 0x0003B580 File Offset: 0x00039780
		public bool FastOnlineChangePossible { get; private set; }

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x06001447 RID: 5191 RVA: 0x0003B58C File Offset: 0x0003978C
		// (set) Token: 0x06001448 RID: 5192 RVA: 0x0003B594 File Offset: 0x00039794
		public bool IsUpToDate { get; private set; }

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x06001449 RID: 5193 RVA: 0x0003B5A0 File Offset: 0x000397A0
		public bool OnlineChangePossible
		{
			get
			{
				return this.\u0005;
			}
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x0003B5A8 File Offset: 0x000397A8
		private bool \u0001()
		{
			return this.OnlineChangePossible || this.\u0002;
		}

		// Token: 0x0600144B RID: 5195 RVA: 0x0003B5BC File Offset: 0x000397BC
		private void \u0001(EPouSetChange \u0002)
		{
			if (\u0002 == EPouSetChange.Undefined)
			{
				this.\u0001 = \u0002;
				this.\u0005 = true;
			}
			else
			{
				this.\u0001 |= \u0002;
				this.\u0005 = false;
			}
			if (!this.OnlineChangePossible)
			{
				this.FastOnlineChangePossible = false;
			}
		}

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x0600144C RID: 5196 RVA: 0x0003B5F8 File Offset: 0x000397F8
		public IList<string> Changes
		{
			get
			{
				IList<string> list = new LList<string>();
				foreach (IChangedLMObject changedLMObject in this.DetailedChanges)
				{
					string item;
					if (string.IsNullOrEmpty(changedLMObject.Name))
					{
						item = changedLMObject.Description;
					}
					else
					{
						item = string.Format("{0}:\t{1}", changedLMObject.Name, changedLMObject.Description);
					}
					list.Add(item);
				}
				return list;
			}
		}

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x0600144D RID: 5197 RVA: 0x0003B67C File Offset: 0x0003987C
		public IList<IChangedLMObject> DetailedChanges { get; } = new LList<IChangedLMObject>();

		// Token: 0x0600144E RID: 5198 RVA: 0x0003B684 File Offset: 0x00039884
		public void CheckNextPOU()
		{
			this.\u0001 = EPouSetChange.Undefined;
		}

		// Token: 0x0600144F RID: 5199 RVA: 0x0003B690 File Offset: 0x00039890
		public bool AdditionalSignInPrecompile(_ISignature sign, _ISignature signCompiled)
		{
			this.IsUpToDate = false;
			if (signCompiled == null)
			{
				this.FastOnlineChangePossible = false;
			}
			if (\u001F.\u0004.\u0001(sign) && signCompiled == null)
			{
				this.\u0001(EPouSetChange.NewCheckFunctionInserted);
			}
			this.\u0002(sign, signCompiled);
			string text = DetailedIsUpToDateStrategy.\u0001(this.\u0001, sign);
			if (this.\u0001)
			{
				string text2 = string.Format("Interface {0} changed", text);
				if (signCompiled == null)
				{
					text2 += ": compiled interface not found";
				}
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, text2, Severity.Information, MessageId.None));
			}
			if (signCompiled == null)
			{
				this.\u0001(text, sign, global::\u0011.\u0001.Change_InterfaceAdded, EPouSetChange.InterfaceAdded);
			}
			return false;
		}

		// Token: 0x06001450 RID: 5200 RVA: 0x0003B730 File Offset: 0x00039930
		public bool AdditionalSignInPool(_ISignature signPool, _ISignature signCompiled)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.\u0002(signPool, signCompiled);
			string text = DetailedIsUpToDateStrategy.\u0001(this.\u0001, signPool);
			if (this.\u0001)
			{
				string text2 = string.Format("Interface {0} changed", text);
				if (signCompiled == null)
				{
					text2 += ": compiled interface not found";
				}
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, text2, Severity.Information, MessageId.None));
			}
			if (signCompiled == null)
			{
				this.\u0001(text, signPool, global::\u0011.\u0001.Change_InterfaceAdded, EPouSetChange.InterfaceAdded);
			}
			return false;
		}

		// Token: 0x06001451 RID: 5201 RVA: 0x0003B7B8 File Offset: 0x000399B8
		public void CheckGlobalOnlineChangePreConditions()
		{
			if (!this.\u0001.TaskList.IsEqual(this.\u0001.TaskList))
			{
				this.\u0001(EPouSetChange.TaskListChanged);
			}
			if (!this.IsUpToDate && this.\u0001.IsDefined("global_init_in_cycle"))
			{
				this.\u0001(EPouSetChange.GlobalInitInCycle);
			}
			if (!this.OnlineChangePossible)
			{
				this.FastOnlineChangePossible = false;
			}
		}

		// Token: 0x06001452 RID: 5202 RVA: 0x0003B824 File Offset: 0x00039A24
		public bool CompiledPouChanged(_ISignature sign, _ICompiledPOU cpou, _ICompiledPOU cpouPrecomp)
		{
			this.IsUpToDate = false;
			if (sign != null && sign.GetFlag(SignatureFlag.InhibitOnlineChangeOnCodeChanges))
			{
				this.\u0001(EPouSetChange.InhibitOnlineChangeOnCodeChanges);
			}
			if (sign != null && sign.HasAttribute("prevent-fastonlinechange-oncodechanges"))
			{
				this.FastOnlineChangePossible = false;
			}
			if (cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
			{
				this.FastOnlineChangePossible = false;
			}
			ISignature signature = this.\u0001[cpou.SignatureId];
			bool flag;
			string text = (signature != null) ? DetailedIsUpToDateStrategy.\u0001(this.\u0001, signature, out flag) : string.Empty;
			if (this.\u0001)
			{
				string text2 = string.Format("Code of POU {0} changed", text);
				if (cpouPrecomp == null)
				{
					text2 += ": precompiled pou not found";
				}
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, text2, Severity.Information, MessageId.None));
			}
			if (cpouPrecomp != null)
			{
				this.\u0001(text, signature, global::\u0011.\u0001.Change_CodeChanged, EPouSetChange.CodeChanged | DetailedIsUpToDateStrategy.\u0001(signature));
			}
			return false;
		}

		// Token: 0x06001453 RID: 5203 RVA: 0x0003B90C File Offset: 0x00039B0C
		public bool CompileOptionsChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.\u0001(EPouSetChange.CompileOptionsChanged);
			if (this.\u0001)
			{
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, "Compile options changed!", Severity.Information, MessageId.None));
			}
			return false;
		}

		// Token: 0x06001454 RID: 5204 RVA: 0x0003B94C File Offset: 0x00039B4C
		public bool DefinesChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this.\u0001)
			{
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, "Defines changed!", Severity.Information, MessageId.None));
			}
			this.\u0001(global::\u0011.\u0001.Change_DefineChanged, EPouSetChange.DefineChanged);
			return false;
		}

		// Token: 0x06001455 RID: 5205 RVA: 0x0003B9A4 File Offset: 0x00039BA4
		public bool ExternalSignatureFlagChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.\u0001(EPouSetChange.ExternalSignatureFlagChanged);
			return false;
		}

		// Token: 0x06001456 RID: 5206 RVA: 0x0003B9C4 File Offset: 0x00039BC4
		public bool GlobalError()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this.\u0001)
			{
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, "Global errors changed (namespaces in libraries ??)!", Severity.Information, MessageId.None));
			}
			this.\u0001(global::\u0011.\u0001.Change_GlobalErrors, EPouSetChange.GlobalErrorsChanged);
			return false;
		}

		// Token: 0x06001457 RID: 5207 RVA: 0x0003BA1C File Offset: 0x00039C1C
		public bool LibraryListChanged()
		{
			if (this.\u0001)
			{
				string u = "Referenced Libraries Changed";
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None));
			}
			this.\u0001(global::\u0011.\u0001.Change_ReferencedLibrariesChanged, EPouSetChange.ReferencedLibrariesChanged);
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06001458 RID: 5208 RVA: 0x0003BA74 File Offset: 0x00039C74
		public bool LibraryParamTablesChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.\u0001(global::\u0011.\u0001.Change_ParameterTablesChanged, EPouSetChange.ParameterTablesChanged);
			if (this.\u0001)
			{
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, "Parameter Tables changed!", Severity.Information, MessageId.None));
			}
			return false;
		}

		// Token: 0x06001459 RID: 5209 RVA: 0x0003BACC File Offset: 0x00039CCC
		public bool MemorySettingsChanged()
		{
			this.IsUpToDate = false;
			this.\u0001(EPouSetChange.MemorySettingsChanged);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x0600145A RID: 5210 RVA: 0x0003BAEC File Offset: 0x00039CEC
		public bool ParentContextChanged()
		{
			this.IsUpToDate = false;
			this.\u0001(EPouSetChange.ParentContextChanged);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x0600145B RID: 5211 RVA: 0x0003BB0C File Offset: 0x00039D0C
		public bool ParentContextNull()
		{
			this.IsUpToDate = false;
			this.\u0001(EPouSetChange.ParentContextNull);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x0600145C RID: 5212 RVA: 0x0003BB2C File Offset: 0x00039D2C
		public bool SignatureChanged(_ISignature sign, _ISignature signPrecom)
		{
			this.IsUpToDate = false;
			this.\u0006(sign, signPrecom);
			this.\u0005(sign, signPrecom);
			this.\u0004(sign, signPrecom);
			if (this.FastOnlineChangePossible)
			{
				this.FastOnlineChangePossible = DetailedIsUpToDateStrategy.\u0001(sign, signPrecom);
			}
			this.\u0001(sign);
			this.\u0001(sign, signPrecom);
			this.\u0003(sign, signPrecom);
			bool flag;
			string text = DetailedIsUpToDateStrategy.\u0001(this.\u0001, sign, out flag);
			if (this.\u0001)
			{
				string text2 = string.Format("Interface {0} changed", text);
				if (signPrecom == null)
				{
					text2 += ": Precompile not found";
				}
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, text2, Severity.Information, MessageId.None));
			}
			if (!sign.GetFlag(SignatureFlag.Generated))
			{
				if (signPrecom == null)
				{
					this.\u0001(text, sign, global::\u0011.\u0001.Change_InterfaceDeleted, EPouSetChange.InterfaceDeleted | DetailedIsUpToDateStrategy.\u0001(sign));
				}
				else
				{
					this.\u0001(this.\u0001, sign, signPrecom);
				}
			}
			return false;
		}

		// Token: 0x0600145D RID: 5213 RVA: 0x0003BC10 File Offset: 0x00039E10
		private void \u0001(_ISignature \u0002)
		{
			if (!this.\u0001())
			{
				return;
			}
			if (\u001F.\u0004.\u0001(\u0002))
			{
				this.\u0001(EPouSetChange.CheckFunction);
			}
		}

		// Token: 0x0600145E RID: 5214 RVA: 0x0003BC30 File Offset: 0x00039E30
		private void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			if (!this.\u0001())
			{
				return;
			}
			this.\u0002(\u0002, \u0003);
		}

		// Token: 0x0600145F RID: 5215 RVA: 0x0003BC44 File Offset: 0x00039E44
		private void \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			if (!\u0002.GetFlag(SignatureFlag.InhibitOnlineChange))
			{
				return;
			}
			if (\u0003 == null)
			{
				if (!\u0002.HasAttribute("allow_add_or_remove_signature"))
				{
					this.\u0001(EPouSetChange.InhibitOnlineChange);
				}
				return;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_INITIAL_VALUE_CHANGES))
			{
				if (\u0003.ChecksumNoInit != \u0002.ChecksumNoInit)
				{
					this.\u0001(EPouSetChange.InhibitOnlineChange);
				}
				return;
			}
			this.\u0001(EPouSetChange.InhibitOnlineChange);
		}

		// Token: 0x06001460 RID: 5216 RVA: 0x0003BCA8 File Offset: 0x00039EA8
		private void \u0003(_ISignature \u0002, _ISignature \u0003)
		{
			if (!this.\u0001())
			{
				return;
			}
			if (((\u0002.POUType == Operator.VarGlobal && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL)) || (\u0003 != null && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL))) && (\u0003 == null || !\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL) || \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL) != \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL)))
			{
				this.\u0001(EPouSetChange.TaskLocalGvl);
			}
		}

		// Token: 0x06001461 RID: 5217 RVA: 0x0003BD20 File Offset: 0x00039F20
		private void \u0004(_ISignature \u0002, _ISignature \u0003)
		{
			if (!this.\u0001())
			{
				return;
			}
			if (\u0002.BaseExpression != null && \u0003 != null && !((_IExpression)\u0002.BaseExpression).IsEqual(\u0003.BaseExpression))
			{
				this.\u0001(EPouSetChange.BaseExpressionChanged);
			}
			if (\u0003 == null || \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_SKIP_IMPLEMENTED_ITF_CHECK))
			{
				return;
			}
			IList<IExpression> list = Enumerable.ToLList<IExpression>(\u0002.InterfaceExpressions);
			if (\u0002.POUType == Operator.Interface && \u0002.BaseExpression != null)
			{
				list.Add(\u0002.BaseExpression);
			}
			IList<IExpression> list2 = Enumerable.ToLList<IExpression>(\u0003.InterfaceExpressions);
			if (\u0003.POUType == Operator.Interface && \u0003.BaseExpression != null)
			{
				list2.Add(\u0003.BaseExpression);
			}
			IEnumerable<string> enumerable = list.Select(new Func<IExpression, string>(DetailedIsUpToDateStrategy.<>c.<>9.\u0001));
			IEnumerable<string> enumerable2 = list2.Select(new Func<IExpression, string>(DetailedIsUpToDateStrategy.<>c.<>9.\u0002));
			if (enumerable.Except(enumerable2).Any<string>())
			{
				this.\u0001(EPouSetChange.InterfacesRemoved);
				return;
			}
			IEnumerable<string> second = enumerable2.Take(enumerable.Count<string>());
			if (!enumerable.SequenceEqual(second))
			{
				this.\u0001(EPouSetChange.InterfaceSequenceChanged);
			}
		}

		// Token: 0x06001462 RID: 5218 RVA: 0x0003BE4C File Offset: 0x0003A04C
		private void \u0005(_ISignature \u0002, _ISignature \u0003)
		{
			if (!this.\u0001())
			{
				return;
			}
			if (\u0003 == null)
			{
				return;
			}
			if (\u0003.HasFlag(SignatureFlag.Persistent))
			{
				return;
			}
			foreach (IVariable u in \u0002.AllVariables)
			{
				this.\u0001(\u0003, u);
			}
		}

		// Token: 0x06001463 RID: 5219 RVA: 0x0003BEB8 File Offset: 0x0003A0B8
		private void \u0001(_ISignature \u0002, IVariable \u0003)
		{
			IVariable variable = \u0002[\u0003.Name];
			if (variable == null)
			{
				return;
			}
			if (\u0003.Type.Class != TypeClass.Userdef)
			{
				return;
			}
			bool flag = variable.Type.Class != TypeClass.Userdef;
			if (variable.Type.Class == TypeClass.Lazy && \u0003.Type.Class == TypeClass.Userdef)
			{
				_ISignature isignature = this.\u0001[((IUserdefType2)\u0003.Type).SignatureId] as _ISignature;
				if (isignature != null && isignature.POUType == Operator.Type)
				{
					flag = false;
				}
			}
			if (\u0003.Type.Class == TypeClass.Userdef && flag)
			{
				this.\u0001(EPouSetChange.PrecomTypeMayLeaveDeadRefs);
				return;
			}
			if (variable.Type.Class == TypeClass.Lazy)
			{
				return;
			}
			_IUserdefType iuserdefType = (_IUserdefType)((_IVariable)\u0003).CompiledTypeInternal;
			_IUserdefType iuserdefType2 = (_IUserdefType)variable.Type;
			if (iuserdefType2 is IGenericUserdefType)
			{
				if (iuserdefType2.ToString() != iuserdefType.ToString())
				{
					this.\u0001(EPouSetChange.NameExpressionChanged);
					return;
				}
			}
			else if (!((_IExpression)iuserdefType.NameExpression).IsEqual(iuserdefType2.NameExpression))
			{
				this.\u0001(EPouSetChange.NameExpressionChanged);
			}
		}

		// Token: 0x06001464 RID: 5220 RVA: 0x0003BFE4 File Offset: 0x0003A1E4
		private void \u0006(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0003 == null || !\u0002.HasAttribute("contains_blobinitconst"))
			{
				return;
			}
			foreach (IVariable variable in \u0002.AllVariables)
			{
				if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST_ONLINECHANGESUPPORT))
				{
					IVariable variable2 = \u0003[variable.Name];
					if (variable2 != null)
					{
						string attributeValue = variable2.GetAttributeValue("initial_value_crc");
						string attributeValue2 = variable.GetAttributeValue("initial_value_crc");
						if (string.IsNullOrEmpty(attributeValue) || string.IsNullOrEmpty(attributeValue2) || attributeValue2 != attributeValue)
						{
							this.\u0001(EPouSetChange.BlobInitChanged);
						}
					}
				}
			}
		}

		// Token: 0x06001465 RID: 5221 RVA: 0x0003C0A8 File Offset: 0x0003A2A8
		public bool SignatureChangedByChecksumAttribute(_ISignature sign, _ISignature signPrecom)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.\u0001(EPouSetChange.SignatureChangedByChecksumAttribute);
			bool flag;
			string text = DetailedIsUpToDateStrategy.\u0001(this.\u0001, sign, out flag);
			if (this.\u0001)
			{
				string u = string.Format("Interface {0} changed, explicitly invalidated", text);
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None));
			}
			this.\u0001(text, sign, global::\u0011.\u0001.Change_InterfaceChange_Explicitly, EPouSetChange.InterfaceChangedExplicitly);
			return false;
		}

		// Token: 0x06001466 RID: 5222 RVA: 0x0003C124 File Offset: 0x0003A324
		public bool SimulationModeChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06001467 RID: 5223 RVA: 0x0003C138 File Offset: 0x0003A338
		public bool SubSignaturesChanged(_ISignature sign)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this.\u0001)
			{
				string u = string.Format("Interface {0} changed: number of subsignatures changed", sign.OrgName);
				this.\u0001.AddMessage(this.\u0001, \u0019.\u0003.\u0001(null, u, Severity.Information, MessageId.None));
			}
			bool flag;
			string u2 = DetailedIsUpToDateStrategy.\u0001(this.\u0001, sign, out flag);
			this.\u0001(u2, sign, global::\u0011.\u0001.Change_NumOfSubSignaturesChanged, EPouSetChange.NumberOfSubSignaturesChanged);
			return false;
		}

		// Token: 0x06001468 RID: 5224 RVA: 0x0003C1B0 File Offset: 0x0003A3B0
		private void \u0001(string \u0002, ISignature \u0003, IVariable \u0004, string \u0005, EPouSetChange \u0006)
		{
			this.\u0001(\u0002, \u0003, \u0005, \u0006).ChildName = \u0004.OrgName;
		}

		// Token: 0x06001469 RID: 5225 RVA: 0x0003C1CC File Offset: 0x0003A3CC
		private global::\u0003.\u0003 \u0001(string \u0002, ISignature \u0003, string \u0004, EPouSetChange \u0005)
		{
			Operator u001A_u = (\u0003 == null) ? Operator.None : \u0003.POUType;
			global::\u0003.\u0003 u = new global::\u0003.\u0003(\u0002, u001A_u, \u0004, this.\u0001 | \u0005);
			this.DetailedChanges.Add(u);
			return u;
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x0003C208 File Offset: 0x0003A408
		private void \u0001(string \u0002, EPouSetChange \u0003)
		{
			this.\u0001("", null, \u0002, \u0003);
		}

		// Token: 0x0600146B RID: 5227 RVA: 0x0003C21C File Offset: 0x0003A41C
		internal void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			bool flag;
			string text = DetailedIsUpToDateStrategy.\u0001(\u0002, \u0003, out flag);
			bool flag2 = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0003, GUIHidingFlags.AllCommon);
			if (flag || !flag2)
			{
				foreach (_IVariable u in \u0004.AllVariables)
				{
					this.\u0001(\u0003, \u0004, text, u);
				}
				foreach (_IVariable u2 in \u0003.AllVariables)
				{
					this.\u0001(\u0004, text, u2);
				}
			}
			EPouSetChange epouSetChange = DetailedIsUpToDateStrategy.\u0001(\u0004);
			this.\u0001(text, \u0004, global::\u0011.\u0001.Change_InterfaceChanged, EPouSetChange.InterfaceChanged | epouSetChange);
			if (!ObjectsToCompileDetector.\u0001(\u0004, \u0003))
			{
				this.\u0001(text, \u0004, global::\u0011.\u0001.Change_AttributesChanged, EPouSetChange.AttributesChanged | epouSetChange);
				this.FastOnlineChangePossible = false;
			}
		}

		// Token: 0x0600146C RID: 5228 RVA: 0x0003C32C File Offset: 0x0003A52C
		private void \u0001(_ISignature \u0002, _ISignature \u0003, string \u0004, _IVariable \u0005)
		{
			_IVariable ivariable = this.\u0001(\u0002, \u0005);
			string u = \u0004;
			if (\u0005.MessageGuid != Guid.Empty)
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, \u0005.MessageGuid))
				{
					u = APEnvironmentFacade.Instance.GetObjectName(projectHandle, \u0005.MessageGuid);
				}
			}
			EPouSetChange epouSetChange = DetailedIsUpToDateStrategy.\u0001(\u0005);
			if (ivariable == null)
			{
				this.\u0001(u, \u0003, \u0005, string.Format(global::\u0011.\u0001.Change_VariableInserted, \u0005.OrgName), EPouSetChange.VariableInserted | epouSetChange);
				return;
			}
			if (!ivariable.IsEqual(\u0005, true, true, false, null))
			{
				this.\u0001(u, \u0003, \u0005, string.Format(global::\u0011.\u0001.Change_VariableChanged, \u0005.OrgName), EPouSetChange.VariableChanged | epouSetChange);
			}
		}

		// Token: 0x0600146D RID: 5229 RVA: 0x0003C400 File Offset: 0x0003A600
		private void \u0001(_ISignature \u0002, string \u0003, _IVariable \u0004)
		{
			_IVariable ivariable = \u0002[\u0004.Name] as _IVariable;
			string u = \u0003;
			if (\u0004.GetFlag(VarFlag.Implicit))
			{
				return;
			}
			if (\u0004.MessageGuid != Guid.Empty)
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0002.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, \u0004.MessageGuid))
				{
					u = APEnvironmentFacade.Instance.GetObjectName(projectHandle, \u0004.MessageGuid);
				}
			}
			if (ivariable == null)
			{
				this.\u0001(u, \u0002, \u0004, string.Format(global::\u0011.\u0001.Change_VariableDeleted, \u0004.OrgName), EPouSetChange.VariableDeleted | DetailedIsUpToDateStrategy.\u0001(\u0004));
			}
		}

		// Token: 0x0600146E RID: 5230 RVA: 0x0003C4B0 File Offset: 0x0003A6B0
		private static EPouSetChange \u0001(_IVariable \u0002)
		{
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(\u0002, GUIHidingFlags.AllCommon))
			{
				return EPouSetChange.Undefined;
			}
			return EPouSetChange.ImplicitObject;
		}

		// Token: 0x0600146F RID: 5231 RVA: 0x0003C4DC File Offset: 0x0003A6DC
		private static EPouSetChange \u0001(ISignature \u0002)
		{
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0002, GUIHidingFlags.AllCommon))
			{
				return EPouSetChange.Undefined;
			}
			return EPouSetChange.ImplicitObject;
		}

		// Token: 0x06001470 RID: 5232 RVA: 0x0003C508 File Offset: 0x0003A708
		private _IVariable \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			_IVariable ivariable = \u0002[\u0003.Name] as _IVariable;
			if (ivariable != null)
			{
				return ivariable;
			}
			if (\u0003.GetFlag(VarFlag.Temp) && Operator.FunctionBlock == \u0002.POUType)
			{
				ISignature subSignature = \u0002.GetSubSignature("__MAIN");
				if (subSignature == null)
				{
					return null;
				}
				ivariable = (subSignature[\u0003.Name] as _IVariable);
				if (ivariable == null)
				{
					return null;
				}
				ivariable = ivariable.Duplicate(true);
				if (ivariable.GetFlag(VarFlag.Local))
				{
					ivariable.SetFlag(VarFlag.Local, false);
					ivariable.SetFlag(VarFlag.Temp, true);
				}
			}
			return ivariable;
		}

		// Token: 0x06001471 RID: 5233 RVA: 0x0003C598 File Offset: 0x0003A798
		private static string \u0001(IPreCompileContext \u0002, ISignature \u0003)
		{
			bool flag;
			string result;
			if (DetailedIsUpToDateStrategy.\u0001(\u0003 as _ISignature, out flag))
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, \u0003.MessageGuid))
				{
					result = APEnvironmentFacade.Instance.GetObjectName(projectHandle, \u0003.MessageGuid);
				}
				else
				{
					result = \u0003.OrgName;
				}
			}
			else
			{
				result = \u0003.OrgName;
				if (\u0003.ParentObjectGuid != Guid.Empty)
				{
					ISignature signature = \u0002.GetSignature(\u0003.ParentObjectGuid);
					if (signature != null)
					{
						result = signature.OrgName + "." + \u0003.OrgName;
					}
				}
			}
			return result;
		}

		// Token: 0x06001472 RID: 5234 RVA: 0x0003C640 File Offset: 0x0003A840
		internal static string \u0001(_ICompileContext \u0002, ISignature \u0003, out bool \u0004)
		{
			\u0004 = false;
			bool flag;
			string text;
			if (DetailedIsUpToDateStrategy.\u0001(\u0003 as _ISignature, out flag))
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0003.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, \u0003.MessageGuid))
				{
					text = APEnvironmentFacade.Instance.GetObjectName(projectHandle, \u0003.MessageGuid);
					\u0004 = true;
				}
				else
				{
					text = \u0003.OrgName;
				}
			}
			else
			{
				text = \u0003.OrgName;
				if (\u0003.ParentSignatureId != Helper.InvalidId)
				{
					ISignature signature = \u0002[\u0003.ParentSignatureId];
					if (signature != null)
					{
						text = signature.OrgName + "." + \u0003.OrgName;
					}
				}
				if (!string.IsNullOrEmpty(\u0003.LibraryPath))
				{
					text = text + "@" + \u0003.LibraryPath;
				}
			}
			return text;
		}

		// Token: 0x06001473 RID: 5235 RVA: 0x0003C708 File Offset: 0x0003A908
		private static bool \u0001(_ISignature \u0002, out bool \u0003)
		{
			\u0003 = true;
			if (\u0002.ObjectGuid != \u0002.MessageGuid && \u0002.MessageGuid != Guid.Empty)
			{
				Guid guid = Guid.Empty;
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					if (guid == Guid.Empty)
					{
						guid = ivariable.MessageGuid;
					}
					else if (!(ivariable.MessageGuid == Guid.Empty) && ivariable.MessageGuid != guid)
					{
						\u0003 = false;
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001474 RID: 5236 RVA: 0x0003C7C4 File Offset: 0x0003A9C4
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			if (\u0003 == null || \u0002 == null)
			{
				return false;
			}
			if (\u0002.POUType == Operator.Type)
			{
				return false;
			}
			if (\u0002.HasAttribute("prevent-fastonlinechange"))
			{
				return false;
			}
			if (\u0003.ChecksumNoInit == \u0002.ChecksumNoInit)
			{
				bool result = true;
				ISignatureWithOptionalInputs signatureWithOptionalInputs = \u0003 as ISignatureWithOptionalInputs;
				ISignatureWithOptionalInputs signatureWithOptionalInputs2 = \u0002 as ISignatureWithOptionalInputs;
				if (signatureWithOptionalInputs != null && signatureWithOptionalInputs2 != null)
				{
					result = (signatureWithOptionalInputs2.ChecksumOptionalInputs == signatureWithOptionalInputs.ChecksumOptionalInputs);
				}
				return result;
			}
			if (\u0002.GetSizeOfMemoryReserve() != \u0003.GetSizeOfMemoryReserve())
			{
				return false;
			}
			foreach (_IVariable u in \u0003.AllVariables.OfType<_IVariable>())
			{
				if (!DetailedIsUpToDateStrategy.\u0001(\u0002, \u0003, u))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001475 RID: 5237 RVA: 0x0003C88C File Offset: 0x0003AA8C
		private static bool \u0001(_ISignature \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			_IVariable ivariable = \u0002[\u0004.OrgName] as _IVariable;
			if (ivariable != null)
			{
				return ivariable.IsEqual(\u0004, false, true, false, null);
			}
			if (\u0004.HasFlag(VarFlag.ReplacedConstant))
			{
				return true;
			}
			if (\u0002.HasAttribute("subsequent"))
			{
				return false;
			}
			if (DetailedIsUpToDateStrategy.\u0002(\u0003, \u0004))
			{
				if (!DetailedIsUpToDateStrategy.\u0001(\u0002))
				{
					return false;
				}
			}
			else if (\u0002.POUType == Operator.VarGlobal && !\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
			{
				return false;
			}
			return DetailedIsUpToDateStrategy.\u0001(\u0003, \u0004);
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x0003C910 File Offset: 0x0003AB10
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				if (\u0003.GetFlag(VarFlag.Input))
				{
					return false;
				}
				if (\u0003.GetFlag(VarFlag.Output))
				{
					return false;
				}
			}
			return !\u0003.GetFlag(VarFlag.Inout);
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x0003C94C File Offset: 0x0003AB4C
		private static bool \u0001(_ISignature \u0002)
		{
			return \u0002.POUType == Operator.FunctionBlock && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS) && \u0002.HighestUsedOffset < \u0002.Size;
		}

		// Token: 0x06001478 RID: 5240 RVA: 0x0003C978 File Offset: 0x0003AB78
		internal static bool \u0002(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0002.POUType == Operator.FunctionBlock)
			{
				return \u0003.HasFlag(VarFlag.Local | VarFlag.Input | VarFlag.Output);
			}
			return \u0003.HasFlag(VarFlag.AllocateInInstance);
		}

		// Token: 0x06001479 RID: 5241 RVA: 0x0003C99C File Offset: 0x0003AB9C
		public bool PrecomNameHashChanged()
		{
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x0400037B RID: 891
		private const string \u0001 = "";

		// Token: 0x0400037C RID: 892
		private readonly bool \u0001;

		// Token: 0x0400037D RID: 893
		private readonly IMessageCategory \u0001;

		// Token: 0x0400037E RID: 894
		private readonly IMessageStorage \u0001;

		// Token: 0x0400037F RID: 895
		private readonly _ICompileContext \u0001;

		// Token: 0x04000380 RID: 896
		private readonly _IPreCompileContext \u0001;

		// Token: 0x04000381 RID: 897
		private readonly _IScope \u0001;

		// Token: 0x04000382 RID: 898
		private readonly bool \u0002;

		// Token: 0x04000383 RID: 899
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x04000384 RID: 900
		[CompilerGenerated]
		private bool \u0004;

		// Token: 0x04000385 RID: 901
		private bool \u0005;

		// Token: 0x04000386 RID: 902
		private EPouSetChange \u0001;

		// Token: 0x04000387 RID: 903
		[CompilerGenerated]
		private readonly IList<IChangedLMObject> \u0001;
	}
}
