using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010A RID: 266
	[TypeGuid("{E2645E22-A6BE-4732-A779-A2F997A723FA}")]
	public class WarningHelper : _IWarningHelper2, _IWarningHelper, ILMWarningConfiguration, ILMWarningConfiguration2
	{
		// Token: 0x06001406 RID: 5126 RVA: 0x0003B6BC File Offset: 0x0003A6BC
		public WarningHelper()
		{
			APEnvironmentFacade.Instance.PrimaryProjectSwitched += this.OnPrimaryProjectSwitched;
			APEnvironmentFacade.Instance.OptionChanged += this.OnOptionChanged;
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x0003B711 File Offset: 0x0003A711
		private void OnPrimaryProjectSwitched(IProject oldProject, IProject newProject)
		{
			this._hsDisabledWarningIds = null;
			this._disabledOEMIds = null;
			this._hsWarningAsErrorIds = null;
		}

		// Token: 0x06001408 RID: 5128 RVA: 0x0003B711 File Offset: 0x0003A711
		private void OnOptionChanged(object sender, OptionEventArgs e)
		{
			this._hsDisabledWarningIds = null;
			this._disabledOEMIds = null;
			this._hsWarningAsErrorIds = null;
		}

		// Token: 0x06001409 RID: 5129 RVA: 0x0003B728 File Offset: 0x0003A728
		public bool IsWarningMessageDisabled(MessageId id)
		{
			ICollection<int> disabledWarningIds = this.GetDisabledWarningIds();
			if (disabledWarningIds != null && disabledWarningIds.Contains((int)id))
			{
				ICollection<int> warningAsErrorIds = this.GetWarningAsErrorIds();
				return warningAsErrorIds == null || !warningAsErrorIds.Contains((int)id);
			}
			return false;
		}

		// Token: 0x0600140A RID: 5130 RVA: 0x0003B760 File Offset: 0x0003A760
		public bool IsWarning(MessageId messageId)
		{
			return messageId.ToString().StartsWith("Wrn_");
		}

		// Token: 0x0600140B RID: 5131 RVA: 0x0003B77E File Offset: 0x0003A77E
		public bool IsError(MessageId messageId)
		{
			return messageId.ToString().StartsWith("Err_");
		}

		// Token: 0x0600140C RID: 5132 RVA: 0x0003B79C File Offset: 0x0003A79C
		public ICollection<int> GetDisabledWarningIds()
		{
			if (this._hsDisabledWarningIds == null)
			{
				this.ReadDisabledWarningIds();
			}
			return this._hsDisabledWarningIds;
		}

		// Token: 0x0600140D RID: 5133 RVA: 0x0003B7B4 File Offset: 0x0003A7B4
		private void ReadDisabledWarningIds()
		{
			if (this._disabledOEMIds != null)
			{
				this._disabledOEMIds.Clear();
			}
			if (WarningHelper.DisabledWarningIdsKey.HasValue(WarningHelper.VALUE_DISABLED_WARNING_IDs, typeof(string)))
			{
				if (this._hsDisabledWarningIds == null)
				{
					this._hsDisabledWarningIds = new HashSet<int>();
				}
				this._hsDisabledWarningIds.Clear();
				string[] array = ((string)WarningHelper.DisabledWarningIdsKey[WarningHelper.VALUE_DISABLED_WARNING_IDs]).Split(new char[]
				{
					','
				});
				for (int i = 0; i < array.Length; i++)
				{
					int item = int.Parse(array[i]);
					if (!this._hsDisabledWarningIds.Contains(item))
					{
						this._hsDisabledWarningIds.Add(item);
					}
				}
			}
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			if (oemcustomization != null && oemcustomization.HasValue(this.OEM_CUSTOMIZATION_SECTION, this.OEM_CUSTOMIZATION_KEY))
			{
				if (this._hsDisabledWarningIds == null)
				{
					this._hsDisabledWarningIds = new HashSet<int>();
				}
				if (this._disabledOEMIds == null)
				{
					this._disabledOEMIds = new HashSet<int>();
				}
				string stringValue = oemcustomization.GetStringValue(this.OEM_CUSTOMIZATION_SECTION, this.OEM_CUSTOMIZATION_KEY);
				if (!string.IsNullOrWhiteSpace(stringValue))
				{
					string[] array = stringValue.Split(new char[]
					{
						','
					});
					for (int i = 0; i < array.Length; i++)
					{
						int item2;
						if (int.TryParse(array[i], out item2))
						{
							this._hsDisabledWarningIds.Add(item2);
							this._disabledOEMIds.Add(item2);
						}
					}
				}
			}
		}

		// Token: 0x0600140E RID: 5134 RVA: 0x0003B918 File Offset: 0x0003A918
		private void ReadWarningAsErrorIds()
		{
			if (!WarningHelper.WarningAsErrorIdsKey.HasValue(WarningHelper.VALUE_WARNING_AS_ERROR_IDs, typeof(string)))
			{
				return;
			}
			if (this._hsWarningAsErrorIds == null)
			{
				this._hsWarningAsErrorIds = new HashSet<int>();
			}
			this._hsWarningAsErrorIds.Clear();
			string[] array = ((string)WarningHelper.WarningAsErrorIdsKey[WarningHelper.VALUE_WARNING_AS_ERROR_IDs]).Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				int item = int.Parse(array[i]);
				if (!this._hsWarningAsErrorIds.Contains(item))
				{
					this._hsWarningAsErrorIds.Add(item);
				}
			}
		}

		// Token: 0x0600140F RID: 5135 RVA: 0x0003B9B5 File Offset: 0x0003A9B5
		public bool IsOEMDisabledId(MessageId id)
		{
			if (this._hsDisabledWarningIds == null)
			{
				this.ReadDisabledWarningIds();
			}
			return this._disabledOEMIds != null && this._disabledOEMIds.Contains((int)id);
		}

		// Token: 0x06001410 RID: 5136 RVA: 0x0003B9DC File Offset: 0x0003A9DC
		public void WriteDisabledWarningIds(ICollection<int> hsDisabledWarningIds)
		{
			string asCsv = WarningHelper.GetAsCsv(hsDisabledWarningIds);
			if (asCsv != string.Empty)
			{
				WarningHelper.DisabledWarningIdsKey[WarningHelper.VALUE_DISABLED_WARNING_IDs] = asCsv;
				this._hsDisabledWarningIds = new HashSet<int>(hsDisabledWarningIds);
				return;
			}
			APEnvironmentFacade.Instance.DeleteSubKey(OptionRoot.Project, WarningHelper.SUB_KEY_DISABLED_WARNING_IDs);
			this._hsDisabledWarningIds = null;
		}

		// Token: 0x17000574 RID: 1396
		// (get) Token: 0x06001411 RID: 5137 RVA: 0x0003BA34 File Offset: 0x0003AA34
		public IEnumerable<int> WarningsSet
		{
			get
			{
				List<int> list = new List<int>();
				foreach (MemberInfo memberInfo in typeof(MessageId).GetMembers(BindingFlags.Static | BindingFlags.Public))
				{
					int num = (int)((FieldInfo)memberInfo).GetValue(memberInfo.GetType());
					if (num > 0 && APEnvironmentFacade.Instance.WarningHelper.IsWarning((MessageId)num) && (num != 312 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300) && !APEnvironmentFacade.Instance.WarningHelper.IsOEMDisabledId((MessageId)num))
					{
						list.Add(num);
					}
				}
				return list;
			}
		}

		// Token: 0x17000575 RID: 1397
		// (get) Token: 0x06001412 RID: 5138 RVA: 0x0003BAD1 File Offset: 0x0003AAD1
		// (set) Token: 0x06001413 RID: 5139 RVA: 0x0003BADC File Offset: 0x0003AADC
		public IEnumerable<int> DisabledWarningsSet
		{
			get
			{
				return this.GetDisabledWarningIds();
			}
			set
			{
				HashSet<int> hsDisabledWarningIds = new HashSet<int>(value);
				this.WriteDisabledWarningIds(hsDisabledWarningIds);
			}
		}

		// Token: 0x06001414 RID: 5140 RVA: 0x0003BAF7 File Offset: 0x0003AAF7
		public string GetLocalizedWarningText(int iWarningId)
		{
			return CompilerProxy.GetStringOfMessageId((MessageId)iWarningId);
		}

		// Token: 0x06001415 RID: 5141 RVA: 0x0003BB00 File Offset: 0x0003AB00
		private static string GetAsCsv(ICollection<int> hsWithMessageIds)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < hsWithMessageIds.Count; i++)
			{
				string value = string.Format("{0}", hsWithMessageIds.ElementAt(i));
				stringBuilder.Append(value);
				if (i < hsWithMessageIds.Count - 1)
				{
					stringBuilder.Append(",");
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06001416 RID: 5142 RVA: 0x0003BB60 File Offset: 0x0003AB60
		public bool IsWarningAsError(MessageId messageId)
		{
			ICollection<int> warningAsErrorIds = this.GetWarningAsErrorIds();
			return warningAsErrorIds != null && warningAsErrorIds.Contains((int)messageId);
		}

		// Token: 0x06001417 RID: 5143 RVA: 0x0003BB80 File Offset: 0x0003AB80
		public ICollection<int> GetWarningAsErrorIds()
		{
			if (this._hsWarningAsErrorIds == null)
			{
				this.ReadWarningAsErrorIds();
			}
			return this._hsWarningAsErrorIds;
		}

		// Token: 0x06001418 RID: 5144 RVA: 0x0003BB98 File Offset: 0x0003AB98
		public void WriteWarningAsErrorIds(ICollection<int> hsWarningAsErrorIds)
		{
			string asCsv = WarningHelper.GetAsCsv(hsWarningAsErrorIds);
			if (!string.IsNullOrEmpty(asCsv))
			{
				WarningHelper.WarningAsErrorIdsKey[WarningHelper.VALUE_WARNING_AS_ERROR_IDs] = asCsv;
				this._hsWarningAsErrorIds = new HashSet<int>(hsWarningAsErrorIds);
				return;
			}
			APEnvironmentFacade.Instance.DeleteSubKey(OptionRoot.Project, "{eeeeeeee-3909-4298-8022-501ac3238667}");
			this._hsWarningAsErrorIds = null;
		}

		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x06001419 RID: 5145 RVA: 0x0003BBE8 File Offset: 0x0003ABE8
		// (set) Token: 0x0600141A RID: 5146 RVA: 0x0003BBF0 File Offset: 0x0003ABF0
		public IEnumerable<int> WarningAsErrorSet
		{
			get
			{
				return this.GetWarningAsErrorIds();
			}
			set
			{
				HashSet<int> hsWarningAsErrorIds = new HashSet<int>(value);
				this.WriteWarningAsErrorIds(hsWarningAsErrorIds);
			}
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x0600141B RID: 5147 RVA: 0x0003BC0B File Offset: 0x0003AC0B
		private static IOptionKey DisabledWarningIdsKey
		{
			get
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.Project, WarningHelper.SUB_KEY_DISABLED_WARNING_IDs);
			}
		}

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x0600141C RID: 5148 RVA: 0x0003BC1D File Offset: 0x0003AC1D
		private static IOptionKey WarningAsErrorIdsKey
		{
			get
			{
				return APEnvironmentFacade.Instance.CreateSubKey(OptionRoot.Project, "{eeeeeeee-3909-4298-8022-501ac3238667}");
			}
		}

		// Token: 0x04000482 RID: 1154
		public readonly string OEM_CUSTOMIZATION_SECTION = "LanguageModelManager";

		// Token: 0x04000483 RID: 1155
		public readonly string OEM_CUSTOMIZATION_KEY = "DisabledWarningIds";

		// Token: 0x04000484 RID: 1156
		private static readonly string SUB_KEY_DISABLED_WARNING_IDs = "{8F99A816-E488-41E4-9FA3-846536012284}";

		// Token: 0x04000485 RID: 1157
		private const string SUB_KEY_WARNING_AS_ERROR_IDs = "{eeeeeeee-3909-4298-8022-501ac3238667}";

		// Token: 0x04000486 RID: 1158
		public static readonly string VALUE_DISABLED_WARNING_IDs = "DisabledWarningIds";

		// Token: 0x04000487 RID: 1159
		public static readonly string VALUE_WARNING_AS_ERROR_IDs = "WarningAsErrorIds";

		// Token: 0x04000488 RID: 1160
		private ICollection<int> _hsDisabledWarningIds;

		// Token: 0x04000489 RID: 1161
		private ICollection<int> _hsWarningAsErrorIds;

		// Token: 0x0400048A RID: 1162
		private HashSet<int> _disabledOEMIds;
	}
}
