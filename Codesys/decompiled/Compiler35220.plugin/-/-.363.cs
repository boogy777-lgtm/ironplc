using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0084;

namespace \u0004
{
	// Token: 0x020003B0 RID: 944
	internal static class \u0018
	{
		// Token: 0x06003660 RID: 13920 RVA: 0x000DAAC8 File Offset: 0x000D8CC8
		internal static string \u0001(_IVariable \u0002, _ISignature \u0003, IScope5 \u0004)
		{
			_IStatement istatement = global::\u0004.\u0018.\u0001(\u0004.ApplicationContext as _ICompileContext, \u0002, \u0003, \u0004, false, false, true, false, false, \u0003, null, global::\u0019.\u0003.\u0001("TRUE"), global::\u0019.\u0003.\u0001("FALSE"));
			if (istatement == null)
			{
				return "";
			}
			return istatement.ToString();
		}

		// Token: 0x06003661 RID: 13921 RVA: 0x000DAB14 File Offset: 0x000D8D14
		private static ICompiledType \u0001(IVariable \u0002)
		{
			ICompiledType compiledType = \u0002.CompiledType;
			if (compiledType is _IImplicitReferenceType)
			{
				compiledType = ((_IImplicitReferenceType)compiledType).BaseType;
			}
			if (compiledType.DeRefType.Class == TypeClass.Array)
			{
				compiledType = \u0084.\u0004.\u0001(compiledType.DeRefType as _IArrayType);
			}
			return compiledType;
		}

		// Token: 0x06003662 RID: 13922 RVA: 0x000DAB60 File Offset: 0x000D8D60
		internal static bool \u0001(IVariable \u0002, IScope5 \u0003)
		{
			ICompiledType compiledType = global::\u0004.\u0018.\u0001(\u0002);
			bool result = false;
			if (compiledType.DeRefType.Class == TypeClass.Userdef)
			{
				_IUserdefType iuserdefType = compiledType.DeRefType as _IUserdefType;
				_ISignature isignature = \u0003[iuserdefType.SignatureId] as _ISignature;
				if (isignature != null && isignature.POUType == Operator.Type)
				{
					if (isignature.GetFlag(SignatureFlag.Alias))
					{
						result = false;
					}
					else if (isignature.GetFlag(SignatureFlag.Structure))
					{
						result = true;
					}
					else
					{
						Debug.\u0001(isignature.GetFlag(SignatureFlag.Enum));
					}
					if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
					{
						result = false;
					}
					if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT))
					{
						result = false;
					}
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06003663 RID: 13923 RVA: 0x000DAC10 File Offset: 0x000D8E10
		internal static _IExprement \u0001(_IVariable \u0002, _ISignature \u0003, _IExpression \u0004, _IExprement \u0005)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			if (\u0004 != null && \u0005 is _IStatement)
			{
				isequenceStatement.Add(\u0005 as _IStatement);
				_IExprement iexprement = \u0004.Duplicate();
				if (iexprement is _IStructureInitialization)
				{
					(iexprement as _IStructureInitialization).DefaultInitializationDone = true;
				}
				else if (iexprement is _IArrayInitialization)
				{
					(iexprement as _IArrayInitialization).DefaultInitializationDone = true;
				}
				isequenceStatement.Add(global::\u0004.\u0018.\u0001(\u0002, \u0003, iexprement as _IExpression));
			}
			return isequenceStatement;
		}

