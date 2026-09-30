using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x02000362 RID: 866
	public class OnlineChangeDetails : _IOnlineChangeDetails, IOnlineChangeDetails3, IOnlineChangeDetails2, IOnlineChangeDetails
	{
		// Token: 0x060033D4 RID: 13268 RVA: 0x000CBC9C File Offset: 0x000C9E9C
		public OnlineChangeDetails()
		{
		}

		// Token: 0x060033D5 RID: 13269 RVA: 0x000CBCDC File Offset: 0x000C9EDC
		public OnlineChangeDetails(bool bInterfaceChanged, bool bCodeChanged)
		{
			this.InterfaceChanged = bInterfaceChanged;
			this.CodeChanged = bCodeChanged;
		}

		// Token: 0x17000883 RID: 2179
		// (get) Token: 0x060033D6 RID: 13270 RVA: 0x000CBD34 File Offset: 0x000C9F34
		public bool InterfaceChanged { get; }

		// Token: 0x17000884 RID: 2180
		// (get) Token: 0x060033D7 RID: 13271 RVA: 0x000CBD3C File Offset: 0x000C9F3C
		public bool CodeChanged { get; }

		// Token: 0x17000885 RID: 2181
		// (get) Token: 0x060033D8 RID: 13272 RVA: 0x000CBD44 File Offset: 0x000C9F44
		// (set) Token: 0x060033D9 RID: 13273 RVA: 0x000CBD4C File Offset: 0x000C9F4C
		public long TotalNumberOfRelinkTests { get; set; }

		// Token: 0x17000886 RID: 2182
		// (get) Token: 0x060033DA RID: 13274 RVA: 0x000CBD58 File Offset: 0x000C9F58
		public List<IVariableInfo> VariablesAffected { get; } = new List<IVariableInfo>();

		// Token: 0x17000887 RID: 2183
		// (get) Token: 0x060033DB RID: 13275 RVA: 0x000CBD60 File Offset: 0x000C9F60
		public List<IVariableInfo> DeletedVariables { get; } = new List<IVariableInfo>();

		// Token: 0x17000888 RID: 2184
		// (get) Token: 0x060033DC RID: 13276 RVA: 0x000CBD68 File Offset: 0x000C9F68
		public List<IVariableInfo> InterfacesToTest
		{
			get
			{
				return this.\u0001.Values.ToList<IVariableInfo>();
			}
		}

		// Token: 0x17000889 RID: 2185
		// (get) Token: 0x060033DD RID: 13277 RVA: 0x000CBD7C File Offset: 0x000C9F7C
		// (set) Token: 0x060033DE RID: 13278 RVA: 0x000CBD84 File Offset: 0x000C9F84
		public IDictionary<string, IVariableInfo> InterfacesToRelink
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = new LDictionary<string, IVariableInfo>(value);
			}
		}

		// Token: 0x1700088A RID: 2186
		// (get) Token: 0x060033DF RID: 13279 RVA: 0x000CBD94 File Offset: 0x000C9F94
		// (set) Token: 0x060033E0 RID: 13280 RVA: 0x000CBD9C File Offset: 0x000C9F9C
		public IDictionary<string, IVariableInfo> InstancesToMove
		{
			get
			{
				return this.\u0002;
			}
			set
			{
				this.\u0002 = new LDictionary<string, IVariableInfo>(value);
			}
		}

		// Token: 0x1700088B RID: 2187
		// (get) Token: 0x060033E1 RID: 13281 RVA: 0x000CBDAC File Offset: 0x000C9FAC
		public IList<IVariable> VariablesWithOnlineChangeFlag
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x060033E2 RID: 13282 RVA: 0x000CBDB4 File Offset: 0x000C9FB4
		public void ResetOnlineChangeFlags()
		{
			foreach (IVariable variable in this.\u0001)
			{
				((_IVariable)variable).SetFlag(VarFlag.LocationChanged | VarFlag.OnlChangeCopy | VarFlag.OnlChangeInit | VarFlag.OnlChangeVFInit | VarFlag.OnlChangeExit | VarFlag.OnlChangeReInit, false);
			}
		}

		// Token: 0x060033E3 RID: 13283 RVA: 0x000CBE10 File Offset: 0x000CA010
		public void AddResetVariable(_IVariable var)
		{
			this.\u0001.Add(var);
		}

		// Token: 0x060033E4 RID: 13284 RVA: 0x000CBE20 File Offset: 0x000CA020
		public void AddDeletedVariable(IVariable var, ISignature sign, ICollection<string> optionalInstancePaths = null)
		{
			IVariableInfo item;
			if (optionalInstancePaths == null || optionalInstancePaths.Count == 0)
			{
				item = new VariableInfo(var.Id, sign.Id, VarFlag.LocationChanged);
			}
			else
			{
				item = new VariableInfoWithInstancePath(var.Id, sign.Id, VarFlag.LocationChanged, optionalInstancePaths);
			}
			this.DeletedVariables.Add(item);
		}

		// Token: 0x060033E5 RID: 13285 RVA: 0x000CBE78 File Offset: 0x000CA078
		public void AddVariableInfo(IVariable var, ISignature sign)
		{
			VarFlag varFlag = VarFlag.None;
			if (var.HasFlag(VarFlag.LocationChanged))
			{
				varFlag |= VarFlag.LocationChanged;
			}
			if (var.HasFlag(VarFlag.OnlChangeCopy))
			{
				varFlag |= VarFlag.OnlChangeCopy;
			}
			if (var.HasFlag(VarFlag.OnlChangeInit))
			{
				varFlag |= VarFlag.OnlChangeInit;
			}
			if (var.HasFlag(VarFlag.OnlChangeExit))
			{
				varFlag |= VarFlag.OnlChangeExit;
			}
			if (var.HasFlag(VarFlag.OnlChangeReInit))
			{
				varFlag |= VarFlag.OnlChangeReInit;
			}
			if (var.HasFlag((VarFlag)((ulong)-2147483648)))
			{
				varFlag |= (VarFlag)((ulong)int.MinValue);
			}
			this.VariablesAffected.Add(new VariableInfo(var.Id, sign.Id, varFlag));
		}

		// Token: 0x040009FE RID: 2558
		private LDictionary<string, IVariableInfo> \u0001 = new LDictionary<string, IVariableInfo>();

		// Token: 0x040009FF RID: 2559
		private LDictionary<string, IVariableInfo> \u0002 = new LDictionary<string, IVariableInfo>();

		// Token: 0x04000A00 RID: 2560
		private readonly LList<IVariable> \u0001 = new LList<IVariable>();

		// Token: 0x04000A01 RID: 2561
		[CompilerGenerated]
		private readonly bool \u0001;

		// Token: 0x04000A02 RID: 2562
		[CompilerGenerated]
		private readonly bool \u0002;

		// Token: 0x04000A03 RID: 2563
		[CompilerGenerated]
		private long \u0001;

		// Token: 0x04000A04 RID: 2564
		[CompilerGenerated]
		private readonly List<IVariableInfo> \u0001;

		// Token: 0x04000A05 RID: 2565
		[CompilerGenerated]
		private readonly List<IVariableInfo> \u0002;

		// Token: 0x04000A06 RID: 2566
		public const VarFlag TemporaryOnlineChangeFlags = VarFlag.LocationChanged | VarFlag.OnlChangeCopy | VarFlag.OnlChangeInit | VarFlag.OnlChangeVFInit | VarFlag.OnlChangeExit | VarFlag.OnlChangeReInit;
	}
}
