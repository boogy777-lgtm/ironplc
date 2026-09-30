using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Will be fixed with CDS-77832")]
	internal static class InitialValue
	{
		public static InitialValueResult Determine(IEvaluationContext ctxBase, string stAccessPath, out IExpression expInit, out IEvaluationContext2 ctxExpInit)
		{
			expInit = null;
			ctxExpInit = null;
			if (ctxBase == null || string.IsNullOrEmpty(stAccessPath))
			{
				throw new ArgumentNullException("");
			}
			IPreCompileContext precompileContext = Helpers.GetPrecompileContext(ctxBase);
			if (precompileContext == null)
			{
				throw new ArgumentException("context: no precompile context found");
			}
			ISignature2 signature = (ISignature2)precompileContext.GetSignature(ctxBase.ScopeIdentification);
			if (signature == null)
			{
				if (ctxBase is IEvaluationContext4)
				{
					signature = (ISignature2)precompileContext.GetSignature(((IEvaluationContext4)ctxBase).LocalScopeIdentification);
				}
				if (signature == null)
				{
					throw new ArgumentException("context: no signature");
				}
			}
			IExpression expression = Helpers.ParseExpression(stAccessPath);
			if (expression == null)
			{
				return InitialValueResult.AccessPathInvalid;
			}
			int attractingProject = Helpers.GetAttractingProject(ctxBase);
			ISignature2 sigSub;
			ISignature2 sigExpInitValue;
			IArrayType typArraySub;
			InitialValueResult initialValueResult = ((!(ctxBase is IEvaluationContext3)) ? DetermineRec(attractingProject, signature, null, null, expression, out sigSub, out expInit, out sigExpInitValue, out typArraySub, new GetLibInformation()) : DetermineRec(attractingProject, signature, null, null, expression, out sigSub, out expInit, out sigExpInitValue, out typArraySub, (ctxBase as IEvaluationContext3).LibInfo));
			if (initialValueResult == InitialValueResult.OK)
			{
				if (ctxBase is IEvaluationContext3)
				{
					ctxExpInit = Helpers.GetContextFromSignature(attractingProject, sigExpInitValue, null, (ctxBase as IEvaluationContext3).LibInfo);
				}
				else
				{
					ctxExpInit = Helpers.GetContextFromSignature(attractingProject, sigExpInitValue, null, new GetLibInformation());
				}
			}
			else
			{
				ctxExpInit = null;
			}
			return initialValueResult;
		}

		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-77832")]
		[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Will be fixed with CDS-77832")]
		private static InitialValueResult DetermineRec(int nProjAttracting, ISignature2 sigCur, ISignature2 sigExpInit, IExpression expInit, IExpression expAccessPath, out ISignature2 sigSub, out IExpression expInitValue, out ISignature2 sigExpInitValue, out IArrayType typArraySub, IGetLibInformation libInfo)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			expInitValue = null;
			sigExpInitValue = sigExpInit;
			sigSub = null;
			typArraySub = null;
			if (expAccessPath is IVariableExpression)
			{
				IVariableExpression variableExpression = (IVariableExpression)expAccessPath;
				Pair<ISignature2, IVariable> val = FindVar(sigCur, variableExpression.ToString());
				IVariable v = val.second;
				if (v == null)
				{
					return InitialValueResult.VariableNotFound;
				}
				IType type = v.Type;
				if (type is IArrayType)
				{
					typArraySub = (IArrayType)type;
					type = GetRootBaseType(type);
				}
				if (Helpers.IsPrimitiveType(type) || Helpers.IsSubrangeType(type))
				{
					sigSub = null;
				}
				else
				{
					ISignature2 first = val.first;
					int projectHandleFromSignature = Helpers.GetProjectHandleFromSignature(first);
					sigSub = Helpers.FindTypeSignature2FromObj(projectHandleFromSignature, first.ObjectGuid, type.ToString());
					if (sigSub == null)
					{
						sigSub = Helpers.FindTypeSignature2FromObj(projectHandleFromSignature, first.ObjectGuid, Fun.Last<string>((IEnumerable<string>)Common.SplitAtDot(type.ToString())));
					}
					if (sigSub == null)
					{
						return InitialValueResult.SourceCodeInvalid;
					}
				}
				if (expInit == null)
				{
					expInitValue = MergeWithDeclaration(v.Initial, sigSub);
					sigExpInitValue = sigCur;
				}
				else
				{
					if (!(expInit is IStructureInitialization))
					{
						return InitialValueResult.SourceCodeInvalid;
					}
					IStructureInitialization structureInitialization = (IStructureInitialization)expInit;
					IAssignmentExpression assignmentExpression = Fun.Find<IAssignmentExpression>((Fun1<IAssignmentExpression, bool>)((IAssignmentExpression e) => Helpers.StrEqCI(e.LValue.ToString(), v.OrgName)), (IEnumerable<IAssignmentExpression>)structureInitialization.CompoInits);
					if (assignmentExpression != null)
					{
						expInitValue = assignmentExpression.RValue;
					}
					else
					{
						expInitValue = v.Initial;
						sigExpInitValue = sigCur;
					}
				}
				return InitialValueResult.OK;
			}
			if (expAccessPath is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = (ICompoAccessExpression)expAccessPath;
				ISignature2 sigSub2;
				IExpression expInitValue2;
				ISignature2 sigExpInitValue2;
				IArrayType typArraySub2;
				InitialValueResult initialValueResult = DetermineRec(nProjAttracting, sigCur, sigExpInit, expInit, compoAccessExpression.Left, out sigSub2, out expInitValue2, out sigExpInitValue2, out typArraySub2, libInfo);
				if (initialValueResult != 0)
				{
					return initialValueResult;
				}
				return DetermineRec(nProjAttracting, sigSub2, sigExpInitValue2, expInitValue2, compoAccessExpression.Right, out sigSub, out expInitValue, out sigExpInitValue, out typArraySub, libInfo);
			}
			if (expAccessPath is IIndexAccessExpression)
			{
				IIndexAccessExpression indexAccessExpression = (IIndexAccessExpression)expAccessPath;
				IExpression expInitValue3;
				IArrayType typArraySub3;
				InitialValueResult initialValueResult2 = DetermineRec(nProjAttracting, sigCur, sigExpInit, expInit, indexAccessExpression.Var, out sigSub, out expInitValue3, out sigExpInitValue, out typArraySub3, libInfo);
				if (initialValueResult2 != 0)
				{
					return initialValueResult2;
				}
				typArraySub = typArraySub3.Base as IArrayType;
				try
				{
					IEvaluationContext4 contextFromSignature = Helpers.GetContextFromSignature(nProjAttracting, sigCur, sigExpInitValue, libInfo);
					long lIndexFlat;
					InitialValueResult initialValueResult3 = ComputeFlatArrayIndex(contextFromSignature, typArraySub3, indexAccessExpression.Accesses, out lIndexFlat);
					if (initialValueResult3 != 0)
					{
						return initialValueResult3;
					}
					IExpression expInitArray;
					InitialValueResult arrayElemInitialValue = GetArrayElemInitialValue(contextFromSignature, expInitValue3, lIndexFlat, out expInitArray);
					if (arrayElemInitialValue != 0)
					{
						return arrayElemInitialValue;
					}
					expInitValue = expInitArray;
					return InitialValueResult.OK;
				}
				catch (LanguageModelUtilitiesException)
				{
					return InitialValueResult.SourceCodeInvalid;
				}
			}
			return InitialValueResult.AccessPathInvalid;
		}

		private static string IECLiteralFromObject(object value, TypeClass type)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetConverterToIEC(bOmitPrefixesWherePossible: true, bUseShortPrefixes: true, DisplayMode.Decimal).GetLiteralText(value, type);
		}

		private static InitialValueResult ComputeFlatArrayIndex(IEvaluationContext context, IArrayType t, IExpression[] indices, out long lIndexFlat)
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			lIndexFlat = 0L;
			if (t.Dimensions.Length != indices.Length)
			{
				return InitialValueResult.SourceCodeInvalid;
			}
			ArrayDimensionEvaluator dimmer = new ArrayDimensionEvaluator(context);
			ConstantEvaluator conster = new ConstantEvaluator(context);
			IEnumerable<Pair<int, int>> enumerable = Fun.Reversed<Pair<int, int>>(Fun.Map<IArrayDimension, Pair<int, int>>((Fun1<IArrayDimension, Pair<int, int>>)delegate(IArrayDimension dim)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				dimmer.EvaluateArrayDimension(dim, out var iLowerBorder, out var iUpperBorder);
				return Fun.Pair<int, int>(iLowerBorder, iUpperBorder);
			}, (IEnumerable<IArrayDimension>)t.Dimensions));
			IEnumerable<long> enumerable2 = Fun.Reversed<long>(Fun.Map<IExpression, long>((Fun1<IExpression, long>)((IExpression exp) => GetLongLitVal(conster.Evaluate(exp))), (IEnumerable<IExpression>)indices));
			long num = 1L;
			foreach (Pair<long, Pair<int, int>> item in Fun.Zip<long, Pair<int, int>>(enumerable2, enumerable))
			{
				long first = item.first;
				Pair<int, int> second = item.second;
				if (first < second.first || first > second.second)
				{
					return InitialValueResult.SourceCodeInvalid;
				}
				lIndexFlat += (first - second.first) * num;
				num *= Math.Max(0, 1 + (second.second - second.first));
			}
			return InitialValueResult.OK;
		}

		private static long GetLongLitVal(ILiteralValue litVal)
		{
			if (litVal.KindOf == KindOfLiteral.SignedInteger)
			{
				return litVal.SignedLong;
			}
			if (litVal.KindOf == KindOfLiteral.UnsignedInteger)
			{
				return (long)litVal.UnsignedLong;
			}
			throw new LanguageModelUtilitiesException("Invalid literal");
		}

		private static InitialValueResult GetArrayElemInitialValue(IEvaluationContext context, IExpression expInit, long lIndexFlat, out IExpression expInitArray)
		{
			expInitArray = null;
			if (expInit == null)
			{
				return InitialValueResult.OK;
			}
			if (!(expInit is IArrayInitialization))
			{
				return InitialValueResult.SourceCodeInvalid;
			}
			IArrayInitialization obj = (IArrayInitialization)expInit;
			long num = 0L;
			IExpression[] initValues = obj.InitValues;
			foreach (IExpression expression in initValues)
			{
				IExpression expression2 = expression;
				long num2 = 1L;
				if (expression is IMultipleIndexInitialization)
				{
					IMultipleIndexInitialization multipleIndexInitialization = (IMultipleIndexInitialization)expression;
					num2 = GetLongLitVal(new ConstantEvaluator(context).Evaluate(multipleIndexInitialization.Number));
					expression2 = multipleIndexInitialization.Value;
				}
				num += num2;
				if (num > lIndexFlat)
				{
					expInitArray = expression2;
					break;
				}
			}
			return InitialValueResult.OK;
		}

		private static IType GetRootBaseType(IType t)
		{
			if (t is IArrayType)
			{
				return GetRootBaseType(((IArrayType)t).Base);
			}
			return t;
		}

		public static Pair<ISignature2, IVariable> FindVar(ISignature2 sig, string stName)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			Fun1<Pair<ISignature2, IVariable>, bool> obj = (Pair<ISignature2, IVariable> p) => Helpers.StrEqCI(p.second.OrgName, stName);
			Fun1<ISignature2, IEnumerable<Pair<ISignature2, IVariable>>> obj2 = GetVarPairs;
			ILanguageModelManager22 languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			return Fun.Find<Pair<ISignature2, IVariable>>(obj, Fun.Join<Pair<ISignature2, IVariable>>(Fun.Map<ISignature2, IEnumerable<Pair<ISignature2, IVariable>>>(obj2, Fun.IterateNull<ISignature2>(sig, (Fun1<ISignature2, ISignature2>)languageModelMgr.GetBaseSignature))));
		}

		private static IEnumerable<Pair<ISignature2, IVariable>> GetVarPairs(ISignature2 sig)
		{
			return Fun.Map<IVariable, Pair<ISignature2, IVariable>>((Fun1<IVariable, Pair<ISignature2, IVariable>>)((IVariable v) => Fun.Pair<ISignature2, IVariable>(sig, v)), (IEnumerable<IVariable>)sig.All);
		}

		private static IExpression MergeWithDeclaration(IExpression initValueOfInstance, ISignature2 sigSub)
		{
			if (initValueOfInstance is IStructureInitialization structureInitialization && sigSub != null)
			{
				List<IAssignmentExpression> list = new List<IAssignmentExpression>();
				ILanguageModelBuilder languageModelBuilder = APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder();
				IAssignmentExpression[] compoInits = structureInitialization.CompoInits;
				foreach (IAssignmentExpression assignmentExpression in compoInits)
				{
					bool flag = false;
					IVariable variable = sigSub[assignmentExpression.LValue.ToString()];
					if (variable != null)
					{
						IStructureInitialization structureInitialization2 = assignmentExpression.RValue as IStructureInitialization;
						IStructureInitialization structureInitialization3 = variable.Initial as IStructureInitialization;
						if (structureInitialization2 != null && structureInitialization3 != null)
						{
							SortedList<string, IAssignmentExpression> sortedList = new SortedList<string, IAssignmentExpression>();
							IAssignmentExpression[] compoInits2 = structureInitialization2.CompoInits;
							foreach (IAssignmentExpression assignmentExpression2 in compoInits2)
							{
								sortedList.Add(assignmentExpression2.LValue.ToString(), assignmentExpression2);
							}
							compoInits2 = structureInitialization3.CompoInits;
							foreach (IAssignmentExpression assignmentExpression3 in compoInits2)
							{
								if (!sortedList.ContainsKey(assignmentExpression3.LValue.ToString()))
								{
									sortedList.Add(assignmentExpression3.LValue.ToString(), assignmentExpression3);
								}
							}
							List<IAssignmentExpression> list2 = new List<IAssignmentExpression>();
							list2.AddRange(sortedList.Values);
							IStructureInitialization expRight = languageModelBuilder.CreateStructureInitialisation(null, list2);
							IAssignmentExpression item = languageModelBuilder.CreateAssignmentExpression(null, assignmentExpression.LValue, expRight);
							list.Add(item);
							flag = true;
						}
					}
					if (!flag)
					{
						list.Add(assignmentExpression);
					}
				}
				return languageModelBuilder.CreateStructureInitialisation(null, list);
			}
			return initValueOfInstance;
		}
	}
}
