using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using \u0007;
using \u000F;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003CF RID: 975
	internal static class CodeInitGenerator
	{
		// Token: 0x0600370B RID: 14091 RVA: 0x000E1110 File Offset: 0x000DF310
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0003.FPDataLocation == null)
			{
				return;
			}
			_IVariable ivariable = \u0019.\u0003.\u0001(null);
			ivariable.Name = string.Format(IdentifierConstants.SignFPAddressTemplate, \u0003.Id);
			ivariable.SetType(\u0019.\u0003.\u0001(TypeTable.DWord));
			ivariable.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit, true);
			ivariable.DataLocation = \u0003.FPDataLocation;
			IVariable variable = (\u0005 != null) ? \u0005[ivariable.VersionedName] : null;
			ivariable.Id = ((variable != null) ? variable.Id : \u0004.NextId);
			\u0004.AddVariable(ivariable);
			_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0003.Id);
			if (((icompiledPOU != null) ? icompiledPOU.TryCatchFPAddresses : null) != null)
			{
				for (int i = 0; i < icompiledPOU.TryCatchFPAddresses.Count; i++)
				{
					_IVariable ivariable2 = \u0019.\u0003.\u0001(null);
					ivariable2.Name = string.Format(IdentifierConstants.SignTryCatchAddressTemplate, \u0003.Id, i);
					ivariable2.SetType(\u0019.\u0003.\u0001(TypeTable.DWord));
					ivariable2.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit, true);
					ivariable2.DataLocation = icompiledPOU.TryCatchFPAddresses[i];
					IVariable variable2 = (\u0005 != null) ? \u0005[ivariable2.VersionedName] : null;
					ivariable2.Id = ((variable2 != null) ? variable2.Id : \u0004.NextId);
					\u0004.AddVariable(ivariable2);
				}
			}
		}

		// Token: 0x0600370C RID: 14092 RVA: 0x000E1274 File Offset: 0x000DF474
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(IdentifierConstants.GlobalImplicitFunctionPointers, lstringBuilder.ToString(), true);
			_ISignature isignature2 = null;
			Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(\u0002.ApplicationGuid);
			if (\u0003 != null && parentApplication == Guid.Empty)
			{
				isignature2 = \u0003[IdentifierConstants.GlobalImplicitFunctionPointers];
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
			foreach (_ISignature isignature3 in \u0002.AllSignatureList)
			{
				CodeInitGenerator.\u0001(\u0002, isignature3, isignature, isignature2);
				foreach (_ISignature u in isignature3.SubSignatures)
				{
					CodeInitGenerator.\u0001(\u0002, u, isignature, isignature2);
				}
			}
			isignature.SetFlag(SignatureFlag.NoInit | SignatureFlag.NoCompareWithNew | SignatureFlag.Compiled | SignatureFlag.SuperGlobal, true);
			\u0002.AddSignature(isignature, isignature2, \u0003, true);
		}

		// Token: 0x0600370D RID: 14093 RVA: 0x000E138C File Offset: 0x000DF58C
		internal static _ISignature \u0001(_ICompileContext \u0002, bool \u0003)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001(IdentifierConstants.GlobalImplicitSignature, lstringBuilder.ToString(), true);
			foreach (_ISignature isignature2 in \u0002.AllSignatureList)
			{
				CodeInitGenerator.\u0001(\u0002, isignature, isignature2, \u0003);
				foreach (_ISignature u in isignature2.SubSignatures)
				{
					CodeInitGenerator.\u0001(\u0002, isignature, u, \u0003);
				}
			}
			isignature.SetFlag(SignatureFlag.NoInit | SignatureFlag.Generated | SignatureFlag.Compiled, true);
			\u0002.AddSignature(isignature, null, null, true);
			return isignature;
		}

		// Token: 0x0600370E RID: 14094 RVA: 0x000E1454 File Offset: 0x000DF654
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, bool \u0003, bool \u0004, _ICompileContext \u0005, out _ISignature \u0006, bool \u0007)
		{
			IList<_ISignature> allSignatureList = \u0002.AllSignatureList;
			if (\u0004)
			{
				\u0006 = \u0002[IdentifierConstants.GlobalImplicitSignature];
			}
			else
			{
				\u0006 = CodeInitGenerator.\u0001(\u0002, \u0003 && !\u0002.DataManager._MemorySettings.OnlineChangeInOwnSegment);
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0006.Id);
			scope.LocalSignature = \u0006;
			scope.MethodSignature = \u0002[IdentifierConstants.GlobalImplicitFunctionPointers];
			LStringBuilder lstringBuilder = new LStringBuilder();
			_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
			if (\u0003)
			{
				_ISignature isignature = \u0002["GLOBAL__EXIT__COPY"];
				if (isignature != null)
				{
					CodeInitGenerator.\u0001(\u0002, isignature, isequenceStatement, \u0003);
					_ICallExpression u = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001("GLOBAL__EXIT__COPY"), Token.Empty);
					isequenceStatement.Add(\u0019.\u0003.\u0001(u, Token.Empty));
				}
			}
			int nParts;
			if (\u0002.IsCodegenMultithreadingAllowed())
			{
				nParts = Environment.ProcessorCount;
			}
			else
			{
				nParts = 1;
			}
			IEnumerable<IEnumerable<_ISignature>> enumerable = allSignatureList.ChunkIntoParts(nParts);
			if (enumerable.Sum(new Func<IEnumerable<_ISignature>, int>(CodeInitGenerator.<>c.<>9.\u0001)) != allSignatureList.Count)
			{
				throw new InvalidOperationException("Detected bug CDS-87636");
			}
			CodeInitGenerator.\u0001(\u0002, \u0003, \u0007, enumerable, isequenceStatement);
			CodeInitGenerator.\u0001(\u0002, \u0003, \u0005, enumerable, isequenceStatement);
			if (\u0003 && \u0002["GLOBAL__COPY__CODE"] != null)
			{
				isequenceStatement.Add(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("GLOBAL__COPY__CODE"), Token.Empty), Token.Empty));
			}
			CodeInitGenerator.\u0001(\u0002, isequenceStatement);
			_IStatement parseTree = isequenceStatement;
			Debug.\u0001(lstringBuilder.Length == 0);
			_ICompiledPOU icompiledPOU = \u0019.\u0003.\u0001(IdentifierConstants.CodeInitName);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SignatureId = \u0006.Id;
			icompiledPOU.SetParseTree(parseTree);
			ExpressionTypifierWithSpecialTasks visitor = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			icompiledPOU.Accept(visitor);
			TypeCheckerVisitor visitor2 = new TypeCheckerVisitor(scope, \u0002);
			icompiledPOU.Accept(visitor2);
			ErrorVisitor errorVisitor = new ErrorVisitor();
			icompiledPOU.Accept(errorVisitor);
			Debug.\u0001(errorVisitor.MessageList.Count == 0);
			icompiledPOU.SignatureId = \u0006.Id;
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			return icompiledPOU;
		}

		// Token: 0x0600370F RID: 14095 RVA: 0x000E165C File Offset: 0x000DF85C
		private static void \u0001(_ICompileContext \u0002, _ISequenceStatement \u0003)
		{
			_ISignature isignature = \u0002["CDS_87724"];
			if (isignature != null && isignature.HasAttribute("TestVariablesForOnlineChange"))
			{
				ISequenceStatement state = new global::\u0011.\u0006("CDS_87724.bOnlineChangeRunning := TRUE; CDS_87724.tTimeStart := LTIME();").\u0001();
				ISequenceStatement state2 = new global::\u0011.\u0006("CDS_87724.bOnlineChangeRunning := FALSE; CDS_87724.bOnlineChangeDone := TRUE; CDS_87724.tTimeDiff := LTIME() - CDS_87724.tTimeStart;").\u0001();
				\u0003.InsertStatement(0, state);
				\u0003.AddStatement(state2);
			}
		}

		// Token: 0x06003710 RID: 14096 RVA: 0x000E16B4 File Offset: 0x000DF8B4
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISequenceStatement \u0004, bool \u0005)
		{
			_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0003.Id);
			if (\u0005 && (icompiledPOU == null || !icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile)))
			{
				return;
			}
			if (icompiledPOU == null)
			{
				return;
			}
			if (\u0003.HasAttribute("no-function-pointer"))
			{
				return;
			}
			if (\u0003.FPDataLocation != null)
			{
				string stName = string.Format(IdentifierConstants.SignFPAddressTemplate, \u0003.Id);
				string u = \u0003.Name + ":" + Environment.NewLine;
				\u0004.Add(\u0019.\u0003.\u0001(u, Token.Empty));
				ISignature signature = \u0002[IdentifierConstants.GlobalImplicitSignature];
				string stName2 = string.Format(IdentifierConstants.POUStartAddressTemplate, \u0003.Id);
				_IVariableExpression exp = \u0019.\u0003.\u0001(signature[stName2] as _IVariable, signature as _ISignature);
				_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr);
				ioperatorExpression.AddOperand(exp);
				ISignature signature2 = \u0002[IdentifierConstants.GlobalImplicitFunctionPointers];
				IVariable variable = signature2[stName];
				_IAssignmentExpression iassignmentExpression = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(variable as _IVariable, signature2 as _ISignature));
				iassignmentExpression._RValue = ioperatorExpression;
				ICodegenerator3 codegenerator = \u0002.Codegenerator as ICodegenerator3;
				if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.Thumb2))
				{
					_IOperatorExpression ioperatorExpression2 = \u0019.\u0003.\u0001(Operator.Or);
					ioperatorExpression2.AddOperand(ioperatorExpression);
					ioperatorExpression2.AddOperand(\u0019.\u0003.\u0001(1L, TypeClass.DWord));
					iassignmentExpression._RValue = ioperatorExpression2;
				}
				\u0004.Add(\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty));
				if (icompiledPOU.TryCatchCodeAddresses != null && icompiledPOU.TryCatchFPAddresses != null && icompiledPOU.TryCatchCodeAddresses.Count == icompiledPOU.TryCatchFPAddresses.Count)
				{
					for (int i = 0; i < icompiledPOU.TryCatchCodeAddresses.Count; i++)
					{
						string stName3 = string.Format(IdentifierConstants.SignTryCatchAddressTemplate, \u0003.Id, i);
						string stName4 = string.Format(IdentifierConstants.TryCatchCodeAddressTemplate, \u0003.Id, i);
						IVariable variable2 = signature[stName4];
						variable = signature2[stName3];
						exp = \u0019.\u0003.\u0001(variable2 as _IVariable, signature as _ISignature);
						ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr);
						ioperatorExpression.AddOperand(exp);
						iassignmentExpression = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(variable as _IVariable, signature2 as _ISignature));
						iassignmentExpression._RValue = ioperatorExpression;
						\u0004.Add(\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty));
					}
				}
			}
		}

		// Token: 0x06003711 RID: 14097 RVA: 0x000E1918 File Offset: 0x000DFB18
		private static void \u0001(_ICompileContext \u0002, bool \u0003, bool \u0004, IEnumerable<IEnumerable<_ISignature>> \u0005, _ISequenceStatement \u0006)
		{
			CodeInitGenerator.\u0001 u = new CodeInitGenerator.\u0001();
			u.\u0001 = \u0002;
			u.\u0001 = \u0004;
			u.\u0002 = \u0003;
			foreach (_ISequenceStatement sm in Task.WhenAll<_ISequenceStatement>(\u0005.Select(new Func<IEnumerable<_ISignature>, Task<_ISequenceStatement>>(u.\u0001))).Result)
			{
				\u0006.Add(sm);
			}
		}

		// Token: 0x06003712 RID: 14098 RVA: 0x000E1978 File Offset: 0x000DFB78
		private static void \u0001(_ICompileContext \u0002, bool \u0003, _ICompileContext \u0004, IEnumerable<IEnumerable<_ISignature>> \u0005, _ISequenceStatement \u0006)
		{
			CodeInitGenerator.\u0003 u = new CodeInitGenerator.\u0003();
			u.\u0001 = \u0004;
			u.\u0001 = \u0003;
			u.\u0002 = \u0002;
			foreach (_ISequenceStatement sm in Task.WhenAll<_ISequenceStatement>(\u0005.Select(new Func<IEnumerable<_ISignature>, Task<_ISequenceStatement>>(u.\u0001))).Result)
			{
				\u0006.Add(sm);
			}
		}

		// Token: 0x06003713 RID: 14099 RVA: 0x000E19D8 File Offset: 0x000DFBD8
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, bool \u0005)
		{
			_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0004.Id);
			if (icompiledPOU == null)
			{
				return;
			}
			if (\u0005 && !icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile))
			{
				return;
			}
			if (\u0004.HasAttribute("no-function-pointer"))
			{
				return;
			}
			if (\u0004.Name == IdentifierConstants.RelocateCodeName)
			{
				return;
			}
			if (\u0004.FPDataLocation != null && !\u0004.GetFlag(SignatureFlag.External))
			{
				_IVariable ivariable = \u0019.\u0003.\u0001(null);
				ivariable.Name = string.Format(IdentifierConstants.POUStartAddressTemplate, \u0004.Id);
				ivariable.SetType(\u0019.\u0003.\u0001(TypeTable.Byte));
				ivariable.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
				ivariable.DataLocation = icompiledPOU.CompiledCode.Location;
				ivariable.Id = \u0003.NextId;
				\u0003.AddVariable(ivariable);
			}
			if (!icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode) && icompiledPOU.TryCatchCodeAddresses != null)
			{
				for (int i = 0; i < icompiledPOU.TryCatchCodeAddresses.Count; i++)
				{
					_IVariable ivariable2 = \u0019.\u0003.\u0001(null);
					ivariable2.Name = string.Format(IdentifierConstants.TryCatchCodeAddressTemplate, \u0004.Id, i);
					ivariable2.SetType(\u0019.\u0003.\u0001(TypeTable.Byte));
					ivariable2.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
					ivariable2.DataLocation = \u0019.\u0003.\u0001(icompiledPOU.CompiledCode.Location.Area, icompiledPOU.CompiledCode.Location.Offset + icompiledPOU.TryCatchCodeAddresses[i]);
					ivariable2.Id = \u0003.NextId;
					\u0003.AddVariable(ivariable2);
				}
			}
		}

		// Token: 0x020003D1 RID: 977
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06003718 RID: 14104 RVA: 0x000E1B88 File Offset: 0x000DFD88
			internal Task<_ISequenceStatement> \u0001(IEnumerable<_ISignature> \u0002)
			{
				return Task.Run<_ISequenceStatement>(new Func<_ISequenceStatement>(new CodeInitGenerator.\u0002
				{
					\u0001 = this,
					\u0001 = \u0002
				}.\u0001));
			}

			// Token: 0x04000ABC RID: 2748
			public _ICompileContext \u0001;

			// Token: 0x04000ABD RID: 2749
			public bool \u0001;

			// Token: 0x04000ABE RID: 2750
			public bool \u0002;
		}

		// Token: 0x020003D2 RID: 978
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x0600371A RID: 14106 RVA: 0x000E1BB8 File Offset: 0x000DFDB8
			internal _ISequenceStatement \u0001()
			{
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				foreach (_ISignature isignature in this.\u0001)
				{
					if (isignature.POUType != Operator.Interface)
					{
						_ICompiledPOU icompiledPOU = this.\u0001.\u0001._GetCompiledPOUById(isignature.Id);
						bool flag = icompiledPOU != null && icompiledPOU.GetFlag(CompiledPOUFlags.Linked) && !this.\u0001.\u0001;
						if (!isignature.GetFlag(SignatureFlag.External) && !flag)
						{
							CodeInitGenerator.\u0001(this.\u0001.\u0001, isignature, isequenceStatement, this.\u0001.\u0002 && !this.\u0001.\u0001.DataManager._MemorySettings.OnlineChangeInOwnSegment);
						}
						foreach (_ISignature isignature2 in isignature.SubSignatures)
						{
							_ICompiledPOU icompiledPOU2 = this.\u0001.\u0001._GetCompiledPOUById(isignature2.Id);
							flag = (icompiledPOU2 != null && icompiledPOU2.GetFlag(CompiledPOUFlags.Linked));
							if (!isignature2.GetFlag(SignatureFlag.External) && !flag)
							{
								CodeInitGenerator.\u0001(this.\u0001.\u0001, isignature2, isequenceStatement, this.\u0001.\u0002 && !this.\u0001.\u0001.DataManager._MemorySettings.OnlineChangeInOwnSegment);
							}
						}
					}
				}
				return isequenceStatement;
			}

			// Token: 0x04000ABF RID: 2751
			public IEnumerable<_ISignature> \u0001;

			// Token: 0x04000AC0 RID: 2752
			public CodeInitGenerator.\u0001 \u0001;
		}

		// Token: 0x020003D3 RID: 979
		[CompilerGenerated]
		private sealed class \u0003
		{
			// Token: 0x0600371C RID: 14108 RVA: 0x000E1D60 File Offset: 0x000DFF60
			internal Task<_ISequenceStatement> \u0001(IEnumerable<_ISignature> \u0002)
			{
				return Task.Run<_ISequenceStatement>(new Func<_ISequenceStatement>(new CodeInitGenerator.\u0004
				{
					\u0001 = this,
					\u0001 = \u0002
				}.\u0001));
			}

			// Token: 0x04000AC1 RID: 2753
			public _ICompileContext \u0001;

			// Token: 0x04000AC2 RID: 2754
			public bool \u0001;

			// Token: 0x04000AC3 RID: 2755
			public _ICompileContext \u0002;
		}

		// Token: 0x020003D4 RID: 980
		[CompilerGenerated]
		private sealed class \u0004
		{
			// Token: 0x0600371E RID: 14110 RVA: 0x000E1D90 File Offset: 0x000DFF90
			internal _ISequenceStatement \u0001()
			{
				_ISequenceStatement isequenceStatement = \u0019.\u0003.\u0001();
				foreach (_ISignature isignature in this.\u0001)
				{
					if (isignature.POUType != Operator.Interface)
					{
						_ISignature isignature2 = null;
						if (this.\u0001.\u0001 != null)
						{
							isignature2 = this.\u0001.\u0001[isignature.Id];
						}
						_IVirtualFunctionTable ivirtualFunctionTable = isignature.VirtualFunctionTable as _IVirtualFunctionTable;
						_IVirtualFunctionTable ivirtualFunctionTable2 = null;
						if (isignature2 != null)
						{
							ivirtualFunctionTable2 = (isignature2.VirtualFunctionTable as _IVirtualFunctionTable);
						}
						if (ivirtualFunctionTable != null && (!this.\u0001.\u0001 || ivirtualFunctionTable2 == null || !ivirtualFunctionTable.DataLocation.IsEqual(ivirtualFunctionTable2.DataLocation) || isignature.GetFlag(SignatureFlag.InitializeVirtualFunctionTable)))
						{
							_ICommentStatement sm = \u0019.\u0003.\u0001("code for initializing virtual function table of " + isignature.Name, Token.Empty);
							isequenceStatement.Add(sm);
							\u0018.\u0001(ivirtualFunctionTable, isequenceStatement, this.\u0001.\u0002);
						}
					}
				}
				return isequenceStatement;
			}

			// Token: 0x04000AC4 RID: 2756
			public IEnumerable<_ISignature> \u0001;

			// Token: 0x04000AC5 RID: 2757
			public CodeInitGenerator.\u0003 \u0001;
		}
	}
}
