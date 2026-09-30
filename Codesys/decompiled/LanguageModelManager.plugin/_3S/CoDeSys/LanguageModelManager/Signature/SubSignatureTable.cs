using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000260 RID: 608
	internal class SubSignatureTable
	{
		// Token: 0x0600298F RID: 10639 RVA: 0x000695E4 File Offset: 0x000685E4
		private static _ISignature GetPlaceholderSignature(string stMethodName)
		{
			_ISignature isignature = CompilerProxy.CreateParser(string.Format("METHOD {0}\r\nVAR\r\nEND_VAR", stMethodName))._ParseInterface(null, null);
			isignature.ObjectGuid = Guid.NewGuid();
			isignature.SetFlagInternal(SignatureFlagInternal.Overloaded, true);
			isignature.SetFlag(SignatureFlag.Generated, true);
			return isignature;
		}

		// Token: 0x06002990 RID: 10640 RVA: 0x00069624 File Offset: 0x00068624
		internal SubSignatureTable Duplicate()
		{
			SubSignatureTable subSignatureTable = new SubSignatureTable();
			if (this.m_htSignatures != null)
			{
				subSignatureTable.m_htSignatures = new CaseInsensitiveHashtable();
				foreach (object obj in this.m_htSignatures.Keys)
				{
					string key = (string)obj;
					subSignatureTable.m_htSignatures.Add(key, this.m_htSignatures[key]);
				}
			}
			foreach (string text in this._overloads.Keys)
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				foreach (_ISignature isignature in this._overloads[text])
				{
					llist.Add(isignature);
				}
				subSignatureTable._overloads.Add(text, llist);
			}
			return subSignatureTable;
		}

		// Token: 0x06002991 RID: 10641 RVA: 0x00069750 File Offset: 0x00068750
		internal void SetSubSignatures(CaseInsensitiveHashtable htSignatures)
		{
			this.m_htSignatures = htSignatures;
		}

		// Token: 0x06002992 RID: 10642 RVA: 0x00069759 File Offset: 0x00068759
		internal CaseInsensitiveHashtable GetSubSignaturesTable()
		{
			return this.m_htSignatures;
		}

		// Token: 0x06002993 RID: 10643 RVA: 0x00069764 File Offset: 0x00068764
		internal bool IsEqual(SubSignatureTable subSignatureTable)
		{
			if (subSignatureTable == null)
			{
				return false;
			}
			if (this.m_htSignatures == null && subSignatureTable.m_htSignatures == null)
			{
				return true;
			}
			if (this.m_htSignatures == null || subSignatureTable.m_htSignatures == null)
			{
				return false;
			}
			if (this.m_htSignatures.Count != subSignatureTable.m_htSignatures.Count)
			{
				return false;
			}
			foreach (object obj in this.m_htSignatures)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				if (!subSignatureTable.m_htSignatures.Contains(dictionaryEntry.Key))
				{
					return false;
				}
				if (!object.Equals(dictionaryEntry.Value, subSignatureTable.m_htSignatures[dictionaryEntry.Key]))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002994 RID: 10644 RVA: 0x00069838 File Offset: 0x00068838
		private bool Overloads(_ISignature signNew)
		{
			_ISignature isignature = this.m_htSignatures[signNew.Name] as _ISignature;
			if (isignature != null)
			{
				return signNew.ObjectGuid != isignature.ObjectGuid;
			}
			return signNew.HasAttribute("overloaded");
		}

		// Token: 0x06002995 RID: 10645 RVA: 0x0006987C File Offset: 0x0006887C
		public bool Add(ISignature sign)
		{
			_ISignature isignature = sign as _ISignature;
			if (isignature == null)
			{
				return false;
			}
			if (this.Overloads(isignature))
			{
				this.AddOverloadedSignature(isignature.Name, isignature);
			}
			if (this.m_htSignatures.ContainsKey(isignature.Name))
			{
				_ISignature isignature2 = (_ISignature)this.m_htSignatures[isignature.Name];
				isignature.AddAttribute("shadowed", isignature2.ObjectGuid.ToString());
				return false;
			}
			this.m_htSignatures.Add(isignature.Name, sign);
			return true;
		}

		// Token: 0x06002996 RID: 10646 RVA: 0x0006990C File Offset: 0x0006890C
		public void ChangeSubSignatureName(_ISignature signsub, _IExpression nameexpression)
		{
			if (this.m_htSignatures.ContainsKey(signsub.Name))
			{
				this.m_htSignatures.Remove(signsub.Name);
			}
			signsub._NameExpression = nameexpression;
			if (this.m_htSignatures.ContainsKey(signsub.Name))
			{
				return;
			}
			this.m_htSignatures.Add(signsub.Name, signsub);
		}

		// Token: 0x06002997 RID: 10647 RVA: 0x0006996C File Offset: 0x0006896C
		private void AddOverloadedSignature(string stName, _ISignature sign)
		{
			string mangledName = this.GetMangledName(sign);
			sign.SetFlagInternal(SignatureFlagInternal.Overloading, true);
			sign.AddAttribute("mangled_name", mangledName);
			VariableExpression variableExpression = new VariableExpression(mangledName);
			variableExpression._Position = sign._NameExpression._Position;
			sign.AddAttribute("overloads", sign.Name);
			sign._NameExpression = variableExpression;
			IList<_ISignature> list;
			if (!this._overloads.TryGetValue(stName, ref list))
			{
				list = new LList<_ISignature>();
				this._overloads[stName] = list;
			}
			list.Add(sign);
		}

		// Token: 0x06002998 RID: 10648 RVA: 0x000699F4 File Offset: 0x000689F4
		private string GetMangledName(_ISignature sign)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("`");
			stringBuilder.Append(sign.Name);
			foreach (IVariable variable in sign.AllInputs)
			{
				stringBuilder.Append(string.Format("@{0}@", variable.Type));
			}
			stringBuilder.Append("`");
			return stringBuilder.ToString();
		}

		// Token: 0x06002999 RID: 10649 RVA: 0x00069A62 File Offset: 0x00068A62
		public bool Remove(ISignature sign)
		{
			if (this.m_htSignatures.Contains(sign.Name))
			{
				this.m_htSignatures.Remove(sign.Name);
				return true;
			}
			return false;
		}

		// Token: 0x0600299A RID: 10650 RVA: 0x00069A8B File Offset: 0x00068A8B
		public IEnumerable<_ISignature> GetSignatures()
		{
			return this.m_htSignatures.Values.OfType<_ISignature>();
		}

		// Token: 0x0600299B RID: 10651 RVA: 0x00069AA0 File Offset: 0x00068AA0
		public _ISignature[] GetSubSignatures()
		{
			_ISignature[] array = new _ISignature[this.m_htSignatures.Count];
			this.m_htSignatures.Values.CopyTo(array, 0);
			return array;
		}

		// Token: 0x0600299C RID: 10652 RVA: 0x00069AD4 File Offset: 0x00068AD4
		public _ISignature[] GetOrderedSubSignatures()
		{
			int count = this.m_htSignatures.Count;
			LSortedList<string, _ISignature> lsortedList;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351410)
			{
				StringComparer invariantCultureIgnoreCase = StringComparer.InvariantCultureIgnoreCase;
				lsortedList = new LSortedList<string, _ISignature>(count, invariantCultureIgnoreCase);
			}
			else
			{
				lsortedList = new LSortedList<string, _ISignature>(count);
			}
			foreach (object obj in this.m_htSignatures.Values)
			{
				Signature signature = (Signature)obj;
				string text = signature.Name;
				if (lsortedList.ContainsKey(signature.Name) && signature.GetFlagInternal(SignatureFlagInternal.Overloaded))
				{
					text = signature.GetAttributeValue("mangled_name");
				}
				lsortedList.Add(text, signature);
			}
			return lsortedList.Values.ToArray<_ISignature>();
		}

		// Token: 0x0600299D RID: 10653 RVA: 0x00069BB0 File Offset: 0x00068BB0
		public void SetSubSignatures(IList<_ISignature> signatures)
		{
			this.m_htSignatures = new CaseInsensitiveHashtable(signatures.Count);
			foreach (_ISignature sign in signatures)
			{
				this.Add(sign);
			}
		}

		// Token: 0x0600299E RID: 10654 RVA: 0x00069C0C File Offset: 0x00068C0C
		public ISignature GetSubSignature(string stName)
		{
			return this.m_htSignatures[stName] as ISignature;
		}

		// Token: 0x0600299F RID: 10655 RVA: 0x00069C20 File Offset: 0x00068C20
		public ISignature GetSubSignatureById(int nId)
		{
			foreach (object obj in this.m_htSignatures.Keys)
			{
				ISignature signature = (ISignature)obj;
				if (signature.Id == nId)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060029A0 RID: 10656 RVA: 0x00069C88 File Offset: 0x00068C88
		public IList<_ISignature> GetOverloadedSignatures(string stName)
		{
			IList<_ISignature> result;
			if (!this._overloads.TryGetValue(stName, ref result))
			{
				return new LList<_ISignature>();
			}
			return result;
		}

		// Token: 0x060029A1 RID: 10657 RVA: 0x00069CAE File Offset: 0x00068CAE
		public void SetOverloadedSignatures(string stName, IList<_ISignature> signatureList)
		{
			this._overloads[stName] = signatureList;
		}

		// Token: 0x060029A2 RID: 10658 RVA: 0x00069CBD File Offset: 0x00068CBD
		public IEnumerable<string> GetOverloadedNames()
		{
			return this._overloads.Keys;
		}

		// Token: 0x060029A3 RID: 10659 RVA: 0x00069CCC File Offset: 0x00068CCC
		public void CreateOverloadPlaceholderSignatures(IList<_ISignature> subsignatures)
		{
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary = new CaseInsensitiveDictionary<bool>();
			foreach (_ISignature isignature in subsignatures)
			{
				bool flag = false;
				if (caseInsensitiveDictionary.ContainsKey(isignature.Name))
				{
					flag = true;
				}
				else if (isignature.HasAttribute("overloaded"))
				{
					flag = true;
				}
				caseInsensitiveDictionary[isignature.Name] = flag;
			}
			foreach (string text in caseInsensitiveDictionary.Keys)
			{
				if (caseInsensitiveDictionary[text])
				{
					_ISignature placeholderSignature = SubSignatureTable.GetPlaceholderSignature(text);
					subsignatures.Insert(0, placeholderSignature);
				}
			}
		}

		// Token: 0x040007CA RID: 1994
		private const string METHOD_TEMPLATE = "METHOD {0}\r\nVAR\r\nEND_VAR";

		// Token: 0x040007CB RID: 1995
		private CaseInsensitiveHashtable m_htSignatures = new CaseInsensitiveHashtable();

		// Token: 0x040007CC RID: 1996
		private readonly CaseInsensitiveDictionary<IList<_ISignature>> _overloads = new CaseInsensitiveDictionary<IList<_ISignature>>();
	}
}
