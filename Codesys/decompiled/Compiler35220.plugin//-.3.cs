using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0084;

namespace \u0081
{
	// Token: 0x020000C8 RID: 200
	internal sealed class \u0003
	{
		// Token: 0x06000EA7 RID: 3751 RVA: 0x00028418 File Offset: 0x00026618
		internal \u0003(_ICompileContext \u0001\u0002, \u0080.\u0005.\u0001 \u0098\u0004)
		{
			this.\u0001 = \u0001\u0002;
			this.\u0001 = \u0098\u0004;
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x00028430 File Offset: 0x00026630
		internal bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			\u0081.\u0003.\u0001 u = new \u0081.\u0003.\u0001();
			u.\u0001 = this;
			u.\u0001 = \u0002;
			u.\u0002 = \u0003;
			return u.\u0002.AllVariables.Any(new Func<_IVariable, bool>(u.\u0001));
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x00028474 File Offset: 0x00026674
		internal void \u0001(_ISignature \u0002, InstancePathsContainer \u0003, _ISignature \u0004, InstancePathsContainer \u0005)
		{
			foreach (_IVariable ivariable in \u0004.AllVariables)
			{
				ICompiledType compiledTypeInternal = ivariable.CompiledTypeInternal;
				if (this.\u0001(\u0002, \u0004, ivariable))
				{
					TypeClass @class = compiledTypeInternal.Class;
					if (@class != TypeClass.Array)
					{
						if (@class == TypeClass.Userdef)
						{
							this.\u0001(\u0003, \u0004, \u0005, ivariable);
						}
					}
					else
					{
						this.\u0001(\u0003, \u0004, \u0005, compiledTypeInternal, ivariable);
					}
				}
			}
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x000284F8 File Offset: 0x000266F8
		private bool \u0001(_ISignature \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			ICompiledType compiledTypeInternal = \u0004.CompiledTypeInternal;
			if (this.\u0001(\u0004))
			{
				return false;
			}
			_IUserdefType u = \u0081.\u0003.\u0001(compiledTypeInternal);
			return !this.\u0001(\u0002, u) && (!\u0004.GetFlag(VarFlag.Absolut) || this.\u0001[\u0003.Id] != null);
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x00028550 File Offset: 0x00026750
		private void \u0001(InstancePathsContainer \u0002, _ISignature \u0003, InstancePathsContainer \u0004, ICompiledType \u0005, _IVariable \u0006)
		{
			_IArrayType u = \u0005 as _IArrayType;
			IScope5 u2 = global::\u0007.\u0005.\u0001(this.\u0001, \u0003.Id);
			LList<string> llist = InstancePathService.\u0001(u, u2);
			foreach (string str in \u0004.InstancePaths)
			{
				foreach (string str2 in llist)
				{
					string u3 = str + "." + \u0006.OrgName + str2;
					\u0002.\u0001(u3, \u0006, \u0003);
				}
			}
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x00028610 File Offset: 0x00026810
		private void \u0001(InstancePathsContainer \u0002, _ISignature \u0003, InstancePathsContainer \u0004, _IVariable \u0005)
		{
			foreach (string str in \u0004.InstancePaths)
			{
				string u = str + "." + \u0005.OrgName;
				\u0002.\u0001(u, \u0005, \u0003);
			}
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x00028674 File Offset: 0x00026874
		private bool \u0001(_ISignature \u0002, _IUserdefType \u0003)
		{
			if (\u0002.POUType == Operator.Interface)
			{
				string stName = Helper.\u0001(IdentifierConstants.InterfaceUnion(\u0002.OrgName), \u0002, this.\u0001);
				ISignature signature = this.\u0001[stName];
				if (signature == null)
				{
					string u = string.Format(\u0081.\u0002.Err_InternalErrorProhibitingOnlineChange, 4);
					IMessage cm = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_InternalErrorProhibitingOnlineChange);
					\u0002.AddError(cm);
				}
				if (\u0003 == null || signature == null || (\u0003.SignatureId != \u0002.Id && \u0003.SignatureId != signature.Id))
				{
					return true;
				}
			}
			else if (\u0003 == null || \u0003.SignatureId != \u0002.Id)
			{
				return true;
			}
			return false;
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x00028714 File Offset: 0x00026914
		private static _IUserdefType \u0001(ICompiledType \u0002)
		{
			_IUserdefType result = null;
			if (\u0002.Class == TypeClass.Userdef)
			{
				result = (\u0002 as _IUserdefType);
			}
			else if (\u0002.Class == TypeClass.Array)
			{
				result = (\u0084.\u0004.\u0002(\u0002 as _IArrayType) as _IUserdefType);
			}
			return result;
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x00028754 File Offset: 0x00026954
		private bool \u0001(_IVariable \u0002)
		{
			if (\u0002.GetFlag(VarFlag.Inout))
			{
				return true;
			}
			ICompiledType compiledTypeInternal = \u0002.CompiledTypeInternal;
			return (compiledTypeInternal.Class != TypeClass.Userdef && compiledTypeInternal.Class != TypeClass.Array) || \u0002.IsProperty || (\u0002.GetFlag(VarFlag.Temp) && !this.\u0001.\u0002) || ((\u0002.GetFlag(VarFlag.Constant) || \u0002.GetFlag(VarFlag.ReplacedConstant)) && !this.\u0001.\u0004);
		}

		// Token: 0x04000295 RID: 661
		private readonly _ICompileContext \u0001;

		// Token: 0x04000296 RID: 662
		private readonly \u0080.\u0005.\u0001 \u0001;

		// Token: 0x020000C9 RID: 201
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06000EB1 RID: 3761 RVA: 0x000287E0 File Offset: 0x000269E0
			internal bool \u0001(_IVariable \u0002)
			{
				return this.\u0001.\u0001(this.\u0001, this.\u0002, \u0002);
			}

			// Token: 0x04000297 RID: 663
			public \u0081.\u0003 \u0001;

			// Token: 0x04000298 RID: 664
			public _ISignature \u0001;

			// Token: 0x04000299 RID: 665
			public _ISignature \u0002;
		}
	}
}
