using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using \u0003;
using \u0004;
using \u0007;
using \u0010;
using \u0011;
using \u0012;
using \u0018;
using \u0019;
using \u001A;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x0200035C RID: 860
	internal sealed class OnlineChangeCopyCodeGenerator
	{
		// Token: 0x0600339C RID: 13212 RVA: 0x000C9F78 File Offset: 0x000C8178
		public OnlineChangeCopyCodeGenerator(_ICompileContext comconNew, _ICompileContext comconRef, bool bNoDataChanged, OnlineChangeDetails onlineChangeDetails, Codegeneration codegeneration)
		{
			this.\u0001 = comconNew;
			this.\u0002 = comconRef;
			this.\u0001 = onlineChangeDetails;
			this.\u0001 = codegeneration;
			this.\u0001 = global::\u0007.\u0005.\u0001(comconNew);
		}

		// Token: 0x0600339D RID: 13213 RVA: 0x000C9FAC File Offset: 0x000C81AC
		internal _ICompiledPOU \u0001()
		{
			_ICompiledPOU icompiledPOU = this.\u0001(this.\u0001, this.\u0002, false, this.\u0001);
			_ISignature u = this.\u0001[icompiledPOU.SignatureId];
			this.\u0001.\u0001(icompiledPOU, null, u);
			return icompiledPOU;
		}

		// Token: 0x0600339E RID: 13214 RVA: 0x000C9FF4 File Offset: 0x000C81F4
		internal _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, OnlineChangeDetails \u0005)
		{
			LList<_ISignature> llist = Helper.\u0001(\u0002.AllFlat);
			this.\u0001 = \u0002["GLOBAL__COPY__CODE"];
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, this.\u0001.Id);
			_ICompiledPOU icompiledPOU = global::\u0019.\u0003.\u0001(this.\u0001.Name);
			global::\u0007.\u0013.\u0001(\u0002, icompiledPOU, this.\u0001, null);
			_ISequenceStatement isequenceStatement = icompiledPOU.ParseTree as _ISequenceStatement;
			this.\u0001(\u0002, icompiledPOU, isequenceStatement);
			global::\u0010.\u0011.\u0001(\u0002, \u0003, this.\u0001, isequenceStatement);
			this.\u0001(\u0004, \u0005, llist, this.\u0001, isequenceStatement, icompiledPOU);
			if (this.\u0001(\u0005))
			{
				Helper.\u0001(\u0002, llist, SignatureFlagInternal.OnlineChangePartialInit);
				this.\u0001(llist, isequenceStatement, icompiledPOU);
				this.\u0001(llist);
				OnlineChangeCopyCodeGenerator.\u0001(\u0002, llist, u, icompiledPOU, isequenceStatement);
				this.\u0001(\u0005, icompiledPOU, isequenceStatement);
				global::\u0018.\u000E.\u0001(isequenceStatement, \u0002);
			}
			Locator.\u0001(this.\u0001, null, \u0002, null);
			icompiledPOU.SetParseTree(isequenceStatement);
			icompiledPOU.SignatureId = this.\u0001.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone, true);
			return icompiledPOU;
		}

		// Token: 0x0600339F RID: 13215 RVA: 0x000CA104 File Offset: 0x000C8304
		private bool \u0001(_IOnlineChangeDetails \u0002)
		{
			bool result = true;
			LStack<int> lstack = new LStack<int>();
			foreach (IVariableInfo variableInfo in \u0002.VariablesAffected)
			{
				OnlineChangeCopyCodeGenerator.\u0001(this.\u0001[variableInfo.SignatureId][variableInfo.VariableId] as _IVariable, lstack);
			}
			LHashSet<int> lhashSet = new LHashSet<int>();
			while (lstack.Count > 0)
			{
				int num = lstack.Pop();
				if (!lhashSet.Contains(num))
				{
					lhashSet.Add(num);
					_ISignature isignature = this.\u0001[num] as _ISignature;
					if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_NO_ONLINE_CHANGE))
					{
						isignature.AddMessage(Severity.Error, MessageId.Err_NoCopyCodeAllowed, Array.Empty<object>());
						result = false;
					}
					foreach (_IVariable u in isignature.AllVariables)
					{
						OnlineChangeCopyCodeGenerator.\u0001(u, lstack);
					}
				}
			}
			return result;
		}

		// Token: 0x060033A0 RID: 13216 RVA: 0x000CA22C File Offset: 0x000C842C
		private static void \u0001(_IVariable \u0002, LStack<int> \u0003)
		{
			ICompiledType compiledType = \u0002._Type.EffectiveType;
			if (compiledType.Class == TypeClass.Array)
			{
				compiledType = \u0084.\u0004.\u0001(compiledType as _IArrayType);
			}
			if (compiledType.Class != TypeClass.Userdef)
			{
				return;
			}
			\u0003.Push(((_IUserdefType)compiledType).SignatureId);
		}

		// Token: 0x060033A1 RID: 13217 RVA: 0x000CA278 File Offset: 0x000C8478
		private void \u0001(IEnumerable<_ISignature> \u0002, _ISequenceStatement \u0003)
		{
			foreach (_ISignature isignature in \u0002)
			{
				_ISignature u = null;
				if (this.\u0002 != null)
				{
					u = this.\u0002[isignature.Id];
				}
				global::\u0004.\u0014.\u0001(this.\u0001, this.\u0002, \u0003, isignature, u);
			}
		}

		// Token: 0x060033A2 RID: 13218 RVA: 0x000CA2EC File Offset: 0x000C84EC
		private void \u0001(IScope5 \u0002, _ICompiledPOU \u0003, _ISequenceStatement \u0004, _ISequenceStatement \u0005)
		{
			\u0005.Accept(new ExpressionTypifierWithSpecialTasks(\u0002, this.\u0001, false, \u0003)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			\u0005.Accept(new TypeCheckerVisitor(\u0002, this.\u0001, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0005.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			\u0004.Add(\u0005);
		}

		// Token: 0x060033A3 RID: 13219 RVA: 0x000CA36C File Offset: 0x000C856C
		private void \u0001(IEnumerable<_ISignature> \u0002, _ISignature \u0003, _ISequenceStatement \u0004, _ICompiledPOU \u0005)
		{
			foreach (_ISignature isignature in \u0002)
			{
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
				scope.LocalSignature = isignature;
				scope.MethodSignature = \u0003;
				if (!isignature.HasAttribute("mapping_unchanged"))
				{
					string u = GVLInitialisationFunctionCreator.\u0002(isignature);
					_IExpression expVariable = global::\u0019.\u0003.\u0001("__bInitRetains");
					_IExpression exp = global::\u0019.\u0003.\u0001(true);
					_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(u));
					icallExpression.AddParam(exp, expVariable);
					isequenceStatement.Add(global::\u0019.\u0003.\u0001(icallExpression));
					this.\u0001(scope, \u0005, \u0004, isequenceStatement);
				}
			}
		}

		// Token: 0x060033A4 RID: 13220 RVA: 0x000CA430 File Offset: 0x000C8630
		private void \u0001(bool \u0002, _IOnlineChangeDetails \u0003, IEnumerable<_ISignature> \u0004, _ISignature \u0005, _ISequenceStatement \u0006, _ICompiledPOU \u0007)
		{
			foreach (_ISignature isignature in \u0004)
			{
				_ISignature u = null;
				if (this.\u0002 != null)
				{
					u = this.\u0002[isignature.Id];
				}
				bool flag = false;
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
				scope.LocalSignature = isignature;
				scope.MethodSignature = \u0005;
				IEnumerable<_IVariable> allVariables = isignature.AllVariables;
				_ILocalSignatureIdPragma sm = global::\u0019.\u0003.\u0001("localsignature " + isignature.Id.ToString(), isignature.Id);
				isequenceStatement.Add(sm);
				bool u2 = isignature.HasAttribute("vfinitonly");
				foreach (_IVariable ivariable in allVariables)
				{
					if (ivariable.GetFlag(VarFlag.Absolut))
					{
						OnlineChangeCopyCodeGenerator.\u0001(\u0003, ivariable, isignature);
						flag = OnlineChangeCopyCodeGenerator.\u0001(this.\u0001, this.\u0002, \u0002, \u0003, \u0005, ivariable, isignature, scope, isequenceStatement, flag, u2, u);
					}
				}
				if (flag)
				{
					this.\u0001(scope, \u0007, \u0006, isequenceStatement);
				}
			}
		}

		// Token: 0x060033A5 RID: 13221 RVA: 0x000CA59C File Offset: 0x000C879C
		private void \u0002(bool \u0002, _IOnlineChangeDetails \u0003, IEnumerable<_ISignature> \u0004, _ISignature \u0005, _ISequenceStatement \u0006, _ICompiledPOU \u0007)
		{
			foreach (_ISignature isignature in \u0004)
			{
				_ISignature u = null;
				if (this.\u0002 != null)
				{
					u = this.\u0002[isignature.Id];
				}
				bool flag = false;
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
				scope.LocalSignature = isignature;
				scope.MethodSignature = \u0005;
				IEnumerable<_IVariable> allVariables = isignature.AllVariables;
				_ILocalSignatureIdPragma sm = global::\u0019.\u0003.\u0001("localsignature " + isignature.Id.ToString(), isignature.Id);
				isequenceStatement.Add(sm);
				bool u2 = isignature.HasAttribute("vfinitonly");
				foreach (_IVariable ivariable in allVariables)
				{
					if (ivariable.GetFlag(VarFlag.Absolut))
					{
						flag = OnlineChangeCopyCodeGenerator.\u0002(this.\u0001, this.\u0002, \u0002, \u0003, \u0005, ivariable, isignature, scope, isequenceStatement, flag, u2, u);
					}
				}
				if (flag)
				{
					this.\u0001(scope, \u0007, \u0006, isequenceStatement);
				}
			}
		}

		// Token: 0x060033A6 RID: 13222 RVA: 0x000CA6FC File Offset: 0x000C88FC
		private void \u0003(bool \u0002, _IOnlineChangeDetails \u0003, IEnumerable<_ISignature> \u0004, _ISignature \u0005, _ISequenceStatement \u0006, _ICompiledPOU \u0007)
		{
			foreach (_ISignature isignature in \u0004)
			{
				_ISignature u = null;
				if (this.\u0002 != null)
				{
					u = this.\u0002[isignature.Id];
				}
				bool flag = false;
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
				scope.LocalSignature = isignature;
				scope.MethodSignature = \u0005;
				IEnumerable<_IVariable> allVariables = isignature.AllVariables;
				_ILocalSignatureIdPragma sm = global::\u0019.\u0003.\u0001("localsignature " + isignature.Id.ToString(), isignature.Id);
				isequenceStatement.Add(sm);
				bool u2 = isignature.HasAttribute("vfinitonly");
				foreach (_IVariable ivariable in allVariables)
				{
					if (ivariable.GetFlag(VarFlag.Absolut))
					{
						flag = OnlineChangeCopyCodeGenerator.\u0004(this.\u0001, this.\u0002, \u0002, \u0003, \u0005, ivariable, isignature, scope, isequenceStatement, flag, u2, u);
					}
				}
				if (flag)
				{
					this.\u0001(scope, \u0007, \u0006, isequenceStatement);
				}
			}
		}

		// Token: 0x060033A7 RID: 13223 RVA: 0x000CA85C File Offset: 0x000C8A5C
		private void \u0001(bool \u0002, OnlineChangeDetails \u0003, IList<_ISignature> \u0004, _ISignature \u0005, _ISequenceStatement \u0006, _ICompiledPOU \u0007)
		{
			IEnumerable<_ISignature> enumerable = \u0004.Where(new Func<_ISignature, bool>(OnlineChangeCopyCodeGenerator.\u0001)).ToList<_ISignature>();
			IEnumerable<_ISignature> enumerable2 = enumerable.Where(new Func<_ISignature, bool>(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0001)).ToList<_ISignature>();
			IEnumerable<_ISignature> u = enumerable.Except(enumerable2).ToList<_ISignature>();
			this.\u0001(enumerable, \u0006);
			this.\u0001(\u0002, \u0003, u, \u0005, \u0006, \u0007);
			this.\u0002(\u0002, \u0003, u, \u0005, \u0006, \u0007);
			this.\u0003(\u0002, \u0003, u, \u0005, \u0006, \u0007);
			this.\u0001(enumerable2, \u0005, \u0006, \u0007);
		}

		// Token: 0x060033A8 RID: 13224 RVA: 0x000CA8FC File Offset: 0x000C8AFC
		private static bool \u0001(_ISignature \u0002)
		{
			return !\u0002.GetFlag(SignatureFlag.NoCopy) && !\u0002.GetFlagInternal(SignatureFlagInternal.ExplicitInitExitHandling);
		}

		// Token: 0x060033A9 RID: 13225 RVA: 0x000CA91C File Offset: 0x000C8B1C
		private static bool \u0001(_IVariable \u0002)
		{
			return !\u0002.GetFlag(VarFlag.NoCopy) && !\u0002.GetFlag(VarFlag.ReplacedConstant) && !\u0002.IsProperty;
		}

		// Token: 0x060033AA RID: 13226 RVA: 0x000CA948 File Offset: 0x000C8B48
		private static void \u0001(_IOnlineChangeDetails \u0002, _IVariable \u0003, _ISignature \u0004)
		{
			if (!OnlineChangeCopyCodeGenerator.\u0001(\u0003))
			{
				return;
			}
			if (\u0003.HasFlag((VarFlag)((ulong)-2134900736)))
			{
				\u0002.AddVariableInfo(\u0003, \u0004);
			}
			if (\u0003.HasFlag(VarFlag.LocationChanged | VarFlag.OnlChangeCopy | VarFlag.OnlChangeInit | VarFlag.OnlChangeVFInit | VarFlag.OnlChangeExit | VarFlag.OnlChangeReInit))
			{
				\u0002.AddResetVariable(\u0003);
			}
			if (\u0003.HasFlag(VarFlag.OnlChangeReInit))
			{
				\u0002.AddVariableInfo(\u0003, \u0004);
			}
		}

		// Token: 0x060033AB RID: 13227 RVA: 0x000CA9A8 File Offset: 0x000C8BA8
		private static bool \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, _IOnlineChangeDetails \u0005, _ISignature \u0006, _IVariable \u0007, _ISignature \u0008, IScope5 \u000E, _ISequenceStatement \u000F, bool \u0010, bool \u0011, _ISignature \u0012)
		{
			if (\u0007.GetFlag(VarFlag.Constant))
			{
				return OnlineChangeCopyCodeGenerator.\u0003(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012);
			}
			return \u0010;
		}

		// Token: 0x060033AC RID: 13228 RVA: 0x000CA9E0 File Offset: 0x000C8BE0
		private static bool \u0002(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, _IOnlineChangeDetails \u0005, _ISignature \u0006, _IVariable \u0007, _ISignature \u0008, IScope5 \u000E, _ISequenceStatement \u000F, bool \u0010, bool \u0011, _ISignature \u0012)
		{
			if (!\u0007.GetFlag(VarFlag.Constant))
			{
				return OnlineChangeCopyCodeGenerator.\u0003(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u000F, \u0010, \u0011, \u0012);
			}
			return \u0010;
		}

		// Token: 0x060033AD RID: 13229 RVA: 0x000CAA18 File Offset: 0x000C8C18
		private static bool \u0003(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, _IOnlineChangeDetails \u0005, _ISignature \u0006, _IVariable \u0007, _ISignature \u0008, IScope5 \u000E, _ISequenceStatement \u000F, bool \u0010, bool \u0011, _ISignature \u0012)
		{
			if (!OnlineChangeCopyCodeGenerator.\u0001(\u0007))
			{
				return \u0010;
			}
			bool flag = \u0007.GetFlag(VarFlag.OnlChangeInit) && !\u0007.GetFlag(VarFlag.NoInit) && !\u0008.GetFlag(SignatureFlag.NoInit) && !\u0011 && !\u0004;
			if (\u0007.GetFlag((VarFlag)((ulong)-2147483648)) && !flag)
			{
				_IExpression u = global::\u0019.\u0003.\u0001(\u0007.OrgName);
				bool flag2;
				_IStatement sm = \u0080.\u001A.\u0001(\u0007, true, \u0007._Type.DeRefType as _IType, u, \u000E, out flag2, \u0006, null, "bInitRetains", "bInCopyCode", 0, \u0002) as _IStatement;
				\u000F.Add(sm);
				\u0010 = true;
			}
			if (flag)
			{
				_IExpression u2 = global::\u0019.\u0003.\u0001(true);
				if (\u0012 != null && \u0007.Type.Class == TypeClass.Array)
				{
					IVariable variable = \u0012[\u0007.Id];
					if (variable != null)
					{
						StringBuilder stringBuilder = new StringBuilder();
						IArrayType arrayType = variable.CompiledType as IArrayType;
						if (arrayType != null)
						{
							for (int i = 0; i < arrayType.Dimensions.Length; i++)
							{
								string implicitIndexVariable = IdentifierConstants.GetImplicitIndexVariable(i);
								if (i > 0)
								{
									stringBuilder.Append(" AND ");
								}
								stringBuilder.AppendFormat("({0} >= {1}) AND ({0} <= {2})", implicitIndexVariable, arrayType.Dimensions[i].LowerBorder, arrayType.Dimensions[i].UpperBorder);
							}
							u2 = (new global::\u0011.\u0006(stringBuilder.ToString(), true).\u0002() as _IExpression);
						}
					}
				}
				_IStatement sm2 = global::\u0004.\u0018.\u0001(\u0002, \u0007, \u0008, \u000E, false, false, true, false, true, \u0006, null, global::\u0019.\u0003.\u0001(true), u2);
				\u000F.Add(sm2);
				\u0010 = true;
			}
			return \u0010;
		}

		// Token: 0x060033AE RID: 13230 RVA: 0x000CABC4 File Offset: 0x000C8DC4
		private static bool \u0004(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, _IOnlineChangeDetails \u0005, _ISignature \u0006, _IVariable \u0007, _ISignature \u0008, IScope5 \u000E, _ISequenceStatement \u000F, bool \u0010, bool \u0011, _ISignature \u0012)
		{
			if (!OnlineChangeCopyCodeGenerator.\u0001(\u0007))
			{
				return \u0010;
			}
			bool flag = true;
			if (\u0007.HasAttribute(CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE))
			{
				flag = false;
			}
			if (\u0007.HasAttribute(CompileAttributes.ATTRIBUTE_NO_COPY))
			{
				flag = false;
			}
			if (\u0007.Type.Class == TypeClass.Reference)
			{
				flag = false;
			}
			string versionedName = \u0007.VersionedName;
			if (\u0012 != null && \u0007.GetFlag(VarFlag.OnlChangeCopy) && flag)
			{
				IVariable variable = \u0012[\u0007.Id];
				if (variable != null)
				{
					LStringBuilder lstringBuilder = new LStringBuilder();
					global::\u0004.\u0014.\u0001(\u0002, \u0007, \u0008, lstringBuilder, \u0006, \u000E, versionedName, versionedName, \u0007.CompiledType, variable.CompiledType, \u0003, 0);
					_ISequenceStatement sm = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001() as _ISequenceStatement;
					\u000F.Add(sm);
					\u0010 = true;
				}
			}
			return \u0010;
		}

		// Token: 0x060033AF RID: 13231 RVA: 0x000CAC90 File Offset: 0x000C8E90
		private void \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _ISequenceStatement \u0004)
		{
			ReflectionAssignmentCoder.\u0001(\u0004, \u0002, \u0003);
		}

		// Token: 0x060033B0 RID: 13232 RVA: 0x000CAC9C File Offset: 0x000C8E9C
		private void \u0001(IList<_ISignature> \u0002, _ISequenceStatement \u0003, _ICompiledPOU \u0004)
		{
			OnlineChangeCopyCodeGenerator.CreatePartialInitCalleeExp u = new OnlineChangeCopyCodeGenerator.CreatePartialInitCalleeExp(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0001);
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			foreach (_ISignature isignature in \u0002)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					if (ivariable.GetFlag(VarFlag.Absolut))
					{
						this.\u0001(isignature, ivariable, u, isequenceStatement, this.\u0001, null);
					}
				}
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, this.\u0001.Id);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, this.\u0001, false, \u0004)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			};
			isequenceStatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, this.\u0001, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			};
			isequenceStatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			isequenceStatement.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			\u0003.Add(isequenceStatement);
		}

		// Token: 0x060033B1 RID: 13233 RVA: 0x000CADF0 File Offset: 0x000C8FF0
		private void \u0001(IList<_ISignature> \u0002)
		{
			OnlineChangeCopyCodeGenerator.CreatePartialInitCalleeExp u = new OnlineChangeCopyCodeGenerator.CreatePartialInitCalleeExp(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0002);
			foreach (_ISignature isignature in \u0002)
			{
				if (isignature.GetFlagInternal(SignatureFlagInternal.OnlineChangePartialInit) && (isignature.POUType == Operator.FunctionBlock || isignature.HasFlag(SignatureFlag.Structure)))
				{
					_ISignature isignature2 = isignature.GetSubSignature(IdentifierConstants.PartialInitMethodName) as _ISignature;
					_ISignature isignature3 = this.\u0002.GetSignatureById(isignature2.Id) as _ISignature;
					IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature2.Id);
					bool flag = false;
					_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
					isequenceStatement.Add(global::\u0019.\u0003.\u0001("implicit on", true));
					if (this.\u0001(isignature, isequenceStatement))
					{
						flag = true;
					}
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.GetFlag(VarFlag.OnlChangeInit))
						{
							_IExpression u2 = global::\u0019.\u0003.\u0001(false);
							_IStatement sm = global::\u0004.\u0018.\u0001(this.\u0001, ivariable, isignature, scope, false, false, false, false, false, isignature2, null, global::\u0019.\u0003.\u0001(true), u2);
							isequenceStatement.Add(sm);
							flag = true;
						}
						else if (this.\u0001(isignature, ivariable, u, isequenceStatement, isignature2, isignature3))
						{
							flag = true;
						}
					}
					isequenceStatement.Add(global::\u0019.\u0003.\u0001("implicit off", false));
					if (flag)
					{
						Locator.\u0001(this.\u0001.DataManager, this.\u0001, this.\u0002, isignature2, isignature3);
						_ICompiledPOU icompiledPOU = this.\u0001.GetCompiledPOUById(isignature2.Id) as _ICompiledPOU;
						ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, this.\u0001, false, icompiledPOU)
						{
							TreatReferenceAsPointer = true,
							InterfaceAsInterface = true
						};
						isequenceStatement.Accept(ivisit);
						TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, this.\u0001, true)
						{
							TreatReferenceAsPointer = true,
							InterfaceAsInterface = true
						};
						isequenceStatement.Accept(ivisit2);
						ErrorVisitor errorVisitor = new ErrorVisitor();
						isequenceStatement.Accept(errorVisitor);
						Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
						icompiledPOU.SetParseTree(isequenceStatement);
						icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
						this.\u0001.\u0001(icompiledPOU, isignature, isignature2);
						ushort u3 = 255;
						int u4 = -1;
						if (!MemoryCompiler.\u0003(this.\u0001.DataManager, ref u3, ref u4, this.\u0001.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.\u0001.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
						{
							IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
							string u5 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
							{
								icompiledPOU.Name,
								icompiledPOU.CompiledCode.CodeSize
							});
							_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u5, Severity.Error, MessageId.Err_OutOfCodeMemory);
							APEnvironmentFacade.Instance.AddMessage(messageCategory, message);
						}
						else
						{
							icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u3, u4);
						}
					}
				}
			}
		}

		// Token: 0x060033B2 RID: 13234 RVA: 0x000CB13C File Offset: 0x000C933C
		private bool \u0001(_ISignature \u0002, _ISequenceStatement \u0003)
		{
			bool result = false;
			if (\u0002.BaseSignatureId != Helper.InvalidId && global::\u0012.\u0014.\u0002(this.\u0001[\u0002.BaseSignatureId] as _ISignature))
			{
				_IStatement sm = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(null)), global::\u0019.\u0003.\u0001(IdentifierConstants.PartialInitMethodName))));
				\u0003.Add(sm);
				result = true;
			}
			return result;
		}

		// Token: 0x060033B3 RID: 13235 RVA: 0x000CB1A4 File Offset: 0x000C93A4
		private bool \u0001(_ISignature \u0002, _IVariable \u0003, OnlineChangeCopyCodeGenerator.CreatePartialInitCalleeExp \u0004, _ISequenceStatement \u0005, _ISignature \u0006, _ISignature \u0007)
		{
			bool result = false;
			ICompiledType compiledType = \u0003.CompiledType;
			ICompiledType compiledType2 = \u0003.CompiledType;
			if (compiledType.Class == TypeClass.Array)
			{
				compiledType2 = \u0084.\u0004.\u0001(compiledType as _IArrayType);
			}
			if (compiledType2.Class == TypeClass.Userdef)
			{
				_ISignature isignature = this.\u0001[((_IUserdefType)compiledType2).SignatureId];
				if (isignature != null && isignature.GetFlagInternal(SignatureFlagInternal.OnlineChangePartialInit))
				{
					if (compiledType.Class == TypeClass.Array)
					{
						IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, isignature.Id);
						bool flag = false;
						_IArrayType iarrayType = compiledType as _IArrayType;
						int num = Helper.\u0001(iarrayType);
						int numOfElements = iarrayType.GetNumOfElements(scope, out flag);
						if (num == 0)
						{
							return false;
						}
						if (flag && numOfElements == 0)
						{
							return false;
						}
						Helper.\u0001(num, \u0006, \u0007);
						_IStatement u = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(Helper.\u0001(\u0004(\u0002, \u0003), iarrayType, 0), global::\u0019.\u0003.\u0001(IdentifierConstants.PartialInitMethodName))));
						\u0005.Add(Helper.\u0001(iarrayType, u, scope, 0));
					}
					else
					{
						\u0005.Add(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(\u0004(\u0002, \u0003), global::\u0019.\u0003.\u0001(IdentifierConstants.PartialInitMethodName)))));
					}
					result = true;
					if (\u0003.HasFlag(VarFlag.Absolut))
					{
						this.\u0001.AddVariableInfo(\u0003, \u0002);
					}
				}
			}
			return result;
		}

		// Token: 0x060033B4 RID: 13236 RVA: 0x000CB2F4 File Offset: 0x000C94F4
		private static void \u0001(_ICompileContext \u0002, IList<_ISignature> \u0003, IScope5 \u0004, _ICompiledPOU \u0005, _ISequenceStatement \u0006)
		{
			LDictionary<ISignature, LList<ISignature>> ldictionary = new LDictionary<ISignature, LList<ISignature>>();
			foreach (_ISignature isignature in \u0003)
			{
				foreach (object obj in isignature._SubSignatures)
				{
					_ISignature isignature2 = (_ISignature)obj;
					if (isignature2.POUType == Operator.Method && isignature2.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE))
					{
						string attributeValue = isignature2.GetAttributeValue(CompileAttributes.ATTRIBUTE_CALL_ON_TYPE_CHANGE);
						if (!string.IsNullOrEmpty(attributeValue))
						{
							foreach (string stName in attributeValue.Split(new char[]
							{
								','
							}).Select(new Func<string, string>(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0001)))
							{
								IList<ISignature> list = \u0004[stName];
								if (list != null && list.Count == 1 && list[0].GetFlag(SignatureFlag.OnlineChanged))
								{
									if (!ldictionary.ContainsKey(isignature))
									{
										ldictionary[isignature] = new LList<ISignature>();
									}
									ldictionary[isignature].Add(isignature2);
								}
							}
						}
					}
				}
			}
			foreach (ISignature signature in ldictionary.Keys)
			{
				IScope5 u = global::\u0007.\u0005.\u0001(\u0002, signature.Id);
				LList<ISignature> llist = ldictionary[signature];
				if (signature.POUType == Operator.FunctionBlock)
				{
					IVariable[] array2;
					ISignature[] array3;
					foreach (string u2 in \u0080.\u0005.\u0001(\u0002, signature, out array2, out array3, true, false, false))
					{
						foreach (ISignature u3 in llist)
						{
							OnlineChangeCopyCodeGenerator.\u0001(\u0002, \u0005, \u0006, u2, u3, u);
						}
					}
				}
				else if (signature.POUType == Operator.Program)
				{
					foreach (ISignature u4 in llist)
					{
						OnlineChangeCopyCodeGenerator.\u0001(\u0002, \u0005, \u0006, signature.Name, u4, u);
					}
				}
			}
		}

		// Token: 0x060033B5 RID: 13237 RVA: 0x000CB5F8 File Offset: 0x000C97F8
		private static void \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _ISequenceStatement \u0004, string \u0005, ISignature \u0006, IScope5 \u0007)
		{
			_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(new global::\u0011.\u0006(\u0005).\u0002() as _IExpression, Token.Empty);
			icompoAccessExpression._Right = global::\u0019.\u0003.\u0001(\u0006.OrgName);
			_IExpressionStatement iexpressionStatement = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression, Token.Empty), Token.Empty);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u0007, \u0002, false, \u0003)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			};
			iexpressionStatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(\u0007, \u0002, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			};
			iexpressionStatement.Accept(ivisit2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			iexpressionStatement.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			\u0004.Add(iexpressionStatement);
		}

		// Token: 0x060033B6 RID: 13238 RVA: 0x000CB6B0 File Offset: 0x000C98B0
		private void \u0001(OnlineChangeDetails \u0002, _ICompiledPOU \u0003, _ISequenceStatement \u0004)
		{
			IScope5 u = global::\u0007.\u0005.\u0001(this.\u0002);
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(true, global::\u0011.\u0001.OnlineChangeRelinkProgress, \u0002.VariablesAffected.Count, "Variables");
			InterfaceRelinkCode interfaceRelinkCode = new InterfaceRelinkCode(this.\u0001, this.\u0002, this.\u0001);
			this.\u0001(\u0002, u, interfaceRelinkCode);
			\u0002.InterfacesToRelink = interfaceRelinkCode.\u0002();
			\u0002.InstancesToMove = interfaceRelinkCode.\u0001();
			\u0002.TotalNumberOfRelinkTests = (long)\u0002.InterfacesToRelink.Count;
			if (!APEnvironmentFacade.Instance.LanguageModelMgr.RaiseOnBeforeGenerateRelinkCode(this.\u0001.ApplicationGuid, \u0002))
			{
				throw new CancelledByUserException();
			}
			APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyNextTask(true, global::\u0011.\u0001.OnlineChangeRelinkCodegenProgress, (int)\u0002.TotalNumberOfRelinkTests, string.Empty);
			string text = interfaceRelinkCode.\u0001();
			if (string.IsNullOrWhiteSpace(text))
			{
				return;
			}
			_IStatement istatement = new global::\u0011.\u0006(text, true).\u0001();
			IScope5 scope = global::\u0007.\u0005.\u0001(this.\u0001, this.\u0001.Id);
			istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, this.\u0001, false, \u0003)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			istatement.Accept(new TypeCheckerVisitor(scope, this.\u0001, true)
			{
				TreatReferenceAsPointer = true,
				InterfaceAsInterface = true
			});
			ErrorVisitor errorVisitor = new ErrorVisitor();
			istatement.Accept(errorVisitor);
			Debug.\u0001(!Helper.\u0001(errorVisitor.MessageList, Severity.Error));
			\u0004.Add(istatement);
		}

		// Token: 0x060033B7 RID: 13239 RVA: 0x000CB834 File Offset: 0x000C9A34
		private static IEnumerable<IVariableInfo> \u0001(IVariableInfo \u0002, IScope5 \u0003, _ICompileContext \u0004)
		{
			OnlineChangeCopyCodeGenerator.\u0001 u = new OnlineChangeCopyCodeGenerator.\u0001();
			u.\u0001 = \u0003;
			u.\u0001 = \u0004;
			u.\u0001 = \u0002;
			VariableInfoWithInstancePath variableInfoWithInstancePath = u.\u0001 as VariableInfoWithInstancePath;
			if (variableInfoWithInstancePath != null)
			{
				return variableInfoWithInstancePath.InstancePaths.Select(new Func<string, _IExpression>(u.\u0001)).Where(new Func<_IExpression, bool>(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0001)).Select(new Func<_IExpression, VariableInfo>(u.\u0001));
			}
			return new IVariableInfo[]
			{
				u.\u0001
			};
		}

		// Token: 0x060033B8 RID: 13240 RVA: 0x000CB8C8 File Offset: 0x000C9AC8
		private static _IExpression \u0001(IScope5 \u0002, _ICompileContext \u0003, string \u0004)
		{
			_IParser iparser = global::\u0019.\u0001.\u0001(\u0004);
			iparser.UsedScanner.AllowMultipleUnderlines = true;
			_IExpression iexpression = (_IExpression)iparser.ParseExpression();
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(\u0002, \u0003, false, null);
			iexpression.Accept(ivisit);
			for (;;)
			{
				IDataLocation dataLocation = (iexpression != null) ? iexpression.DataLocation(\u0002) : null;
				if (dataLocation == null || !dataLocation.IsRelativ)
				{
					break;
				}
				_ICompoAccessExpression icompoAccessExpression = iexpression as _ICompoAccessExpression;
				if (icompoAccessExpression != null)
				{
					iexpression = icompoAccessExpression._Left;
				}
				else
				{
					iexpression = null;
				}
			}
			return iexpression;
		}

		// Token: 0x060033B9 RID: 13241 RVA: 0x000CB934 File Offset: 0x000C9B34
		private void \u0001(OnlineChangeDetails \u0002, IScope5 \u0003, InterfaceRelinkCode \u0004)
		{
			OnlineChangeCopyCodeGenerator.\u0002 u = new OnlineChangeCopyCodeGenerator.\u0002();
			u.\u0001 = \u0003;
			u.\u0001 = this;
			foreach (IVariableInfo variableInfo in \u0002.DeletedVariables.SelectMany(new Func<IVariableInfo, IEnumerable<IVariableInfo>>(u.\u0001)).Concat(\u0002.VariablesAffected).Distinct(VarInfoComparer_NoFlag.Instance).OrderBy(new Func<IVariableInfo, IVariableInfo>(OnlineChangeCopyCodeGenerator.<>c.<>9.\u0001), VarInfoComparer_NoFlag.Instance))
			{
				if ((variableInfo.Flags & (VarFlag.LocationChanged | VarFlag.OnlChangeCopy)) != VarFlag.None)
				{
					ISignature signature = u.\u0001[variableInfo.SignatureId];
					IVariable variable = (signature != null) ? signature[variableInfo.VariableId] : null;
					if (variable != null)
					{
						_IUserdefType iuserdefType = variable.Type as _IUserdefType;
						if (variable.Type.Class == TypeClass.Array)
						{
							iuserdefType = (\u0084.\u0004.\u0001(variable.Type as _IArrayType) as _IUserdefType);
						}
						if (iuserdefType != null)
						{
							string u2 = string.Format("{0}.{1}", signature.OrgName, variable.OrgName);
							if (!string.IsNullOrEmpty(signature.LibraryPath))
							{
								_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
								Debug.\u0001(libraryContext != null);
								string arg = Helper.\u0001(this.\u0001, libraryContext);
								u2 = string.Format("{0}.{1}.{2}", arg, signature.OrgName, variable.OrgName);
							}
							global::\u001A.\u0014 u3 = new global::\u001A.\u0014(this.\u0001, u.\u0001)
							{
								VariablePath = u2,
								MovedVariableRef = variable,
								VariableType = (variable.Type as _IType),
								ContainingSignatureRef = signature
							};
							\u0004.\u0001(u3);
						}
					}
				}
			}
		}

		// Token: 0x040009E9 RID: 2537
		private readonly _ICompileContext \u0001;

		// Token: 0x040009EA RID: 2538
		private readonly _ICompileContext \u0002;

		// Token: 0x040009EB RID: 2539
		private readonly OnlineChangeDetails \u0001;

		// Token: 0x040009EC RID: 2540
		private readonly Codegeneration \u0001;

		// Token: 0x040009ED RID: 2541
		private _ISignature \u0001;

		// Token: 0x040009EE RID: 2542
		private readonly IScope5 \u0001;

		// Token: 0x0200035D RID: 861
		// (Invoke) Token: 0x060033BB RID: 13243
		private delegate _IExpression CreatePartialInitCalleeExp(_ISignature sign, _IVariable var);

		// Token: 0x0200035F RID: 863
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x060033C7 RID: 13255 RVA: 0x000CBBA4 File Offset: 0x000C9DA4
			internal _IExpression \u0001(string \u0002)
			{
				return OnlineChangeCopyCodeGenerator.\u0001(this.\u0001, this.\u0001, \u0002);
			}

			// Token: 0x060033C8 RID: 13256 RVA: 0x000CBBB8 File Offset: 0x000C9DB8
			internal VariableInfo \u0001(_IExpression \u0002)
			{
				return new VariableInfo(\u0002.GetVariable(this.\u0001).Id, \u0002.SignatureId, this.\u0001.Flags);
			}

			// Token: 0x040009F6 RID: 2550
			public IScope5 \u0001;

			// Token: 0x040009F7 RID: 2551
			public _ICompileContext \u0001;

			// Token: 0x040009F8 RID: 2552
			public IVariableInfo \u0001;
		}

		// Token: 0x02000360 RID: 864
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x060033CA RID: 13258 RVA: 0x000CBBEC File Offset: 0x000C9DEC
			internal IEnumerable<IVariableInfo> \u0001(IVariableInfo \u0002)
			{
				return OnlineChangeCopyCodeGenerator.\u0001(\u0002, this.\u0001, this.\u0001.\u0002);
			}

			// Token: 0x040009F9 RID: 2553
			public IScope5 \u0001;

			// Token: 0x040009FA RID: 2554
			public OnlineChangeCopyCodeGenerator \u0001;
		}
	}
}
