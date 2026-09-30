using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003CD RID: 973
	public class InterfaceHierarchy : IInterfaceHierarchy
	{
		// Token: 0x060036FC RID: 14076 RVA: 0x000E064C File Offset: 0x000DE84C
		public void Initialize(IScope5 scope, _ISignature sign)
		{
			int num;
			this.Initialize(scope, sign, out num);
		}

		// Token: 0x060036FD RID: 14077 RVA: 0x000E0664 File Offset: 0x000DE864
		public void Initialize(IScope5 scope, _ISignature sign, out int Index)
		{
			int num = -1;
			Index = -1;
			if (sign.BaseSignatureId != Helper.InvalidId)
			{
				IInterfaceHierarchy interfaceHierarchy = (scope[sign.BaseSignatureId] as _ISignature).InterfaceHierarchy;
				Debug.\u0001(interfaceHierarchy != null);
				num = this.AddInterfaceHierarchy(interfaceHierarchy, sign.BaseSignatureId);
				foreach (InterfaceInfo interfaceInfo in this.\u0001)
				{
					((IInterfaceInfo)interfaceInfo).IsBaseInterfaceInfo = true;
				}
			}
			if (sign.POUType == Operator.Interface)
			{
				_ISignature isignature = scope[sign.Id] as _ISignature;
				InterfaceInfo interfaceInfo2 = new InterfaceInfo();
				interfaceInfo2.InterfaceId = sign.Id;
				interfaceInfo2.OwnIndex = this.\u0001.Count<InterfaceInfo>();
				interfaceInfo2.ParentInterfaceIndex = num;
				interfaceInfo2.IsEqualParent = false;
				if (num >= 0)
				{
					IInterfaceInfo interfaceInfo3 = this[num];
					interfaceInfo3.NoInit = true;
					interfaceInfo3.DerivedInterfaceIndex = this.\u0001.Count<InterfaceInfo>();
					interfaceInfo2.IsEqualParent = true;
				}
				interfaceInfo2.NoInit = false;
				interfaceInfo2.IsBaseInterfaceInfo = false;
				interfaceInfo2.OrgName = isignature.OrgName;
				Index = this.\u0001.Count<InterfaceInfo>();
				this.\u0001.Add(interfaceInfo2);
			}
			foreach (int num2 in sign.InterfaceIds)
			{
				if (!this.CheckIfAlreadyInserted(num2))
				{
					IInterfaceHierarchy interfaceHierarchy2 = (scope[num2] as _ISignature).InterfaceHierarchy;
					Debug.\u0001(interfaceHierarchy2 != null);
					this.AddInterfaceHierarchy(interfaceHierarchy2, num2);
				}
			}
		}

		// Token: 0x060036FE RID: 14078 RVA: 0x000E07FC File Offset: 0x000DE9FC
		public int AddInterfaceHierarchy(IInterfaceHierarchy ih, int nIdBaseToLookup)
		{
			int count = this.\u0001.Count;
			int result = -1;
			foreach (IInterfaceInfo interfaceInfo in ih.Interfaces)
			{
				InterfaceInfo interfaceInfo2 = new InterfaceInfo();
				interfaceInfo2.InterfaceId = interfaceInfo.InterfaceId;
				interfaceInfo2.OrgName = interfaceInfo.OrgName;
				interfaceInfo2.OwnIndex = interfaceInfo.OwnIndex + count;
				interfaceInfo2.DerivedInterfaceIndex = interfaceInfo.DerivedInterfaceIndex + count;
				interfaceInfo2.ParentInterfaceIndex = interfaceInfo.ParentInterfaceIndex + count;
				interfaceInfo2.IsBaseInterfaceInfo = false;
				interfaceInfo2.IsEqualParent = interfaceInfo.IsEqualParent;
				interfaceInfo2.NoInit = interfaceInfo.NoInit;
				Debug.\u0001(interfaceInfo2.OwnIndex == this.\u0001.Count);
				if (nIdBaseToLookup == interfaceInfo.InterfaceId)
				{
					result = interfaceInfo2.OwnIndex;
				}
				this.\u0001.Add(interfaceInfo2);
			}
			return result;
		}

		// Token: 0x060036FF RID: 14079 RVA: 0x000E0900 File Offset: 0x000DEB00
		public bool CheckIfAlreadyInserted(int nInterfaceId)
		{
			foreach (IInterfaceInfo interfaceInfo in this.\u0001)
			{
				if (!interfaceInfo.NoInit && interfaceInfo.InterfaceId == nInterfaceId)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x170008EE RID: 2286
		public IInterfaceInfo this[int index]
		{
			get
			{
				return this.\u0001[index];
			}
		}

		// Token: 0x170008EF RID: 2287
		// (get) Token: 0x06003701 RID: 14081 RVA: 0x000E0970 File Offset: 0x000DEB70
		public IEnumerable<IInterfaceInfo> Interfaces
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06003702 RID: 14082 RVA: 0x000E0978 File Offset: 0x000DEB78
		public IEnumerable<IInterfaceInfo> GetInterfacesInVFTableOrder()
		{
			LList<IInterfaceInfo> llist = new LList<IInterfaceInfo>();
			LDictionary<int, int> ldictionary = new LDictionary<int, int>();
			for (int i = 0; i < this.\u0001.Count; i++)
			{
				if (!ldictionary.ContainsKey(i))
				{
					ldictionary[i] = i;
					IInterfaceInfo interfaceInfo = this.\u0001[i];
					while (interfaceInfo.NoInit)
					{
						Debug.\u0001(interfaceInfo.DerivedInterfaceIndex > 0);
						ldictionary[interfaceInfo.DerivedInterfaceIndex] = interfaceInfo.DerivedInterfaceIndex;
						interfaceInfo = this.\u0001[interfaceInfo.DerivedInterfaceIndex];
					}
					llist.Add(interfaceInfo);
				}
			}
			return llist;
		}

		// Token: 0x06003703 RID: 14083 RVA: 0x000E0A0C File Offset: 0x000DEC0C
		public string GetUniqueInterfaceVariableName(IInterfaceInfo ii)
		{
			bool flag = true;
			foreach (IInterfaceInfo interfaceInfo in this.\u0001)
			{
				if (interfaceInfo == ii)
				{
					break;
				}
				if (interfaceInfo.InterfaceId == ii.InterfaceId)
				{
					flag = false;
				}
			}
			if (flag)
			{
				return IdentifierConstants.GetInterfacePointerName(ii.InterfaceId);
			}
			string text = IdentifierConstants.GetInterfacePointerName(ii.InterfaceId);
			IInterfaceInfo interfaceInfo2 = ii;
			for (;;)
			{
				text = text + "_" + interfaceInfo2.OwnIndex.ToString();
				if (interfaceInfo2.ParentInterfaceIndex <= 0)
				{
					break;
				}
				interfaceInfo2 = this.\u0001[interfaceInfo2.ParentInterfaceIndex];
			}
			return text;
		}

		// Token: 0x04000AB9 RID: 2745
		private readonly LList<InterfaceInfo> \u0001 = new LList<InterfaceInfo>();
	}
}
