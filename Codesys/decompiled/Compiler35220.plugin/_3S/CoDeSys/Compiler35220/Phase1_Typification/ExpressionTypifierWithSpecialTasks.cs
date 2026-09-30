using System;
using System.Runtime.CompilerServices;
using \u0014;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0082;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200032C RID: 812
	internal sealed class ExpressionTypifierWithSpecialTasks : TypifierAndCrossReferenceCollector
	{
		// Token: 0x06003045 RID: 12357 RVA: 0x000B7A08 File Offset: 0x000B5C08
		public ExpressionTypifierWithSpecialTasks()
		{
		}

		// Token: 0x06003046 RID: 12358 RVA: 0x000B7A18 File Offset: 0x000B5C18
		public ExpressionTypifierWithSpecialTasks(IScope5 supscope, _ICompileContext comcon, _ICompiledPOU cpou) : base(supscope, comcon, cpou)
		{
		}

		// Token: 0x06003047 RID: 12359 RVA: 0x000B7A2C File Offset: 0x000B5C2C
		public ExpressionTypifierWithSpecialTasks(IScope5 supscope, _ICompileContext comcon, bool bInterpretPragmas, _ICompiledPOU cpou) : base(supscope, comcon, cpou)
		{
			this.\u0001(supscope, comcon, bInterpretPragmas, cpou);
		}

		// Token: 0x06003048 RID: 12360 RVA: 0x000B7A4C File Offset: 0x000B5C4C
		public ExpressionTypifierWithSpecialTasks(IScope5 supscope, _ICompileContext comcon, bool bInterpretPragmas, bool bAllowUndecidedPragmaExpression, _ICompiledPOU cpou) : base(supscope, comcon, cpou)
		{
			this.\u0001(supscope, comcon, bInterpretPragmas, cpou);
		}

		// Token: 0x06003049 RID: 12361 RVA: 0x000B7A6C File Offset: 0x000B5C6C
		public ExpressionTypifierWithSpecialTasks(IScope5 supscope, _ICompileContext comcon, bool bInterpretPragmas, _ISignature sign, _IVariable var) : base(supscope, comcon, null)
		{
			this.\u0001(supscope, comcon, bInterpretPragmas, sign, var);
		}

		// Token: 0x0600304A RID: 12362 RVA: 0x000B7A8C File Offset: 0x000B5C8C
		public ExpressionTypifierWithSpecialTasks(IScope5 supscope, _ICompileContext comcon, ICompiledType typeExpected, bool bInterpretPragmas, bool bContributeToCompile, _ICompiledPOU cpou) : base(supscope, comcon, cpou)
		{
			this.\u0001(supscope, comcon, typeExpected, bInterpretPragmas, bContributeToCompile, cpou);
		}

		// Token: 0x0600304B RID: 12363 RVA: 0x000B7AB0 File Offset: 0x000B5CB0
		internal new void \u0001(IScope5 \u0002, _ICompileContext \u0003, bool \u0004, _ICompiledPOU \u0005)
		{
			base.\u0001(\u0002, null, AccessFlag.Unknown, null);
			this.\u0001 = \u0003;
			base.InterpretPragmas = \u0004;
			this.\u0001 = \u0005;
			if (\u0002 != null)
			{
				base.TypeChecker = new global::\u0014.\u0012(\u0002 as _IScope2, \u0003, this.\u0001);
			}
		}

		// Token: 0x0600304C RID: 12364 RVA: 0x000B7AF0 File Offset: 0x000B5CF0
		private new void \u0001(IScope5 \u0002, _ICompileContext \u0003, bool \u0004, _ISignature \u0005, _IVariable \u0006)
		{
			base.\u0001(\u0002, null, AccessFlag.Unknown, null);
			this.\u0001 = \u0003;
			base.InterpretPragmas = \u0004;
			this.\u0001 = \u0005;
			this.\u0001 = \u0006;
		}

		// Token: 0x0600304D RID: 12365 RVA: 0x000B7B1C File Offset: 0x000B5D1C
		private new void \u0001(IScope5 \u0002, _ICompileContext \u0003, ICompiledType \u0004, bool \u0005, bool \u0006, _ICompiledPOU \u0007)
		{
			base.\u0001(\u0002, \u0004, AccessFlag.Unknown, null);
			this.\u0001 = \u0003;
			base.InterpretPragmas = \u0005;
			this.ContributeToCompile = \u0006;
			this.NoCrossReferences = !\u0006;
			this.\u0001 = \u0007;
		}

		// Token: 0x17000801 RID: 2049
		// (get) Token: 0x0600304E RID: 12366 RVA: 0x000B7B54 File Offset: 0x000B5D54
		// (set) Token: 0x0600304F RID: 12367 RVA: 0x000B7B5C File Offset: 0x000B5D5C
		public bool IgnoreErrors { get; set; }

		// Token: 0x17000802 RID: 2050
		// (get) Token: 0x06003050 RID: 12368 RVA: 0x000B7B68 File Offset: 0x000B5D68
		// (set) Token: 0x06003051 RID: 12369 RVA: 0x000B7B70 File Offset: 0x000B5D70
		public bool ContributeToCompile { get; set; }

		// Token: 0x17000803 RID: 2051
		// (get) Token: 0x06003052 RID: 12370 RVA: 0x000B7B7C File Offset: 0x000B5D7C
		// (set) Token: 0x06003053 RID: 12371 RVA: 0x000B7B84 File Offset: 0x000B5D84
		public bool CheckForFastOnlineChange { get; set; }

		// Token: 0x17000804 RID: 2052
		// (get) Token: 0x06003054 RID: 12372 RVA: 0x000B7B90 File Offset: 0x000B5D90
		// (set) Token: 0x06003055 RID: 12373 RVA: 0x000B7B98 File Offset: 0x000B5D98
		public bool Optimize { get; set; }

		// Token: 0x17000805 RID: 2053
		// (get) Token: 0x06003056 RID: 12374 RVA: 0x000B7BA4 File Offset: 0x000B5DA4
		// (set) Token: 0x06003057 RID: 12375 RVA: 0x000B7BAC File Offset: 0x000B5DAC
		public bool FastOnlineChangeOK { get; set; } = true;

		// Token: 0x17000806 RID: 2054
		// (get) Token: 0x06003058 RID: 12376 RVA: 0x000B7BB8 File Offset: 0x000B5DB8
		// (set) Token: 0x06003059 RID: 12377 RVA: 0x000B7BC0 File Offset: 0x000B5DC0
		public bool NoCrossReferences { get; set; }

		// Token: 0x17000807 RID: 2055
		// (get) Token: 0x0600305A RID: 12378 RVA: 0x000B7BCC File Offset: 0x000B5DCC
		// (set) Token: 0x0600305B RID: 12379 RVA: 0x000B7BD4 File Offset: 0x000B5DD4
		public bool TreatReferenceAsPointer { get; set; }

		// Token: 0x17000808 RID: 2056
		// (get) Token: 0x0600305C RID: 12380 RVA: 0x000B7BE0 File Offset: 0x000B5DE0
		// (set) Token: 0x0600305D RID: 12381 RVA: 0x000B7BE8 File Offset: 0x000B5DE8
		public override bool InterfaceAsInterface { get; set; }

		// Token: 0x0600305E RID: 12382 RVA: 0x000B7BF4 File Offset: 0x000B5DF4
		protected override void \u0001(_IExprement \u0002, MessageId \u0003, params object[] \u0004)
		{
			if (!this.IgnoreErrors)
			{
				base.\u0001(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x0600305F RID: 12383 RVA: 0x000B7C08 File Offset: 0x000B5E08
		protected override void \u0001(_ICompiledPOU \u0002, CompiledPOUFlags \u0003)
		{
			if (!this.ContributeToCompile || this.CheckForFastOnlineChange || this.Optimize)
			{
				if (this.CheckForFastOnlineChange && !\u0002.GetFlag(\u0003))
				{
					this.FastOnlineChangeOK = false;
					return;
				}
			}
			else
			{
				base.\u0001(\u0002, \u0003);
			}
		}

		// Token: 0x06003060 RID: 12384 RVA: 0x000B7C44 File Offset: 0x000B5E44
		protected override void \u0001(_ISignature \u0002, SignatureFlag \u0003)
		{
			if (!this.ContributeToCompile || this.CheckForFastOnlineChange || this.Optimize)
			{
				if (this.CheckForFastOnlineChange && !\u0002.GetFlag(\u0003))
				{
					this.FastOnlineChangeOK = false;
					return;
				}
			}
			else
			{
				base.\u0001(\u0002, \u0003);
			}
		}

		// Token: 0x06003061 RID: 12385 RVA: 0x000B7C80 File Offset: 0x000B5E80
		internal new void \u0001(IVariable \u0002, ISignature \u0003, ICompiledPOU \u0004)
		{
			if (!this.CheckForFastOnlineChange || !this.FastOnlineChangeOK)
			{
				return;
			}
			bool flag = \u0002.Address != null;
			\u0017.\u0004 u = new \u0017.\u0004(\u0002.Id, \u0003.Id);
			if (base.VarsToCheck != null && base.VarsToCheck.ContainsKey(u))
			{
				flag = true;
			}
			if (\u0002.GetFlag(VarFlag.Retain) && this.\u0001 != null && this.\u0001.RetainInCycle)
			{
				flag = true;
			}
			if (\u0002.GetFlag(VarFlag.Persistent) && this.\u0001 != null && this.\u0001.DoPersistentCode)
			{
				flag = true;
			}
			if (flag)
			{
				ISignature signature = base._Scope[\u0004.SignatureId];
				byte[] taskReferenceList = signature.TaskReferenceList;
				LDictionary<byte, byte> ldictionary = new LDictionary<byte, byte>();
				foreach (ICrossReference crossReference in \u0002.CrossReferences)
				{
					ISignature signature2 = base._Scope[crossReference.CodeId];
					if (signature2 != null)
					{
						if (signature2.Id == signature.Id)
						{
							return;
						}
						foreach (byte b in signature2.TaskReferenceList)
						{
							ldictionary[b] = b;
						}
					}
				}
				foreach (byte b2 in taskReferenceList)
				{
					if (!ldictionary.ContainsKey(b2))
					{
						this.FastOnlineChangeOK = false;
					}
				}
			}
		}

		// Token: 0x06003062 RID: 12386 RVA: 0x000B7DEC File Offset: 0x000B5FEC
		protected override void \u0001(_IVariable \u0002, _ISignature \u0003, int \u0004)
		{
			if (this.NoCrossReferences)
			{
				return;
			}
			this.\u0001(\u0002, \u0003, this.\u0001);
			base.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06003063 RID: 12387 RVA: 0x000B7E10 File Offset: 0x000B6010
		protected override void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			if (!this.NoCrossReferences)
			{
				base.\u0001(\u0002, \u0003);
			}
		}

		// Token: 0x06003064 RID: 12388 RVA: 0x000B7E24 File Offset: 0x000B6024
		protected override void \u0001(_IVariableExpression \u0002, IVariable \u0003, ISignature \u0004)
		{
			if (!this.NoCrossReferences)
			{
				base.\u0001(\u0002, \u0003, \u0004);
			}
		}

		// Token: 0x06003065 RID: 12389 RVA: 0x000B7E38 File Offset: 0x000B6038
		protected override void \u0001(ISignature \u0002)
		{
			if (!this.NoCrossReferences)
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x06003066 RID: 12390 RVA: 0x000B7E4C File Offset: 0x000B604C
		protected override void \u0001(IVariable \u0002, ISignature \u0003)
		{
			if (!this.NoCrossReferences)
			{
				base.\u0001(\u0002, \u0003);
			}
		}

		// Token: 0x06003067 RID: 12391 RVA: 0x000B7E60 File Offset: 0x000B6060
		protected override void \u0001(_ISignature \u0002, int \u0003)
		{
			if (!this.NoCrossReferences)
			{
				base.\u0001(\u0002, \u0003);
			}
		}

		// Token: 0x06003068 RID: 12392 RVA: 0x000B7E74 File Offset: 0x000B6074
		protected override void \u0001(_ISignature \u0002)
		{
			int num = base.\u0001();
			if (this.CheckForFastOnlineChange)
			{
				int[] callerIds = \u0002.CallerIds;
				for (int i = 0; i < callerIds.Length; i++)
				{
					if (callerIds[i] == num)
					{
						return;
					}
				}
				_ISignature isignature = \u0002.Duplicate(true);
				this.\u0001.ReplaceSignature(\u0002, isignature);
				\u0002 = isignature;
			}
			if (!this.NoCrossReferences)
			{
				\u0002.AddCaller(num);
			}
		}

		// Token: 0x06003069 RID: 12393 RVA: 0x000B7ED4 File Offset: 0x000B60D4
		protected override void \u0002(_ISignature \u0002, int \u0003)
		{
			if (this.CheckForFastOnlineChange)
			{
				int[] calleeIds = \u0002.CalleeIds;
				for (int i = 0; i < calleeIds.Length; i++)
				{
					if (calleeIds[i] == \u0003)
					{
						return;
					}
				}
				_ISignature isignature = \u0002.Duplicate(true);
				this.\u0001.ReplaceSignature(\u0002, isignature);
				\u0002 = isignature;
			}
			if (!this.NoCrossReferences)
			{
				\u0002.AddCallee(\u0003, false);
			}
		}

		// Token: 0x0600306A RID: 12394 RVA: 0x000B7F30 File Offset: 0x000B6130
		protected override void \u0001(_ISignature \u0002, int \u0003, bool \u0004)
		{
			if (this.CheckForFastOnlineChange)
			{
				int[] calleeIds = \u0002.CalleeIds;
				for (int i = 0; i < calleeIds.Length; i++)
				{
					if (calleeIds[i] == \u0003)
					{
						return;
					}
				}
				_ISignature isignature = \u0002.Duplicate(true);
				this.\u0001.ReplaceSignature(\u0002, isignature);
				\u0002 = isignature;
			}
			if (!this.NoCrossReferences)
			{
				\u0002.AddCallee(\u0003, \u0004);
			}
		}

		// Token: 0x0600306B RID: 12395 RVA: 0x000B7F8C File Offset: 0x000B618C
		protected override void \u0001(_IOperatorExpression \u0002)
		{
			if (this.ContributeToCompile)
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x0600306C RID: 12396 RVA: 0x000B7FA0 File Offset: 0x000B61A0
		protected override void \u0001(_IConversionExpression \u0002)
		{
			if (this.ContributeToCompile)
			{
				base.\u0001(\u0002);
			}
		}

		// Token: 0x0600306D RID: 12397 RVA: 0x000B7FB4 File Offset: 0x000B61B4
		public override void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			base.\u0001(AccessFlag.Read);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				bool u = this.InterfaceAsInterface;
				this.InterfaceAsInterface = false;
				\u0002.GetAccess(i).Accept(this);
				this.InterfaceAsInterface = u;
			}
			base.\u0003();
			TypifierAndCrossReferenceCollector.\u0001(\u0002);
		}

		// Token: 0x0600306E RID: 12398 RVA: 0x000B8014 File Offset: 0x000B6214
		public override void \u0001(_IDeRefAccessExpression \u0002)
		{
			base.\u0001(AccessFlag.Read);
			\u0002._Base.Accept(this);
			base.\u0003();
			if (\u0002._Base.Type == null)
			{
				return;
			}
			_IPointerType ipointerType = \u0002._Base.Type.DeRefType as _IPointerType;
			if (ipointerType != null)
			{
				\u0002.Type = ipointerType._Base;
				if (this.TreatReferenceAsPointer && \u0002._Base.Type is _IReferenceType)
				{
					_IReferenceType ireferenceType = \u0002._Base.Type as _IReferenceType;
					if (ireferenceType != null)
					{
						\u0002.Type = ireferenceType.BaseType;
					}
				}
			}
			else if (this.TreatReferenceAsPointer)
			{
				_IReferenceType ireferenceType2 = \u0002._Base.Type as _IReferenceType;
				if (ireferenceType2 != null)
				{
					\u0002.Type = ireferenceType2.BaseType;
				}
			}
			base.TopOfStack.\u0002 = true;
			if (\u0002._Base is _IBaseExpression)
			{
				base.TopOfStack.\u0002 = false;
				base.TopOfStack.\u0003 = true;
			}
		}

		// Token: 0x0600306F RID: 12399 RVA: 0x000B8104 File Offset: 0x000B6304
		public override void \u0001(_ICurrentTaskExpression \u0002)
		{
			this.FastOnlineChangeOK = false;
			base.\u0001(\u0002);
		}

		// Token: 0x06003070 RID: 12400 RVA: 0x000B8114 File Offset: 0x000B6314
		public override void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
			if (base.InterpretPragmas)
			{
				_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
				global::\u0082.\u0012.\u0001(ipragmaExpression, base._Scope, this.\u0001, false);
				if (ipragmaExpression != null && !ipragmaExpression.Value)
				{
					ipragmaExpression.AddError(\u0002.ErrorOutput, MessageId.None);
				}
			}
		}

		// Token: 0x06003071 RID: 12401 RVA: 0x000B8170 File Offset: 0x000B6370
		public override void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			base.StatementReplacement = global::\u0019.\u0003.\u0001();
			if (base.InterpretPragmas)
			{
				global::\u0082.\u0012.\u0001(ipragmaExpression, base._Scope, this.\u0001, false);
				if (ipragmaExpression != null && ipragmaExpression.Value)
				{
					base.StatementReplacement = \u0002.IfThen;
					\u0002.IfThen.Accept(this);
					return;
				}
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					ipragmaElseIf.Condition.Accept(this);
					ipragmaExpression = (ipragmaElseIf.Condition as _IPragmaExpression);
					global::\u0082.\u0012.\u0001(ipragmaExpression, base._Scope, this.\u0001, false);
					if (ipragmaExpression != null && ipragmaExpression.Value)
					{
						ipragmaElseIf.Controlled.Accept(this);
						base.StatementReplacement = ipragmaElseIf.Controlled;
						return;
					}
				}
				if (\u0002.IfElse != null)
				{
					base.StatementReplacement = \u0002.IfElse;
					\u0002.IfElse.Accept(this);
					return;
				}
			}
			else
			{
				\u0002.IfThen.Accept(this);
				foreach (_IPragmaElseIf ipragmaElseIf2 in \u0002.ElseIf)
				{
					ipragmaElseIf2.Condition.Accept(this);
					ipragmaElseIf2.Controlled.Accept(this);
				}
				if (\u0002.IfElse != null)
				{
					\u0002.IfElse.Accept(this);
				}
			}
		}

		// Token: 0x04000935 RID: 2357
		[CompilerGenerated]
		private new bool \u0001;

		// Token: 0x04000936 RID: 2358
		[CompilerGenerated]
		private new bool \u0002;

		// Token: 0x04000937 RID: 2359
		[CompilerGenerated]
		private new bool \u0003;

		// Token: 0x04000938 RID: 2360
		[CompilerGenerated]
		private new bool \u0004;

		// Token: 0x04000939 RID: 2361
		[CompilerGenerated]
		private new bool \u0005;

		// Token: 0x0400093A RID: 2362
		[CompilerGenerated]
		private new bool \u0006;

		// Token: 0x0400093B RID: 2363
		[CompilerGenerated]
		private new bool \u0007;

		// Token: 0x0400093C RID: 2364
		[CompilerGenerated]
		private new bool \u0008;
	}
}
