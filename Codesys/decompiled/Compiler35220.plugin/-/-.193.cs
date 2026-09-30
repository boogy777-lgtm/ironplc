using System;
using System.Collections.Generic;
using System.Linq;
using \u0003;
using \u0004;
using \u0007;
using \u000E;
using \u0011;
using \u0012;
using \u0014;
using \u0019;
using \u001A;
using \u001E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0084;

namespace \u0016
{
	// Token: 0x0200022D RID: 557
	internal sealed class \u000F
	{
		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x060024F4 RID: 9460 RVA: 0x0007EDB0 File Offset: 0x0007CFB0
		private ICodegenerator codegen
		{
			get
			{
				return this.\u0001.\u0001;
			}
		}

		// Token: 0x060024F5 RID: 9461 RVA: 0x0007EDC0 File Offset: 0x0007CFC0
		public \u000F(global::\u0004.\u000E \u0010\u0006, global::\u000E.\u0011 \u0083\u0005)
		{
			this.\u0001 = \u0010\u0006;
			this.\u0001 = new ConversionReplacer(\u0083\u0005);
		}

		// Token: 0x060024F6 RID: 9462 RVA: 0x0007EDDC File Offset: 0x0007CFDC
		private static _IExpression \u0001(_IExpression \u0002)
		{
			_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(\u0002);
			icompoAccessExpression._Right = global::\u0019.\u0003.\u0001("__vfTablePointer");
			return icompoAccessExpression;
		}

		// Token: 0x060024F7 RID: 9463 RVA: 0x0007EDF4 File Offset: 0x0007CFF4
		internal void \u0001(_ICallExpression \u0002)
		{
			global::\u0012.\u0011 u;
			bool flag;
			this.\u0001(\u0002, out u, out flag);
			\u0002.CallInfo.HasSideEffect = true;
			_IExpression u2 = \u0002._Callee.Duplicate() as _IExpression;
			this.\u0001.\u0001(0, null, null, AccessModeFlags.Call);
			\u0002._Callee.Accept(this.\u0001);
			bool u3 = this.\u0001.TopOfStack.VirtualFunctionCall;
			this.\u0001.\u0001();
			_ISignature isignature = global::\u0016.\u000F.\u0001(\u0002, this.\u0001._Scope);
			_ISignature isignature2 = this.\u0001._Scope[isignature.ParentSignatureId] as _ISignature;
			_ISignature u4 = isignature2 ?? isignature;
			_IExpression iexpression = null;
			string u5;
			_IExpression u6;
			_IExpression iexpression2;
			IIntermediateValueLocation intermediateValueLocation;
			this.\u0001(\u0002, flag, u2, isignature, ref u4, ref isignature2, ref iexpression, out u5, out u6, out iexpression2, out intermediateValueLocation);
			bool flag2 = isignature.GetFlag(SignatureFlag.Action);
			bool flag3 = false;
			ISignature u7 = isignature;
			if (isignature.POUType == Operator.FunctionBlock)
			{
				_ISignature isignature3 = global::\u0016.\u000F.\u0001(isignature);
				if (isignature3 == null)
				{
					throw new InvalidOperationException("No __MAIN method on functoin block.");
				}
				isignature2 = isignature;
				isignature = isignature3;
				flag2 = true;
				flag3 = true;
			}
			if (flag)
			{
				this.\u0001(u, ref iexpression, ref u6, iexpression2, intermediateValueLocation);
			}
			u.IdCalledSignature = isignature.Id;
			if (isignature.POUType == Operator.Method && iexpression != null)
			{
				global::\u0016.\u000F.\u0001(\u0002, isignature, iexpression);
			}
			global::\u0016.\u000F.\u0001(\u0002, flag2, u7);
			global::\u0016.\u000F.\u0001(\u0002, this.\u0001, u7);
			u.KindOfCall = global::\u0016.\u000F.\u0001(this.\u0001, u3, isignature, isignature2);
			if (\u0002._Condition != null)
			{
				this.\u0001.\u0001(0, null, null, AccessModeFlags.Read);
				\u0002._Condition.Accept(this.\u0001);
				this.\u0001.\u0001();
			}
			this.\u0001(\u0002, u5, u6, flag2, flag);
			global::\u0016.\u000F.\u0001(this.\u0001, \u0002, isignature, flag3);
			this.\u0001(\u0002, isignature);
			this.\u0001(\u0002, this.\u0001);
			if (u.KindOfCall == KindOfCall.InterfaceCall || u.KindOfCall == KindOfCall.VirtualFunctionCall)
			{
				this.\u0001.Cpou.SetFlag(CompiledPOUFlags.ContainsVirtualFunctionCalls, true);
			}
			if (isignature.GetFlag(SignatureFlag.Abstract))
			{
				global::\u0016.\u000F.\u0001(\u0002, isignature, u);
			}
			u.ExpLoadTargetAddress = global::\u0016.\u000F.\u0001(\u0002, this.\u0001, u2, u, isignature, u4, iexpression, iexpression2, intermediateValueLocation, flag3, flag);
			KindOfCall kindOfCall = \u0002.CallInfo.KindOfCall;
			if (kindOfCall - KindOfCall.StaticFunctionCall <= 2 && isignature.Size > this.\u0001.NMaxParamSize)
			{
				this.\u0001.NMaxParamSize = isignature.Size;
			}
		}

		// Token: 0x060024F8 RID: 9464 RVA: 0x0007F070 File Offset: 0x0007D270
		private void \u0001(_ICallExpression \u0002, out global::\u0012.\u0011 \u0003, out bool \u0004)
		{
			global::\u0012.\u0011 u = \u0002.CallInfo as global::\u0012.\u0011;
			if (u != null)
			{
				\u0003 = u;
				\u0004 = u.ComplexCall;
				return;
			}
			\u0004 = (this.codegen is ICodegenerator4 && \u0080.\u0013.\u0001(\u0002._Callee, this.\u0001._Scope));
			\u0003 = global::\u0012.\u0011.\u0001();
			\u0002.CallInfo = \u0003;
			\u0003.ComplexCall = \u0004;
		}

		// Token: 0x060024F9 RID: 9465 RVA: 0x0007F0D8 File Offset: 0x0007D2D8
		private static _ISignature \u0001(_ISignature \u0002)
		{
			foreach (ISignature signature in \u0002.SubSignatures)
			{
				if (signature.Name == IdentifierConstants.MainSignatureName)
				{
					return (_ISignature)signature;
				}
			}
			return null;
		}

