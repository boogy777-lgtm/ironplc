using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u0008;
using \u001A;
using \u001E;
using \u001F;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x0200022A RID: 554
	internal sealed class Codegeneration
	{
		// Token: 0x060024DC RID: 9436 RVA: 0x0007E758 File Offset: 0x0007C958
		public Codegeneration(_ICompileContext comcon, _ICompileContext comconRef, bool bKeepCompileInformation, bool bOnlineChange) : this(comcon, comconRef, bKeepCompileInformation, bOnlineChange, comcon.Codegenerator)
		{
		}

		// Token: 0x060024DD RID: 9437 RVA: 0x0007E76C File Offset: 0x0007C96C
		public Codegeneration(_ICompileContext comcon, _ICompileContext comconRef, bool bKeepCompileInformation, bool bOnlineChange, ICodegenerator codegen)
		{
			this.KeepCompileInformation = bKeepCompileInformation;
			this.Codegenerator = codegen;
			TypeGuidAttribute typeGuidAttribute = (TypeGuidAttribute)this.Codegenerator.GetType().GetCustomAttributes(typeof(TypeGuidAttribute), false).FirstOrDefault<object>();
			if (typeGuidAttribute == null)
			{
				throw new LateCompileErrorException("unexpected codegenerator");
			}
			this.\u0001 = typeGuidAttribute.Guid;
			this.\u0001 = global::\u0007.\u0005.\u0001(comcon);
			this.Adapter = new \u001E.\u000E(this.\u0001, bKeepCompileInformation, comcon);
			this._ICompileContext = comcon;
			this.RefContext = comconRef;
			this.Codegenerator.Initialize(this.Adapter, this.\u0001);
			this.Adapter.Codegen = this.Codegenerator;
			this.\u0001 = new CheckFunctions(comcon);
			if (bOnlineChange)
			{
				this.Optimizer = new global::\u0008.\u000E(this.\u0001, global::\u0007.\u0005.\u0001(this.RefContext), this._ICompileContext.DataManager, this._ICompileContext, this.Codegenerator, this);
			}
			else
			{
				this.Optimizer = new global::\u0008.\u000E(this.\u0001, this._ICompileContext.DataManager, this._ICompileContext, this.Codegenerator, this);
			}
			this.OnlineChange = bOnlineChange;
		}

		// Token: 0x170006B0 RID: 1712
		// (get) Token: 0x060024DE RID: 9438 RVA: 0x0007E89C File Offset: 0x0007CA9C
		private \u001E.\u000E Adapter { get; }

		// Token: 0x170006B1 RID: 1713
		// (get) Token: 0x060024DF RID: 9439 RVA: 0x0007E8A4 File Offset: 0x0007CAA4
		private global::\u0008.\u000E Optimizer { get; }

		// Token: 0x170006B2 RID: 1714
		// (get) Token: 0x060024E0 RID: 9440 RVA: 0x0007E8AC File Offset: 0x0007CAAC
		public bool OnlineChange { get; }

		// Token: 0x170006B3 RID: 1715
		// (get) Token: 0x060024E1 RID: 9441 RVA: 0x0007E8B4 File Offset: 0x0007CAB4
		public _ICompileContext _ICompileContext { get; }

		// Token: 0x170006B4 RID: 1716
		// (get) Token: 0x060024E2 RID: 9442 RVA: 0x0007E8BC File Offset: 0x0007CABC
		public _ICompileContext RefContext { get; }

		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x060024E3 RID: 9443 RVA: 0x0007E8C4 File Offset: 0x0007CAC4
		public ICodegenerator Codegenerator { get; }

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x060024E4 RID: 9444 RVA: 0x0007E8CC File Offset: 0x0007CACC
		public int PointerSize
		{
			get
			{
				return this._ICompileContext.PointerSize;
			}
		}

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x060024E5 RID: 9445 RVA: 0x0007E8DC File Offset: 0x0007CADC
		public bool KeepCompileInformation { get; }

		// Token: 0x060024E6 RID: 9446 RVA: 0x0007E8E4 File Offset: 0x0007CAE4
		public bool \u0001(IScope5 \u0002, string \u0003, _IExprement \u0004)
		{
			return this.\u0001.CheckForCheckFunHide(\u0002, \u0003);
		}

		// Token: 0x060024E7 RID: 9447 RVA: 0x0007E8F4 File Offset: 0x0007CAF4
		private void \u0001(_ICompiledPOU \u0002, _ISignature \u0003 = null)
		{
			this.\u0001.ClearMessages();
			if (\u0003 == null)
			{
				this.Optimizer.\u0002(\u0002);
			}
			else
			{
				this.Optimizer.\u0001(\u0002, \u0003);
			}
			\u0002.Accept(this.Adapter);
			_ISignature isignature = this._ICompileContext[\u0002.SignatureId];
			global::\u001A.\u000E.\u0001(\u0002, this.Adapter);
			bool flag = true;
			if (this.\u0001 == Codegeneration.\u0002 && isignature != null)
			{
				if (isignature.Outputs.Any(new Func<IVariable, bool>(Codegeneration.<>c.<>9.\u0001)))
				{
					flag = false;
				}
			}
			if (flag)
			{
				this.\u0002(\u0002, isignature);
			}
			this.\u0001(\u0002);
			this.Adapter.\u0002();
			IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			foreach (_ICompilerMessage icompilerMessage in this.\u0001.GetAllMessages())
			{
				_ISignature isignature2 = this._ICompileContext[icompilerMessage.ObjectGuid];
				if (isignature2 != null)
				{
					isignature2.AddMessage(icompilerMessage);
				}
			}
		}

		// Token: 0x060024E8 RID: 9448 RVA: 0x0007EA2C File Offset: 0x0007CC2C
		private bool \u0001(_ISignature \u0002)
		{
			foreach (byte b in \u0002.TaskReferenceList)
			{
				ITaskInfo2 taskInfo = this._ICompileContext.AllTasks[(int)b] as ITaskInfo2;
				if (taskInfo != null && !string.IsNullOrEmpty(taskInfo.ParentTaskName))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060024E9 RID: 9449 RVA: 0x0007EA78 File Offset: 0x0007CC78
		private void \u0002(_ICompiledPOU \u0002, _ISignature \u0003)
		{
			IExprementVisitor2 visitor;
			if (\u0003 != null && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_REDUCED_BP_SET))
			{
				visitor = new \u0084.\u0013(this.Adapter);
			}
			else
			{
				visitor = new \u001F.\u000E(this.Adapter);
			}
			ISignature signature = this.\u0001.MethodSignature ?? this.\u0001.LocalSignature;
			if (!this._ICompileContext.IsDefined("SuppressBreakpoints"))
			{
				bool flag = !APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signature, GUIHidingFlags.AllCommon);
				if (!flag && signature != null)
				{
					if (!((_ISignature)signature).IsCompiledLibraryObject)
					{
						if (signature.Name == IdentifierConstants.MainSignatureName)
						{
							flag = true;
						}
						if (!flag && signature.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
						{
							flag = true;
						}
						if (signature.OrgName.IndexOf("__", StringComparison.OrdinalIgnoreCase) < 0)
						{
							if (!flag && !((_ISignature)signature).IsLibraryObject)
							{
								flag = true;
							}
							if (!flag && (signature as _ISignature).IsSourceLibraryObject)
							{
								flag = true;
							}
						}
					}
					if (!flag && signature.HasAttribute(CompileAttributes.ATTRIBUTE_GENERATE_BP))
					{
						flag = true;
					}
				}
				if (this.\u0001(\u0003))
				{
					flag = false;
				}
				if (flag)
				{
					\u0002.Accept(visitor);
				}
				return;
			}
			if (signature != null && signature.HasAttribute("DoGenerateBP"))
			{
				\u0002.Accept(visitor);
				return;
			}
			\u0002.SetBreakpointList(\u0002.BreakpointList);
		}

		// Token: 0x060024EA RID: 9450 RVA: 0x0007EBBC File Offset: 0x0007CDBC
		private void \u0001(_ICompiledPOU \u0002)
		{
			ICompiledCode6 compiledCode = \u0002.CompiledCode as ICompiledCode6;
			if (compiledCode != null)
			{
				int num = compiledCode.CodeSize % this._ICompileContext.DataManager.PackMode;
				if (num != 0)
				{
					int nSizeBytes = this._ICompileContext.DataManager.PackMode - num;
					compiledCode.PadWithNops(nSizeBytes);
				}
			}
		}

		// Token: 0x060024EB RID: 9451 RVA: 0x0007EC10 File Offset: 0x0007CE10
		public void \u0001(_ICompiledPOU \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			this.\u0001.MethodSignature = \u0004;
			this.\u0001.LocalSignature = \u0003;
			this.\u0001.InitFriend();
			((global::\u0007.\u0005)this.\u0001).\u0003();
			this.\u0001(\u0002, null);
		}

		// Token: 0x060024EC RID: 9452 RVA: 0x0007EC50 File Offset: 0x0007CE50
		public void \u0002(_ICompiledPOU \u0002)
		{
			_ISignature isignature = this._ICompileContext[\u0002.SignatureId];
			if (isignature.POUType == Operator.Method)
			{
				this.\u0001.MethodSignature = isignature;
				isignature = this._ICompileContext[isignature.ParentSignatureId];
			}
			else
			{
				this.\u0001.MethodSignature = null;
			}
			this.\u0001.LocalSignature = isignature;
			this.\u0001.InitFriend();
			((global::\u0007.\u0005)this.\u0001).\u0003();
			this.\u0001(\u0002, null);
		}

		// Token: 0x060024ED RID: 9453 RVA: 0x0007ECD4 File Offset: 0x0007CED4
		public void \u0003(_ICompiledPOU \u0002, _ISignature \u0003)
		{
			this.\u0001.MethodSignature = null;
			this.\u0001.LocalSignature = null;
			this.\u0001.InitFriend();
			((global::\u0007.\u0005)this.\u0001).\u0003();
			this.\u0001(\u0002, \u0003);
		}

		// Token: 0x060024EE RID: 9454 RVA: 0x0007ED14 File Offset: 0x0007CF14
		public void \u0003(_ICompiledPOU \u0002)
		{
			this.\u0001.MethodSignature = null;
			this.\u0001.LocalSignature = null;
			this.\u0001.InitFriend();
			((global::\u0007.\u0005)this.\u0001).\u0003();
			this.\u0001(\u0002, null);
		}

		// Token: 0x0400068D RID: 1677
		private readonly IScope5 \u0001;

		// Token: 0x0400068E RID: 1678
		private readonly Guid \u0001;

		// Token: 0x0400068F RID: 1679
		public readonly CheckFunctions \u0001;

		// Token: 0x04000690 RID: 1680
		[CompilerGenerated]
		private readonly \u001E.\u000E \u0001;

		// Token: 0x04000691 RID: 1681
		[CompilerGenerated]
		private readonly global::\u0008.\u000E \u0001;

		// Token: 0x04000692 RID: 1682
		[CompilerGenerated]
		private readonly bool \u0001;

		// Token: 0x04000693 RID: 1683
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000694 RID: 1684
		[CompilerGenerated]
		private readonly _ICompileContext \u0002;

		// Token: 0x04000695 RID: 1685
		[CompilerGenerated]
		private readonly ICodegenerator \u0001;

		// Token: 0x04000696 RID: 1686
		[CompilerGenerated]
		private readonly bool \u0002;

		// Token: 0x04000697 RID: 1687
		private static readonly Guid \u0002 = new Guid("{976AE257-C47E-4874-B5E9-5090FB4B8D5B}");
	}
}