		// Token: 0x06003664 RID: 13924 RVA: 0x000DAC84 File Offset: 0x000D8E84
		internal static _ISequenceStatement \u0001(_ISignature \u0002, _IExpression \u0003, IScope5 \u0004, _ISequenceStatement \u0005 = null)
		{
			_ISignature isignature = \u0002;
			while (isignature != null && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
			{
				foreach (object obj in isignature._SubSignatures)
				{
					ISignature signature = (ISignature)obj;
					if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_CALL_AFTER_INIT))
					{
						_IExpression u;
						if (\u0003 != null)
						{
							u = global::\u0019.\u0003.\u0001(\u0003.Duplicate() as _IExpression, global::\u0019.\u0003.\u0001(signature.OrgName));
						}
						else
						{
							u = global::\u0019.\u0003.\u0001(signature.OrgName);
						}
						_IExpressionStatement sm = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(u, Token.Empty), Token.Empty);
						if (\u0005 == null)
						{
							\u0005 = global::\u0019.\u0003.\u0001();
						}
						\u0005.Add(sm);
					}
				}
				isignature = (\u0004[isignature.BaseSignatureId] as _ISignature);
			}
			return \u0005;
		}

		// Token: 0x06003665 RID: 13925 RVA: 0x000DAD6C File Offset: 0x000D8F6C
		private static _IStatement \u0001(_IVariable \u0002, _ISignature \u0003, _IExpression \u0004)
		{
			_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001();
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(\u0002.VersionedName);
			iassignmentExpression._LValue = ivariableExpression;
			ivariableExpression._CompiledType = \u0002._Type;
			ivariableExpression.VariableId = \u0002.Id;
			ivariableExpression.SignatureId = \u0003.Id;
			iassignmentExpression._LValue._Position = global::\u0019.\u0003.\u0001(\u0002._SourcePosition);
			iassignmentExpression._RValue = \u0004;
			IMinimalPosition minimalPosition = \u0004.PositionIntern;
			if (minimalPosition == null)
			{
				minimalPosition = global::\u0019.\u0003.\u0001(\u0002._SourcePosition);
			}
			iassignmentExpression._RValue._Position = minimalPosition;
			return global::\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty);
		}

		// Token: 0x06003666 RID: 13926 RVA: 0x000DAE00 File Offset: 0x000D9000
		internal static _IStatement \u0001(_ICompileContext \u0002, _IVariable \u0003, _ISignature \u0004, IScope5 \u0005, bool \u0006, bool \u0007, bool \u0008, bool \u000E, bool \u000F, _ISignature \u0010, _ISignature \u0011, _IExpression \u0012, _IExpression \u0013)
		{
			if (\u0003.HasFlag(VarFlag.Inout | VarFlag.External | VarFlag.ReplacedConstant))
			{
				return null;
			}
			ICompiledType compiledType = \u0084.\u0004.\u0001(\u0003.CompiledType);
			if (\u0003.HasFlag(VarFlag.NoInit))
			{
				if (compiledType.Class != TypeClass.Userdef)
				{
					return null;
				}
				\u0007 = true;
			}
			bool flag = true;
			bool flag2 = false;
			if (\u000E && \u0003.HasFlag(VarFlag.Input) && compiledType.Class == TypeClass.Userdef)
			{
				ISignature signature = (compiledType as _IUserdefType).GetSignature(\u0005);
				if (signature != null && signature.POUType == Operator.FunctionBlock)
				{
					flag2 = true;
					\u0007 = true;
				}
			}
			if (\u0007 && compiledType.Class != TypeClass.Userdef)
			{
				return null;
			}
			ICompiledType compiledType2 = \u0003.CompiledType;
			if (compiledType2 is _IImplicitReferenceType)
			{
				compiledType2 = ((_IImplicitReferenceType)compiledType2).BaseType;
				flag = \u000F;
			}
			bool flag3 = \u0003.Address != null && \u0003.Address.Location == DirectVariableLocation.Input;
			if (\u000E && \u0003.HasFlag(VarFlag.Input) && !flag2)
			{
				return null;
			}
			if (\u0008 && !\u0003.HasFlag(VarFlag.Absolut))
			{
				return null;
			}
			if (!\u0008 && \u0003.HasFlag(VarFlag.Absolut))
			{
				return null;
			}
			if (\u0003.IsProperty)
			{
				return null;
			}
			if (compiledType2.Class == TypeClass.Reference)
			{
				return global::\u0004.\u0018.\u0001(\u0002, \u0003, \u0004);
			}
			bool flag4 = false;
			if (compiledType2.DeRefType.Class == TypeClass.Array)
			{
				_IArrayType u = compiledType2.DeRefType as _IArrayType;
				compiledType2 = \u0084.\u0004.\u0001(u);
				int num = Helper.\u0001(u);
				if (num != 0)
				{
					Helper.\u0001(num, \u0010, \u0011);
				}
			}
			if (compiledType2.DeRefType.Class == TypeClass.Userdef && \u0003.Initial != null)
			{
				int num2 = Helper.\u0001(\u0003.Initial);
				if (num2 != 0)
				{
					Helper.\u0001(num2, \u0010, \u0011);
				}
			}
			IExpression expression;
			if (\u0007)
			{
				expression = null;
			}
			else
			{
				expression = \u0003.Initial;
			}
			if (compiledType2.DeRefType.Class == TypeClass.Userdef)
			{
				global::\u0004.\u0018.\u0001(\u0003, \u0004, \u0005, ref compiledType2, ref flag4, ref expression);
			}
			if (expression == null && \u0006 && !flag4)
			{
				return null;
			}
			bool flag5 = true;
			_IExprement iexprement;
			if (expression == null || flag4)
			{
				_IExpression u2 = global::\u0019.\u0003.\u0001(true);
				_IExpression iexpression = global::\u0019.\u0003.\u0001(false);
				_IExpression u3;
				if (\u0003.GetFlag(VarFlag.RelativeInstance))
				{
					_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001()), Token.Empty);
					icompoAccessExpression._Right = global::\u0019.\u0003.\u0001(\u0003.VersionedName);
					u3 = icompoAccessExpression;
				}
				else
				{
					u3 = global::\u0019.\u0003.\u0001(\u0003.VersionedName);
				}
				ICompiledType compiledType3 = \u0003.CompiledTypeInternal;
				if (compiledType3 is _IImplicitReferenceType)
				{
					compiledType3 = ((_IImplicitReferenceType)compiledType3).BaseType;
				}
				if (\u000E)
				{
					iexprement = \u0080.\u001A.\u0001(\u0003, \u0007, compiledType3 as _IType, u3, \u0005, out flag5, \u0010, \u0011, iexpression, iexpression, 0, \u0002);
				}
				else if (\u000F)
				{
					if (\u0003.GetFlag(VarFlag.OnlChangeCopy))
					{
						iexprement = \u0080.\u001A.\u0001(\u0003, \u0007, compiledType3 as _IType, u3, \u0005, out flag5, \u0010, \u0011, iexpression, u2, 0, \u0002);
					}
					else
					{
						iexprement = \u0080.\u001A.\u0001(\u0003, \u0007, compiledType3 as _IType, u3, \u0005, out flag5, \u0010, \u0011, iexpression, iexpression, 0, \u0002);
					}
				}
				else
				{
					iexprement = \u0080.\u001A.\u0001(\u0003, \u0007, compiledType3 as _IType, u3, \u0005, out flag5, \u0010, \u0011, \u0012, \u0013, 0, \u0002);
				}
				if (expression != null && iexprement is _IStatement)
				{
					Debug.\u0001(!flag5);
					_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
					isequenceStatement.Add(iexprement as _IStatement);
					iexprement = (expression as _IExpression).Duplicate();
					if (iexprement is _IStructureInitialization)
					{
						(iexprement as _IStructureInitialization).DefaultInitializationDone = true;
					}
					else if (iexprement is _IArrayInitialization)
					{
						(iexprement as _IArrayInitialization).DefaultInitializationDone = true;
					}
					isequenceStatement.Add(global::\u0004.\u0018.\u0001(\u0003, \u0004, iexprement as _IExpression));
					iexprement = isequenceStatement;
				}
			}
			else
			{
				iexprement = (expression as _IExpression).Duplicate();
			}
			if (flag3 && (flag5 || !flag4))
			{
				return null;
			}
			if (iexprement == null)
			{
				return null;
			}
			_IStatement istatement;
			if (iexprement is _IStatement)
			{
				istatement = (iexprement as _IStatement);
			}
			else
			{
				if (!(iexprement is _IExpression) || !flag5)
				{
					Debug.\u0001(false);
					return null;
				}
				istatement = global::\u0004.\u0018.\u0001(\u0003, \u0004, iexprement as _IExpression);
			}
			istatement = global::\u0004.\u0018.\u0001(\u0003, istatement);
			if (!flag)
			{
				IExpression u4 = global::\u0019.\u0003.\u0001(Operator.Not, \u0013);
				_ISequenceStatement u5 = global::\u0019.\u0003.\u0001(new List<IStatement>(1)
				{
					istatement
				});
				istatement = global::\u0019.\u0003.\u0001(u4, u5);
			}
			return istatement;
		}

		// Token: 0x06003667 RID: 13927 RVA: 0x000DB208 File Offset: 0x000D9408
		internal static _ISequenceStatement \u0001(_ICompileContext \u0002, _ISignature \u0003, IScope5 \u0004, ConstantInit \u0005, bool \u0006, bool \u0007, bool \u0008, bool \u000E, _ISignature \u000F, _ISignature \u0010, string \u0011, string \u0012)
		{
			_IExpression u = global::\u0019.\u0003.\u0001(\u0011);
			_IExpression u2 = global::\u0019.\u0003.\u0001(\u0012);
			return global::\u0004.\u0018.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008, \u000E, \u000F, \u0010, u, u2);
		}

		// Token: 0x06003668 RID: 13928 RVA: 0x000DB23C File Offset: 0x000D943C
		private static _ISequenceStatement \u0001(_ICompileContext \u0002, _ISignature \u0003, IScope5 \u0004, ConstantInit \u0005, bool \u0006, bool \u0007, bool \u0008, bool \u000E, _ISignature \u000F, _ISignature \u0010, _IExpression \u0011, _IExpression \u0012)
		{
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			global::\u0004.\u0018.\u0001(isequenceStatement, \u0003, \u0004, \u0006, \u0008, \u000F);
			bool flag = \u0003.HasAttribute("vfinitonly");
			if (\u0005 != ConstantInit.Constants && !\u000E)
			{
				IList<IVariable> allRetains = \u0003.AllRetains;
				foreach (IVariable variable in allRetains)
				{
					_IVariable ivariable = (_IVariable)variable;
					_IStatement istatement = global::\u0004.\u0018.\u0001(\u0002, ivariable, \u0003, \u0004, \u0007, true, \u0008, \u000E, false, \u000F, \u0010, \u0011, \u0012);
					if (istatement != null)
					{
						istatement.PositionIntern = global::\u0019.\u0003.\u0001(ivariable._SourcePosition);
						isequenceStatement.Add(global::\u0019.\u0003.\u0001(ivariable, \u0003));
						isequenceStatement.Add(istatement);
					}
				}
				if (!flag)
				{
					_ISequenceStatement isequenceStatement2 = global::\u0019.\u0003.\u0001();
					foreach (IVariable variable2 in allRetains)
					{
						_IVariable ivariable2 = (_IVariable)variable2;
						if (\u0005 == ConstantInit.All || (\u0005 == ConstantInit.Constants && ivariable2.GetFlag(VarFlag.Constant)) || (\u0005 == ConstantInit.NoConstants && !ivariable2.GetFlag(VarFlag.Constant)))
						{
							_IStatement istatement2 = global::\u0004.\u0018.\u0001(\u0002, ivariable2, \u0003, \u0004, \u0007, flag, \u0008, \u000E, false, \u000F, \u0010, \u0011, \u0012);
							if (istatement2 != null)
							{
								isequenceStatement2.Add(global::\u0019.\u0003.\u0001(ivariable2, \u0003));
								isequenceStatement2.Add(istatement2);
							}
						}
					}
					if (isequenceStatement2._StatementList.Any<_IStatement>())
					{
						_IIfStatement sm = global::\u0019.\u0003.\u0001(\u0011, isequenceStatement2);
						isequenceStatement.Add(sm);
					}
				}
			}
			global::\u0004.\u0018.\u0001(\u0002, \u0003, \u0004, ref \u0007, isequenceStatement);
			foreach (IVariable variable3 in \u0003.AllForInitCode)
			{
				_IVariable ivariable3 = (_IVariable)variable3;
				if (\u0005 == ConstantInit.All || (\u0005 == ConstantInit.Constants && ivariable3.GetFlag(VarFlag.Constant)) || (\u0005 == ConstantInit.NoConstants && !ivariable3.GetFlag(VarFlag.Constant)))
				{
					_IStatement istatement3 = global::\u0004.\u0018.\u0001(\u0002, ivariable3, \u0003, \u0004, \u0007, flag, \u0008, \u000E, false, \u000F, \u0010, \u0011, \u0012);
					if (istatement3 != null)
					{
						isequenceStatement.Add(global::\u0019.\u0003.\u0001(ivariable3, \u0003));
						isequenceStatement.Add(istatement3);
					}
				}
			}
			if (\u0003.POUType == Operator.Program && \u0008)
			{
				global::\u0004.\u0018.\u0001(\u0003, null, \u0004, isequenceStatement);
			}
			if (isequenceStatement._StatementList.Count > 0)
			{
				isequenceStatement.InsertStatement(0, global::\u0019.\u0003.\u0001(string.Format("(*Initialisation code for _ISignature {0}*)", \u0003.Name), Token.Empty));
				_IPragmaStatement state = global::\u0019.\u0003.\u0001("implicit on", true);
				isequenceStatement.InsertStatement(0, state);
				_IPragmaStatement sm2 = global::\u0019.\u0003.\u0001("implicit off", false);
				isequenceStatement.Add(sm2);
				return isequenceStatement;
			}
			return null;
		}

		// Token: 0x06003669 RID: 13929 RVA: 0x000DB4DC File Offset: 0x000D96DC
		private static void \u0001(_IVariable \u0002, _ISignature \u0003, IScope5 \u0004, ref ICompiledType \u0005, ref bool \u0006, ref IExpression \u0007)
		{
			_IUserdefType iuserdefType = \u0005.DeRefType as _IUserdefType;
			_ISignature isignature = \u0004[iuserdefType.SignatureId] as _ISignature;
			if (isignature != null && isignature.POUType == Operator.Type)
			{
				if (isignature.GetFlag(SignatureFlag.Alias))
				{
					\u0005 = (\u0003.AllVariables[0].CompiledType as _IType);
					if (\u0007 == null)
					{
						\u0007 = \u0003.AllVariables[0].Initial;
					}
				}
				else if (isignature.GetFlag(SignatureFlag.Structure))
				{
					\u0006 = true;
				}
				else
				{
					Debug.\u0001(isignature.GetFlag(SignatureFlag.Enum));
				}
				if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					\u0006 = false;
				}
				if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINIT))
				{
					\u0006 = false;
					return;
				}
			}
			else
			{
				\u0006 = true;
			}
		}

		// Token: 0x0600366A RID: 13930 RVA: 0x000DB5AC File Offset: 0x000D97AC
		private static _IStatement \u0001(_ICompileContext \u0002, _IVariable \u0003, _ISignature \u0004)
		{
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(\u0003, \u0004);
			ivariableExpression.Name = \u0003.OrgName;
			_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(ivariableExpression);
			iassignmentExpression.KindOf = Operator.RefAssign;
			if (\u0003.Initial != null)
			{
				iassignmentExpression._RValue = (\u0003.Initial as _IExpression);
			}
			else
			{
				TypeClass u = TypeClass.DWord;
				if (Helper.\u0001(CodegeneratorProperties.LWordPointer, \u0002.Codegenerator))
				{
					u = TypeClass.LWord;
				}
				iassignmentExpression._RValue = global::\u0019.\u0003.\u0001(0UL, u, Token.Empty);
			}
			return global::\u0019.\u0003.\u0001(iassignmentExpression, Token.Empty);
		}

		// Token: 0x0600366B RID: 13931 RVA: 0x000DB628 File Offset: 0x000D9828
		private static _IStatement \u0001(_IVariable \u0002, _IStatement \u0003)
		{
			if (\u0002.HasAttribute("suppress_warning_0"))
			{
				_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
				_ISequenceStatement isequenceStatement2 = global::\u0019.\u0003.\u0001();
				int num = 0;
				for (;;)
				{
					string stAttribute = string.Format("suppress_warning_{0}", num++);
					if (!\u0002.HasAttribute(stAttribute))
					{
						break;
					}
					_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement = global::\u0019.\u0003.\u0001();
					iwarningDisableRestorePragmaStatement.Restore = false;
					iwarningDisableRestorePragmaStatement.Id = \u0002.GetAttributeValue(stAttribute);
					isequenceStatement.AddStatement(iwarningDisableRestorePragmaStatement);
					_IWarningDisableRestorePragmaStatement iwarningDisableRestorePragmaStatement2 = iwarningDisableRestorePragmaStatement.Duplicate() as _IWarningDisableRestorePragmaStatement;
					iwarningDisableRestorePragmaStatement2.Restore = true;
					isequenceStatement2.AddStatement(iwarningDisableRestorePragmaStatement2);
				}
				isequenceStatement.AddStatement(\u0003);
				foreach (_IStatement state in isequenceStatement2._StatementList)
				{
					isequenceStatement.AddStatement(state);
				}
				return isequenceStatement;
			}
			return \u0003;
		}

		// Token: 0x0600366C RID: 13932 RVA: 0x000DB708 File Offset: 0x000D9908
		private static void \u0001(_ISequenceStatement \u0002, _ISignature \u0003, IScope5 \u0004, bool \u0005, bool \u0006, _ISignature \u0007)
		{
			_ISignature isignature = \u0004[\u0003.BaseSignatureId] as _ISignature;
			if (!\u0005 || isignature == null || \u0006)
			{
				return;
			}
			_ISignature isignature2 = isignature.GetSubSignature(\u0007.Name) as _ISignature;
			if (isignature2 == null)
			{
				return;
			}
			IVariable[] inputs = isignature2.Inputs;
			_ILanguageModelBuilder ilanguageModelBuilder = global::\u0019.\u0003.Builder;
			IBaseExpression expBase = ilanguageModelBuilder.CreateSuperExpression(null);
			IDeRefAccessExpression expLeft = ilanguageModelBuilder.CreateDeRefAccessExpression(null, expBase);
			IVariableExpression2 expRight = ilanguageModelBuilder.CreateVariableExpression(null, IdentifierConstants.InitMethodName);
			ICompoAccessExpression expCallee = ilanguageModelBuilder.CreateCompoAccessExpression(null, expLeft, expRight);
			List<IAssignmentExpression> list = new List<IAssignmentExpression>();
			List<IAssignmentExpression> outputassignments = new List<IAssignmentExpression>();
			for (int i = 0; i < inputs.Length - 1; i++)
			{
				IVariableExpression expLeft2 = ilanguageModelBuilder.CreateVariableExpression(null, inputs[i].OrgName);
				IVariableExpression expRight2 = ilanguageModelBuilder.CreateVariableExpression(null, inputs[i].OrgName);
				IAssignmentExpression item = ilanguageModelBuilder.CreateAssignmentExpression(null, expLeft2, expRight2);
				list.Add(item);
			}
			_IExpressionStatement iexpressionStatement = ilanguageModelBuilder.CreateCallStatement(null, expCallee, null, null, list, outputassignments) as _IExpressionStatement;
			iexpressionStatement.SetFlag(StatementFlag.Implicit, true);
			\u0002.AddStatement(iexpressionStatement);
			if (!\u0003.GetFlag(SignatureFlag.Structure))
			{
				return;
			}
			ISignature[] array = \u0004.FindSignature(\u0003.BaseExpression);
			if (array.Length == 1)
			{
				ISignature signature = array[0];
				if (signature.GetFlag(SignatureFlag.Alias))
				{
					IVariable variable = signature.All[0];
					if (variable.Initial is _IStructureInitialization)
					{
						foreach (IAssignmentExpression expInner in (variable.Initial as _IStructureInitialization)._CompoInits)
						{
							\u0002.AddStatement(ilanguageModelBuilder.CreateExpressionStatement(expInner));
						}
					}
				}
			}
		}

		// Token: 0x0600366D RID: 13933 RVA: 0x000DB8B0 File Offset: 0x000D9AB0
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, IScope5 \u0004, ref bool \u0005, _ISequenceStatement \u0006)
		{
			IList<IVariable> allForInitCode = \u0003.AllForInitCode;
			if (!\u0005 && (\u0003.POUType == Operator.VarGlobal || \u0003.POUType == Operator.Program) && \u0003.HasAttribute("subsequent") && \u0003.HasAttribute("memsetinit") && \u0002.GetCodegeneratorProperty(CodegeneratorProperties.OperatorMemSetImplemented))
			{
				foreach (IVariable variable in allForInitCode)
				{
					_IVariable ivariable = (_IVariable)variable;
					if (!ivariable.GetFlag(VarFlag.Absolut) || ivariable.GetFlag(VarFlag.Constant) || ivariable.GetFlag(VarFlag.ReplacedConstant))
					{
						return;
					}
				}
				_IVariable ivariable2 = allForInitCode[0] as _IVariable;
				int offset = ivariable2.DataLocation.Offset;
				_IVariable ivariable3 = allForInitCode[allForInitCode.Count<IVariable>() - 1] as _IVariable;
				int num = ivariable3.DataLocation.Offset + ivariable3.CompiledType.Size(\u0004) - offset;
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr);
				ioperatorExpression.AddOperand(global::\u0019.\u0003.\u0001(ivariable2.Name));
				_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.__MemorySet);
				ioperatorExpression2.AddOperand(ioperatorExpression);
				ioperatorExpression2.AddOperand(global::\u0019.\u0003.\u0001(0L, TypeClass.Byte));
				ioperatorExpression2.AddOperand(global::\u0019.\u0003.\u0001((long)num, TypeClass.DWord));
				_IExpressionStatement iexpressionStatement = global::\u0019.\u0003.\u0001();
				iexpressionStatement._Expr = ioperatorExpression2;
				\u0006.Add(global::\u0019.\u0003.\u0001(ioperatorExpression2.ToString(), Token.Empty));
				\u0006.Add(iexpressionStatement);
				\u0005 = true;
			}
		}

		// Token: 0x0600366E RID: 13934 RVA: 0x000DBA48 File Offset: 0x000D9C48
		internal static void \u0001(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004)
		{
			if (\u0002.POUType != Operator.FunctionBlock && !\u0002.GetFlag(SignatureFlag.Structure))
			{
				return;
			}
			_ISignature isignature = null;
			foreach (object obj in \u0002._SubSignatures)
			{
				_ISignature isignature2 = (_ISignature)obj;
				global::\u0019.\u0003.\u0001(\u0002.Name).SignatureId = \u0002.Id;
				if (isignature2.POUType == Operator.Method && isignature2.Name == IdentifierConstants.InitMethodName && isignature2.AllInputs.Length >= 3 && isignature2.AllInputs[0].Type.Class == TypeClass.Bool && isignature2.AllInputs[0].Name == "BINITRETAINS" && isignature2.AllInputs[1].Type.Class == TypeClass.Bool && isignature2.AllInputs[1].Name == "BINCOPYCODE")
				{
					isignature = isignature2;
				}
			}
			if (isignature == null)
			{
				return;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, \u0002.Id);
			_IStatement istatement = global::\u0004.\u0018.\u0001(\u0002, \u0003, \u0004, isignature, scope);
			_ICompiledPOU icompiledPOU = \u0003._GetCompiledPOUById(isignature.Id);
			if (icompiledPOU != null && icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
			{
				return;
			}
			if (icompiledPOU == null)
			{
				icompiledPOU = global::\u0019.\u0003.\u0001(IdentifierConstants.InitMethodName);
				icompiledPOU.SetParseTree(istatement);
				\u0003.AddCompiledPOU(icompiledPOU, isignature, true, \u0004);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
				icompiledPOU.MessageGuid = \u0002.ObjectGuid;
				icompiledPOU.LibraryPath = \u0002.LibraryPath;
			}
			else
			{
				_ISequenceStatement isequenceStatement = icompiledPOU.GetParseTree() as _ISequenceStatement;
				if (isequenceStatement == null)
				{
					icompiledPOU.SetParseTree(istatement);
				}
				else
				{
					_ISequenceStatement isequenceStatement2 = global::\u0019.\u0003.\u0001();
					isequenceStatement2.Add(istatement);
					isequenceStatement2.Add(isequenceStatement);
					icompiledPOU.SetParseTree(isequenceStatement2);
				}
			}
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0003, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0003);
			istatement.Accept(ivisit2);
		}

		// Token: 0x0600366F RID: 13935 RVA: 0x000DBC3C File Offset: 0x000D9E3C
		internal static _IStatement \u0001(_ISignature \u0002, _ICompileContext \u0003, _ICompileContext \u0004, _ISignature \u0005, IScope5 \u0006)
		{
			_ISignature isignature = null;
			if (\u0004 != null)
			{
				isignature = \u0004[\u0002.Id];
			}
			_ISignature u = null;
			if (isignature != null)
			{
				u = (isignature.GetSubSignature(\u0005.Name) as _ISignature);
			}
			Debug.\u0001(\u0005 != null);
			\u0006.MethodSignature = \u0005;
			_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
			isequenceStatement.AddStatement(global::\u0019.\u0003.\u0001("implicit on", true));
			isequenceStatement.AddStatement(global::\u0019.\u0003.\u0001("nobp"));
			_ISequenceStatement isequenceStatement2 = global::\u0004.\u0018.\u0001(\u0003, \u0002, \u0006, ConstantInit.Constants, true, false, false, false, \u0005, u, "bInitRetains", "bInCopyCode");
			if (isequenceStatement2 != null)
			{
				foreach (IStatement state in isequenceStatement2.StatementList)
				{
					isequenceStatement.AddStatement(state);
				}
			}
			_ISequenceStatement isequenceStatement3 = global::\u0004.\u0018.\u0001(\u0003, \u0002, \u0006, ConstantInit.NoConstants, false, false, false, false, \u0005, u, "bInitRetains", "bInCopyCode");
			if (isequenceStatement3 != null)
			{
				foreach (IStatement state2 in isequenceStatement3.StatementList)
				{
					isequenceStatement.AddStatement(state2);
				}
			}
			isequenceStatement.AddStatement(global::\u0019.\u0003.\u0001("bp"));
			isequenceStatement.AddStatement(global::\u0019.\u0003.\u0001("implicit off", false));
			isequenceStatement.SetFlag(StatementFlag.Implicit, true);
			return isequenceStatement;
		}
	}
}