		// Token: 0x060024FA RID: 9466 RVA: 0x0007F118 File Offset: 0x0007D318
		private static void \u0001(_ICallExpression \u0002, ISignature \u0003, _IExpression \u0004)
		{
			_IVariable ivariable = (_IVariable)\u0003[IdentifierConstants.InstancePointer];
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(ivariable.VersionedName);
			ivariableExpression.VariableId = ivariable.Id;
			ivariableExpression.SignatureId = \u0003.Id;
			ivariableExpression.Type = ivariable._Type;
			\u0002.AddParam(\u0004, ivariableExpression);
		}

		// Token: 0x060024FB RID: 9467 RVA: 0x0007F170 File Offset: 0x0007D370
		private void \u0001(global::\u0012.\u0011 \u0002, ref _IExpression \u0003, ref _IExpression \u0004, _IExpression \u0005, IIntermediateValueLocation \u0006)
		{
			if (\u0003 == null)
			{
				throw new ArgumentNullException("expInstancePointer");
			}
			if (\u0005 != null)
			{
				_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(true, \u0005._CompiledType, \u0006));
				iassignmentExpression._RValue = \u0005;
				\u0002.InstanceAssignment = iassignmentExpression;
				global::\u0011.\u0007 u = new global::\u0011.\u0007
				{
					AccessModeFlags = AccessModeFlags.Read
				};
				this.\u0001.\u0001<_IExpression>(\u0003, u, false, true, true);
				return;
			}
			_IAssignmentExpression iassignmentExpression2 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(true, \u0003._CompiledType, \u0006));
			iassignmentExpression2._RValue = \u0003;
			\u0002.InstanceAssignment = iassignmentExpression2;
			\u0003 = global::\u0019.\u0003.\u0001(false, \u0003._CompiledType, \u0006);
			\u0004 = global::\u0019.\u0003.\u0001(\u0003, Token.Empty);
		}

		// Token: 0x060024FC RID: 9468 RVA: 0x0007F218 File Offset: 0x0007D418
		private static _ISignature \u0001(_ICallExpression \u0002, IScope \u0003)
		{
			if (\u0002._Callee.GetVariable(\u0003) != null)
			{
				return ((_IUserdefType)\u0002._Callee.Type.DeRefType).GetSignature(\u0003) as _ISignature;
			}
			_ISignature isignature = \u0002._Callee.GetSignature(\u0003) as _ISignature;
			if (isignature != null)
			{
				return isignature;
			}
			return ((_IUserdefType)\u0002._Callee.Type).GetSignature(\u0003) as _ISignature;
		}

		// Token: 0x060024FD RID: 9469 RVA: 0x0007F288 File Offset: 0x0007D488
		private void \u0001(_ICallExpression \u0002, bool \u0003, _IExpression \u0004, _ISignature \u0005, ref _ISignature \u0006, ref _ISignature \u0007, ref _IExpression \u0008, out string \u000E, out _IExpression \u000F, out _IExpression \u0010, out IIntermediateValueLocation \u0011)
		{
			string u = null;
			\u000E = null;
			\u000F = null;
			\u0010 = null;
			\u0011 = global::\u0019.\u0003.Builder.CreateIntermediateValueLocation();
			if (\u0005.POUType == Operator.FunctionBlock || (\u0007 != null && (\u0007.POUType == Operator.FunctionBlock || \u0007.POUType == Operator.Type || \u0007.POUType == Operator.Interface)))
			{
				bool u2 = false;
				if (\u0007 != null && \u0007.POUType == Operator.Interface)
				{
					this.\u0001(\u0002, \u0003, \u0004, \u0005, ref \u0006, ref \u0007, ref \u0008, ref \u0010, \u0011, ref u);
					u2 = true;
				}
				else if (\u0005.POUType == Operator.FunctionBlock)
				{
					u = "ADR(" + \u0004.ToString() + ")";
					\u000E = \u0004.ToString();
					\u000F = (_IExpression)\u0004.Duplicate();
					\u000F.Type = \u0004.Type;
					_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr);
					_IExpression iexpression = (_IExpression)\u0004.Duplicate();
					iexpression.Type = \u0004.Type;
					ioperatorExpression.AddOperand(iexpression);
					ioperatorExpression.Type = global::\u0019.\u0003.\u0001(iexpression.Type as _IType);
					\u0008 = ioperatorExpression;
					global::\u0011.\u0007 u3 = new global::\u0011.\u0007
					{
						AccessModeFlags = AccessModeFlags.Read
					};
					this.\u0001.\u0001<_IExpression>(\u0008, u3, false, true, true);
				}
				else
				{
					_ICompoAccessExpression icompoAccessExpression = \u0002._Callee as _ICompoAccessExpression;
					if (icompoAccessExpression != null)
					{
						u = string.Format("ADR({0})", icompoAccessExpression.Left);
						\u000E = icompoAccessExpression.Left.ToString();
						\u000F = (_IExpression)icompoAccessExpression._Left.Duplicate();
						\u000F.Type = icompoAccessExpression._Left.Type;
						_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Adr);
						_IExpression left = icompoAccessExpression._Left;
						left.Type = icompoAccessExpression._Left.Type;
						ioperatorExpression2.AddOperand(left);
						ioperatorExpression2.Type = global::\u0019.\u0003.\u0001(left.Type as _IType);
						\u0008 = ioperatorExpression2;
					}
					else
					{
						u = "__InstancePointer";
						\u000E = "__InstancePointer^";
						\u000F = global::\u0016.\u000F.\u0001(this.\u0001._Scope);
					}
				}
				global::\u0016.\u000F.\u0001(this.\u0001, \u0005, \u0006, ref \u0008, ref u, u2);
				if (\u0008 == null)
				{
					\u0008 = this.\u0001.\u0001(u, null, false);
				}
			}
		}

		// Token: 0x060024FE RID: 9470 RVA: 0x0007F4C4 File Offset: 0x0007D6C4
		private static _IVariableExpression \u0001(IScope5 \u0002)
		{
			_IVariableExpression ivariableExpression = \u001E.\u0011.\u0001(\u0002);
			ISignature localSignature = \u0002.LocalSignature;
			if (localSignature != null && (localSignature.POUType == Operator.FunctionBlock || localSignature.GetFlag(SignatureFlag.Structure)))
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(localSignature.Name);
				iuserdefType.SignatureId = localSignature.Id;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				ivariableExpression.Type = type;
			}
			return ivariableExpression;
		}

		// Token: 0x060024FF RID: 9471 RVA: 0x0007F51C File Offset: 0x0007D71C
		private static _IDeRefAccessExpression \u0001(IScope5 \u0002)
		{
			_IVariableExpression ivariableExpression = global::\u0016.\u000F.\u0001(\u0002);
			_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.\u0001(ivariableExpression, Token.Empty);
			_IPointerType ipointerType = ivariableExpression.Type as _IPointerType;
			if (ipointerType != null)
			{
				ideRefAccessExpression.Type = ipointerType.BaseType;
			}
			ideRefAccessExpression.Info = new global::\u0014.\u000F
			{
				InstanceAccess = true
			};
			return ideRefAccessExpression;
		}

		// Token: 0x06002500 RID: 9472 RVA: 0x0007F56C File Offset: 0x0007D76C
		private static void \u0001(global::\u0004.\u000E \u0002, _ISignature \u0003, _ISignature \u0004, ref _IExpression \u0005, ref string \u0006, bool \u0007)
		{
			if (!\u0007 && (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) || \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL)))
			{
				_IVirtualFunctionTable ivirtualFunctionTable = (_IVirtualFunctionTable)\u0004.VirtualFunctionTable;
				int interfaceOffset;
				if (\u0003.POUType == Operator.FunctionBlock)
				{
					interfaceOffset = ivirtualFunctionTable.GetInterfaceOffset(IdentifierConstants.MainSignatureName, \u0002._Scope, \u0002.CompCon);
				}
				else
				{
					interfaceOffset = ivirtualFunctionTable.GetInterfaceOffset(\u0003.Name, \u0002._Scope, \u0002.CompCon);
				}
				if (interfaceOffset != CompilerServicesInternal.InvalidSignatureOffset)
				{
					\u0006 = \u0006 + " + " + interfaceOffset.ToString();
					if (\u0005 != null)
					{
						\u0005 = \u0002.\u0001(\u0006, null, false);
					}
				}
			}
		}

		// Token: 0x06002501 RID: 9473 RVA: 0x0007F614 File Offset: 0x0007D814
		private static void \u0001(_IExpression \u0002, IScope \u0003, ICompileContext \u0004)
		{
			_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)\u0002;
			_IUserdefType iuserdefType = (_IUserdefType)icompoAccessExpression._Left._CompiledType;
			_IVariableExpression ivariableExpression = (_IVariableExpression)icompoAccessExpression._Right;
			_ISignature isignature = (_ISignature)\u0003[iuserdefType.SignatureId];
			Debug.\u0001(isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion));
			IVariable[] array;
			ISignature[] array2;
			IScope scope;
			global::\u0007.\u0005.\u0001(\u0004, isignature.Id, false).FindDeclaration(ivariableExpression, out array, out array2, out scope);
			ivariableExpression._CompiledType = array[0].CompiledType;
			icompoAccessExpression._CompiledType = ivariableExpression._CompiledType;
			_IVariableExpression ivariableExpression2 = ivariableExpression;
			IVariable variable = array[0];
			ivariableExpression2.VariableId = ((variable != null) ? variable.Id : Helper.InvalidId);
			_IVariableExpression ivariableExpression3 = ivariableExpression;
			ISignature signature = array2[0];
			ivariableExpression3.SignatureId = ((signature != null) ? signature.Id : Helper.InvalidId);
			ivariableExpression.ScopeId = ((scope != null) ? scope.Id : Helper.InvalidId);
			icompoAccessExpression.Info = new global::\u0003.\u000F();
			icompoAccessExpression.Info.DoGeneration = false;
			global::\u0016.\u000F.\u0001(icompoAccessExpression, ivariableExpression._CompiledType);
		}

		// Token: 0x06002502 RID: 9474 RVA: 0x0007F708 File Offset: 0x0007D908
		private static void \u0001(IExprement \u0002, ICompiledType \u0003)
		{
			_IDeRefAccessExpression ideRefAccessExpression = \u0002 as _IDeRefAccessExpression;
			if (ideRefAccessExpression != null)
			{
				ideRefAccessExpression.DeRefInfo.CompiledType = \u0003;
				return;
			}
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				if (((global::\u0003.\u000F)icompoAccessExpression.Info).GenerateRight)
				{
					global::\u0016.\u000F.\u0001(icompoAccessExpression._Right, \u0003);
					return;
				}
				global::\u0016.\u000F.\u0001(icompoAccessExpression._Left, \u0003);
				return;
			}
			else
			{
				_IIndexAccessExpression iindexAccessExpression = \u0002 as _IIndexAccessExpression;
				if (iindexAccessExpression != null)
				{
					global::\u0016.\u000F.\u0001(iindexAccessExpression._Var, \u0003);
					return;
				}
				_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
				if (ivariableExpression != null)
				{
					ivariableExpression.VarInfo.CompiledType = \u0003;
				}
				return;
			}
		}

		// Token: 0x06002503 RID: 9475 RVA: 0x0007F790 File Offset: 0x0007D990
		private void \u0001(_ICallExpression \u0002, bool \u0003, _IExpression \u0004, _ISignature \u0005, ref _ISignature \u0006, ref _ISignature \u0007, ref _IExpression \u0008, ref _IExpression \u000E, IIntermediateValueLocation \u000F, ref string \u0010)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0002._Callee as _ICompoAccessExpression;
			if (this.codegen is ICodegenerator4 && \u0003)
			{
				\u000E = global::\u0016.\u000F.\u0001(icompoAccessExpression._Left);
				global::\u0016.\u000F.\u0001(\u000E, this.\u0001._Scope, this.\u0001.CompCon);
				if (\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) || \u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
				{
					\u0008 = global::\u0019.\u0003.\u0001(false, \u000E._CompiledType, \u000F);
				}
				else
				{
					_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Minus, Token.Empty);
					ioperatorExpression.AddOperand(global::\u0019.\u0003.\u0001(false, \u000E._CompiledType, \u000F));
					_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(false, \u000E._CompiledType, \u000F), Token.Empty), Token.Empty), Token.Empty);
					iindexAccessExpression.AddAccess(global::\u0019.\u0003.\u0001(0L));
					if (TypeTable.GetSize(TypeClass.Pointer, this.\u0001._Scope) == 8)
					{
						ioperatorExpression.AddOperand(global::\u0019.\u0003.\u0001(TypeClass.LWord, TypeClass.DWord, iindexAccessExpression));
					}
					else
					{
						ioperatorExpression.AddOperand(iindexAccessExpression);
					}
					\u0008 = ioperatorExpression;
				}
			}
			else
			{
				this.\u0001(\u0004, \u0005, out \u0010);
			}
			global::\u0016.\u000F.\u0001(this.\u0001, ref \u0006, ref \u0007, icompoAccessExpression);
		}

		// Token: 0x06002504 RID: 9476 RVA: 0x0007F8D0 File Offset: 0x0007DAD0
		private void \u0001(_IExpression \u0002, _ISignature \u0003, out string \u0004)
		{
			string interfaceSubstitute = IdentifierConstants.GetInterfaceSubstitute(((_ICompoAccessExpression)\u0002).Left.ToString());
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) || \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL))
			{
				\u0004 = interfaceSubstitute;
				return;
			}
			if (TypeTable.GetSize(TypeClass.Pointer, this.\u0001._Scope) == 8)
			{
				\u0004 = string.Format("{0} - LWORD_TO_DWORD({0}^^[0])", interfaceSubstitute);
				return;
			}
			\u0004 = string.Format("{0} - {0}^^[0]", interfaceSubstitute);
		}

		// Token: 0x06002505 RID: 9477 RVA: 0x0007F944 File Offset: 0x0007DB44
		private static void \u0001(global::\u0004.\u000E \u0002, ref _ISignature \u0003, ref _ISignature \u0004, _ICompoAccessExpression \u0005)
		{
			_IUserdefType iuserdefType = \u0005.Left.Type as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature = (_ISignature)iuserdefType.GetSignature(\u0002._Scope);
				if (isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					_IUserdefType iuserdefType2 = isignature["__Interface"].CompiledType.BaseType as _IUserdefType;
					if (iuserdefType2 != null)
					{
						isignature = (iuserdefType2.GetSignature(\u0002._Scope) as _ISignature);
					}
					if (isignature != null)
					{
						\u0004 = isignature;
						\u0003 = \u0004;
					}
				}
			}
		}

		// Token: 0x06002506 RID: 9478 RVA: 0x0007F9C0 File Offset: 0x0007DBC0
		private static _IExpression \u0001(_ICallExpression \u0002, global::\u0004.\u000E \u0003, _IExpression \u0004, global::\u0012.\u0011 \u0005, _ISignature \u0006, _ISignature \u0007, _IExpression \u0008, _IExpression \u000E, IIntermediateValueLocation \u000F, bool \u0010, bool \u0011)
		{
			string empty = string.Empty;
			bool flag = true;
			_IExpression result = null;
			KindOfCall kindOfCall = \u0005.KindOfCall;
			_IExpression iexpression;
			if (kindOfCall != KindOfCall.VirtualFunctionCall)
			{
				if (kindOfCall != KindOfCall.InterfaceCall)
				{
					iexpression = global::\u0016.\u000F.\u0001(\u0003, \u0006, ref empty, ref flag, ref result);
				}
				else
				{
					iexpression = global::\u0016.\u000F.\u0001(\u0003, \u0004, \u0006, \u0007, \u000E, \u000F, \u0011, ref empty);
				}
			}
			else
			{
				iexpression = global::\u0016.\u000F.\u0001(\u0002, \u0003, \u0004, \u0006, \u0007, \u0008, \u0010, \u0011, ref empty);
			}
			if (flag)
			{
				if (iexpression != null)
				{
					global::\u0011.\u0007 u = new global::\u0011.\u0007
					{
						AccessModeFlags = AccessModeFlags.Read
					};
					result = \u0003.\u0001<_IExpression>(iexpression, u, false, true, true);
				}
				else
				{
					global::\u0011.\u0007 u2 = new global::\u0011.\u0007
					{
						AccessModeFlags = AccessModeFlags.Read
					};
					result = \u0003.\u0001(empty, u2, false);
				}
			}
			return result;
		}

		// Token: 0x06002507 RID: 9479 RVA: 0x0007FA64 File Offset: 0x0007DC64
		private static _IExpression \u0001(global::\u0004.\u000E \u0002, _ISignature \u0003, ref string \u0004, ref bool \u0005, ref _IExpression \u0006)
		{
			bool flag = global::\u0016.\u0004.GenerateDirectCalls.GetBoolValue(\u0002.CompCon.GetTargetSettings()) && !\u0003.GetFlag(SignatureFlag.External);
			bool flag2 = \u0002.\u0001.\u0001(CodegeneratorProperties.DirectCallPossible);
			if (flag)
			{
				\u0003.AddAttribute(CompileAttributes.ATTRIBUTE_DIRECTCALL, null);
			}
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_DIRECTCALL) && flag2)
			{
				\u0004 = "0";
				return global::\u0019.\u0003.\u0001(0L);
			}
			_IVariableExpression ivariableExpression = LateCodeGenerator.\u0001(\u0003.Id);
			IList<ISignature> list = \u0002._Scope[IdentifierConstants.GlobalImplicitFunctionPointers];
			_ISignature isignature = null;
			_IVariable ivariable = null;
			if (list != null && list.Count<ISignature>() == 1)
			{
				isignature = (_ISignature)list[0];
				ivariable = (isignature[ivariableExpression.Name] as _IVariable);
			}
			if (ivariable == null)
			{
				_ICompileContext parentContext = \u0002.CompCon.ParentContext;
				while (parentContext != null && ivariable == null)
				{
					isignature = parentContext[IdentifierConstants.GlobalImplicitFunctionPointers];
					ivariable = (isignature[ivariableExpression.Name] as _IVariable);
					parentContext = parentContext.ParentContext;
				}
			}
			if (ivariable != null)
			{
				ivariableExpression.VariableId = ivariable.Id;
				ivariableExpression.SignatureId = isignature.Id;
				ivariableExpression.Type = ivariable.CompiledType;
				\u0005 = false;
				ivariableExpression.Accept(\u0002);
				\u0006 = ivariableExpression;
			}
			return ivariableExpression;
		}

		// Token: 0x06002508 RID: 9480 RVA: 0x0007FB9C File Offset: 0x0007DD9C
		private static _IExpression \u0001(global::\u0004.\u000E \u0002, _IExpression \u0003, _ISignature \u0004, _ISignature \u0005, _IExpression \u0006, IIntermediateValueLocation \u0007, bool \u0008, ref string \u000E)
		{
			_IExpression iexpression;
			if (\u0008)
			{
				int num = ((_IVirtualFunctionTable)\u0005.VirtualFunctionTable)[\u0004.Name] / \u0002._Scope.PointerSize;
				if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
				{
					num--;
				}
				_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(false, \u0006._CompiledType, \u0007), Token.Empty), Token.Empty), Token.Empty);
				iindexAccessExpression.AddAccess(global::\u0019.\u0003.\u0001((long)num));
				iexpression = iindexAccessExpression;
			}
			else
			{
				_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)\u0003;
				string interfaceSubstitute = IdentifierConstants.GetInterfaceSubstitute(icompoAccessExpression.Left.ToString());
				int num2 = ((_IVirtualFunctionTable)\u0005.VirtualFunctionTable)[\u0004.Name] / \u0002._Scope.PointerSize;
				if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
				{
					num2--;
				}
				\u000E = string.Format("{0}^^[{1}]", interfaceSubstitute, num2);
				_ICompoAccessExpression icompoAccessExpression2 = global::\u0019.\u0003.\u0001(icompoAccessExpression._Left.Duplicate() as _IExpression, Token.Empty);
				icompoAccessExpression2._Right = global::\u0019.\u0003.\u0001("__vfTablePointer");
				_IIndexAccessExpression iindexAccessExpression2 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression2, Token.Empty), Token.Empty), Token.Empty);
				iindexAccessExpression2.AddAccess(global::\u0019.\u0003.\u0001((long)num2));
				iexpression = iindexAccessExpression2;
			}
			if (!\u0002.CompCon.NewVFTable)
			{
				\u000E += "^";
				iexpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
			}
			return iexpression;
		}

		// Token: 0x06002509 RID: 9481 RVA: 0x0007FD00 File Offset: 0x0007DF00
		private static _IExpression \u0001(_ICallExpression \u0002, global::\u0004.\u000E \u0003, _IExpression \u0004, _ISignature \u0005, _ISignature \u0006, _IExpression \u0007, bool \u0008, bool \u000E, ref string \u000F)
		{
			if (\u0006 == null)
			{
				throw new ArgumentNullException("signInstance");
			}
			int num = ((_IVirtualFunctionTable)\u0006.VirtualFunctionTable)[\u0005.Name] / \u0003._Scope.PointerSize;
			_IExpression iexpression;
			if (\u000E)
			{
				_IExpression u = global::\u0019.\u0003.\u0001(\u0007, Token.Empty);
				_IExpression right = global::\u0019.\u0003.\u0001("__VFTABLEPOINTER");
				_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(u, Token.Empty);
				icompoAccessExpression._Right = right;
				_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression, Token.Empty), Token.Empty);
				iindexAccessExpression.AddAccess(global::\u0019.\u0003.\u0001((long)num));
				iexpression = iindexAccessExpression;
			}
			else if (\u0008)
			{
				\u000F = string.Format("{0}.__VFTABLEPOINTER^[{1}]", \u0004, num);
				_IExpression u2 = \u0004.Duplicate() as _IExpression;
				_IExpression right2 = global::\u0019.\u0003.\u0001("__VFTABLEPOINTER");
				_ICompoAccessExpression icompoAccessExpression2 = global::\u0019.\u0003.\u0001(u2, Token.Empty);
				icompoAccessExpression2._Right = right2;
				_IIndexAccessExpression iindexAccessExpression2 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression2, Token.Empty), Token.Empty);
				iindexAccessExpression2.AddAccess(global::\u0019.\u0003.\u0001((long)num));
				iexpression = iindexAccessExpression2;
			}
			else if (\u0002.Callee is _ICompoAccessExpression)
			{
				_ICompoAccessExpression icompoAccessExpression3 = (_ICompoAccessExpression)\u0004;
				\u000F = string.Format("{0}.__VFTABLEPOINTER^[{1}]", icompoAccessExpression3.Left, num);
				_IExpression u3 = icompoAccessExpression3._Left.Duplicate() as _IExpression;
				_IExpression right3 = global::\u0019.\u0003.\u0001("__VFTABLEPOINTER");
				_ICompoAccessExpression icompoAccessExpression4 = global::\u0019.\u0003.\u0001(u3, Token.Empty);
				icompoAccessExpression4._Right = right3;
				_IIndexAccessExpression iindexAccessExpression3 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression4, Token.Empty), Token.Empty);
				iindexAccessExpression3.AddAccess(global::\u0019.\u0003.\u0001((long)num));
				iexpression = iindexAccessExpression3;
			}
			else
			{
				\u000F = string.Format("__INSTANCEPOINTER^.__VFTABLEPOINTER^[{0}]", num);
				_IExpression u4 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(IdentifierConstants.InstancePointer), Token.Empty);
				_IExpression right4 = global::\u0019.\u0003.\u0001(IdentifierConstants.VFTablePointer);
				_ICompoAccessExpression icompoAccessExpression5 = global::\u0019.\u0003.\u0001(u4, Token.Empty);
				icompoAccessExpression5._Right = right4;
				_IIndexAccessExpression iindexAccessExpression4 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(icompoAccessExpression5, Token.Empty), Token.Empty);
				iindexAccessExpression4.AddAccess(global::\u0019.\u0003.\u0001((long)num));
				iexpression = iindexAccessExpression4;
			}
			if (!\u0003.CompCon.NewVFTable)
			{
				\u000F += "^";
				iexpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
			}
			return iexpression;
		}

		// Token: 0x0600250A RID: 9482 RVA: 0x0007FF0C File Offset: 0x0007E10C
		private void \u0001(_ICallExpression \u0002, global::\u0004.\u000E \u0003)
		{
			IList<_IAssignmentExpression> outputAssigns = \u0002._OutputAssigns;
			for (int i = 0; i < outputAssigns.Count; i++)
			{
				_IAssignmentExpression iassignmentExpression = outputAssigns[i];
				if (iassignmentExpression != null)
				{
					_IConversionExpression iconversionExpression = iassignmentExpression._RValue as _IConversionExpression;
					if (iconversionExpression != null)
					{
						_IExpression iexpression = this.\u0001.ReplaceConversionExpression(iconversionExpression);
						\u0002.SetFormalOutput(iexpression, i);
						iassignmentExpression._RValue = iexpression;
					}
					\u0003.\u0001(0, null, null, AccessModeFlags.OutParameter);
					\u0003.TopOfStack.Callee = \u0002.Callee;
					iassignmentExpression.Accept(\u0003);
					\u0002.SetFormalOutput(iassignmentExpression._RValue, i);
					\u0002.SetActualOutput(iassignmentExpression._LValue, i);
					\u0003.\u0001();
				}
			}
		}

		// Token: 0x0600250B RID: 9483 RVA: 0x0007FFB8 File Offset: 0x0007E1B8
		private void \u0001(_ICallExpression \u0002, _ISignature \u0003)
		{
			if ((\u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method) && \u0002.Type != null && !\u0084.\u0004.\u0001(\u0002.Type, this.\u0001._Scope, this.codegen))
			{
				Debug.\u0001(\u0003.Outputs.Length != 0);
				_IVariable ivariable = (_IVariable)\u0003.Outputs[0];
				bool flag = this.\u0001.\u0001.\u0001(CodegeneratorProperties.PositiveStackGrow);
				if (flag)
				{
					\u0002.ScratchOffset = this.\u0001.TopOfStack.CurrentScratchOffset;
				}
				this.\u0001.TopOfStack.CurrentScratchOffset += ivariable._Type.Size(this.\u0001._Scope);
				if (!flag)
				{
					\u0002.ScratchOffset = this.\u0001.TopOfStack.CurrentScratchOffset;
				}
				this.\u0001.NMaxScratchSize = Math.Max(this.\u0001.TopOfStack.CurrentScratchOffset, this.\u0001.NMaxScratchSize);
			}
		}

		// Token: 0x0600250C RID: 9484 RVA: 0x000800C0 File Offset: 0x0007E2C0
		private void \u0001(_ICallExpression \u0002, string \u0003, _IExpression \u0004, bool \u0005, bool \u0006)
		{
			if (!\u0005)
			{
				return;
			}
			IList<_IExpression> inputs = \u0002.Inputs;
			for (int i = 0; i < inputs.Count; i++)
			{
				if (global::\u0016.\u000F.\u0001(this.\u0001, inputs[i]).GetFlag(VarFlag.RelativeInstance))
				{
					if (\u0004 != null && inputs[i] is _IVariableExpression)
					{
						_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001(\u0004.Duplicate() as _IExpression, Token.Empty);
						icompoAccessExpression._Right = (inputs[i].Duplicate() as _IExpression);
						_IExpression expInput = this.\u0001.\u0001<_ICompoAccessExpression>(icompoAccessExpression, null, true, true, true);
						\u0002.SetFormalParam(expInput, i);
					}
					else
					{
						string u = string.Format("{0}.{1}", \u0003, inputs[i]);
						_IExpression expInput2 = this.\u0001.\u0001(u, null, true);
						\u0002.SetFormalParam(expInput2, i);
					}
				}
			}
			if (\u0006)
			{
				for (int j = 0; j < \u0002.Outputs.Count; j++)
				{
					if (\u0004 == null)
					{
						throw new InvalidOperationException();
					}
					_ICompoAccessExpression icompoAccessExpression2 = global::\u0019.\u0003.\u0001(\u0004.Duplicate() as _IExpression, Token.Empty);
					icompoAccessExpression2._Right = (\u0002.Outputs[j].Duplicate() as _IExpression);
					_IExpression exp = this.\u0001.\u0001<_ICompoAccessExpression>(icompoAccessExpression2, null, true, true, true);
					\u0002.SetFormalOutput(exp, j);
				}
			}
		}

		// Token: 0x0600250D RID: 9485 RVA: 0x00080214 File Offset: 0x0007E414
		private static IVariable \u0001(global::\u0004.\u000E \u0002, _IExpression \u0003)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0003 as _ICompoAccessExpression;
			if (icompoAccessExpression != null)
			{
				return icompoAccessExpression._Left.GetVariable(\u0002._Scope);
			}
			return ((_IVariableExpression)\u0003).GetVariable(\u0002._Scope);
		}

		// Token: 0x0600250E RID: 9486 RVA: 0x00080250 File Offset: 0x0007E450
		private static KindOfCall \u0001(global::\u0004.\u000E \u0002, bool \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			if (\u0005 != null)
			{
				Operator poutype = \u0005.POUType;
				if (poutype <= Operator.Program)
				{
					if (poutype == Operator.FunctionBlock)
					{
						goto IL_4B;
					}
					if (poutype != Operator.Program)
					{
						goto IL_AD;
					}
				}
				else
				{
					if (poutype == Operator.Type)
					{
						goto IL_4B;
					}
					if (poutype != Operator.VarGlobal)
					{
						if (poutype == Operator.Interface)
						{
							return KindOfCall.InterfaceCall;
						}
						goto IL_AD;
					}
				}
				if (\u0004.POUType == Operator.Method)
				{
					return KindOfCall.StaticFunctionCall;
				}
				return KindOfCall.ProgramCall;
				IL_4B:
				bool flag = \u0005.GetFlag(SignatureFlag.NonVirtual);
				bool flag2 = (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL) || \u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL)) && \u0004.Name != IdentifierConstants.VFInitMethodName;
				if (((\u0003 && !\u0004.GetFlag(SignatureFlag.NonVirtual) && !flag) || flag2) && !\u0002.CompCon.MinimalSystem)
				{
					return KindOfCall.VirtualFunctionCall;
				}
				return KindOfCall.StaticFunctionCall;
				IL_AD:
				Debug.\u0001(false);
				return KindOfCall.None;
			}
			if (\u0004.POUType != Operator.Program)
			{
				return KindOfCall.StaticFunctionCall;
			}
			return KindOfCall.ProgramCall;
		}

		// Token: 0x0600250F RID: 9487 RVA: 0x00080314 File Offset: 0x0007E514
		private static void \u0001(_ICallExpression \u0002, global::\u0004.\u000E \u0003, ISignature \u0004)
		{
			if (\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_REFLECTION))
			{
				IList<_IExpression> inputs = \u0002.Inputs;
				foreach (IVariable variable in \u0004.Locals)
				{
					if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_IS_CONNECTED))
					{
						string attributeValue = variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_IS_CONNECTED);
						bool u = false;
						using (IEnumerator<_IExpression> enumerator = inputs.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.GetVariable(\u0003._Scope).Name == attributeValue.ToUpperInvariant())
								{
									u = true;
									break;
								}
							}
						}
						_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(variable.OrgName);
						ivariableExpression.VariableId = variable.Id;
						ivariableExpression.SignatureId = \u0004.Id;
						ivariableExpression.Type = (variable as _IVariable)._Type;
						_ILiteralExpression iliteralExpression = global::\u0019.\u0003.\u0001(u);
						iliteralExpression.Type = TypeTable.Bool;
						\u0002.AddParam(iliteralExpression, ivariableExpression);
					}
				}
			}
		}

		// Token: 0x06002510 RID: 9488 RVA: 0x0008042C File Offset: 0x0007E62C
		private static void \u0001(_ICallExpression \u0002, bool \u0003, ISignature \u0004)
		{
			if (!\u0004.HasAttribute(CompileAttributes.ATTRIBUTE_INITIALIZE_ON_CALL) || !\u0003)
			{
				return;
			}
			foreach (IVariable variable in \u0004.Inputs)
			{
				if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_INITIALIZE_ON_CALL) && variable.Initial != null && !global::\u0016.\u000F.\u0001(\u0002, variable))
				{
					_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(variable.OrgName);
					ivariableExpression.VariableId = variable.Id;
					ivariableExpression.SignatureId = \u0004.Id;
					ivariableExpression.Type = (variable as _IVariable)._Type;
					\u0002.InsertParam(0, (variable.Initial as _IExpression).Duplicate() as _IExpression, ivariableExpression);
				}
			}
		}

		// Token: 0x06002511 RID: 9489 RVA: 0x000804D8 File Offset: 0x0007E6D8
		private static bool \u0001(_ICallExpression \u0002, IVariable \u0003)
		{
			for (int i = 0; i < \u0002.Inputs.Count - 1; i++)
			{
				if (\u0002.Inputs[i].VariableId == \u0003.Id)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002512 RID: 9490 RVA: 0x0008051C File Offset: 0x0007E71C
		private static void \u0001(_ICallExpression \u0002, _ISignature \u0003, global::\u0012.\u0011 \u0004)
		{
			if (KindOfCall.StaticFunctionCall == \u0004.KindOfCall)
			{
				if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
				{
					string orgName = \u0003.OrgName;
					string text = null;
					if (orgName.StartsWith("__get"))
					{
						text = orgName.Substring("__get".Length);
					}
					else if (orgName.StartsWith("__set"))
					{
						text = orgName.Substring("__set".Length);
					}
					\u0002.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_AbstractPropertyStaticCall, new object[]
					{
						text
					}), MessageId.Err_AbstractPropertyStaticCall);
					return;
				}
				\u0002.AddError(global::\u0003.\u0006.\u0001(MessageId.Err_AbstractMethodStaticCall, new object[]
				{
					\u0003.OrgName
				}), MessageId.Err_AbstractMethodStaticCall);
			}
		}

		// Token: 0x06002513 RID: 9491 RVA: 0x000805D0 File Offset: 0x0007E7D0
		private static void \u0001(global::\u0004.\u000E \u0002, _ICallExpression \u0003, _ISignature \u0004, bool \u0005)
		{
			IList<_IAssignmentExpression> inputAssigns = \u0003._InputAssigns;
			int num = \u0002.TopOfStack.CurrentScratchOffset;
			IVariable[] allInputs = \u0004.AllInputs;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002.CompCon, \u0004.Id);
			LList<_IAssignmentExpression> llist = new LList<_IAssignmentExpression>();
			global::\u0016.\u000F.\u0001(\u0002, \u0003, \u0004, inputAssigns, num, allInputs, scope, llist);
			global::\u0016.\u000F.\u0001(\u0002, \u0003, \u0005, num, scope, llist);
		}

		// Token: 0x06002514 RID: 9492 RVA: 0x0008062C File Offset: 0x0007E82C
		private static void \u0001(global::\u0004.\u000E \u0002, _ICallExpression \u0003, bool \u0004, int \u0005, IScope5 \u0006, LList<_IAssignmentExpression> \u0007)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0007)
			{
				_ICompoAccessExpression icompoAccessExpression = iassignmentExpression.LValue as _ICompoAccessExpression;
				_IExpression iexpression;
				if (icompoAccessExpression != null)
				{
					_IVariableExpression u = global::\u0019.\u0003.\u0001(((IVariableExpression)icompoAccessExpression.Right).Name + "__Array__Info");
					iexpression = global::\u0019.\u0003.\u0001(icompoAccessExpression._Left.Duplicate() as _IExpression, u);
				}
				else
				{
					_IVariableExpression ivariableExpression = iassignmentExpression.LValue as _IVariableExpression;
					if (ivariableExpression == null)
					{
						throw new InvalidOperationException();
					}
					iexpression = global::\u0019.\u0003.\u0001(ivariableExpression.Name + "__Array__Info");
				}
				_IArrayType iarrayType = iassignmentExpression._RValue._CompiledType as _IArrayType;
				if (iarrayType != null)
				{
					for (int i = 0; i < iarrayType._Dimensions.Count; i++)
					{
						_IArrayDimension iarrayDimension = iarrayType._Dimensions[i];
						_ILiteralExpression u2 = global::\u0019.\u0003.\u0001((long)(i + 1), TypeClass.Int);
						_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(iexpression.Duplicate() as _IExpression, u2);
						_IVariableExpression u3 = global::\u0019.\u0003.\u0001("diLower");
						_IVariableExpression u4 = global::\u0019.\u0003.\u0001("diUpper");
						_ICompoAccessExpression u5 = global::\u0019.\u0003.\u0001(iindexAccessExpression, u3);
						IExpression u6 = global::\u0019.\u0003.\u0001(iindexAccessExpression.Duplicate() as IExpression, u4);
						bool flag;
						int num = iarrayDimension.LowerBorderInt(out flag, \u0002._Scope);
						_IAssignmentExpression iassignmentExpression2 = global::\u0019.\u0003.\u0001(u5, global::\u0019.\u0003.\u0001((long)num));
						int num2 = iarrayDimension.UpperBorderInt(out flag, \u0002._Scope);
						_IAssignmentExpression iassignmentExpression3 = global::\u0019.\u0003.\u0001(u6, global::\u0019.\u0003.\u0001((long)num2));
						IScope5 u7 = \u0002._Scope;
						if (!\u0004)
						{
							\u0002._Scope = \u0006;
						}
						\u0002.\u0001<_IAssignmentExpression>(iassignmentExpression2, null, false, false, false);
						\u0002.\u0001<_IAssignmentExpression>(iassignmentExpression3, null, false, false, false);
						\u0002._Scope = u7;
						\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
						\u0002.TopOfStack.Callee = \u0003.Callee;
						\u0002.TopOfStack.CurrentScratchOffset = \u0005;
						iassignmentExpression2.Accept(\u0002);
						\u0003.AddParam(iassignmentExpression2._RValue, iassignmentExpression2._LValue);
						iassignmentExpression3.Accept(\u0002);
						\u0003.AddParam(iassignmentExpression3._RValue, iassignmentExpression3._LValue);
						\u0002.\u0001();
					}
				}
				else
				{
					IVariable variable = iassignmentExpression._RValue.GetVariable(\u0002._Scope);
					if (variable == null || !variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
					{
						throw new InvalidOperationException();
					}
					_IExpression u8 = iexpression.Duplicate() as _IExpression;
					_IExpression iexpression2 = global::\u0019.\u0003.\u0001(variable.OrgName + "__Array__Info");
					IScope5 u9 = \u0002._Scope;
					if (!\u0004)
					{
						\u0002._Scope = \u0006;
					}
					\u0002.\u0001<_IExpression>(u8, null, false, false, false);
					\u0002._Scope = u9;
					\u0002.\u0001<_IExpression>(iexpression2, null, false, false, false);
					_IAssignmentExpression iassignmentExpression4 = global::\u0019.\u0003.\u0001(u8, iexpression2);
					\u0002.\u0001<_IAssignmentExpression>(iassignmentExpression4, null, false, false, false);
					\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
					\u0002.TopOfStack.Callee = \u0003.Callee;
					\u0002.TopOfStack.CurrentScratchOffset = \u0005;
					iassignmentExpression4.Accept(\u0002);
					\u0003.AddParam(iassignmentExpression4._RValue, iassignmentExpression4._LValue);
					\u0002.\u0001();
				}
			}
		}

		// Token: 0x06002515 RID: 9493 RVA: 0x00080960 File Offset: 0x0007EB60
		private static void \u0001(global::\u0004.\u000E \u0002, _ICallExpression \u0003, _ISignature \u0004, IList<_IAssignmentExpression> \u0005, int \u0006, IVariable[] \u0007, IScope5 \u0008, LList<_IAssignmentExpression> \u000E)
		{
			int i = 0;
			while (i < \u0005.Count)
			{
				_IAssignmentExpression iassignmentExpression = \u0005[i];
				if (\u0007.Length > i && \u0007[i].GetFlag(VarFlag.ImplicitParamsStruct))
				{
					int num = \u0005.Count;
					if (\u0004.POUType == Operator.Method)
					{
						num--;
					}
					IExpression u = global::\u0019.\u0003.\u0001(\u0007[i].OrgName);
					_IVariableExpression u2 = global::\u0019.\u0003.\u0001("nCount");
					IExpression u3 = global::\u0019.\u0003.\u0001(u, u2);
					_ILiteralExpression u4 = global::\u0019.\u0003.\u0001((long)(num - i), TypeClass.Int);
					_IAssignmentExpression iassignmentExpression2 = global::\u0019.\u0003.\u0001(u3, u4);
					IScope5 u5 = \u0002._Scope;
					\u0002._Scope = \u0008;
					\u0002.\u0001<_IAssignmentExpression>(iassignmentExpression2, null, false, false, false);
					\u0002._Scope = u5;
					\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
					\u0002.TopOfStack.Callee = \u0003.Callee;
					\u0002.TopOfStack.CurrentScratchOffset = \u0006;
					iassignmentExpression2.Accept(\u0002);
					\u0003.SetFormalParam(iassignmentExpression2._LValue, i);
					\u0003.SetActualParam(iassignmentExpression2._RValue, i);
					\u0002.\u0001();
					for (int j = i; j < num; j++)
					{
						IExpression u6 = global::\u0019.\u0003.\u0001(\u0007[i].OrgName);
						_IVariableExpression u7 = global::\u0019.\u0003.\u0001("arrParams");
						IExpression u8 = global::\u0019.\u0003.\u0001(u6, u7);
						_ILiteralExpression u9 = global::\u0019.\u0003.\u0001((long)(j - i + 1), TypeClass.Int);
						_IAssignmentExpression iassignmentExpression3 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(u8, u9), \u0005[j]._RValue);
						\u0002._Scope = \u0008;
						\u0002.\u0001<_IAssignmentExpression>(iassignmentExpression3, null, false, false, false);
						\u0002._Scope = u5;
						\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
						\u0002.TopOfStack.Callee = \u0003.Callee;
						\u0002.TopOfStack.CurrentScratchOffset = \u0006;
						iassignmentExpression3.Accept(\u0002);
						if (j == \u0005.Count - 1)
						{
							\u0003.AddParam(iassignmentExpression3._RValue, iassignmentExpression3._LValue);
						}
						else
						{
							\u0003.SetFormalParam(iassignmentExpression3._LValue, j + 1);
							\u0003.SetActualParam(iassignmentExpression3._RValue, j + 1);
						}
						\u0002.\u0001();
					}
					if (\u0004.POUType == Operator.Method)
					{
						_IAssignmentExpression iassignmentExpression4 = \u0005[\u0005.Count - 1];
						\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
						\u0002.TopOfStack.Callee = \u0003.Callee;
						\u0002.TopOfStack.CurrentScratchOffset = \u0006;
						iassignmentExpression4.Accept(\u0002);
						\u0003.AddParam(iassignmentExpression4._RValue, iassignmentExpression4._LValue);
						\u0002.\u0001();
						return;
					}
					break;
				}
				else
				{
					if (iassignmentExpression != null)
					{
						IVariable variable = iassignmentExpression._LValue.GetVariable(\u0002._Scope);
						if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
						{
							IVariable variable2 = iassignmentExpression._RValue.GetVariable(\u0002._Scope);
							if (variable2 != null && variable2.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY))
							{
								\u000E.Add(iassignmentExpression.Duplicate() as _IAssignmentExpression);
							}
							else
							{
								_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr, iassignmentExpression._RValue);
								ioperatorExpression.Type = global::\u0019.\u0003.\u0001(iassignmentExpression._RValue.Type as _IType);
								\u000E.Add(iassignmentExpression.Duplicate() as _IAssignmentExpression);
								iassignmentExpression._RValue = ioperatorExpression;
							}
						}
						\u0002.\u0001(0, null, null, AccessModeFlags.Parameter);
						\u0002.TopOfStack.Callee = \u0003.Callee;
						\u0002.TopOfStack.CurrentScratchOffset = \u0006;
						iassignmentExpression.Accept(\u0002);
						\u0003.SetFormalParam(iassignmentExpression._LValue, i);
						\u0003.SetActualParam(iassignmentExpression._RValue, i);
						\u0002.\u0001();
					}
					i++;
				}
			}
		}

		// Token: 0x0400069A RID: 1690
		private readonly global::\u0004.\u000E \u0001;

		// Token: 0x0400069B RID: 1691
		private readonly ConversionReplacer \u0001;
	}
}
