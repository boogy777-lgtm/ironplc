using System;
using System.Collections.Generic;
using \u0003;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u000E
{
	// Token: 0x020002EB RID: 747
	internal sealed class \u0015
	{
		// Token: 0x06002D8F RID: 11663 RVA: 0x000A761C File Offset: 0x000A581C
		private \u0015(_ICompileContext \u001C\u0004)
		{
			this.\u0001 = \u001C\u0004;
		}

		// Token: 0x06002D90 RID: 11664 RVA: 0x000A7644 File Offset: 0x000A5844
		internal static bool \u0001(_ICompileContext \u0002)
		{
			return \u0002.GetTargetSettings() == null || !global::\u0016.\u0004.CheckMultipleTaskOutputWrite.GetBoolValue(\u0002.GetTargetSettings()) || new global::\u000E.\u0015(\u0002).\u0001();
		}

		// Token: 0x06002D91 RID: 11665 RVA: 0x000A7670 File Offset: 0x000A5870
		private bool \u0001()
		{
			bool flag = true;
			try
			{
				IDirectVariable[] allDirectVariables = this.\u0001._GetDirectVariableTable().AllDirectVariables;
				this.\u0001(allDirectVariables);
				foreach (int num in this.\u0001.Keys)
				{
					LList<IDirectVariable> u = this.\u0001[num];
					flag = (this.\u0001(u) && flag);
					flag = (this.\u0002() && flag);
				}
			}
			catch
			{
				return true;
			}
			return flag;
		}

		// Token: 0x06002D92 RID: 11666 RVA: 0x000A7718 File Offset: 0x000A5918
		private bool \u0002()
		{
			bool flag = true;
			if (this.\u0001.Keys.Count > 1)
			{
				bool flag2 = true;
				_ISignature u = null;
				foreach (KeyValuePair<byte, IDirectVariable> keyValuePair in this.\u0001)
				{
					IDirectVariable value = keyValuePair.Value;
					byte key = keyValuePair.Key;
					IAddressCrossReference[] crossReferencesOfDirectVariable = this.\u0001._GetDirectVariableTable().GetCrossReferencesOfDirectVariable(value);
					string taskName = this.\u0001.TaskList[(int)key].TaskName;
					int num = value.ToString().Length;
					foreach (IAddressCrossReference addressCrossReference in crossReferencesOfDirectVariable)
					{
						_ISignature isignature = this.\u0001[addressCrossReference.CodeId];
						string u2;
						Guid guid;
						num = global::\u000E.\u0015.\u0001(num, addressCrossReference, isignature, out u2, out guid);
						if (flag2)
						{
							num = global::\u000E.\u0015.\u0001(value, num, isignature, u2, guid);
							u = isignature;
							flag2 = false;
						}
						flag = (global::\u000E.\u0015.\u0001(u, key, taskName, num, addressCrossReference, isignature, guid) && flag);
					}
				}
			}
			return flag;
		}

		// Token: 0x06002D93 RID: 11667 RVA: 0x000A7844 File Offset: 0x000A5A44
		private static bool \u0001(_ISignature \u0002, byte \u0003, string \u0004, int \u0005, IAddressCrossReference \u0006, _ISignature \u0007, Guid \u0008)
		{
			bool result = true;
			foreach (IAddressCodePosition addressCodePosition in \u0006.Positions)
			{
				if ((addressCodePosition.Access & AccessFlag.Write) == AccessFlag.Write && \u0007.TaskReferenceList.Length == 1 && \u0007.TaskReferenceList[0] == \u0003)
				{
					_ISourcePosition u = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0007.LibraryPath), \u0008, addressCodePosition.EditorPosition, addressCodePosition.PositionOffset, (short)\u0005);
					string u2 = string.Format(\u0081.\u0002.Err_RelatedPositionTaskX, \u0004);
					\u0002.AddError(\u0019.\u0003.\u0001(u, u2, Severity.Information, MessageId.Inf_RelatedPosition));
					result = false;
				}
			}
			return result;
		}

		// Token: 0x06002D94 RID: 11668 RVA: 0x000A78F0 File Offset: 0x000A5AF0
		private static int \u0001(IDirectVariable \u0002, int \u0003, _ISignature \u0004, string \u0005, Guid \u0006)
		{
			string u = global::\u0003.\u0006.\u0001(MessageId.Err_OutputByteWrittenFromDifferentTasks, Array.Empty<object>());
			MessageId u2 = MessageId.Err_OutputByteWrittenFromDifferentTasks;
			_ISourcePosition isourcePosition = \u0019.\u0003.\u0001();
			isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(\u0004.LibraryPath), \u0006);
			if (!string.IsNullOrEmpty(\u0005))
			{
				string u3 = string.Format(\u0081.\u0002.Err_MappedVarWrittenInDiffTasks, \u0005, \u0002);
				\u0004.AddError(\u0019.\u0003.\u0001(isourcePosition, u3, Severity.Error, MessageId.Err_MappedVarWrittenInDiffTasks));
				\u0003 = \u0005.Length;
			}
			else
			{
				\u0004.AddError(\u0019.\u0003.\u0001(isourcePosition, u, Severity.Error, u2));
			}
			return \u0003;
		}

		// Token: 0x06002D95 RID: 11669 RVA: 0x000A7980 File Offset: 0x000A5B80
		private static int \u0001(int \u0002, IAddressCrossReference \u0003, _ISignature \u0004, out string \u0005, out Guid \u0006)
		{
			\u0005 = null;
			IAddressCodePosition[] positions = \u0003.Positions;
			for (int i = 0; i < positions.Length; i++)
			{
				_IAddressCodePosition iaddressCodePosition = positions[i] as _IAddressCodePosition;
				if (iaddressCodePosition != null)
				{
					if (\u0005 == null)
					{
						\u0005 = iaddressCodePosition.VariableName;
					}
					else if (\u0005 != iaddressCodePosition.VariableName)
					{
						\u0005 = string.Empty;
					}
				}
			}
			if (!string.IsNullOrEmpty(\u0005))
			{
				\u0002 = \u0005.Length;
			}
			\u0006 = \u0004.ObjectGuid;
			if (\u0004.Name == IdentifierConstants.MainSignatureName)
			{
				\u0006 = \u0004.ParentObjectGuid;
			}
			return \u0002;
		}

		// Token: 0x06002D96 RID: 11670 RVA: 0x000A7A14 File Offset: 0x000A5C14
		private bool \u0001(LList<IDirectVariable> \u0002)
		{
			bool flag = true;
			this.\u0001.Clear();
			foreach (IDirectVariable directVariable in \u0002)
			{
				foreach (IAddressCrossReference addressCrossReference in this.\u0001._GetDirectVariableTable().GetCrossReferencesOfDirectVariable(directVariable))
				{
					IAddressCodePosition u = Array.Find<IAddressCodePosition>(addressCrossReference.Positions, new Predicate<IAddressCodePosition>(global::\u000E.\u0015.\u0001));
					flag = (this.\u0001(directVariable, addressCrossReference, u) && flag);
				}
			}
			return flag;
		}

		// Token: 0x06002D97 RID: 11671 RVA: 0x000A7AB8 File Offset: 0x000A5CB8
		private static bool \u0001(IAddressCodePosition \u0002)
		{
			return (\u0002.Access & AccessFlag.Write) == AccessFlag.Write;
		}

		// Token: 0x06002D98 RID: 11672 RVA: 0x000A7AC8 File Offset: 0x000A5CC8
		private bool \u0001(IDirectVariable \u0002, IAddressCrossReference \u0003, IAddressCodePosition \u0004)
		{
			if (\u0004 == null)
			{
				return true;
			}
			_ISignature isignature = this.\u0001[\u0003.CodeId];
			if (isignature.TaskReferenceList.Length > 1)
			{
				_ISourcePosition u = \u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid, \u0004.EditorPosition, \u0004.PositionOffset, 0);
				string u2 = global::\u0003.\u0006.\u0001(MessageId.Err_POUCalledFromDifferentTasks, new object[]
				{
					isignature.OrgName,
					\u0002.ToString()
				});
				isignature.AddError(\u0019.\u0003.\u0001(u, u2, Severity.Error, MessageId.Err_POUCalledFromDifferentTasks));
				return false;
			}
			foreach (byte b in isignature.TaskReferenceList)
			{
				this.\u0001[b] = \u0002;
			}
			return true;
		}

		// Token: 0x06002D99 RID: 11673 RVA: 0x000A7B90 File Offset: 0x000A5D90
		private void \u0001(IDirectVariable[] \u0002)
		{
			foreach (IDirectVariable directVariable in \u0002)
			{
				if (directVariable.Location == DirectVariableLocation.Output)
				{
					IMessage message;
					bool flag;
					IDataLocation dataLocation = Locator.\u0001(this.\u0001, out message, out flag, null, directVariable, null);
					LList<IDirectVariable> llist;
					if (!this.\u0001.TryGetValue(dataLocation.Offset, ref llist))
					{
						llist = new LList<IDirectVariable>();
						this.\u0001[dataLocation.Offset] = llist;
					}
					llist.Add(directVariable);
				}
			}
		}

		// Token: 0x040008A9 RID: 2217
		private readonly _ICompileContext \u0001;

		// Token: 0x040008AA RID: 2218
		private readonly LDictionary<byte, IDirectVariable> \u0001 = new LDictionary<byte, IDirectVariable>();

		// Token: 0x040008AB RID: 2219
		private readonly LDictionary<int, LList<IDirectVariable>> \u0001 = new LDictionary<int, LList<IDirectVariable>>();
	}
}
