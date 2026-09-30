using System;
using System.Collections.Generic;
using System.Linq;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.PreCompile
{
	// Token: 0x02000171 RID: 369
	public struct MacroInfoProvider
	{
		// Token: 0x060018EE RID: 6382 RVA: 0x0004DAC4 File Offset: 0x0004BCC4
		private MacroInfoProvider(IPositionTextProvider obj)
		{
			this.\u0001 = obj;
			this.\u0001 = ((obj == null) ? null : obj.Name);
		}

		// Token: 0x060018EF RID: 6383 RVA: 0x0004DAE0 File Offset: 0x0004BCE0
		private MacroInfoProvider(IPositionTextProvider obj, string stName)
		{
			this = new MacroInfoProvider(obj);
			this.\u0001 = stName;
		}

		// Token: 0x060018F0 RID: 6384 RVA: 0x0004DAF0 File Offset: 0x0004BCF0
		public string GetPositionText(long lPosition)
		{
			IPositionTextProvider u = this.\u0001;
			return ((u != null) ? u.GetPositionText(lPosition) : null) ?? \u0081.\u0001.UnknownMacroText;
		}

		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x060018F1 RID: 6385 RVA: 0x0004DB10 File Offset: 0x0004BD10
		public string Name
		{
			get
			{
				if (this.\u0001 == null)
				{
					return \u0081.\u0001.UnknownMacroText;
				}
				return this.\u0001;
			}
		}

		// Token: 0x060018F2 RID: 6386 RVA: 0x0004DB28 File Offset: 0x0004BD28
		public static MacroInfoProvider Create(int nProjectHandle, Guid objectGuid)
		{
			return new MacroInfoProvider(APEnvironmentFacade.Instance.GetPositionTextProvider(nProjectHandle, objectGuid));
		}

		// Token: 0x060018F3 RID: 6387 RVA: 0x0004DB3C File Offset: 0x0004BD3C
		public static MacroInfoProvider Create(int nProjectHandle, ILMPOU lmpou)
		{
			MacroInfoProvider macroInfoProvider = MacroInfoProvider.Create(nProjectHandle, lmpou.POUGuid);
			if (lmpou.ParentObjectGuid != Guid.Empty)
			{
				MacroInfoProvider macroInfoProvider2 = MacroInfoProvider.Create(nProjectHandle, lmpou.ParentObjectGuid);
				string propertyName = IdentifierConstants.GetPropertyName(lmpou.Name);
				string stName;
				if (!string.IsNullOrEmpty(propertyName) && MacroInfoProvider.\u0001(lmpou))
				{
					stName = string.Concat(new string[]
					{
						macroInfoProvider2.Name,
						".",
						propertyName,
						".",
						macroInfoProvider.Name
					});
				}
				else
				{
					stName = macroInfoProvider2.Name + "." + macroInfoProvider.Name;
				}
				return new MacroInfoProvider(macroInfoProvider.\u0001, stName);
			}
			return macroInfoProvider;
		}

		// Token: 0x060018F4 RID: 6388 RVA: 0x0004DBF4 File Offset: 0x0004BDF4
		private static bool \u0001(ILMPOU \u0002)
		{
			if (\u0002.Interface != null)
			{
				_IPOUDeclarationStatement ipoudeclarationStatement = \u0002.Interface.Statements.OfType<_IPOUDeclarationStatement>().FirstOrDefault<_IPOUDeclarationStatement>();
				if (ipoudeclarationStatement != null && (ipoudeclarationStatement.Class == Operator.PropertyGet || ipoudeclarationStatement.Class == Operator.PropertySet))
				{
					return true;
				}
				using (IEnumerator<_IPragmaStatement> enumerator = \u0002.Interface.Statements.OfType<_IPragmaStatement>().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string text;
						string text2;
						if (\u0019.\u0001.\u0001(enumerator.Current, out text, out text2) && text.Equals("property", StringComparison.InvariantCultureIgnoreCase))
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0400046A RID: 1130
		private readonly IPositionTextProvider \u0001;

		// Token: 0x0400046B RID: 1131
		private readonly string \u0001;
	}
}
