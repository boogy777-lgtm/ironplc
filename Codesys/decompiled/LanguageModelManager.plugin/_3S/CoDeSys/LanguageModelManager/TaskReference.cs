using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000074 RID: 116
	[TypeGuid("{386b9a47-7c5b-4028-a614-ca13bf4c0ae9}")]
	[StorageVersion("3.3.0.0")]
	public class TaskReference : ItemReference, _ITaskReference, _IItemReference, _IPragmaExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression
	{
		// Token: 0x0600077D RID: 1917 RVA: 0x0000E359 File Offset: 0x0000D359
		public TaskReference()
		{
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x0000E361 File Offset: 0x0000D361
		public TaskReference(IToken token) : base(token)
		{
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x00012E89 File Offset: 0x00011E89
		public TaskReference(IToken token, string stTaskName) : base(token)
		{
			this.m_stTaskName = stTaskName;
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000780 RID: 1920 RVA: 0x00012E99 File Offset: 0x00011E99
		// (set) Token: 0x06000781 RID: 1921 RVA: 0x00012EA1 File Offset: 0x00011EA1
		public string TaskName
		{
			get
			{
				return this.m_stTaskName;
			}
			set
			{
				this.m_stTaskName = value;
			}
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x00012EAA File Offset: 0x00011EAA
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x00012EB3 File Offset: 0x00011EB3
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x06000784 RID: 1924 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public override void AcceptVisitor(IExprVisitor visitor)
		{
		}

		// Token: 0x06000785 RID: 1925 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return false;
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x00012EBC File Offset: 0x00011EBC
		public override _IExprement Duplicate()
		{
			TaskReference taskReference = new TaskReference();
			taskReference.m_stTaskName = this.m_stTaskName;
			this.DuplicateCommon(taskReference);
			return taskReference;
		}

		// Token: 0x040000FE RID: 254
		[DefaultSerialization("TaskName")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stTaskName;
	}
}
