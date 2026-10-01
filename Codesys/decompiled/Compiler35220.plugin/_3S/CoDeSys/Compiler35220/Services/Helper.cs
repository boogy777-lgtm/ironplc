using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using \u0002;
using \u0003;
using \u0004;
using \u0006;
using \u0007;
using \u000F;
using \u0011;
using \u0012;
using \u0013;
using \u0014;
using \u0015;
using \u0016;
using \u0017;
using \u0018;
using \u0019;
using \u001C;
using CODESYS.Parser;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.InitialisationCode;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0081;
using \u0082;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000F2 RID: 242
	internal sealed class Helper : ICompilerHelper6, ICompilerHelper5, ICompilerHelper4, ICompilerHelper3, ICompilerHelper2, ICompilerHelper
	{
		// Token: 0x0600104C RID: 4172 RVA: 0x0002E25C File Offset: 0x0002C45C
		public string \u0001(string \u0002)
		{
			return global::\u0014.\u0002.\u0001(\u0002);
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x0002E264 File Offset: 0x0002C464
		public bool \u0001(string \u0002)
		{
			return global::\u0014.\u0002.\u0001(\u0002);
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x0002E26C File Offset: 0x0002C46C
		public bool \u0001(string \u0002, out string \u0003, out string \u0004, out Version \u0005)
		{
			return global::\u0014.\u0002.\u0001(\u0002, out \u0003, out \u0004, out \u0005);
		}

		// Token: 0x0600104F RID: 4175 RVA: 0x0002E278 File Offset: 0x0002C478
		public bool \u0001(string \u0002, string \u0003, out Version \u0004)
		{
			return global::\u0014.\u0002.\u0001(\u0002, \u0003, out \u0004);
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x0002E284 File Offset: 0x0002C484
		public byte[] \u0001(string \u0002, TypeClass \u0003, bool \u0004, int \u0005)
		{
			ByteOrder u = \u0004 ? ByteOrder.Motorola : ByteOrder.Intel;
			return global::\u0017.\u0003.Singleton.\u0001(u, \u0003, \u0005, \u0002, StringEncoding.Default);
		}

		// Token: 0x06001051 RID: 4177 RVA: 0x0002E2AC File Offset: 0x0002C4AC
		internal static long \u0001(ILiteralValue \u0002)
		{
			if (\u0002 == null)
			{
				return 0L;
			}
			if (\u0002.KindOf == KindOfLiteral.UnsignedInteger)
			{
				return (long)\u0002.UnsignedLong;
			}
			return \u0002.SignedLong;
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x06001052 RID: 4178 RVA: 0x0002E2CC File Offset: 0x0002C4CC
		public static int InvalidId
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x0002E2D0 File Offset: 0x0002C4D0
		public _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005, out ISignature[] \u0006, out _IPreCompileContext \u0007)
		{
			return Helper.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, out \u0007);
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x0002E2E0 File Offset: 0x0002C4E0
		public _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005)
		{
			return Helper.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x0002E2EC File Offset: 0x0002C4EC
		public Version \u0001(ITargetSettings \u0002)
		{
			return Helper.\u0001(\u0002);
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x0002E2F4 File Offset: 0x0002C4F4
		public IList<_ISignature> \u0001(_ICompileContext \u0002, string \u0003, out IList<IExpression> \u0004)
		{
			LList<IExpression> llist = new LList<IExpression>();
			IList<_ISignature> result = Helper.\u0001(\u0002, \u0003, out llist);
			\u0004 = llist;
			return result;
		}

		// Token: 0x06001057 RID: 4183 RVA: 0x0002E314 File Offset: 0x0002C514
		public string \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, _IPreCompileContext \u0004)
		{
			return Helper.\u0001(\u0002, \u0003, \u0004, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0002E324 File Offset: 0x0002C524
		public string \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003)
		{
			return Helper.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0002E330 File Offset: 0x0002C530
		public void \u0001(IScope \u0002, ISignature \u0003, IDictionary<int, int> \u0004, bool \u0005)
		{
			Helper.\u0001(\u0002 as IScope5, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0002E344 File Offset: 0x0002C544
		public IDataLocation \u0001(_ICompileContext \u0002, out IMessage \u0003, out bool \u0004, ISourcePosition \u0005, IDirectVariable \u0006, IVariable2 \u0007)
		{
			return Locator.\u0001(\u0002, out \u0003, out \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0002E354 File Offset: 0x0002C554
		public string \u0001(IVariable \u0002, ISignature \u0003, IScope \u0004, string \u0005)
		{
			return global::\u0004.\u0018.\u0001(\u0002 as _IVariable, \u0003 as _ISignature, \u0004 as IScope5);
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x0002E370 File Offset: 0x0002C570
		public int \u0001(ICompiledType \u0002, int \u0003, IScope \u0004)
		{
			return Locator.\u0002(\u0002, \u0003, \u0004 as IScope5);
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x0002E380 File Offset: 0x0002C580
		public string[] \u0001(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005, bool \u0006)
		{
			LList<_IVariable> llist = new LList<_IVariable>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001
			{
				\u0001 = \u0006
			};
			LList<string> llist3 = InstancePathService.\u0001(\u0002, \u0003 as _ISignature, Array.Empty<int>(), \u0080.\u0005.\u0001(), llist, llist2, u);
			\u0004 = llist.Cast<IVariable>().ToArray<IVariable>();
			\u0005 = llist2.Cast<ISignature>().ToArray<ISignature>();
			return llist3.ToArray();
		}

		// Token: 0x0600105E RID: 4190 RVA: 0x0002E3E0 File Offset: 0x0002C5E0
		public string[] \u0001(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005)
		{
			return this.\u0001(\u0002, \u0003, out \u0004, out \u0005, false);
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x0002E3F0 File Offset: 0x0002C5F0
		public string[] \u0001(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			return \u0080.\u0005.\u0001(\u0002, \u0003, out \u0004, out \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x0002E404 File Offset: 0x0002C604
		public string[] \u0002(_ICompileContext \u0002, ISignature \u0003, out IVariable[] \u0004, out ISignature[] \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			return \u0080.\u0005.\u0002(\u0002, \u0003, out \u0004, out \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x0002E418 File Offset: 0x0002C618
		public IEnumerable<IInstancePathInfo> \u0001(_ICompileContext \u0002, ISignature \u0003, bool \u0004)
		{
			return \u0080.\u0005.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x0002E424 File Offset: 0x0002C624
		public string[] \u0001(_ICompileContext \u0002, string \u0003)
		{
			string[] array = \u0003.Split(new char[]
			{
				'.'
			});
			if (array.Length == 0)
			{
				return Array.Empty<string>();
			}
			_ISignature isignature = \u0002[array[0]];
			if (isignature == null)
			{
				return Array.Empty<string>();
			}
			if (array.Length == 2)
			{
				isignature = (isignature.GetSubSignature(array[1]) as _ISignature);
			}
			\u0080.\u0005.\u0001 u = new \u0080.\u0005.\u0001();
			LList<_IVariable> u2 = new LList<_IVariable>();
			LList<_ISignature> u3 = new LList<_ISignature>();
			return InstancePathService.\u0001(\u0002, isignature, Array.Empty<int>(), \u0080.\u0005.\u0001(), u2, u3, u).ToArray();
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x0002E4A4 File Offset: 0x0002C6A4
		public string \u0001(string \u0002, _IExpression \u0003)
		{
			return Helper.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x0002E4B0 File Offset: 0x0002C6B0
		public void \u0001()
		{
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x0002E4B4 File Offset: 0x0002C6B4
		public string \u0001(IVariable \u0002, int \u0003)
		{
			return global::\u0014.\u0013.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001066 RID: 4198 RVA: 0x0002E4C0 File Offset: 0x0002C6C0
		public ICompiledType \u0001(ILiteralExpression \u0002, bool \u0003, bool \u0004, bool \u0005, bool \u0006, bool \u0007)
		{
			return Helper.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007);
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x0002E4D0 File Offset: 0x0002C6D0
		public static _ISignature \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002 == null)
			{
				return \u0002;
			}
			if (!\u0002.GetFlag(SignatureFlag.Alias))
			{
				return \u0002;
			}
			_IUserdefType iuserdefType = \u0002.AllVariables[0].CompiledType as _IUserdefType;
			if (!string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				\u0003 = \u0003.CreateLocalScope(\u0002);
			}
			ISignature[] array = \u0003.FindSignature(iuserdefType.NameExpression);
			if (array == null || array.Length != 1)
			{
				return null;
			}
			return array[0] as _ISignature;
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x0002E53C File Offset: 0x0002C73C
		public static _ISignature \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			_ISignature result = null;
			if (\u0002.GetFlag(SignatureFlag.Alias))
			{
				_IEnumType ienumType = \u0002.AllVariables[0].CompiledType as _IEnumType;
				if (ienumType != null)
				{
					result = (\u0003.GetSignatureById(ienumType.SignatureId) as _ISignature);
				}
			}
			return result;
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x0002E584 File Offset: 0x0002C784
		public static bool \u0001(_ILiteralValue \u0002, _IType \u0003, BinaryWriter \u0004, bool \u0005, IScope \u0006, StringEncoding \u0007)
		{
			TypeClass typeClass = TypeClass.None;
			if (\u0003.Class == TypeClass.Subrange)
			{
				\u0003 = (\u0003.DeRefType as _IType);
			}
			if (TypeTable.IsInteger(\u0003.Class) || TypeTable.IsTimeOrDateType(\u0003.Class))
			{
				int num = \u0003.Size(\u0006);
				switch (num)
				{
				case 1:
					typeClass = TypeClass.Byte;
					goto IL_8F;
				case 2:
					typeClass = TypeClass.UInt;
					goto IL_8F;
				case 3:
					break;
				case 4:
					typeClass = TypeClass.UDInt;
					goto IL_8F;
				default:
					if (num == 8)
					{
						if (TypeTable.IsSigned(\u0003.Class))
						{
							typeClass = TypeClass.LInt;
							goto IL_8F;
						}
						typeClass = TypeClass.ULInt;
						goto IL_8F;
					}
					break;
				}
				_3S.CoDeSys.Compiler35220.Tools.Debug.\u0001(false);
			}
			else
			{
				typeClass = \u0003.Class;
			}
			IL_8F:
			bool flag;
			switch (typeClass)
			{
			case TypeClass.Bool:
				if (\u0002.GetBoolV(out flag))
				{
					\u0004.Write(1);
					return true;
				}
				\u0004.Write(0);
				return true;
			case TypeClass.Bit:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.USInt:
				goto IL_336;
			case TypeClass.Byte:
				\u0004.Write((byte)\u0002.GetInt(out flag));
				return true;
			case TypeClass.LInt:
			{
				long num2 = (long)((\u0002.KindOf == KindOfLiteral.UnsignedInteger) ? \u0002.GetUnsignedLong(out flag) : ((ulong)\u0002.GetSignedLong(out flag)));
				if (\u0005)
				{
					\u0004.Write(BitHelper.Swap(num2));
					return true;
				}
				\u0004.Write(num2);
				return true;
			}
			case TypeClass.UInt:
				if (\u0005)
				{
					\u0004.Write(BitHelper.Swap((ushort)\u0002.GetInt(out flag)));
					return true;
				}
				\u0004.Write((ushort)\u0002.GetInt(out flag));
				return true;
			case TypeClass.UDInt:
				break;
			case TypeClass.ULInt:
				goto IL_17E;
			case TypeClass.Real:
				if (\u0005)
				{
					Int32Union int32Union = Int32Union.Empty();
					int32Union.m_float = (float)\u0002.GetFloat(out flag);
					BitHelper.Swap(ref int32Union.m_uint);
					\u0004.Write(int32Union.m_uint);
					return true;
				}
				\u0004.Write((float)\u0002.GetFloat(out flag));
				return true;
			case TypeClass.LReal:
				if (\u0005)
				{
					Int64Union int64Union = Int64Union.Empty();
					int64Union.m_double = \u0002.GetFloat(out flag);
					BitHelper.Swap(ref int64Union.m_ulong);
					\u0004.Write(int64Union.m_ulong);
					return true;
				}
				\u0004.Write(\u0002.GetFloat(out flag));
				return true;
			case TypeClass.String:
			{
				string text = \u0002.GetString(out flag);
				int num3 = \u0003.Size(\u0006) - 1;
				if (num3 < text.Length)
				{
					text = text.Substring(0, num3);
				}
				byte[] buffer = global::\u0019.\u0001.\u0001(text, TypeClass.String, \u0005, 1, \u0007);
				\u0004.Write(buffer);
				return true;
			}
			case TypeClass.WString:
			{
				string text2 = \u0002.GetString(out flag);
				int num4 = \u0003.Size(\u0006) / 2 - 1;
				if (num4 < text2.Length)
				{
					text2 = text2.Substring(0, num4);
				}
				byte[] buffer2 = global::\u0019.\u0001.\u0001(text2, TypeClass.WString, \u0005, 1, \u0007);
				\u0004.Write(buffer2);
				return true;
			}
			default:
				if (typeClass != TypeClass.Pointer)
				{
					goto IL_336;
				}
				if (\u0003.Size(\u0006) == 8)
				{
					goto IL_17E;
				}
				break;
			}
			if (\u0005)
			{
				\u0004.Write(BitHelper.Swap((uint)\u0002.GetInt(out flag)));
				return true;
			}
			\u0004.Write((uint)\u0002.GetInt(out flag));
			return true;
			IL_17E:
			ulong num5 = (\u0002.KindOf == KindOfLiteral.UnsignedInteger) ? \u0002.GetUnsignedLong(out flag) : ((ulong)\u0002.GetSignedLong(out flag));
			if (\u0005)
			{
				\u0004.Write(BitHelper.Swap(num5));
				return true;
			}
			\u0004.Write(num5);
			return true;
			IL_336:
			_3S.CoDeSys.Compiler35220.Tools.Debug.\u0001(false);
			return false;
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x0002E8D0 File Offset: 0x0002CAD0
		public ICheckSumVisitor \u0001(bool \u0002)
		{
			return new global::\u0012.\u0002(\u0002);
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x0002E8D8 File Offset: 0x0002CAD8
		public _IVariableDeclarationChecksumGenerator \u0001()
		{
			return new VarDeclarationChecksumGenerator();
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0002E8E0 File Offset: 0x0002CAE0
		public IPrecompileChecker \u0001(_ISignature \u0002, int \u0003, _IPreCompileContext \u0004, bool \u0005)
		{
			return new SimpleTypeChecker(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x0002E8EC File Offset: 0x0002CAEC
		public IPrecompileScope \u0001(_IPreCompileContext \u0002, _ISignature \u0003)
		{
			if (\u0002.ApplicationGuid == Guid.Empty && string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				return \u0081.\u0007.\u0001(\u0003, 4);
			}
			Guid applicationGuid = Guid.Empty;
			if (\u0002.ApplicationGuid != Guid.Empty)
			{
				applicationGuid = \u0002.ApplicationGuid;
			}
			else if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				applicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
			}
			_ILibraryTable libraryTable = \u0002._GetLibraryTable(applicationGuid);
			if (!string.IsNullOrEmpty((\u0003 != null) ? \u0003.LibraryPath : null))
			{
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(\u0003) as _IPreCompileContext;
				if (ipreCompileContext == null)
				{
					ipreCompileContext = \u0002;
				}
				return new CheckerScope(\u0002.PointerSize, libraryTable, \u0003, ipreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			}
			return new CheckerScope(\u0002.PointerSize, libraryTable, \u0003, \u0002, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x0002E9CC File Offset: 0x0002CBCC
		public IPrecompileScope \u0001(Guid \u0002, _IPreCompileContext \u0003, _ISignature \u0004)
		{
			return \u0081.\u0007.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0002E9D8 File Offset: 0x0002CBD8
		public IIdentifierInfo[] \u0001(string \u0002, ISourcePosition \u0003, WhatToFind \u0004)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0002E9E4 File Offset: 0x0002CBE4
		public IExprement \u0001(ISourcePosition \u0002, WhatToFind \u0003, out IPreCompileContext \u0004)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, out \u0004);
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x0002E9F0 File Offset: 0x0002CBF0
		public _ICheckerThread \u0001()
		{
			return APEnvironmentFacade.Instance.PrecompileChecker;
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x0002E9FC File Offset: 0x0002CBFC
		public bool \u0001()
		{
			return APEnvironmentFacade.Instance.PrecompileChecker.ChecksDone();
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0002EA10 File Offset: 0x0002CC10
		public void \u0001(_ISignature \u0002, _IPreCompileContext \u0003)
		{
			\u001C.\u000E.\u0001(\u0002, \u0003);
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0002EA1C File Offset: 0x0002CC1C
		public _IPrecompileScope \u0001(_ISignature \u0002, _IPreCompileContext \u0003, _IPreCompileContext \u0004)
		{
			return new CheckerScope(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x0002EA28 File Offset: 0x0002CC28
		public _IPrecompileScope \u0001(int \u0002, _ILibraryTable \u0003, _ISignature \u0004, _IPreCompileContext \u0005, _IPreCompileContext \u0006)
		{
			return new CheckerScope(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x0002EA38 File Offset: 0x0002CC38
		public IIdentifierInfo[] \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004, FindSubelementsFlags \u0005, out bool \u0006)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006);
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x0002EA48 File Offset: 0x0002CC48
		public IExpressionInfo \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, false);
		}

		// Token: 0x06001078 RID: 4216 RVA: 0x0002EA54 File Offset: 0x0002CC54
		public IIdentifierInfo[] \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004)
		{
			bool u = true;
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, u);
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x0002EA6C File Offset: 0x0002CC6C
		public IIdentifierInfo[] \u0002(_IPreCompileContext \u0002, Guid \u0003, string \u0004)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, true);
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x0002EA78 File Offset: 0x0002CC78
		public IEnumerable<IDeclarationInfo> \u0001(_IPreCompileContext \u0002, string \u0003, string \u0004, string \u0005)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0002EA84 File Offset: 0x0002CC84
		public ILMPreCompileTypifier \u0001(Guid \u0002)
		{
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(\u0002) as _IPreCompileContext;
			if (ipreCompileContext == null || !string.IsNullOrEmpty(ipreCompileContext.LibraryPath))
			{
				return null;
			}
			return new PreCompileTypifier(ipreCompileContext);
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0002EAC0 File Offset: 0x0002CCC0
		public _ISignature \u0001(string \u0002, bool \u0003)
		{
			return ParserHelper.\u0001(\u0002, \u0003);
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x0002EACC File Offset: 0x0002CCCC
		public _ISignature \u0001(string \u0002)
		{
			return ParserHelper.\u0001(\u0002, false);
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x0002EAD8 File Offset: 0x0002CCD8
		public byte[] \u0001(IVariable \u0002, ISignature \u0003, bool \u0004, ICompileContext \u0005)
		{
			return StaticSignatureTaskReferenceDetector.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x0002EAE4 File Offset: 0x0002CCE4
		public IList<ITaskCrossref> \u0001(IVariable \u0002, ISignature \u0003, bool \u0004, bool \u0005, ICompileContext \u0006)
		{
			return StaticSignatureTaskReferenceDetector.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06001080 RID: 4224 RVA: 0x0002EAF4 File Offset: 0x0002CCF4
		public bool \u0001(Operator \u0002)
		{
			return \u0084.\u0002.\u0001(\u0002);
		}

		// Token: 0x06001081 RID: 4225 RVA: 0x0002EAFC File Offset: 0x0002CCFC
		internal static bool \u0001(_IOperatorExpression \u0002)
		{
			return Helper.\u0001(\u0002.Code);
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x0002EB0C File Offset: 0x0002CD0C
		internal static bool \u0001(Operator \u0002)
		{
			return Operator.SizeOf == \u0002 || Operator.XSizeOf == \u0002;
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x0002EB20 File Offset: 0x0002CD20
		public _ISignature \u0001(string \u0002, string \u0003, bool \u0004)
		{
			return ParserHelper.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x0002EB2C File Offset: 0x0002CD2C
		public ChecksumStream \u0001()
		{
			return new MyChecksumStream(true);
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x0002EB34 File Offset: 0x0002CD34
		public ChecksumStream \u0001(bool \u0002)
		{
			return new MyChecksumStream(false);
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x0002EB40 File Offset: 0x0002CD40
		internal static ICompiledType \u0001(ILiteralExpression \u0002, bool \u0003, bool \u0004, bool \u0005, bool \u0006, bool \u0007)
		{
			return Helper.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, true);
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x0002EB50 File Offset: 0x0002CD50
		internal static ICompiledType \u0001(ILiteralExpression \u0002, bool \u0003, bool \u0004, bool \u0005, bool \u0006, bool \u0007, bool \u0008)
		{
			TypeClass constantType = \u0002.ConstantType;
			if (constantType <= TypeClass.Pointer)
			{
				if (constantType != TypeClass.LWord)
				{
					switch (constantType)
					{
					case TypeClass.LInt:
						if (\u0006)
						{
							if (\u0002.LongValue > 2147483647L || \u0002.LongValue < -2147483648L)
							{
								(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
								{
									\u0002.ToString(),
									TypeTable.Get(Operator.DInt).ToString()
								}), MessageId.Err_ConstantOverflow);
							}
							return TypeTable.DInt;
						}
						goto IL_3ED;
					case TypeClass.USInt:
					case TypeClass.UInt:
					case TypeClass.UDInt:
					case TypeClass.Real:
						goto IL_3ED;
					case TypeClass.ULInt:
						if (\u0006)
						{
							if (\u0002.ULongValue > (ulong)-1)
							{
								(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
								{
									\u0002.ToString(),
									TypeTable.Get(Operator.UDInt).ToString()
								}), MessageId.Err_ConstantOverflow);
							}
							return TypeTable.UDInt;
						}
						goto IL_3ED;
					case TypeClass.LReal:
						if (!\u0004)
						{
							goto IL_3ED;
						}
						break;
					case TypeClass.String:
						goto IL_336;
					case TypeClass.WString:
					{
						_IWStringType iwstringType = global::\u0019.\u0003.\u0001();
						iwstringType.Length = global::\u0019.\u0003.\u0001((long)\u0002.StringValue.Length, TypeClass.Int);
						return iwstringType;
					}
					default:
						if (constantType != TypeClass.Pointer)
						{
							goto IL_3ED;
						}
						if (\u0002.Type != null)
						{
							return \u0002.Type;
						}
						goto IL_3ED;
					}
				}
				else
				{
					if (\u0006)
					{
						if (\u0002.ULongValue > (ulong)-1)
						{
							(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
							{
								\u0002.ToString(),
								TypeTable.Get(Operator.DWord).ToString()
							}), MessageId.Err_ConstantOverflow);
						}
						return TypeTable.DWord;
					}
					goto IL_3ED;
				}
			}
			else
			{
				switch (constantType)
				{
				case TypeClass.AnyInt:
					if (\u0002.Negative)
					{
						long longValue = \u0002.LongValue;
						if (longValue >= -128L && longValue <= 127L)
						{
							if (\u0008)
							{
								return TypeTable.SInt;
							}
							return TypeTable.Int;
						}
						else
						{
							if (longValue >= -32768L && longValue <= 32767L)
							{
								return TypeTable.Int;
							}
							if (longValue >= -2147483648L && longValue <= 2147483647L)
							{
								return TypeTable.DInt;
							}
							if (!\u0005)
							{
								(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
								{
									\u0002.ToString(),
									TypeTable.Get(Operator.DInt).ToString()
								}), MessageId.Err_ConstantOverflow);
								return TypeTable.DInt;
							}
							return TypeTable.LInt;
						}
					}
					else
					{
						ulong ulongValue = \u0002.ULongValue;
						if (ulongValue == 0UL || ulongValue == 1UL)
						{
							return TypeTable.BitConst;
						}
						if (ulongValue <= 127UL)
						{
							if (\u0008)
							{
								return TypeTable.SInt;
							}
							return TypeTable.Int;
						}
						else if (ulongValue <= 255UL)
						{
							if (\u0008)
							{
								return TypeTable.USInt;
							}
							return TypeTable.UInt;
						}
						else
						{
							if (ulongValue <= 32767UL)
							{
								return TypeTable.Int;
							}
							if (ulongValue <= 65535UL)
							{
								return TypeTable.UInt;
							}
							if (ulongValue <= 2147483647UL)
							{
								return TypeTable.DInt;
							}
							if (ulongValue <= (ulong)-1)
							{
								return TypeTable.UDInt;
							}
							if (!\u0005)
							{
								(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
								{
									\u0002.ToString(),
									TypeTable.Get(Operator.UDInt).ToString()
								}), MessageId.Err_ConstantOverflow);
								return TypeTable.UDInt;
							}
							if (ulongValue <= 9223372036854775807UL)
							{
								return TypeTable.LInt;
							}
							return TypeTable.ULInt;
						}
					}
					break;
				case TypeClass.AnyNum:
				case TypeClass.Lazy:
					goto IL_3ED;
				case TypeClass.AnyReal:
					break;
				case TypeClass.LTime:
					if (\u0006)
					{
						if (\u0002.ULongValue > (ulong)-1)
						{
							(\u0002 as _IExprement).AddError(global::\u0003.\u0006.\u0001(MessageId.Err_ConstantOverflow, new object[]
							{
								\u0002.ToString(),
								TypeTable.Get(Operator.Time).ToString()
							}), MessageId.Err_ConstantOverflow);
						}
						return TypeTable.Time;
					}
					goto IL_3ED;
				default:
					if (constantType != TypeClass.XString)
					{
						if (constantType != TypeClass.AnyString)
						{
							goto IL_3ED;
						}
						_3S.CoDeSys.Compiler35220.Tools.Debug.\u0001(false);
						goto IL_336;
					}
					else
					{
						if (\u0007)
						{
							_IStringType istringType = global::\u0019.\u0003.\u0001();
							istringType.Length = global::\u0019.\u0003.\u0001((long)\u0002.StringValue.Length, TypeClass.Int);
							(\u0002 as _ILiteralExpression).ConstantType = TypeClass.String;
							return istringType;
						}
						_IWStringType iwstringType2 = global::\u0019.\u0003.\u0001();
						iwstringType2.Length = global::\u0019.\u0003.\u0001((long)\u0002.StringValue.Length, TypeClass.Int);
						(\u0002 as _ILiteralExpression).ConstantType = TypeClass.WString;
						return iwstringType2;
					}
					break;
				}
			}
			if (!\u0003)
			{
				return TypeTable.Get(Operator.Real);
			}
			return TypeTable.Get(Operator.LReal);
			IL_336:
			_IStringType istringType2 = global::\u0019.\u0003.\u0001();
			istringType2.Length = global::\u0019.\u0003.\u0001(global::\u0017.\u0003.Singleton.\u0001(\u0002.StringValue, ByteOrder.Intel, TypeClass.String, (\u0002 as _IStringLiteralExpression2).StringEncoding), TypeClass.Int);
			return istringType2;
			IL_3ED:
			return TypeTable.Get(\u0002.ConstantType);
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x0002EF58 File Offset: 0x0002D158
		public static string \u0001(_ISignature \u0002)
		{
			return NameManglingService.\u0001(\u0002);
		}

		// Token: 0x06001089 RID: 4233 RVA: 0x0002EF60 File Offset: 0x0002D160
		internal static _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005, out ISignature[] \u0006, out _IPreCompileContext \u0007)
		{
			_ISignature isignature = null;
			\u0007 = null;
			\u0006 = null;
			if (\u0003.ParentSignatureId == Helper.InvalidId)
			{
				if (\u0003.Name.Contains('<'))
				{
					string searchName = \u0003.GetSearchName(\u0002);
					string stName = searchName.Substring(0, searchName.IndexOf('<'));
					_ISignature isignature2 = \u0002[stName];
					if (isignature2 != null)
					{
						return Helper.\u0001(\u0002, isignature2, \u0004, \u0005, out \u0006, out \u0007);
					}
				}
				if (\u0003.LibraryPath == string.Empty)
				{
					if (\u0003.HasFlag(SignatureFlag.PoolSignature))
					{
						isignature = \u0005[\u0003.ObjectGuid];
						\u0006 = \u0005.GetSubSignatures(\u0003.ObjectGuid);
						if (isignature != null)
						{
							\u0007 = \u0005;
						}
					}
					else
					{
						isignature = Helper.\u0001(\u0003, \u0004, ref \u0006, ref \u0007);
					}
				}
				else
				{
					_IPreCompileContext libraryContextIgnoreVersion = \u0002.GetLibraryContextIgnoreVersion(\u0004, \u0005, \u0003.LibraryPath);
					if (libraryContextIgnoreVersion != null)
					{
						isignature = libraryContextIgnoreVersion[\u0003.ObjectGuid];
						\u0006 = libraryContextIgnoreVersion.GetSubSignatures(\u0003.ObjectGuid);
						if (isignature != null)
						{
							\u0007 = libraryContextIgnoreVersion;
						}
					}
				}
				return isignature;
			}
			_ISignature u = \u0002[\u0003.ParentSignatureId];
			Helper.\u0001(\u0002, u, \u0004, \u0005, out \u0006, out \u0007);
			if (\u0006 == null)
			{
				return null;
			}
			if (\u0003.GetFlagInternal(SignatureFlagInternal.Overloading))
			{
				string attributeValue = \u0003.GetAttributeValue("mangled_name");
				foreach (_ISignature isignature3 in \u0006.OfType<_ISignature>())
				{
					string b = Helper.\u0001(isignature3);
					if (string.Equals(attributeValue, b, StringComparison.OrdinalIgnoreCase))
					{
						\u0006 = null;
						return isignature3;
					}
				}
				return null;
			}
			foreach (_ISignature isignature4 in \u0006.OfType<_ISignature>())
			{
				if (string.Equals(isignature4.OrgName, \u0003.OrgName, StringComparison.OrdinalIgnoreCase))
				{
					\u0006 = null;
					return isignature4;
				}
			}
			return null;
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x0002F160 File Offset: 0x0002D360
		private static _ISignature \u0001(_ISignature \u0002, _IPreCompileContext \u0003, ref ISignature[] \u0004, ref _IPreCompileContext \u0005)
		{
			_ISignature isignature = null;
			_IPreCompileContext ipreCompileContext = \u0003;
			while (ipreCompileContext != null && isignature == null)
			{
				isignature = ipreCompileContext[\u0002.ObjectGuid];
				\u0004 = ipreCompileContext.GetSubSignatures(\u0002.ObjectGuid);
				if (isignature != null)
				{
					\u0005 = ipreCompileContext;
				}
				Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(ipreCompileContext.ApplicationGuid);
				if (!(parentApplication != Guid.Empty))
				{
					break;
				}
				ipreCompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(parentApplication) as _IPreCompileContext);
			}
			if (isignature == null)
			{
				isignature = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext[\u0002.ObjectGuid];
				\u0004 = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext.GetSubSignatures(\u0002.ObjectGuid);
				if (isignature != null)
				{
					\u0005 = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext;
				}
			}
			return isignature;
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x0002F228 File Offset: 0x0002D428
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005)
		{
			_ICompiledPOU icompiledPOU = null;
			if (\u0003.LibraryPath == string.Empty)
			{
				_IPreCompileContext ipreCompileContext = \u0004;
				while (ipreCompileContext != null && icompiledPOU == null)
				{
					icompiledPOU = ipreCompileContext.GetPOU(\u0003.ObjectGuid);
					Guid parentApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(ipreCompileContext.ApplicationGuid);
					if (!(parentApplication != Guid.Empty))
					{
						break;
					}
					ipreCompileContext = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(parentApplication) as _IPreCompileContext);
				}
				if (icompiledPOU == null)
				{
					icompiledPOU = APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext.GetPOU(\u0003.ObjectGuid);
				}
				if (icompiledPOU == null)
				{
					icompiledPOU = \u0005.GetPOU(\u0003.ObjectGuid);
				}
			}
			else
			{
				_IPreCompileContext libraryContextIgnoreVersion = \u0002.GetLibraryContextIgnoreVersion(\u0004, \u0005, \u0003.LibraryPath);
				if (libraryContextIgnoreVersion != null)
				{
					icompiledPOU = libraryContextIgnoreVersion.GetPOU(\u0003.ObjectGuid);
				}
			}
			return icompiledPOU;
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x0002F2F4 File Offset: 0x0002D4F4
		internal static Version \u0001(ITargetSettings \u0002)
		{
			string stringValue = global::\u0016.\u0004.RuntimeVersion.GetStringValue(\u0002);
			Version result = new Version(0, 0, 0, 0);
			try
			{
				result = new Version(stringValue);
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x0002F334 File Offset: 0x0002D534
		internal static bool \u0001(ITargetSettings \u0002)
		{
			bool bDefault = Helper.\u0001(\u0002) >= global::\u0016.\u0004.CycleControlVersion2.RuntimeVersion;
			return \u0002.GetBoolValue(global::\u0016.\u0004.CycleControlVersion2.Path, bDefault);
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x0002F368 File Offset: 0x0002D568
		internal static LList<_ISignature> \u0001(IList<_ISignature> \u0002)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			LSortedList<int, LList<_ISignature>> lsortedList = new LSortedList<int, LList<_ISignature>>();
			foreach (_ISignature isignature in \u0002)
			{
				int num = Helper.\u0001(isignature);
				LList<_ISignature> llist2 = null;
				if (!lsortedList.TryGetValue(num, ref llist2))
				{
					llist2 = new LList<_ISignature>();
					lsortedList[num] = llist2;
				}
				llist2.Add(isignature);
			}
			foreach (LList<_ISignature> llist3 in lsortedList.Values)
			{
				llist.AddRange(llist3);
			}
			return llist;
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x0002F42C File Offset: 0x0002D62C
		internal static int \u0001(_ISignature \u0002)
		{
			int result = 50000;
			if (\u0002.POUType == Operator.VarGlobal)
			{
				result = 49990;
			}
			if (\u0002.POUType == Operator.FunctionBlock || \u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				result = 49980;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT))
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_GLOBAL_INIT_SLOT);
				try
				{
					result = int.Parse(attributeValue);
				}
				catch
				{
				}
			}
			return result;
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x0002F4AC File Offset: 0x0002D6AC
		private static int \u0001(_ISignature \u0002, string \u0003)
		{
			int result = 50000;
			string attributeValue = \u0002.GetAttributeValue(\u0003);
			try
			{
				result = int.Parse(attributeValue);
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x0002F4E4 File Offset: 0x0002D6E4
		internal static LList<_ISignature> \u0001(_ICompileContext \u0002, string \u0003, out LList<IExpression> \u0004)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			\u0004 = new LList<IExpression>();
			LSortedList<int, LList<_ISignature>> lsortedList = new LSortedList<int, LList<_ISignature>>();
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002);
			foreach (_ISignature u2 in \u0002.AllFlat)
			{
				Helper.\u0001(\u0003, lsortedList, u, u2);
			}
			foreach (LList<_ISignature> llist2 in lsortedList.Values)
			{
				llist.AddRange(llist2);
			}
			foreach (_ISignature u3 in llist)
			{
				\u0004.Add(Helper.\u0001(\u0002, u3));
			}
			return llist;
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x0002F5D4 File Offset: 0x0002D7D4
		private static void \u0001(string \u0002, LSortedList<int, LList<_ISignature>> \u0003, IScope5 \u0004, _ISignature \u0005)
		{
			if (\u0005.GetFlag(SignatureFlag.NoInit))
			{
				return;
			}
			if (!\u0005.HasAttribute(\u0002))
			{
				return;
			}
			if (Helper.\u0001(\u0005, \u0004))
			{
				return;
			}
			int num = Helper.\u0001(\u0005, \u0002);
			LList<_ISignature> llist;
			if (!\u0003.TryGetValue(num, ref llist))
			{
				llist = new LList<_ISignature>();
				\u0003[num] = llist;
			}
			bool flag = false;
			IVariable[] array;
			ISignature[] array2;
			IScope scope;
			\u0004.FindDeclaration(\u0005.OrgName, out array, out array2, out scope);
			if (array != null && array.Length != 0 && array2 != null && array2.Length != 0)
			{
				string text = array2[0].OrgName + "." + array[0].OrgName;
				\u0005.AddMessage(Severity.Error, MessageId.Err_SlotFunctionHiddenByVariable, new object[]
				{
					\u0005.OrgName,
					\u0002,
					text
				});
				_ISourcePosition isourcePosition = array[0].SourcePosition as _ISourcePosition;
				isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(array2[0].LibraryPath), array2[0].ObjectGuid);
				\u0005.AddMessageString(isourcePosition, Severity.Information, \u0081.\u0002.Inf_RelatedPosition, Array.Empty<object>());
				flag = true;
			}
			else if (array2 != null && array2.Length > 1)
			{
				\u0005.AddMessage(Severity.Error, MessageId.Err_SlotFunctionAmbiguousName, new object[]
				{
					\u0005.OrgName,
					\u0002
				});
				_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(array2[1].LibraryPath), array2[1].ObjectGuid, 0L, 0, 0);
				\u0005.AddMessageString(sourcepos, Severity.Information, \u0081.\u0002.Inf_RelatedPosition, Array.Empty<object>());
				flag = true;
			}
			if (!flag)
			{
				llist.Add(\u0005);
			}
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x0002F764 File Offset: 0x0002D964
		private static bool \u0001(_ISignature \u0002, IScope5 \u0003)
		{
			if (\u0002.ParentSignatureId >= 0)
			{
				_ISignature isignature = \u0003[\u0002.ParentSignatureId] as _ISignature;
				if (isignature != null && isignature.POUType == Operator.Interface)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0002F79C File Offset: 0x0002D99C
		public static string \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003, _IPreCompileContext \u0004, Dictionary<_IPreCompileContext, _IPreCompileContext> \u0005)
		{
			if (\u0005.ContainsKey(\u0004))
			{
				return null;
			}
			\u0005.Add(\u0004, \u0004);
			IList<_IPreCompileContext> visibleLibraries = \u0002._LibraryTable.GetVisibleLibraries(\u0004);
			string namespaceOfLibrary = \u0002._LibraryTable.GetNamespaceOfLibrary(\u0004, \u0003.LibraryPath);
			if (namespaceOfLibrary != null)
			{
				return namespaceOfLibrary;
			}
			foreach (_IPreCompileContext ipreCompileContext in visibleLibraries)
			{
				string text = Helper.\u0001(\u0002, \u0003, ipreCompileContext, \u0005);
				if (text != null)
				{
					return \u0002._LibraryTable.GetNamespaceOfLibrary(\u0004, ipreCompileContext.LibraryPath) + "." + text;
				}
			}
			return null;
		}

		// Token: 0x06001095 RID: 4245 RVA: 0x0002F84C File Offset: 0x0002DA4C
		public static string \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003)
		{
			string localLibraryNamespace = \u0002.GetLocalLibraryNamespace(\u0003);
			if (localLibraryNamespace != null)
			{
				return localLibraryNamespace;
			}
			foreach (_IPreCompileContext ipreCompileContext in \u0002.GetReferencedLibraries(string.Empty))
			{
				string text = Helper.\u0001(\u0002, \u0003, ipreCompileContext, new Dictionary<_IPreCompileContext, _IPreCompileContext>());
				if (text != null)
				{
					return \u0002.GetLocalLibraryNamespace(ipreCompileContext) + "." + text;
				}
			}
			return null;
		}

		// Token: 0x06001096 RID: 4246 RVA: 0x0002F8B4 File Offset: 0x0002DAB4
		public static string \u0001(_ICompileContext \u0002, string \u0003)
		{
			_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0003);
			if (libraryContext == null)
			{
				return string.Empty;
			}
			return Helper.\u0001(\u0002, libraryContext).Replace(".", "#");
		}

		// Token: 0x06001097 RID: 4247 RVA: 0x0002F8F4 File Offset: 0x0002DAF4
		internal static void \u0001(IScope5 \u0002, ISignature \u0003, IDictionary<int, int> \u0004, bool \u0005)
		{
			foreach (int num in \u0003.InterfaceIds)
			{
				\u0004[num] = num;
				if (\u0005)
				{
					ISignature u = \u0002[num];
					Helper.\u0001(\u0002, u, \u0004, \u0005);
				}
			}
			if (\u0003.BaseSignatureId != Helper.InvalidId)
			{
				ISignature signature = \u0002[\u0003.BaseSignatureId];
				if (signature.POUType == Operator.Interface)
				{
					\u0004[signature.Id] = signature.Id;
				}
				Helper.\u0001(\u0002, signature, \u0004, \u0005);
			}
		}

		// Token: 0x06001098 RID: 4248 RVA: 0x0002F97C File Offset: 0x0002DB7C
		public IEnumerable<KeyValuePair<string, string>> \u0001(ISequenceStatement \u0002)
		{
			global::\u0007.\u0002 u = new global::\u0007.\u0002();
			((IExprementVisitor)new global::\u0013.\u0001(u)).visit(\u0002 as _ISequenceStatement);
			return u.Attributes;
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x0002F99C File Offset: 0x0002DB9C
		internal static string \u0001(string \u0002, _IExpression \u0003)
		{
			if (\u0003 is ICompoAccessExpression)
			{
				IExpression left = (\u0003 as ICompoAccessExpression).Left;
				return \u0002.Replace("$access$", left.ToString());
			}
			if (\u0003 == null)
			{
				return \u0002.Replace("$access$.", string.Empty);
			}
			return \u0002;
		}

		// Token: 0x0600109A RID: 4250 RVA: 0x0002F9E4 File Offset: 0x0002DBE4
		public string \u0001(Operator \u0002)
		{
			return Scanner.GetTextOfOperator(\u0002);
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x0002F9EC File Offset: 0x0002DBEC
		public string \u0001(_ISignature \u0002)
		{
			return GVLInitialisationFunctionCreator.\u0002(\u0002);
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x0002F9F4 File Offset: 0x0002DBF4
		internal static uint \u0001(_ICompiledPOU \u0002)
		{
			MyChecksumStream myChecksumStream = new MyChecksumStream(false);
			BinaryWriter binaryWriter = new BinaryWriter(myChecksumStream);
			if (\u0002.CompiledCode is ICompiledCode2 && \u0002.CompiledCode.CodeSize > 65536)
			{
				Stream code = (\u0002.CompiledCode as ICompiledCode2).GetCode();
				int num = (int)code.Length / 65536;
				byte[] buffer = new byte[65536];
				code.Position = 0L;
				for (int i = 0; i < num; i++)
				{
					code.Read(buffer, 0, 65536);
					binaryWriter.Write(buffer);
				}
				int count = (int)(code.Length - code.Position);
				code.Read(buffer, 0, count);
				binaryWriter.Write(buffer, 0, count);
			}
			else
			{
				\u0002.CompiledCode.GetCode(binaryWriter);
			}
			binaryWriter.Flush();
			myChecksumStream.Close();
			return myChecksumStream.Checksum;
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x0002FAD8 File Offset: 0x0002DCD8
		internal static bool \u0001(ICompiledCode \u0002, ICompiledCode \u0003)
		{
			if (\u0002.CodeSize != \u0003.CodeSize)
			{
				return false;
			}
			byte[] array = new byte[\u0002.CodeSize];
			byte[] array2 = new byte[\u0003.CodeSize];
			Stream output = new ChunkedMemoryStream(array);
			ChunkedMemoryStream output2 = new ChunkedMemoryStream(array2);
			BinaryWriter binaryWriter = new BinaryWriter(output);
			BinaryWriter binaryWriter2 = new BinaryWriter(output2);
			\u0002.GetCode(binaryWriter);
			\u0003.GetCode(binaryWriter2);
			binaryWriter.Flush();
			binaryWriter2.Flush();
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != array2[i])
				{
					return false;
				}
			}
			if (\u0002.RelocationList.RelocationAreaLists.Length != \u0003.RelocationList.RelocationAreaLists.Length)
			{
				return false;
			}
			for (int j = 0; j < \u0002.RelocationList.RelocationAreaLists.Length; j++)
			{
				IRelocationAreaList2 relocationAreaList = \u0002.RelocationList.RelocationAreaLists[j] as IRelocationAreaList2;
				IRelocationAreaList2 relocationAreaList2 = \u0003.RelocationList.RelocationAreaLists[j] as IRelocationAreaList2;
				if (relocationAreaList.Area != relocationAreaList2.Area)
				{
					return false;
				}
				if (relocationAreaList.RelocationsEx.Count != relocationAreaList2.RelocationsEx.Count)
				{
					return false;
				}
				for (int k = 0; k < relocationAreaList.RelocationsEx.Count; k++)
				{
					IRelocation relocation = relocationAreaList.RelocationsEx[k];
					IRelocation relocation2 = relocationAreaList2.RelocationsEx[k];
					if (relocation.Offset != relocation2.Offset)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x0002FC50 File Offset: 0x0002DE50
		internal static bool \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			return \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_GEN_IMPLICIT_INIT_FUN) || global::\u0016.\u0004.GvlInitFunctions.GetBoolValue(\u0002.GetTargetSettings());
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x0002FC74 File Offset: 0x0002DE74
		internal static _IExpression \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			_3S.CoDeSys.Compiler35220.Tools.Debug.\u0001(\u0003 != null, "sign != null");
			_IExpression iexpression = global::\u0019.\u0003.\u0001(\u0003.OrgName);
			if (!string.IsNullOrEmpty(\u0003.LibraryId))
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(\u0003.LibraryPath);
				if (libraryContext != null)
				{
					IExpression expression = Helper.\u0001(\u0002, libraryContext);
					if (expression != null)
					{
						_ICompoAccessExpression icompoAccessExpression = global::\u0019.\u0003.\u0001();
						icompoAccessExpression._Left = (expression as _IExpression);
						icompoAccessExpression._Right = iexpression;
						iexpression = icompoAccessExpression;
					}
				}
			}
			return iexpression;
		}

		// Token: 0x060010A0 RID: 4256 RVA: 0x0002FCE8 File Offset: 0x0002DEE8
		internal static IVariable \u0001(string \u0002, IEnumerable<IVariable> \u0003)
		{
			foreach (IVariable variable in \u0003)
			{
				_IVariable ivariable = (_IVariable)variable;
				if (ivariable.VersionedName.ToUpperInvariant() == \u0002.ToUpperInvariant())
				{
					return ivariable;
				}
			}
			return null;
		}

		// Token: 0x060010A1 RID: 4257 RVA: 0x0002FD50 File Offset: 0x0002DF50
		internal static bool \u0001(CodegeneratorProperties \u0002, ICodegenerator \u0003)
		{
			ICodegenerator3 codegenerator = \u0003 as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(\u0002);
		}

		// Token: 0x060010A2 RID: 4258 RVA: 0x0002FD70 File Offset: 0x0002DF70
		internal static void \u0001(int \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			Helper.\u0001(0, \u0002, \u0003, \u0004);
		}

		// Token: 0x060010A3 RID: 4259 RVA: 0x0002FD7C File Offset: 0x0002DF7C
		internal static void \u0001(int \u0002, int \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			for (int i = \u0002; i < \u0002 + \u0003; i++)
			{
				if (\u0004[IdentifierConstants.GetImplicitIndexVariable(i)] == null)
				{
					_IVariable ivariable = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001());
					ivariable.Name = IdentifierConstants.GetImplicitIndexVariable(i);
					ivariable._Type = TypeTable.DInt;
					ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.NoInit | VarFlag.Implicit, true);
					if (\u0004.POUType == Operator.Program)
					{
						ivariable.SetFlag(VarFlag.Temp, true);
					}
					if (\u0005 != null && \u0005[ivariable.VersionedName] != null)
					{
						ivariable.Id = \u0005[ivariable.VersionedName].Id;
					}
					else
					{
						ivariable.Id = \u0004.NextId;
					}
					\u0004.AddVariable(ivariable);
				}
			}
		}

		// Token: 0x060010A4 RID: 4260 RVA: 0x0002FE34 File Offset: 0x0002E034
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			if (\u0003[IdentifierConstants.CurrentTaskInfoPointer] == null)
			{
				ISignature signature = \u0002["__TaskSpecificInfo"];
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(signature.Name);
				iuserdefType.SignatureId = signature.Id;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				_IVariable ivariable = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001());
				ivariable.Name = IdentifierConstants.CurrentTaskInfoPointer;
				ivariable._Type = type;
				ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.NoInit | VarFlag.Implicit, true);
				if (\u0003.POUType == Operator.Program)
				{
					ivariable.SetFlag(VarFlag.Temp, true);
				}
				if (((\u0004 != null) ? \u0004[ivariable.VersionedName] : null) != null)
				{
					ivariable.Id = \u0004[ivariable.VersionedName].Id;
				}
				else
				{
					ivariable.Id = \u0003.NextId;
				}
				ivariable.AddAttribute(CompileAttributes.ATTRIBUTE_NOINIT, string.Empty);
				\u0003.AddVariable(ivariable);
			}
		}

		// Token: 0x060010A5 RID: 4261 RVA: 0x0002FF0C File Offset: 0x0002E10C
		internal static IExpression \u0001(_ICompileContext \u0002, _IPreCompileContext \u0003)
		{
			string text = Helper.\u0001(\u0002, \u0003);
			if (text == null)
			{
				return null;
			}
			return new global::\u0011.\u0006(text).\u0002();
		}

		// Token: 0x060010A6 RID: 4262 RVA: 0x0002FF34 File Offset: 0x0002E134
		internal static int \u0001(IExpression \u0002)
		{
			if (\u0002 == null)
			{
				return 0;
			}
			IList<_IExpression> list = null;
			int num = 0;
			if (\u0002 is _IArrayInitialization)
			{
				_IArrayInitialization iarrayInitialization = \u0002 as _IArrayInitialization;
				_IArrayType iarrayType = \u0002.Type as _IArrayType;
				if (iarrayType == null)
				{
					return Helper.\u0001(iarrayInitialization.Type);
				}
				num = Helper.\u0001(iarrayInitialization.Type);
				ICompiledType compiledType = \u0084.\u0004.\u0001(iarrayType);
				if (compiledType == null)
				{
					return 0;
				}
				if (!(compiledType is _IUserdefType))
				{
					return num;
				}
				list = iarrayInitialization._InitValues;
			}
			else
			{
				if (\u0002 is _IMultipleIndexInitialization)
				{
					return Helper.\u0001((\u0002 as _IMultipleIndexInitialization)._Value);
				}
				if (\u0002 is _IAssignmentExpression)
				{
					return Helper.\u0001((\u0002 as _IAssignmentExpression)._RValue);
				}
				if (\u0002 is _IStructureInitialization)
				{
					_IStructureInitialization istructureInitialization = \u0002 as _IStructureInitialization;
					list = new LList<_IExpression>(istructureInitialization._CompoInits.Count);
					using (IEnumerator<_IAssignmentExpression> enumerator = istructureInitialization._CompoInits.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							_IExpression item = enumerator.Current;
							list.Add(item);
						}
						goto IL_F0;
					}
				}
				return 0;
			}
			IL_F0:
			if (list == null)
			{
				return 0;
			}
			int num2 = 0;
			foreach (_IExpression u in list)
			{
				int num3 = Helper.\u0001(u);
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			return num2 + num;
		}

		// Token: 0x060010A7 RID: 4263 RVA: 0x00030090 File Offset: 0x0002E290
		internal static _IExpression \u0001(_IExpression \u0002, ICompiledType \u0003, int \u0004)
		{
			if (\u0003.DeRefType.Class != TypeClass.Array)
			{
				return \u0002;
			}
			_IArrayType iarrayType = \u0003 as _IArrayType;
			IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
			_IIndexAccessExpression iindexAccessExpression = global::\u0019.\u0003.\u0001(\u0002);
			for (int i = 0; i < dimensions.Count; i++)
			{
				iindexAccessExpression.AddAccess(global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(\u0004++)));
			}
			return Helper.\u0001(iindexAccessExpression, iarrayType.BaseType, \u0004);
		}

		// Token: 0x060010A8 RID: 4264 RVA: 0x000300F8 File Offset: 0x0002E2F8
		internal static IList<_IExpression> \u0001(_IExpression \u0002, IScope5 \u0003, ICompiledType \u0004)
		{
			LList<_IExpression> llist = new LList<_IExpression>();
			if (\u0004.DeRefType.Class != TypeClass.Array)
			{
				return llist;
			}
			_IArrayType u = \u0004 as _IArrayType;
			Helper.\u0001(\u0003, u, \u0002, llist);
			return llist;
		}

		// Token: 0x060010A9 RID: 4265 RVA: 0x00030130 File Offset: 0x0002E330
		private static void \u0001(IScope5 \u0002, _IArrayType \u0003, _IExpression \u0004, LList<_IExpression> \u0005)
		{
			IList<_IArrayDimension> dimensions = \u0003._Dimensions;
			LList<_IIndexAccessExpression>[] array = new LList<_IIndexAccessExpression>[dimensions.Count + 1];
			for (int i = 0; i < dimensions.Count + 1; i++)
			{
				array[i] = new LList<_IIndexAccessExpression>();
			}
			array[0].Add(global::\u0019.\u0003.\u0001(\u0004));
			bool flag = false;
			for (int j = 0; j < dimensions.Count; j++)
			{
				bool flag2;
				int num = dimensions[j].LowerBorderInt(out flag2, \u0002);
				int num2 = dimensions[j].UpperBorderInt(out flag2, \u0002);
				for (int k = 0; k < array[j].Count<_IIndexAccessExpression>(); k++)
				{
					for (int l = num; l <= num2; l++)
					{
						_ILiteralExpression expAcc = global::\u0019.\u0003.\u0001((long)l);
						_IIndexAccessExpression iindexAccessExpression = array[j][k].Duplicate() as _IIndexAccessExpression;
						iindexAccessExpression.AddAccess(expAcc);
						array[j + 1].Add(iindexAccessExpression);
						_IArrayType iarrayType = \u0003._Base as _IArrayType;
						if (iarrayType != null)
						{
							Helper.\u0001(\u0002, iarrayType, iindexAccessExpression, \u0005);
							flag = true;
						}
					}
				}
			}
			if (!flag)
			{
				\u0005.AddRange(array[dimensions.Count<_IArrayDimension>()]);
			}
		}

		// Token: 0x060010AA RID: 4266 RVA: 0x00030250 File Offset: 0x0002E450
		internal static _IStatement \u0001(_IArrayType \u0002, _IStatement \u0003, IScope5 \u0004, int \u0005)
		{
			int num = Helper.\u0001(\u0002);
			_IForStatement iforStatement = null;
			for (int i = 0; i < num; i++)
			{
				iforStatement = global::\u0019.\u0003.\u0001();
				int num2 = 0;
				int num3 = 0;
				Helper.\u0001(\u0002, i, \u0004, out num2, out num3);
				_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(IdentifierConstants.GetImplicitIndexVariable(i + \u0005));
				_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(ivariableExpression);
				iassignmentExpression._RValue = global::\u0019.\u0003.\u0001((long)num2, TypeClass.DInt);
				iforStatement._CounterStart = iassignmentExpression;
				TypeClass u = TypeClass.Int;
				if (num3 < -32768 || num3 > 32767)
				{
					u = TypeClass.DInt;
				}
				iforStatement._UpperBound = global::\u0019.\u0003.\u0001((long)num3, u);
				iforStatement._By = global::\u0019.\u0003.\u0001(1L);
				if (!(\u0003 is ISequenceStatement))
				{
					_ISequenceStatement isequenceStatement = global::\u0019.\u0003.\u0001();
					isequenceStatement.Add(\u0003);
					\u0003 = isequenceStatement;
				}
				iforStatement._Controlled = \u0003;
				_IOperatorExpression ioperatorExpression;
				if ((iforStatement._By as _ILiteralExpression).Negative)
				{
					ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Ge);
				}
				else
				{
					ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Le);
				}
				ioperatorExpression.AddOperand(ivariableExpression.Duplicate() as _IExpression);
				ioperatorExpression.AddOperand(iforStatement._UpperBound.Duplicate() as _IExpression);
				iforStatement._Condition = ioperatorExpression;
				_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Add);
				ioperatorExpression2.AddOperand(ivariableExpression.Duplicate() as _IExpression);
				ioperatorExpression2.AddOperand(iforStatement._By.Duplicate() as _IExpression);
				_IAssignmentExpression iassignmentExpression2 = global::\u0019.\u0003.\u0001(ivariableExpression.Duplicate() as _IExpression);
				iassignmentExpression2._RValue = ioperatorExpression2;
				iforStatement._Counter = iassignmentExpression2;
				\u0003 = iforStatement;
			}
			return iforStatement;
		}

		// Token: 0x060010AB RID: 4267 RVA: 0x000303CC File Offset: 0x0002E5CC
		internal static void \u0001(ICompiledType \u0002, LStringBuilder \u0003, int \u0004)
		{
			if (\u0002.DeRefType.Class != TypeClass.Array)
			{
				return;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
			\u0003.Append("[");
			for (int i = 0; i < dimensions.Count; i++)
			{
				if (i > 0)
				{
					\u0003.Append(", ");
				}
				\u0003.Append(IdentifierConstants.GetImplicitIndexVariable(\u0004++));
			}
			\u0003.Append("]");
			Helper.\u0001(iarrayType.BaseType, \u0003, \u0004);
		}

		// Token: 0x060010AC RID: 4268 RVA: 0x00030450 File Offset: 0x0002E650
		internal static void \u0001(_IArrayType \u0002, string \u0003, LStringBuilder \u0004, IScope5 \u0005, int \u0006)
		{
			int num = Helper.\u0001(\u0002);
			for (int i = 0; i < num; i++)
			{
				int num2 = 0;
				int num3 = 0;
				Helper.\u0001(\u0002, i, \u0005, out num2, out num3);
				\u0004.AppendLine(string.Concat(new string[]
				{
					"FOR ",
					IdentifierConstants.GetImplicitIndexVariable(i + \u0006),
					" := ",
					num2.ToString(),
					" TO ",
					num3.ToString(),
					" DO"
				}));
			}
			\u0004.Append(\u0003);
			for (int j = 0; j < num; j++)
			{
				\u0004.AppendLine("END_FOR");
			}
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x000304F8 File Offset: 0x0002E6F8
		internal static int \u0001(ICompiledType \u0002)
		{
			if (\u0002.DeRefType.Class != TypeClass.Array)
			{
				return 0;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			return iarrayType._Dimensions.Count + Helper.\u0001(iarrayType.BaseType);
		}

		// Token: 0x060010AE RID: 4270 RVA: 0x00030534 File Offset: 0x0002E734
		internal static bool \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _IVariable \u0005, _IVariable \u0006)
		{
			if (\u0005.Initial == null && \u0006.Initial == null)
			{
				return true;
			}
			if (\u0005.Initial == null != (\u0006.Initial == null))
			{
				return false;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, \u0004.Id);
			IScope5 scope2 = global::\u0007.\u0005.\u0001(\u0002, \u0004.Id);
			ILiteralValue literalValue = ((_IExpression)\u0005.Initial).Literal(scope, true);
			ILiteralValue literalValue2 = ((_IExpression)\u0006.Initial).Literal(scope2, true);
			if (literalValue == null != (literalValue2 == null))
			{
				return false;
			}
			if (literalValue == null || literalValue2 == null)
			{
				return \u0005.InitialValueEquals(\u0006);
			}
			bool result;
			if (Helper.\u0001(literalValue2, literalValue, out result))
			{
				return result;
			}
			if (literalValue2.KindOf != literalValue.KindOf)
			{
				return false;
			}
			switch (literalValue2.KindOf)
			{
			case KindOfLiteral.SignedInteger:
				return literalValue2.SignedLong == literalValue.SignedLong;
			case KindOfLiteral.UnsignedInteger:
				return literalValue2.UnsignedLong == literalValue.UnsignedLong;
			case KindOfLiteral.Float:
				return literalValue2.Float == literalValue.Float;
			case KindOfLiteral.String:
				return literalValue2.String == literalValue.String;
			case KindOfLiteral.Bool:
				return literalValue2.Bool == literalValue.Bool;
			default:
				return false;
			}
		}

		// Token: 0x060010AF RID: 4271 RVA: 0x0003065C File Offset: 0x0002E85C
		private static bool \u0001(ILiteralValue \u0002, ILiteralValue \u0003, out bool \u0004)
		{
			\u0004 = false;
			ILiteralValue literalValue;
			ILiteralValue literalValue2;
			if (\u0002.KindOf == KindOfLiteral.UnsignedInteger && \u0003.KindOf == KindOfLiteral.SignedInteger)
			{
				literalValue = \u0003;
				literalValue2 = \u0002;
			}
			else
			{
				if (\u0003.KindOf != KindOfLiteral.UnsignedInteger || \u0002.KindOf != KindOfLiteral.SignedInteger)
				{
					return false;
				}
				literalValue = \u0002;
				literalValue2 = \u0003;
			}
			if (literalValue.SignedLong >= 0L)
			{
				\u0004 = (literalValue.SignedLong == (long)literalValue2.UnsignedLong);
			}
			return true;
		}

		// Token: 0x060010B0 RID: 4272 RVA: 0x000306BC File Offset: 0x0002E8BC
		internal static IList<_IVariable> \u0001(_ISignature \u0002)
		{
			LList<_IVariable> llist = new LList<_IVariable>();
			foreach (IVariable variable in \u0002.AllInputs)
			{
				if (!variable.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INPUT))
				{
					llist.Add(variable as _IVariable);
				}
			}
			return llist;
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x00030704 File Offset: 0x0002E904
		public byte[] \u0001(IScope5 \u0002, bool \u0003, IVariable \u0004, out IRelocationList2 \u0005)
		{
			return global::\u0018.\u0008.\u0001(\u0002, \u0003, \u0004, out \u0005);
		}

		// Token: 0x060010B2 RID: 4274 RVA: 0x00030710 File Offset: 0x0002E910
		private static void \u0001(ICompiledType \u0002, int \u0003, IScope5 \u0004, out int \u0005, out int \u0006)
		{
			\u0005 = 0;
			\u0006 = 0;
			if (\u0002.DeRefType.Class != TypeClass.Array)
			{
				return;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			if (\u0003 < iarrayType._Dimensions.Count)
			{
				bool flag = false;
				\u0005 = iarrayType._Dimensions[\u0003].LowerBorderInt(out flag, \u0004);
				\u0006 = iarrayType._Dimensions[\u0003].UpperBorderInt(out flag, \u0004);
				return;
			}
			Helper.\u0001(iarrayType.BaseType, \u0003 - iarrayType._Dimensions.Count, \u0004, out \u0005, out \u0006);
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x00030798 File Offset: 0x0002E998
		public void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ICompileContext \u0005)
		{
			foreach (_ISignature isignature in \u0003.SubSignatures)
			{
				if (isignature.HasAttribute("instancevar"))
				{
					isignature.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, true);
					foreach (_IVariable ivariable in isignature.All)
					{
						if (ivariable.HasAttribute("instancevar"))
						{
							ivariable.SetFlag(VarFlag.Local, false);
							ivariable.SetFlag(VarFlag.AllocateInInstance, true);
						}
					}
				}
			}
		}

		// Token: 0x060010B4 RID: 4276 RVA: 0x0003082C File Offset: 0x0002EA2C
		public bool \u0001(IPragmaStatement \u0002, out string \u0003, out string \u0004)
		{
			\u0003 = null;
			\u0004 = null;
			if (\u0002 == null)
			{
				return false;
			}
			IPragmaScanner pragmaScanner = \u0082.\u0005.Singleton.Create(\u0002.Text);
			IPragmaToken pragmaToken;
			if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.attribute && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
			{
				\u0003 = pragmaToken.String;
				\u0004 = string.Empty;
				if (pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.Operator && pragmaToken.Operator == PragmaOperator.assign && pragmaScanner.GetNext(out pragmaToken) == PragmaTokenType.SingleByteString)
				{
					\u0004 = pragmaToken.String;
				}
				return true;
			}
			return false;
		}

		// Token: 0x060010B5 RID: 4277 RVA: 0x000308B0 File Offset: 0x0002EAB0
		public _ISourcePosition \u0001(int \u0002, Guid \u0003, long \u0004, short \u0005, short \u0006)
		{
			return global::\u0019.\u0003.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x000308C0 File Offset: 0x0002EAC0
		internal static void \u0001(_ICompileContext \u0002, IList<_ISignature> \u0003, SignatureFlagInternal \u0004)
		{
			LHashSet<string> lhashSet = new LHashSet<string>();
			LStack<_ISignature> lstack = new LStack<_ISignature>();
			using (IEnumerator<_ISignature> enumerator = \u0003.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					_ISignature isignature = enumerator.Current;
					if (isignature.GetFlagInternal(\u0004))
					{
						lstack.Push(isignature);
					}
				}
				goto IL_A8;
			}
			IL_40:
			_ISignature isignature2 = lstack.Pop();
			string searchName = isignature2.GetSearchName(\u0002);
			if (lhashSet.Add(searchName))
			{
				isignature2.SetFlagInternal(SignatureFlagInternal.OnlineChangePartialInit, true);
				foreach (int nId in isignature2.DeclarerIds)
				{
					_ISignature isignature3 = \u0002.GetSignatureById(nId) as _ISignature;
					if (isignature3 != null)
					{
						lstack.Push(isignature3);
					}
				}
			}
			IL_A8:
			if (lstack.Count <= 0)
			{
				return;
			}
			goto IL_40;
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x00030990 File Offset: 0x0002EB90
		public static uint \u0001(params _IPreCompileContext[] \u0002)
		{
			ICRCSum icrcsum = APEnvironmentFacade.Instance.LanguageModelMgr.CreateCheckSumComputer();
			byte[] array = new byte[]
			{
				1
			};
			byte[] array2 = new byte[1];
			foreach (_IPreCompileContext ipreCompileContext in \u0002)
			{
				if (ipreCompileContext != null)
				{
					foreach (_ISignature isignature in ipreCompileContext.AllFlat.OrderBy(new Func<_ISignature, string>(Helper.<>c.<>9.\u0001)))
					{
						if (!isignature.GetFlag(SignatureFlag.TimeStampOnly) && !isignature.GetFlag(SignatureFlag.SuperGlobal) && !isignature.GetFlag(SignatureFlag.Generated))
						{
							byte[] bytes = Encoding.Unicode.GetBytes(isignature.Name);
							icrcsum.CRC32Update(bytes, bytes.Length);
							icrcsum.CRC32Update(isignature.GetFlag(SignatureFlag.TopLevel) ? array : array2, 1);
						}
					}
				}
			}
			return icrcsum.CRC32Finish(array2, 0);
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x00030ABC File Offset: 0x0002ECBC
		public uint \u0001(params _IPreCompileContext[] \u0002)
		{
			return Helper.\u0001(\u0002);
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00030AC4 File Offset: 0x0002ECC4
		public static IEnumerable<_IVariable> \u0001(IScope \u0002, _ISignature \u0003)
		{
			if (\u0003.BaseSignatureId == Helper.InvalidId)
			{
				return \u0003.AllVariables;
			}
			return \u0003.AllVariables.Concat(Helper.\u0001(\u0002, (_ISignature)\u0002[\u0003.BaseSignatureId]));
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x00030AFC File Offset: 0x0002ECFC
		public static bool \u0001(IEnumerable<IMessage> \u0002, Severity \u0003)
		{
			using (IEnumerator<IMessage> enumerator = \u0002.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.Severity <= \u0003)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060010BB RID: 4283 RVA: 0x00030B4C File Offset: 0x0002ED4C
		public int \u0001(ISignature \u0002)
		{
			return Helper.\u0001(\u0002 as _ISignature);
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x00030B5C File Offset: 0x0002ED5C
		public IEnumerable<ISourcePosition> \u0001(Guid \u0002, ISignature \u0003)
		{
			return this.\u0001(\u0002, \u0003, EPouScopeFlags.Declaration | EPouScopeFlags.Implementation);
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x00030B68 File Offset: 0x0002ED68
		public IEnumerable<uint> \u0001(ISignature \u0002, Guid \u0003)
		{
			ICompileContext compileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(\u0003);
			if (compileContext == null)
			{
				return Array.Empty<uint>();
			}
			if (\u0002 == null)
			{
				return Array.Empty<uint>();
			}
			return global::\u0003.\u0002.\u0001((_ISignature)\u0002, compileContext);
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x00030BA4 File Offset: 0x0002EDA4
		public IEnumerable<ISourcePosition> \u0001(Guid \u0002, ISignature \u0003, EPouScopeFlags \u0004)
		{
			_ISignature3 isignature = \u0003 as _ISignature3;
			if (isignature == null)
			{
				return Array.Empty<ISourcePosition>();
			}
			List<ISourcePosition> list = new List<ISourcePosition>();
			if ((EPouScopeFlags.Declaration & \u0004) != (EPouScopeFlags)0)
			{
				list.AddRange(isignature.UnusedDeclarationPositions);
			}
			if ((EPouScopeFlags.Implementation & \u0004) != (EPouScopeFlags)0)
			{
				this.\u0001(\u0002, isignature, list);
			}
			return list;
		}

		// Token: 0x060010BF RID: 4287 RVA: 0x00030BE8 File Offset: 0x0002EDE8
		private void \u0001(Guid \u0002, _ISignature3 \u0003, List<ISourcePosition> \u0004)
		{
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(\u0002) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return;
			}
			_IPreCompileContext ipreCompileContext2 = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(\u0003) as _IPreCompileContext;
			if (ipreCompileContext2 == null)
			{
				return;
			}
			if (!\u0003.GetFlagInternal(SignatureFlagInternal.Checked))
			{
				_ICompiledPOU icompiledPOU = ipreCompileContext2.GetCompiledPOU(\u0003.ObjectGuid) as _ICompiledPOU;
				if (icompiledPOU != null)
				{
					ipreCompileContext2.CheckPOUCode(icompiledPOU);
				}
			}
			IStatement statement = new PreCompileTypifier(ipreCompileContext2).CreateTypifiedParseTree(\u0003.ObjectGuid, false);
			if (statement == null)
			{
				return;
			}
			Hashtable hashtable;
			if (\u0002 != Guid.Empty)
			{
				hashtable = new Hashtable(ipreCompileContext.DefineTable);
				this.\u0001(ipreCompileContext, hashtable);
			}
			else
			{
				hashtable = new Hashtable();
				Helper.\u0001(hashtable);
			}
			global::\u0015.\u0002 u = CheckerScope.\u0001(ipreCompileContext.PointerSize, ipreCompileContext._GetLibraryTable(), \u0003, ipreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			if (global::\u0006.\u0003.\u0001(statement as _IExprement, hashtable, ipreCompileContext, u))
			{
				\u0004.AddRange(global::\u0002.\u0005.\u0001(statement));
			}
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x00030CDC File Offset: 0x0002EEDC
		public IExpressionInfo \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004, bool \u0005)
		{
			return global::\u0017.\u0008.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x00030CE8 File Offset: 0x0002EEE8
		public static void \u0001(Hashtable \u0002)
		{
			string compilerDefinesToUse = APEnvironmentFacade.Instance.LMServiceProvider.ConfigurationService.LibraryDevelopmentOptions.CompilerDefinesToUse;
			if (!string.IsNullOrEmpty(compilerDefinesToUse))
			{
				foreach (string text in compilerDefinesToUse.Split(new char[]
				{
					','
				}))
				{
					\u0002[text.Trim()] = string.Empty;
				}
			}
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x00030D4C File Offset: 0x0002EF4C
		public static IDeviceIdentification \u0001(Guid \u0002)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(\u0002);
			if (guid == Guid.Empty)
			{
				guid = \u0002;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x00030D94 File Offset: 0x0002EF94
		private ICodegenerator \u0001(ITargetSettings \u0002)
		{
			ICodegenerator result;
			try
			{
				string stringValue = global::\u0016.\u0004.CodegeneratorGuid.GetStringValue(\u0002);
				Guid typeGuid = new Guid(stringValue);
				ICodegenerator codegenerator = APEnvironmentFacade.Instance.CreateCodegenerator(typeGuid);
				if (codegenerator != null)
				{
					codegenerator.Setup(\u0002);
				}
				result = codegenerator;
			}
			catch (Exception arg)
			{
				Trace.WriteLine(string.Format("Failed to create a codegenerator: {0}", arg));
				result = null;
			}
			return result;
		}

		// Token: 0x060010C4 RID: 4292 RVA: 0x00030DF8 File Offset: 0x0002EFF8
		private void \u0001(_IPreCompileContext \u0002, Hashtable \u0003)
		{
			IDeviceIdentification u = Helper.\u0001(\u0002.ApplicationGuid);
			ITargetSettings targetSettings = \u0002.GetTargetSettings();
			ICodegenerator u2 = this.\u0001(targetSettings);
			bool boolValue = global::\u0016.\u0004.SupportMulticore.GetBoolValue(targetSettings);
			int intValue = global::\u0016.\u0004.PackMode.GetIntValue(targetSettings);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			global::\u000F.\u0019.\u0001(u, u2, intValue, boolValue, \u0002.SimulationMode, dictionary);
			foreach (KeyValuePair<string, string> keyValuePair in dictionary)
			{
				\u0003.Add(keyValuePair.Key, keyValuePair.Value);
			}
		}

		// Token: 0x060010C5 RID: 4293 RVA: 0x00030EA0 File Offset: 0x0002F0A0
		internal static string \u0001(string \u0002, _ISignature \u0003, _ICompileContext \u0004)
		{
			if (\u0004 != null && \u0003.IsLibraryObject && \u0004.LibraryIsUnique(\u0003.LibraryPath))
			{
				return global::\u0014.\u0002.\u0001(\u0003.LibraryPath) + "." + \u0002;
			}
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME))
			{
				return \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME);
			}
			if (\u0003.IsLibraryObject)
			{
				return \u0003.LibraryId + "." + \u0002;
			}
			if (!string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				return \u0003.LibraryPath + "." + \u0002;
			}
			if (\u0003.GetFlag(SignatureFlag.SystemNamespaceForced))
			{
				return "__SYSTEM." + \u0002;
			}
			if (\u0003.GetFlag(SignatureFlag.PoolSignature))
			{
				return "@pool." + \u0002;
			}
			return \u0002;
		}
	}
}
