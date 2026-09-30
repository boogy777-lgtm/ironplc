using System;
using System.Collections.Generic;
using \u0007;
using \u0011;
using \u0017;
using \u0018;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0019
{
	// Token: 0x02000061 RID: 97
	internal static class \u0001
	{
		// Token: 0x06000688 RID: 1672 RVA: 0x0000DD6C File Offset: 0x0000BF6C
		public static ChecksumStream \u0001(bool \u0002)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002);
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0000DD7C File Offset: 0x0000BF7C
		public static bool \u0001(IPragmaStatement \u0002, out string \u0003, out string \u0004)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002, out \u0003, out \u0004);
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0000DD8C File Offset: 0x0000BF8C
		public static IExpressionTypifier \u0001(int \u0002, _ICompileContext \u0003, ICompiledType \u0004, bool \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0000DDA4 File Offset: 0x0000BFA4
		public static _ISignature \u0001(string \u0002)
		{
			return ParserHelper.\u0001(\u0002, false);
		}

		// Token: 0x0600068C RID: 1676 RVA: 0x0000DDB0 File Offset: 0x0000BFB0
		public static _IParser \u0001(string \u0002)
		{
			return new global::\u0011.\u0006(\u0002);
		}

		// Token: 0x0600068D RID: 1677 RVA: 0x0000DDB8 File Offset: 0x0000BFB8
		private static void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004, ICompiledType \u0005, bool \u0006, bool \u0007, _ICompiledPOU \u0008)
		{
			CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, \u0007, \u0008);
		}

		// Token: 0x0600068E RID: 1678 RVA: 0x0000DDCC File Offset: 0x0000BFCC
		private static void \u0001(IExprement \u0002, IScope \u0003, _ICompileContext \u0004)
		{
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(\u0003 as IScope5, \u0004, false, null, false);
			((_IExprement)\u0002).Accept(ivisit);
		}

		// Token: 0x0600068F RID: 1679 RVA: 0x0000DDF8 File Offset: 0x0000BFF8
		public static void \u0001(_IExprement \u0002, IScope \u0003, _ICompileContext \u0004, _ICompiledPOU \u0005)
		{
			\u0019.\u0001.\u0001(\u0002, \u0003, \u0004, null, true, false, \u0005);
			\u0018.\u000E.\u0001(\u0002, \u0004);
			\u0019.\u0001.\u0001(\u0002, \u0003, \u0004);
		}

		// Token: 0x06000690 RID: 1680 RVA: 0x0000DE18 File Offset: 0x0000C018
		public static IScope5 \u0001(_ICompileContext \u0002)
		{
			return global::\u0007.\u0005.\u0001(\u0002);
		}

		// Token: 0x06000691 RID: 1681 RVA: 0x0000DE20 File Offset: 0x0000C020
		public static byte[] \u0001(string \u0002, TypeClass \u0003, bool \u0004, int \u0005, StringEncoding \u0006)
		{
			ByteOrder u = \u0004 ? ByteOrder.Motorola : ByteOrder.Intel;
			return global::\u0017.\u0003.Singleton.\u0001(u, \u0003, \u0005, \u0002, \u0006);
		}

		// Token: 0x06000692 RID: 1682 RVA: 0x0000DE48 File Offset: 0x0000C048
		public static bool \u0001(_ICompileContext \u0002, ISignature \u0003, IExprementPosition \u0004, string \u0005, ICompiledType \u0006)
		{
			return CompilerServicesInternal.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x0000DE58 File Offset: 0x0000C058
		internal static uint \u0001(params _IPreCompileContext[] \u0002)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002);
		}

		// Token: 0x06000694 RID: 1684 RVA: 0x0000DE68 File Offset: 0x0000C068
		internal static IList<_ICompilerMessage> \u0001(_ICompileContext \u0002)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002);
		}

		// Token: 0x06000695 RID: 1685 RVA: 0x0000DE78 File Offset: 0x0000C078
		internal static _ISignature \u0001(_ICompileContext \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005, out ISignature[] \u0006, out _IPreCompileContext \u0007)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005, out \u0006, out \u0007);
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x0000DE8C File Offset: 0x0000C08C
		internal static _ICompiledPOU \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, _IPreCompileContext \u0004, _IPreCompileContext \u0005)
		{
			return \u0019.\u0001.\u0001.\u0001(\u0002, \u0003, \u0004, \u0005);
		}

		// Token: 0x040000B5 RID: 181
		private static readonly CompilerServicesInternal \u0001 = new CompilerServicesInternal();

		// Token: 0x040000B6 RID: 182
		private static readonly Helper \u0001 = new Helper();
	}
}
