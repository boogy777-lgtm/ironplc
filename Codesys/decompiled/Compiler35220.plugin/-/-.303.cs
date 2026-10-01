using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u0011;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0019
{
	// Token: 0x0200032A RID: 810
	internal sealed class \u0011
	{
		// Token: 0x06003037 RID: 12343 RVA: 0x000B73A4 File Offset: 0x000B55A4
		public \u0011(\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x170007FF RID: 2047
		// (get) Token: 0x06003038 RID: 12344 RVA: 0x000B73B4 File Offset: 0x000B55B4
		private \u001B CompileInformation { get; }

		// Token: 0x17000800 RID: 2048
		// (get) Token: 0x06003039 RID: 12345 RVA: 0x000B73BC File Offset: 0x000B55BC
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x0600303A RID: 12346 RVA: 0x000B73CC File Offset: 0x000B55CC
		internal void \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.POUType == Operator.VarGlobal && \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_TASKLOCALGVL))
			{
				this.\u0001(\u0002);
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					if (ivariable.IsProperty)
					{
						\u0002.AddMessage(Severity.Error, MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
						{
							\u0002.OrgName,
							Operator.Property.ToString()
						});
					}
					else if (ivariable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
					{
						\u0002.AddMessage(ivariable._SourcePosition, Severity.Error, MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
						{
							\u0002.OrgName,
							Operator.Constant.ToString()
						});
					}
					else
					{
						_IVariable u = null;
						if (this.\u0001(ivariable, ivariable._Type, \u0003, out u))
						{
							string u2 = \u0019.\u0011.\u0001(\u0002, ivariable, u);
							_ISourcePosition sourcePosition = ivariable._SourcePosition;
							sourcePosition.SetObjectIdentification(-1, \u0002.ObjectGuid);
							\u0002.AddError(\u0019.\u0003.\u0001(sourcePosition, u2, Severity.Error, MessageId.Err_NotAllowedInTaskLocalVariables));
						}
					}
				}
			}
		}

		// Token: 0x0600303B RID: 12347 RVA: 0x000B7504 File Offset: 0x000B5704
		private static string \u0001(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			string result = string.Empty;
			if (\u0004 != null)
			{
				if (\u0004.Address != null)
				{
					result = \u0019.\u0011.\u0004(\u0002, \u0003, \u0004);
				}
				else if (\u0004.GetFlag(VarFlag.Persistent) || \u0004.GetFlag(VarFlag.Retain) || \u0004.GetFlag(VarFlag.LocalPersistent))
				{
					result = \u0019.\u0011.\u0003(\u0002, \u0003, \u0004);
				}
				else if (\u0004.Type != null)
				{
					result = \u0019.\u0011.\u0002(\u0002, \u0003, \u0004);
				}
			}
			else
			{
				result = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003._Type.ToString()
				});
			}
			return result;
		}

		// Token: 0x0600303C RID: 12348 RVA: 0x000B75A4 File Offset: 0x000B57A4
		private static string \u0002(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			string text;
			if (\u0004 == \u0003)
			{
				text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003._Type.ToString()
				});
			}
			else
			{
				text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003._Type.ToString()
				});
				text = string.Concat(new string[]
				{
					text,
					" (",
					global::\u0011.\u0001.TextContains,
					" ",
					\u0004.Type.ToString(),
					")"
				});
			}
			return text;
		}

		// Token: 0x0600303D RID: 12349 RVA: 0x000B7648 File Offset: 0x000B5848
		private static string \u0003(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			string text = VarFlag.Retain.ToString();
			if (\u0003.GetFlag(VarFlag.Persistent) || \u0003.GetFlag(VarFlag.LocalPersistent))
			{
				text = VarFlag.Persistent.ToString();
			}
			string text2;
			if (\u0004 == \u0003)
			{
				text2 = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					text
				});
			}
			else
			{
				text2 = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003._Type.ToString()
				});
				text2 = string.Concat(new string[]
				{
					text2,
					" (",
					global::\u0011.\u0001.TextContains,
					" ",
					text,
					")"
				});
			}
			return text2;
		}

		// Token: 0x0600303E RID: 12350 RVA: 0x000B7720 File Offset: 0x000B5920
		private static string \u0004(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004)
		{
			string text;
			if (\u0004 == \u0003)
			{
				text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003.Address.ToString()
				});
			}
			else
			{
				text = global::\u0003.\u0006.\u0001(MessageId.Err_NotAllowedInTaskLocalVariables, new object[]
				{
					\u0002.OrgName,
					\u0003._Type.ToString()
				});
				text = string.Concat(new string[]
				{
					text,
					" (",
					global::\u0011.\u0001.TextContains,
					" ",
					\u0004.Address.ToString(),
					")"
				});
			}
			return text;
		}

		// Token: 0x0600303F RID: 12351 RVA: 0x000B77C4 File Offset: 0x000B59C4
		private void \u0001(_ISignature \u0002)
		{
			\u0019.\u0011.\u0001 u = new \u0019.\u0011.\u0001();
			u.\u0001 = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_TASKLOCALGVL);
			if (!string.IsNullOrEmpty(u.\u0001) && new List<ITaskInfo>(this.ComconNew.AllTasks).Find(new Predicate<ITaskInfo>(u.\u0001)) == null)
			{
				_ISourcePosition u2 = \u0019.\u0003.\u0001(-1, \u0002.ObjectGuid, 0L, 0, 0);
				string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_TaskLocalVariablesWriterTaskNotDefined, new object[]
				{
					\u0002.OrgName
				});
				\u0002.AddError(\u0019.\u0003.\u0001(u2, u3, Severity.Error, MessageId.Err_TaskLocalVariablesWriterTaskNotDefined));
			}
		}

		// Token: 0x06003040 RID: 12352 RVA: 0x000B7858 File Offset: 0x000B5A58
		private bool \u0001(_IVariable \u0002, _IType \u0003, IScope5 \u0004, out _IVariable \u0005)
		{
			HashSet<int> u = new HashSet<int>();
			return this.\u0001(\u0002, \u0003, \u0004, out \u0005, u);
		}

		// Token: 0x06003041 RID: 12353 RVA: 0x000B7878 File Offset: 0x000B5A78
		private bool \u0001(_IVariable \u0002, _IType \u0003, IScope5 \u0004, out _IVariable \u0005, HashSet<int> \u0006)
		{
			\u0005 = null;
			if (\u0003 == null)
			{
				return false;
			}
			if (\u0002 != null && (\u0002.Address != null || \u0002.GetFlag(VarFlag.Persistent) || \u0002.GetFlag(VarFlag.Retain) || \u0002.GetFlag(VarFlag.LocalPersistent)))
			{
				\u0005 = \u0002;
				return true;
			}
			if (\u0003.Class == TypeClass.Pointer || \u0003.Class == TypeClass.Reference)
			{
				\u0005 = \u0002;
				return true;
			}
			if (\u0003.Class == TypeClass.Array)
			{
				return this.\u0001(\u0002, ((_IArrayType)\u0003).BaseType as _IType, \u0004, out \u0005, \u0006);
			}
			return \u0003.Class == TypeClass.Userdef && this.\u0002(\u0002, \u0003, \u0004, out \u0005, \u0006);
		}

		// Token: 0x06003042 RID: 12354 RVA: 0x000B7928 File Offset: 0x000B5B28
		private bool \u0002(_IVariable \u0002, _IType \u0003, IScope5 \u0004, out _IVariable \u0005, HashSet<int> \u0006)
		{
			_IUserdefType iuserdefType = \u0003 as _IUserdefType;
			\u0005 = null;
			if (\u0006.Contains(iuserdefType.SignatureId))
			{
				return false;
			}
			\u0006.Add(iuserdefType.SignatureId);
			_ISignature isignature = \u0004[iuserdefType.SignatureId] as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			if (isignature.POUType == Operator.FunctionBlock)
			{
				\u0005 = \u0002;
				return true;
			}
			foreach (_IVariable ivariable in isignature.AllVariables)
			{
				if (this.\u0001(ivariable, ivariable._Type, \u0004, out \u0005, \u0006))
				{
					\u0005 = ivariable;
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000933 RID: 2355
		[CompilerGenerated]
		private readonly \u001B \u0001;

		// Token: 0x0200032B RID: 811
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06003044 RID: 12356 RVA: 0x000B79E8 File Offset: 0x000B5BE8
			internal bool \u0001(ITaskInfo \u0002)
			{
				return \u0002.TaskName.ToUpperInvariant() == this.\u0001.ToUpperInvariant();
			}

			// Token: 0x04000934 RID: 2356
			public string \u0001;
		}
	}
}
