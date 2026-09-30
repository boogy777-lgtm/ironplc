using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Legacy
{
	// Token: 0x02000279 RID: 633
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "no more refactoring in legacy code")]
	public class DetailedIsUpToDateStrategy : _IIsUpTopDateStrategy
	{
		// Token: 0x06002A7D RID: 10877 RVA: 0x0006D320 File Offset: 0x0006C320
		public DetailedIsUpToDateStrategy(_ICompileContext comcon, _IPreCompileContext precomp, bool bCollectAllOnlineChangeProhibitingChanges)
		{
			this._comcon = comcon;
			this._precomp = precomp;
			this._bMessageOut = precomp.IsDefined("uptodate_in_message_out");
			_ICompileContext comcon2 = this._comcon;
			this._globalScope = (((comcon2 != null) ? comcon2.CreateGlobalIScope() : null) as _IScope);
			this._cmc = CompilerMessageCategory.Singleton;
			this._messagestorage = APEnvironmentFacade.Instance.MessageStorage;
			this._bCollectAllOnlineChangeProhibitingChanges = bCollectAllOnlineChangeProhibitingChanges;
			this._bOnlineChangePossible = true;
			this.SetOnlineChangePossible(EPouSetChange.Undefined);
			this.FastOnlineChangePossible = true;
			this.IsUpToDate = true;
		}

		// Token: 0x17000BE6 RID: 3046
		// (get) Token: 0x06002A7E RID: 10878 RVA: 0x0006D3B9 File Offset: 0x0006C3B9
		// (set) Token: 0x06002A7F RID: 10879 RVA: 0x0006D3C1 File Offset: 0x0006C3C1
		public bool FastOnlineChangePossible { get; private set; }

		// Token: 0x17000BE7 RID: 3047
		// (get) Token: 0x06002A80 RID: 10880 RVA: 0x0006D3CA File Offset: 0x0006C3CA
		// (set) Token: 0x06002A81 RID: 10881 RVA: 0x0006D3D2 File Offset: 0x0006C3D2
		public bool IsUpToDate { get; private set; }

		// Token: 0x17000BE8 RID: 3048
		// (get) Token: 0x06002A82 RID: 10882 RVA: 0x0006D3DB File Offset: 0x0006C3DB
		public bool OnlineChangePossible
		{
			get
			{
				return this._bOnlineChangePossible;
			}
		}

		// Token: 0x06002A83 RID: 10883 RVA: 0x0006D3E3 File Offset: 0x0006C3E3
		private bool CheckIsOnlineChangePossible()
		{
			return this.OnlineChangePossible || this._bCollectAllOnlineChangeProhibitingChanges;
		}

		// Token: 0x06002A84 RID: 10884 RVA: 0x0006D3F5 File Offset: 0x0006C3F5
		private void SetOnlineChangePossible(EPouSetChange eReason)
		{
			if (eReason == EPouSetChange.Undefined)
			{
				this._eOnlineNotPossibleReason = eReason;
				this._bOnlineChangePossible = true;
			}
			else
			{
				this._eOnlineNotPossibleReason |= eReason;
				this._bOnlineChangePossible = false;
			}
			if (!this.OnlineChangePossible)
			{
				this.FastOnlineChangePossible = false;
			}
		}

		// Token: 0x17000BE9 RID: 3049
		// (get) Token: 0x06002A85 RID: 10885 RVA: 0x0006D430 File Offset: 0x0006C430
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

		// Token: 0x17000BEA RID: 3050
		// (get) Token: 0x06002A86 RID: 10886 RVA: 0x0006D4B4 File Offset: 0x0006C4B4
		public IList<IChangedLMObject> DetailedChanges { get; } = new LList<IChangedLMObject>();

		// Token: 0x06002A87 RID: 10887 RVA: 0x0006D4BC File Offset: 0x0006C4BC
		public void CheckNextPOU()
		{
			this._eOnlineNotPossibleReason = EPouSetChange.Undefined;
		}

		// Token: 0x06002A88 RID: 10888 RVA: 0x0006D4C8 File Offset: 0x0006C4C8
		public bool AdditionalSignInPrecompile(_ISignature sign, _ISignature signCompiled)
		{
			this.IsUpToDate = false;
			if (signCompiled == null)
			{
				this.FastOnlineChangePossible = false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 && CheckFunctionAttributes.IsCheckFunction(sign) && signCompiled == null)
			{
				this.SetOnlineChangePossible(EPouSetChange.NewCheckFunctionInserted);
			}
			if (sign.GetFlag(SignatureFlag.InhibitOnlineChange))
			{
				if (signCompiled != null && sign.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_INITIAL_VALUE_CHANGES))
				{
					if (sign.ChecksumNoInit != signCompiled.ChecksumNoInit)
					{
						this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
					}
				}
				else
				{
					this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
				}
			}
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._precomp, sign);
			if (this._bMessageOut)
			{
				string text = string.Format("Interface {0} changed", qualifiedSignatureName);
				if (signCompiled == null)
				{
					text += ": compiled interface not found";
				}
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, text, Severity.Information, MessageId.None));
			}
			if (signCompiled == null)
			{
				this.RecordChange(qualifiedSignatureName, sign, Strings.Change_InterfaceAdded, EPouSetChange.InterfaceAdded);
			}
			return false;
		}

		// Token: 0x06002A89 RID: 10889 RVA: 0x0006D5B0 File Offset: 0x0006C5B0
		[SuppressMessage("Critical Code Smell", "S927:Parameter names should match base declaration and other partial definitions", Justification = "No more refactoring in legacy code")]
		public bool AdditionalSignInPool(_ISignature sign, _ISignature signComp)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (sign.GetFlag(SignatureFlag.InhibitOnlineChange))
			{
				if (signComp != null && sign.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_INITIAL_VALUE_CHANGES))
				{
					if (sign.ChecksumNoInit != signComp.ChecksumNoInit)
					{
						this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
					}
				}
				else
				{
					this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
				}
			}
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._precomp, sign);
			if (this._bMessageOut)
			{
				string text = string.Format("Interface {0} changed", qualifiedSignatureName);
				if (signComp == null)
				{
					text += ": compiled interface not found";
				}
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, text, Severity.Information, MessageId.None));
			}
			if (signComp == null)
			{
				this.RecordChange(qualifiedSignatureName, sign, Strings.Change_InterfaceAdded, EPouSetChange.InterfaceAdded);
			}
			return false;
		}

		// Token: 0x06002A8A RID: 10890 RVA: 0x0006D670 File Offset: 0x0006C670
		public void CheckGlobalOnlineChangePreConditions()
		{
			if (!this._comcon.TaskList.IsEqual(this._precomp.TaskList))
			{
				this.SetOnlineChangePossible(EPouSetChange.TaskListChanged);
			}
			if (!this.IsUpToDate && this._comcon.IsDefined("global_init_in_cycle"))
			{
				this.SetOnlineChangePossible(EPouSetChange.GlobalInitInCycle);
			}
			if (!this.OnlineChangePossible)
			{
				this.FastOnlineChangePossible = false;
			}
		}

		// Token: 0x06002A8B RID: 10891 RVA: 0x0006D6DC File Offset: 0x0006C6DC
		[SuppressMessage("Critical Code Smell", "S927:Parameter names should match base declaration and other partial definitions", Justification = "No more refactoring in legacy code")]
		public bool CompiledPouChanged(_ISignature signToCheck, _ICompiledPOU cpou, _ICompiledPOU cpouPrecomp)
		{
			this.IsUpToDate = false;
			if (signToCheck != null && signToCheck.GetFlag(SignatureFlag.InhibitOnlineChangeOnCodeChanges))
			{
				this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChangeOnCodeChanges);
			}
			if (signToCheck != null && signToCheck.HasAttribute("prevent-fastonlinechange-oncodechanges"))
			{
				this.FastOnlineChangePossible = false;
			}
			if (cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
			{
				this.FastOnlineChangePossible = false;
			}
			ISignature signature = this._comcon[cpou.SignatureId];
			bool flag;
			string text = (signature != null) ? DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._comcon, signature, out flag) : string.Empty;
			if (this._bMessageOut)
			{
				string text2 = string.Format("Code of POU {0} changed", text);
				if (cpouPrecomp == null)
				{
					text2 += ": precompiled pou not found";
				}
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, text2, Severity.Information, MessageId.None));
			}
			if (cpouPrecomp != null)
			{
				this.RecordChange(text, signature, Strings.Change_CodeChanged, EPouSetChange.CodeChanged);
			}
			return false;
		}

		// Token: 0x06002A8C RID: 10892 RVA: 0x0006D7BA File Offset: 0x0006C7BA
		public bool CompileOptionsChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.SetOnlineChangePossible(EPouSetChange.CompileOptionsChanged);
			if (this._bMessageOut)
			{
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, "Compile options changed!", Severity.Information, MessageId.None));
			}
			return false;
		}

		// Token: 0x06002A8D RID: 10893 RVA: 0x0006D7FC File Offset: 0x0006C7FC
		public bool DefinesChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this._bMessageOut)
			{
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, "Defines changed!", Severity.Information, MessageId.None));
			}
			this.RecordChange(Strings.Change_DefineChanged, EPouSetChange.DefineChanged);
			return false;
		}

		// Token: 0x06002A8E RID: 10894 RVA: 0x0006D852 File Offset: 0x0006C852
		public bool ExternalSignatureFlagChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.SetOnlineChangePossible(EPouSetChange.ExternalSignatureFlagChanged);
			return false;
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x0006D870 File Offset: 0x0006C870
		public bool GlobalError()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this._bMessageOut)
			{
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, "Global errors changed (namespaces in libraries ??)!", Severity.Information, MessageId.None));
			}
			this.RecordChange(Strings.Change_GlobalErrors, EPouSetChange.GlobalErrorsChanged);
			return false;
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x0006D8C8 File Offset: 0x0006C8C8
		public bool LibraryListChanged()
		{
			if (this._bMessageOut)
			{
				string stError = "Referenced Libraries Changed";
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, stError, Severity.Information, MessageId.None));
			}
			this.RecordChange(Strings.Change_ReferencedLibrariesChanged, EPouSetChange.ReferencedLibrariesChanged);
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06002A91 RID: 10897 RVA: 0x0006D920 File Offset: 0x0006C920
		public bool LibraryParamTablesChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.RecordChange(Strings.Change_ParameterTablesChanged, EPouSetChange.ParameterTablesChanged);
			if (this._bMessageOut)
			{
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, "Parameter Tables changed!", Severity.Information, MessageId.None));
			}
			return false;
		}

		// Token: 0x06002A92 RID: 10898 RVA: 0x0006D976 File Offset: 0x0006C976
		public bool MemorySettingsChanged()
		{
			this.IsUpToDate = false;
			this.SetOnlineChangePossible(EPouSetChange.MemorySettingsChanged);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06002A93 RID: 10899 RVA: 0x0006D993 File Offset: 0x0006C993
		public bool ParentContextChanged()
		{
			this.IsUpToDate = false;
			this.SetOnlineChangePossible(EPouSetChange.ParentContextChanged);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06002A94 RID: 10900 RVA: 0x0006D9B0 File Offset: 0x0006C9B0
		public bool ParentContextNull()
		{
			this.IsUpToDate = false;
			this.SetOnlineChangePossible(EPouSetChange.ParentContextNull);
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06002A95 RID: 10901 RVA: 0x0006D9D0 File Offset: 0x0006C9D0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		public bool SignatureChanged(_ISignature sign, _ISignature signPrecom)
		{
			this.IsUpToDate = false;
			this.CheckForChangedBlobInitConst(sign, signPrecom);
			this.CheckForChangedVariableTypes(sign, signPrecom);
			this.CheckForChangedOOInterfaces(sign, signPrecom);
			if (this.FastOnlineChangePossible)
			{
				this.FastOnlineChangePossible = DetailedIsUpToDateStrategy.CheckForFastOnlineInterfaceChange(sign, signPrecom);
			}
			if (this.CheckIsOnlineChangePossible())
			{
				if (CheckFunctionAttributes.IsCheckFunction(sign) && (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34451 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)))
				{
					this.SetOnlineChangePossible(EPouSetChange.CheckFunction);
				}
				if (sign.GetFlag(SignatureFlag.InhibitOnlineChange))
				{
					if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_ALLOW_INITIAL_VALUE_CHANGES))
					{
						if (signPrecom == null || signPrecom.ChecksumNoInit != sign.ChecksumNoInit)
						{
							this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
						}
					}
					else
					{
						this.SetOnlineChangePossible(EPouSetChange.InhibitOnlineChange);
					}
				}
				if (((sign.POUType == Operator.VarGlobal && sign.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL)) || (signPrecom != null && signPrecom.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL))) && (signPrecom == null || !signPrecom.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL) || signPrecom.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL) != sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL)))
				{
					this.SetOnlineChangePossible(EPouSetChange.TaskLocalGvl);
				}
			}
			bool flag;
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._comcon, sign, out flag);
			if (this._bMessageOut)
			{
				string text = string.Format("Interface {0} changed", qualifiedSignatureName);
				if (signPrecom == null)
				{
					text += ": Precompile not found";
				}
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, text, Severity.Information, MessageId.None));
			}
			if (!sign.GetFlag(SignatureFlag.Generated))
			{
				if (signPrecom == null)
				{
					this.RecordChange(qualifiedSignatureName, sign, Strings.Change_InterfaceDeleted, EPouSetChange.InterfaceDeleted);
				}
				else
				{
					this.RecordInterfaceChange(this._comcon, sign, signPrecom);
				}
			}
			return false;
		}

		// Token: 0x06002A96 RID: 10902 RVA: 0x0006DB88 File Offset: 0x0006CB88
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private void CheckForChangedOOInterfaces(_ISignature sign, _ISignature signPrecomp)
		{
			if (!this.CheckIsOnlineChangePossible())
			{
				return;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 && sign.BaseExpression != null && signPrecomp != null && !((_IExpression)sign.BaseExpression).IsEqual(signPrecomp.BaseExpression))
			{
				this.SetOnlineChangePossible(EPouSetChange.BaseExpressionChanged);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && signPrecomp != null && !signPrecomp.HasAttribute(CompileAttributes.ATTRIBUTE_SKIP_IMPLEMENTED_ITF_CHECK))
			{
				IList<IExpression> list = Enumerable.ToLList<IExpression>(sign.InterfaceExpressions);
				if (sign.POUType == Operator.Interface && sign.BaseExpression != null)
				{
					list.Add(sign.BaseExpression);
				}
				IList<IExpression> list2 = Enumerable.ToLList<IExpression>(signPrecomp.InterfaceExpressions);
				if (signPrecomp.POUType == Operator.Interface && signPrecomp.BaseExpression != null)
				{
					list2.Add(signPrecomp.BaseExpression);
				}
				IEnumerable<string> enumerable = from x in list
				select x.ToString();
				IEnumerable<string> enumerable2 = from x in list2
				select x.ToString();
				if (enumerable.Except(enumerable2).Any<string>())
				{
					this.SetOnlineChangePossible(EPouSetChange.InterfacesRemoved);
					return;
				}
				IEnumerable<string> second = enumerable2.Take(enumerable.Count<string>());
				if (!enumerable.SequenceEqual(second))
				{
					this.SetOnlineChangePossible(EPouSetChange.InterfaceSequenceChanged);
				}
			}
		}

		// Token: 0x06002A97 RID: 10903 RVA: 0x0006DCDC File Offset: 0x0006CCDC
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private void CheckForChangedVariableTypes(_ISignature sign, _ISignature signPrecomp)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && this.CheckIsOnlineChangePossible() && signPrecomp != null && !signPrecomp.HasFlag(SignatureFlag.Persistent))
			{
				foreach (IVariable variable in sign.AllVariables)
				{
					IVariable variable2 = signPrecomp[variable.Name];
					if (variable2 != null && variable.Type.Class == TypeClass.Userdef)
					{
						bool flag = variable2.Type.Class != TypeClass.Userdef;
						if (variable2.Type.Class == TypeClass.Lazy && variable.Type.Class == TypeClass.Userdef)
						{
							_ISignature isignature = this._globalScope[((IUserdefType2)variable.Type).SignatureId] as _ISignature;
							if (isignature != null && isignature.POUType == Operator.Type)
							{
								flag = false;
							}
						}
						if (variable.Type.Class == TypeClass.Userdef && flag)
						{
							this.SetOnlineChangePossible(EPouSetChange.PrecomTypeMayLeaveDeadRefs);
						}
						else if (variable2.Type.Class != TypeClass.Lazy)
						{
							_IUserdefType iuserdefType = (_IUserdefType)((_IVariable)variable).CompiledTypeInternal;
							_IUserdefType iuserdefType2 = (_IUserdefType)variable2.Type;
							if (iuserdefType2 is IGenericUserdefType)
							{
								if (iuserdefType2.ToString() != iuserdefType.ToString())
								{
									this.SetOnlineChangePossible(EPouSetChange.NameExpressionChanged);
								}
							}
							else if (!((_IExpression)iuserdefType.NameExpression).IsEqual(iuserdefType2.NameExpression))
							{
								this.SetOnlineChangePossible(EPouSetChange.NameExpressionChanged);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002A98 RID: 10904 RVA: 0x0006DE98 File Offset: 0x0006CE98
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private void CheckForChangedBlobInitConst(_ISignature sign, _ISignature signPrecomp)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35340 && signPrecomp != null && sign.HasAttribute("contains_blobinitconst"))
			{
				foreach (IVariable variable in sign.AllVariables)
				{
					if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) && !variable.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST_ONLINECHANGESUPPORT))
					{
						IVariable variable2 = signPrecomp[variable.Name];
						if (variable2 != null)
						{
							string attributeValue = variable2.GetAttributeValue("initial_value_crc");
							string attributeValue2 = variable.GetAttributeValue("initial_value_crc");
							if (string.IsNullOrEmpty(attributeValue) || string.IsNullOrEmpty(attributeValue2) || attributeValue2 != attributeValue)
							{
								this.SetOnlineChangePossible(EPouSetChange.BlobInitChanged);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002A99 RID: 10905 RVA: 0x0006DF74 File Offset: 0x0006CF74
		public bool SignatureChangedByChecksumAttribute(_ISignature sign, _ISignature signPrecom)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			this.SetOnlineChangePossible(EPouSetChange.SignatureChangedByChecksumAttribute);
			bool flag;
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._comcon, sign, out flag);
			if (this._bMessageOut)
			{
				string stError = string.Format("Interface {0} changed, explicitly invalidated", qualifiedSignatureName);
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, stError, Severity.Information, MessageId.None));
			}
			this.RecordChange(qualifiedSignatureName, sign, Strings.Change_InterfaceChange_Explicitly, EPouSetChange.InterfaceChangedExplicitly);
			return false;
		}

		// Token: 0x06002A9A RID: 10906 RVA: 0x0006DFED File Offset: 0x0006CFED
		public bool SimulationModeChanged()
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x06002A9B RID: 10907 RVA: 0x0006E000 File Offset: 0x0006D000
		public bool SubSignaturesChanged(_ISignature sign)
		{
			this.IsUpToDate = false;
			this.FastOnlineChangePossible = false;
			if (this._bMessageOut)
			{
				string stError = string.Format("Interface {0} changed: number of subsignatures changed", sign.OrgName);
				this._messagestorage.AddMessage(this._cmc, new CompilerMessage(null, stError, Severity.Information, MessageId.None));
			}
			bool flag;
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(this._comcon, sign, out flag);
			this.RecordChange(qualifiedSignatureName, sign, Strings.Change_NumOfSubSignaturesChanged, EPouSetChange.NumberOfSubSignaturesChanged);
			return false;
		}

		// Token: 0x06002A9C RID: 10908 RVA: 0x0006E075 File Offset: 0x0006D075
		private void RecordChange(string stAffectedPou, ISignature signature, IVariable var, string strChange, EPouSetChange eChange)
		{
			this.RecordChange(stAffectedPou, signature, strChange, eChange).ChildName = var.OrgName;
		}

		// Token: 0x06002A9D RID: 10909 RVA: 0x0006E090 File Offset: 0x0006D090
		private ChangedLMObject RecordChange(string stAffectedPou, ISignature signature, string strChange, EPouSetChange eChange)
		{
			Operator ePOUType = (signature == null) ? Operator.None : signature.POUType;
			ChangedLMObject changedLMObject = new ChangedLMObject(stAffectedPou, ePOUType, strChange, this._eOnlineNotPossibleReason | eChange);
			this.DetailedChanges.Add(changedLMObject);
			return changedLMObject;
		}

		// Token: 0x06002A9E RID: 10910 RVA: 0x0006E0C9 File Offset: 0x0006D0C9
		private void RecordChange(string strChange, EPouSetChange eChange)
		{
			this.RecordChange("", null, strChange, eChange);
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x0006E0DC File Offset: 0x0006D0DC
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		internal void RecordInterfaceChange(_ICompileContext comcon, _ISignature signCompiled, _ISignature signPrecompiled)
		{
			bool flag;
			string qualifiedSignatureName = DetailedIsUpToDateStrategy.GetQualifiedSignatureName(comcon, signCompiled, out flag);
			bool flag2 = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signCompiled, GUIHidingFlags.AllCommon);
			if (flag || !flag2)
			{
				foreach (_IVariable ivariable in signPrecompiled.AllVariables)
				{
					_IVariable ivariable2 = signCompiled[ivariable.Name] as _IVariable;
					string stAffectedPou = qualifiedSignatureName;
					if (ivariable.MessageGuid != Guid.Empty)
					{
						int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signPrecompiled.LibraryPath);
						if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, ivariable.MessageGuid))
						{
							stAffectedPou = APEnvironmentFacade.Instance.GetObjectName(projectHandle, ivariable.MessageGuid);
						}
					}
					if (ivariable2 == null)
					{
						this.RecordChange(stAffectedPou, signPrecompiled, ivariable, string.Format(Strings.Change_VariableInserted, ivariable.OrgName), EPouSetChange.VariableInserted);
					}
					else if (!ivariable2.IsEqual(ivariable, true, true, false, null))
					{
						this.RecordChange(stAffectedPou, signPrecompiled, ivariable, string.Format(Strings.Change_VariableChanged, ivariable.OrgName), EPouSetChange.VariableChanged);
					}
				}
				foreach (_IVariable ivariable3 in signCompiled.AllVariables)
				{
					_IVariable ivariable4 = signPrecompiled[ivariable3.Name] as _IVariable;
					string stAffectedPou2 = qualifiedSignatureName;
					if (!ivariable3.GetFlag(VarFlag.Implicit))
					{
						if (ivariable3.MessageGuid != Guid.Empty)
						{
							int projectHandle2 = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signPrecompiled.LibraryPath);
							if (APEnvironmentFacade.Instance.ExistsObject(projectHandle2, ivariable3.MessageGuid))
							{
								stAffectedPou2 = APEnvironmentFacade.Instance.GetObjectName(projectHandle2, ivariable3.MessageGuid);
							}
						}
						if (ivariable4 == null)
						{
							this.RecordChange(stAffectedPou2, signPrecompiled, ivariable3, string.Format(Strings.Change_VariableDeleted, ivariable3.OrgName), EPouSetChange.VariableDeleted);
						}
					}
				}
			}
			this.RecordChange(qualifiedSignatureName, signPrecompiled, Strings.Change_InterfaceChanged, EPouSetChange.InterfaceChanged);
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x0006E324 File Offset: 0x0006D324
		private static string GetQualifiedSignatureName(IPreCompileContext precom, ISignature sign)
		{
			string result = string.Empty;
			bool flag;
			if (DetailedIsUpToDateStrategy.CheckMessageGuid(sign as _ISignature, out flag))
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(sign.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, sign.MessageGuid))
				{
					result = APEnvironmentFacade.Instance.GetObjectName(projectHandle, sign.MessageGuid);
				}
				else
				{
					result = sign.OrgName;
				}
			}
			else
			{
				result = sign.OrgName;
				if (sign.ParentObjectGuid != Guid.Empty)
				{
					ISignature signature = precom.GetSignature(sign.ParentObjectGuid);
					if (signature != null)
					{
						result = signature.OrgName + "." + sign.OrgName;
					}
				}
			}
			return result;
		}

		// Token: 0x06002AA1 RID: 10913 RVA: 0x0006E3D4 File Offset: 0x0006D3D4
		internal static string GetQualifiedSignatureName(_ICompileContext comcon, ISignature sign, out bool bMessageGuidObjectExists)
		{
			string text = string.Empty;
			bMessageGuidObjectExists = false;
			bool flag;
			if (DetailedIsUpToDateStrategy.CheckMessageGuid(sign as _ISignature, out flag))
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(sign.LibraryPath);
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, sign.MessageGuid))
				{
					text = APEnvironmentFacade.Instance.GetObjectName(projectHandle, sign.MessageGuid);
					bMessageGuidObjectExists = true;
				}
				else
				{
					text = sign.OrgName;
				}
			}
			else
			{
				text = sign.OrgName;
				if (sign.ParentSignatureId != Common.InvalidID)
				{
					ISignature signature = comcon[sign.ParentSignatureId];
					if (signature != null)
					{
						text = signature.OrgName + "." + sign.OrgName;
					}
				}
				if (!string.IsNullOrEmpty(sign.LibraryPath))
				{
					text = text + "@" + sign.LibraryPath;
				}
			}
			return text;
		}

		// Token: 0x06002AA2 RID: 10914 RVA: 0x0006E4A4 File Offset: 0x0006D4A4
		private static bool CheckMessageGuid(_ISignature sign, out bool bUnique)
		{
			bUnique = true;
			if (sign.ObjectGuid != sign.MessageGuid && sign.MessageGuid != Guid.Empty)
			{
				Guid guid = Guid.Empty;
				foreach (_IVariable ivariable in sign.AllVariables)
				{
					if (guid == Guid.Empty)
					{
						guid = ivariable.MessageGuid;
					}
					else if (ivariable.MessageGuid != Guid.Empty && ivariable.MessageGuid != guid)
					{
						bUnique = false;
						return false;
					}
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002AA3 RID: 10915 RVA: 0x0006E560 File Offset: 0x0006D560
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForFastOnlineInterfaceChangeGE351200(_ISignature signCompiled, _ISignature signPrecompiled)
		{
			if (signPrecompiled == null || signCompiled == null)
			{
				return false;
			}
			if (signCompiled.POUType == Operator.Type)
			{
				return false;
			}
			if (signCompiled.HasAttribute("prevent-fastonlinechange"))
			{
				return false;
			}
			if (signPrecompiled.ChecksumNoInit == signCompiled.ChecksumNoInit)
			{
				bool result = true;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
				{
					ISignatureWithOptionalInputs signatureWithOptionalInputs = signPrecompiled as ISignatureWithOptionalInputs;
					ISignatureWithOptionalInputs signatureWithOptionalInputs2 = signCompiled as ISignatureWithOptionalInputs;
					if (signatureWithOptionalInputs != null && signatureWithOptionalInputs2 != null)
					{
						result = (signatureWithOptionalInputs2.ChecksumOptionalInputs == signatureWithOptionalInputs.ChecksumOptionalInputs);
					}
				}
				return result;
			}
			if (signCompiled.GetSizeOfMemoryReserve() != signPrecompiled.GetSizeOfMemoryReserve())
			{
				return false;
			}
			foreach (_IVariable ivariable in signPrecompiled.AllVariables.OfType<_IVariable>())
			{
				_IVariable ivariable2 = signCompiled[ivariable.OrgName] as _IVariable;
				if (ivariable2 == null)
				{
					if (!ivariable.HasFlag(VarFlag.ReplacedConstant))
					{
						if (signCompiled.HasAttribute("subsequent"))
						{
							return false;
						}
						if (DetailedIsUpToDateStrategy.IsInstanceVariable(signPrecompiled, ivariable))
						{
							if (!DetailedIsUpToDateStrategy.CanAllocateAdditionalInstanceVar(signCompiled))
							{
								return false;
							}
						}
						else if (signCompiled.POUType == Operator.VarGlobal && !signCompiled.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY))
						{
							return false;
						}
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700 && !DetailedIsUpToDateStrategy.CheckForChangedExternalInterface(signPrecompiled, ivariable))
						{
							return false;
						}
					}
				}
				else if (!ivariable2.IsEqual(ivariable, false, true, false, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002AA4 RID: 10916 RVA: 0x0006E6D0 File Offset: 0x0006D6D0
		private static bool CheckForChangedExternalInterface(_ISignature signChanged, _IVariable varAdded)
		{
			if (signChanged.POUType == Operator.Function || signChanged.POUType == Operator.Method)
			{
				if (varAdded.GetFlag(VarFlag.Input))
				{
					return false;
				}
				if (varAdded.GetFlag(VarFlag.Output))
				{
					return false;
				}
			}
			return !varAdded.GetFlag(VarFlag.Inout);
		}

		// Token: 0x06002AA5 RID: 10917 RVA: 0x0006E70C File Offset: 0x0006D70C
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "No more refactoring in legacy code")]
		private static bool CheckForFastOnlineInterfaceChangeLegacy(_ISignature signCompiled, _ISignature signPrecompiled)
		{
			if (signPrecompiled == null || signCompiled == null)
			{
				return false;
			}
			if (signPrecompiled.ChecksumNoInit == signCompiled.ChecksumNoInit)
			{
				return true;
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700 && signCompiled.HasAttribute("prevent-fastonlinechange"))
			{
				return false;
			}
			IEnumerable<IVariable> allVariables = signPrecompiled.AllVariables;
			IEnumerable<IVariable> allVariables2 = signCompiled.AllVariables;
			if (allVariables2.Count<IVariable>() > allVariables.Count<IVariable>())
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
				{
					using (IEnumerator<_IVariable> enumerator = allVariables2.OfType<_IVariable>().GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							_IVariable ivariable = enumerator.Current;
							if (!(signPrecompiled[ivariable.OrgName] is _IVariable) && !DetailedIsUpToDateStrategy.IsStackVariable(signCompiled, ivariable) && !ivariable.GetFlag(VarFlag.Implicit))
							{
								return false;
							}
						}
						goto IL_D5;
					}
				}
				return false;
			}
			IL_D5:
			foreach (_IVariable ivariable2 in allVariables.OfType<_IVariable>())
			{
				_IVariable ivariable3 = signCompiled[ivariable2.OrgName] as _IVariable;
				if (ivariable3 == null)
				{
					bool flag = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && signCompiled.HasAttribute("subsequent");
					if ((signCompiled.POUType != Operator.Program || !ivariable2.GetFlag(VarFlag.Local) || flag) && !DetailedIsUpToDateStrategy.IsStackVariable(signPrecompiled, ivariable2))
					{
						return false;
					}
				}
				else if (!ivariable3.IsEqual(ivariable2, false, true, false, null))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002AA6 RID: 10918 RVA: 0x0006E8B0 File Offset: 0x0006D8B0
		private static bool CheckForFastOnlineInterfaceChange(_ISignature signCompiled, _ISignature signPrecompiled)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351200)
			{
				return DetailedIsUpToDateStrategy.CheckForFastOnlineInterfaceChangeGE351200(signCompiled, signPrecompiled);
			}
			return DetailedIsUpToDateStrategy.CheckForFastOnlineInterfaceChangeLegacy(signCompiled, signPrecompiled);
		}

		// Token: 0x06002AA7 RID: 10919 RVA: 0x0006E8D2 File Offset: 0x0006D8D2
		internal static bool IsStackVariable(_ISignature sign, _IVariable var)
		{
			return var.GetFlag(VarFlag.Temp) || (var.GetFlag(VarFlag.Local) && (sign.POUType == Operator.Function || sign.POUType == Operator.Method));
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x0006E906 File Offset: 0x0006D906
		private static bool CanAllocateAdditionalInstanceVar(_ISignature signCompiled)
		{
			return signCompiled.POUType == Operator.FunctionBlock && signCompiled.HasAttribute(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS) && signCompiled.HighestUsedOffset < signCompiled.Size;
		}

		// Token: 0x06002AA9 RID: 10921 RVA: 0x0006E92F File Offset: 0x0006D92F
		internal static bool IsInstanceVariable(_ISignature sign, _IVariable var)
		{
			if (sign.POUType == Operator.FunctionBlock)
			{
				return var.HasFlag(VarFlag.Local | VarFlag.Input | VarFlag.Output);
			}
			return var.HasFlag(VarFlag.AllocateInInstance);
		}

		// Token: 0x06002AAA RID: 10922 RVA: 0x0006E953 File Offset: 0x0006D953
		public bool PrecomNameHashChanged()
		{
			this.FastOnlineChangePossible = false;
			return false;
		}

		// Token: 0x04000812 RID: 2066
		private const string NO_DEDICATED_POU = "";

		// Token: 0x04000813 RID: 2067
		private readonly bool _bMessageOut;

		// Token: 0x04000814 RID: 2068
		private readonly CompilerMessageCategory _cmc;

		// Token: 0x04000815 RID: 2069
		private readonly IMessageStorage _messagestorage;

		// Token: 0x04000816 RID: 2070
		private readonly _ICompileContext _comcon;

		// Token: 0x04000817 RID: 2071
		private readonly _IPreCompileContext _precomp;

		// Token: 0x04000818 RID: 2072
		private readonly _IScope _globalScope;

		// Token: 0x04000819 RID: 2073
		private readonly bool _bCollectAllOnlineChangeProhibitingChanges;

		// Token: 0x0400081C RID: 2076
		private bool _bOnlineChangePossible;

		// Token: 0x0400081D RID: 2077
		private EPouSetChange _eOnlineNotPossibleReason;
	}
}
