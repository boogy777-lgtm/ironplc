using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0017;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000339 RID: 825
	internal sealed class GenericTypeChecker
	{
		// Token: 0x1700081C RID: 2076
		// (get) Token: 0x060031C8 RID: 12744 RVA: 0x000C0774 File Offset: 0x000BE974
		private Action<_IExpression, string, MessageId> AddError { get; }

		// Token: 0x060031C9 RID: 12745 RVA: 0x000C077C File Offset: 0x000BE97C
		public GenericTypeChecker(Action<_IExpression, string, MessageId> addError)
		{
			this.AddError = addError;
		}

		// Token: 0x060031CA RID: 12746 RVA: 0x000C078C File Offset: 0x000BE98C
		internal bool \u0001(IGenericUserdefType \u0002)
		{
			IEnumerable<_IExpression> genericConstantsInitializations = \u0002.GenericConstantsInitializations;
			_IExpression[] source = (genericConstantsInitializations as _IExpression[]) ?? genericConstantsInitializations.ToArray<_IExpression>();
			int num = source.OfType<_IAssignmentExpression>().Count<_IAssignmentExpression>();
			if (num > 0 && num != source.Count<_IExpression>())
			{
				this.AddError((_IExpression)\u0002.NameExpression, global::\u0003.\u0006.\u0001(MessageId.Err_GenericParamsAllExplicitOrNone, Array.Empty<object>()), MessageId.Err_GenericParamsAllExplicitOrNone);
				return false;
			}
			return true;
		}

		// Token: 0x060031CB RID: 12747 RVA: 0x000C07F8 File Offset: 0x000BE9F8
		internal void \u0001(_IUserdefType \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			if (\u0003 == null)
			{
				return;
			}
			if (\u0003.GetFlagInternal(SignatureFlagInternal.ContainsGenericConstants))
			{
				if (\u0002.NameExpression.ToString().Contains("<"))
				{
					return;
				}
				int num = 0;
				IGenericUserdefType genericUserdefType = \u0002 as IGenericUserdefType;
				if (genericUserdefType != null)
				{
					num = genericUserdefType.GenericConstantsInitializations.Count<_IExpression>();
				}
				int num2 = \u0003.AllVariables.Count(new Func<_IVariable, bool>(GenericTypeChecker.<>c.<>9.\u0001));
				if (num2 != num && \u0004 != null && !\u0004.GetFlag(VarFlag.Implicit))
				{
					this.AddError((_IExpression)\u0002.NameExpression, global::\u0003.\u0006.\u0001(MessageId.Err_GenericWrongNumberOfInitializer, new object[]
					{
						\u0003.OrgName,
						num2
					}), MessageId.Err_GenericWrongNumberOfInitializer);
					return;
				}
			}
			else if (\u0002 is IGenericUserdefType)
			{
				string arg = global::\u0003.\u0006.\u0001(MessageId.Err_TypeIsNotGeneric, new object[]
				{
					\u0003.OrgName
				});
				this.AddError((_IExpression)\u0002.NameExpression, arg, MessageId.Err_TypeIsNotGeneric);
			}
		}

		// Token: 0x060031CC RID: 12748 RVA: 0x000C090C File Offset: 0x000BEB0C
		internal bool \u0001(_IExpression \u0002, IVariable \u0003, global::\u0017.\u0006 \u0004)
		{
			if (!\u0004.\u0001(\u0002))
			{
				this.AddError(\u0002, global::\u0003.\u0006.\u0001(MessageId.Err_GenericNotConstant, new object[]
				{
					\u0002.ToString(),
					\u0003.OrgName
				}), MessageId.Err_GenericNotConstant);
				return false;
			}
			return true;
		}

		// Token: 0x060031CD RID: 12749 RVA: 0x000C0958 File Offset: 0x000BEB58
		internal bool \u0001(_IVariable[] \u0002, IGenericUserdefType \u0003)
		{
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary = new CaseInsensitiveDictionary<bool>();
			CaseInsensitiveDictionary<bool> caseInsensitiveDictionary2 = new CaseInsensitiveDictionary<bool>();
			foreach (_IVariable variable in \u0002)
			{
				caseInsensitiveDictionary2[variable.Name] = true;
			}
			bool result = true;
			foreach (_IAssignmentExpression iassignmentExpression in \u0003.GenericConstantsInitializations.OfType<_IAssignmentExpression>())
			{
				string text = iassignmentExpression._LValue.ToString();
				if (!caseInsensitiveDictionary2.ContainsKey(text))
				{
					this.AddError(iassignmentExpression._LValue, global::\u0003.\u0006.\u0001(MessageId.Err_GenericParamUnknown, new object[]
					{
						text,
						\u0003.NameExpression
					}), MessageId.Err_GenericParamUnknown);
				}
				if (!caseInsensitiveDictionary.ContainsKey(text))
				{
					caseInsensitiveDictionary.Add(text, true);
				}
			}
			if (caseInsensitiveDictionary.Count > 0)
			{
				foreach (_IVariable ivariable in \u0002)
				{
					if (!caseInsensitiveDictionary.ContainsKey(ivariable.Name))
					{
						this.AddError((_IExpression)\u0003.NameExpression, global::\u0003.\u0006.\u0001(MessageId.Err_GenericParamMissing, new object[]
						{
							ivariable.OrgName
						}), MessageId.Err_GenericParamMissing);
						result = false;
					}
				}
			}
			return result;
		}

		// Token: 0x04000965 RID: 2405
		[CompilerGenerated]
		private readonly Action<_IExpression, string, MessageId> \u0001;
	}
}
