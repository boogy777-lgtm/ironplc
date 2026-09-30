using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.Legacy
{
	internal static class LegacySwitch
	{
		private static bool TryCreateQualifierService(out ILMQualifierService qualifierService)
		{
			qualifierService = null;
			if (APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService is ILMPreCompileService5 iLMPreCompileService)
			{
				qualifierService = iLMPreCompileService.CreateQualifierService();
				return qualifierService != null;
			}
			return false;
		}

		private static bool TryGetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, out string stQualifiedTypeText)
		{
			stQualifiedTypeText = null;
			if (!TryCreateQualifierService(out var qualifierService))
			{
				return false;
			}
			IType qualifiedType = qualifierService.GetQualifiedType(type, (ILMPreCompileSet)precomSource, (ILMPreCompileSet)precomDest);
			if (qualifiedType != null)
			{
				stQualifiedTypeText = qualifiedType.ToString();
				return true;
			}
			return false;
		}

		private static bool TryQualifyExpression(IExpression expressionToQualify, string stQualificationNamespace, out string stQualifiedExpression)
		{
			stQualifiedExpression = null;
			if (!TryCreateQualifierService(out var qualifierService))
			{
				return false;
			}
			IExpression qualifiedExpression = qualifierService.GetQualifiedExpression(expressionToQualify, stQualificationNamespace);
			if (qualifiedExpression == null)
			{
				return false;
			}
			stQualifiedExpression = qualifiedExpression.ToString();
			return true;
		}

		private static bool TryQualifyExpression(IExpression expressionToQualify, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, out string stQualifiedExpression)
		{
			stQualifiedExpression = null;
			if (!TryCreateQualifierService(out var qualifierService))
			{
				return false;
			}
			IExpression qualifiedExpression = qualifierService.GetQualifiedExpression(expressionToQualify, (ILMPreCompileSet)precomSource, (ILMPreCompileSet)precomDest);
			if (qualifiedExpression == null)
			{
				return false;
			}
			stQualifiedExpression = qualifiedExpression.ToString();
			return true;
		}

		internal static string GetQualifiedTypeText(IType type, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest)
		{
			if (TryGetQualifiedTypeText(type, precomSource, precomDest, out var stQualifiedTypeText))
			{
				return stQualifiedTypeText;
			}
			return LegacyQualifiedTypeTextCreator.GetQualifiedTypeText(type, precomSource, precomDest);
		}

		internal static string DumpQualifiedExpressionText(IExpression expressionToQualify, string stQualificationNamespace)
		{
			if (TryQualifyExpression(expressionToQualify, stQualificationNamespace, out var stQualifiedExpression))
			{
				return stQualifiedExpression;
			}
			return LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(expressionToQualify, stQualificationNamespace);
		}

		internal static string DumpQualifiedExpressionText(IExpression expressionToQualify, IPreCompileContext3 precomSource, IPreCompileContext3 precomDest, string stQualificationNamespace)
		{
			if (TryQualifyExpression(expressionToQualify, precomSource, precomDest, out var stQualifiedExpression))
			{
				return stQualifiedExpression;
			}
			return LegacyQualifiedExpressionTextVisitor.DumpQualifiedExpressionText(expressionToQualify, stQualificationNamespace);
		}
	}
}
