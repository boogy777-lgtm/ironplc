using System;
using System.Collections.Generic;
using System.Reflection;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000BF RID: 191
	internal class AttributeManagerX : IAttributeManager
	{
		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000B77 RID: 2935 RVA: 0x0001CF35 File Offset: 0x0001BF35
		public static AttributeManagerX Singleton
		{
			get
			{
				if (AttributeManagerX.s_man == null)
				{
					AttributeManagerX.s_man = new AttributeManagerX();
				}
				return AttributeManagerX.s_man;
			}
		}

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000B78 RID: 2936 RVA: 0x0001CF4D File Offset: 0x0001BF4D
		public IEnumerable<IAttribute> RegisteredAttributes
		{
			get
			{
				return this._RegisteredAttributes;
			}
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x0001CF55 File Offset: 0x0001BF55
		public bool CheckAttribute(string attribute, string value, AttributeScope scope, ISignature sign, IVariable variable, out IList<string> errors)
		{
			return this._CheckAttribute(attribute, value, scope, sign, variable, out errors);
		}

		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000B7A RID: 2938 RVA: 0x0001CF66 File Offset: 0x0001BF66
		// (set) Token: 0x06000B7B RID: 2939 RVA: 0x0001CF6E File Offset: 0x0001BF6E
		internal bool EngineInitialized { get; private set; }

		// Token: 0x06000B7C RID: 2940 RVA: 0x0001CF78 File Offset: 0x0001BF78
		internal void Initialize(bool bCalledFromEngineInitialized)
		{
			if (bCalledFromEngineInitialized)
			{
				this.EngineInitialized = true;
			}
			else if (!this.EngineInitialized)
			{
				return;
			}
			foreach (IAttributeProvider attributeProvider in APEnvironmentFacade.Instance.CreateAttributeProviders())
			{
				foreach (IAttribute attribute in attributeProvider.ProvidedAttributes)
				{
					LList<IAttribute> llist;
					if (this._registeredAttributes.ContainsKey(attribute.Name))
					{
						llist = this._registeredAttributes[attribute.Name];
					}
					else
					{
						llist = new LList<IAttribute>();
					}
					llist.Add(attribute);
					this._registeredAttributes[attribute.Name] = llist;
				}
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x0001D050 File Offset: 0x0001C050
		internal void Reset()
		{
			this._registeredAttributes.Clear();
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x0001D05D File Offset: 0x0001C05D
		private void RefillHashtable()
		{
			if (0 < this._registeredAttributes.Count)
			{
				return;
			}
			if (!this.EngineInitialized)
			{
				return;
			}
			this.Initialize(false);
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x0001D080 File Offset: 0x0001C080
		internal IEnumerable<IAttribute> _RegisteredAttributes
		{
			get
			{
				this.RefillHashtable();
				LList<IAttribute> llist = new LList<IAttribute>();
				foreach (LList<IAttribute> llist2 in this._registeredAttributes.Values)
				{
					llist.AddRange(llist2);
				}
				return llist;
			}
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0001D0E8 File Offset: 0x0001C0E8
		internal bool _CheckAttribute(string attribute, string value, AttributeScope scope, ISignature sign, IVariable variable, out IList<string> errors)
		{
			this.RefillHashtable();
			errors = new LList<string>();
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 || string.IsNullOrEmpty(attribute))
			{
				return true;
			}
			if (!this._registeredAttributes.ContainsKey(attribute))
			{
				LList<string> llist = new LList<string>();
				llist.Add(string.Format(Strings.AttributeUnknown, attribute));
				errors = llist;
				return false;
			}
			foreach (IAttribute attribute2 in this._registeredAttributes[attribute])
			{
				ICheckedAttribute checkedAttribute = attribute2 as ICheckedAttribute;
				if (checkedAttribute != null)
				{
					AttributeManagerX.CheckAttribute(attribute, value, scope, sign, variable, errors, checkedAttribute);
				}
			}
			return errors.Count == 0;
		}

		// Token: 0x06000B81 RID: 2945 RVA: 0x0001D1AC File Offset: 0x0001C1AC
		private static void CheckAttribute(string attribute, string value, AttributeScope scope, ISignature sign, IVariable variable, IList<string> errors, ICheckedAttribute checkedAttribute)
		{
			string item;
			if ((checkedAttribute.Scope != AttributeScope.Signature || variable == null || !variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY)) && (checkedAttribute.Scope & scope) == AttributeScope.None)
			{
				item = string.Format((scope == AttributeScope.Signature) ? Strings.AttributeWrongScopeSigns : Strings.AttributeWrongScopeVars, attribute);
				errors.Add(item);
			}
			Version requiredCompilerVersion = checkedAttribute.RequiredCompilerVersion;
			Version version = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse();
			if (requiredCompilerVersion != null && requiredCompilerVersion > version)
			{
				string arg = APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEMTextSave(requiredCompilerVersion);
				string arg2 = APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEMTextSave(version);
				item = string.Format(Strings.AttributeNotSupportedWithCompilerVersion, attribute, arg, arg2);
				errors.Add(item);
			}
			if (!checkedAttribute.CheckValue(value, scope, sign, variable, out item))
			{
				errors.Add(item);
			}
		}

		// Token: 0x06000B82 RID: 2946 RVA: 0x0001D27E File Offset: 0x0001C27E
		public bool IsCheckFunction(ISignature sign)
		{
			return CheckFunctionAttributes.IsCheckFunction(sign);
		}

		// Token: 0x040001E8 RID: 488
		[Obfuscation(Feature = "rename")]
		private LDictionary<string, LList<IAttribute>> _registeredAttributes = new LDictionary<string, LList<IAttribute>>();

		// Token: 0x040001E9 RID: 489
		private static AttributeManagerX s_man;
	}
}
