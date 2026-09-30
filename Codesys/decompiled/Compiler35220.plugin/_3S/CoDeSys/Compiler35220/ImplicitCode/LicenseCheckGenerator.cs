using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using \u0003;
using \u0007;
using \u0011;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003C7 RID: 967
	internal static class LicenseCheckGenerator
	{
		// Token: 0x060036DA RID: 14042 RVA: 0x000DF814 File Offset: 0x000DDA14
		internal static bool \u0001(_ISignature \u0002)
		{
			return \u0002.Name == "__VALIDATELICENSEMETRICS" || \u0002.Name == "__VALIDATELICENSEMETRICSAFTERONLINECHANGE";
		}

		// Token: 0x060036DB RID: 14043 RVA: 0x000DF83C File Offset: 0x000DDA3C
		internal static void \u0001(_ICompileContext \u0002, global::\u0014.\u0001 \u0003, Codegeneration \u0004)
		{
			\u0003.\u0001("CalculateLicenseMetrics");
			ValueTuple<uint, uint> valueTuple = LicenseCheckGenerator.\u0001(\u0002.ApplicationGuid);
			uint item = valueTuple.Item1;
			uint item2 = valueTuple.Item2;
			LicenseCheckGenerator.\u0001(\u0002, \u0004, "__ValidateLicenseMetrics", new Version(3, 5, 19, 0), item, item2);
			LicenseCheckGenerator.\u0001(\u0002, \u0004, "__ValidateLicenseMetricsAfterOnlineChange", new Version(3, 5, 19, 20), item, item2);
			\u0003.\u0004("CalculateLicenseMetrics");
			string u = string.Format("Zeit zur Berechnung der LicenseMetric (CRC 0x{0:x8}): {1} ms", item, 0);
			\u0003.\u0002("CalculateLicenseMetrics", u);
		}

		// Token: 0x060036DC RID: 14044 RVA: 0x000DF8CC File Offset: 0x000DDACC
		private static void \u0001(_ICompileContext \u0002, Codegeneration \u0003, string \u0004, Version \u0005, uint \u0006, uint \u0007)
		{
			_ISignature isignature = (_ISignature)\u0002.GetSignature(\u0004);
			isignature.SetFlag(SignatureFlag.Generated, true);
			_ICompiledPOU icompiledPOU = (_ICompiledPOU)\u0002.GetCompiledPOUById(isignature.Id);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			scope.LocalSignature = isignature;
			LStringBuilder lstringBuilder = new LStringBuilder();
			string text = string.Empty;
			if (Helper.\u0001(\u0002.GetTargetSettings()) >= \u0005)
			{
				text = string.Format("IF NOT ___ValidateLicenseMetrics(5, {0}) THEN ___ValidateLicenseMetrics(4, {1}); END_IF", \u0007, \u0006);
			}
			lstringBuilder.AppendFormat("{{implicit on}}\r\n\t\t{{nobp}}\r\n\t\t{{noflow}}\r\n\t\t\t{0}\r\n\t\t{{implicit off}}", new object[]
			{
				text
			});
			_IStatement istatement = new global::\u0011.\u0006(lstringBuilder.ToString(), true).\u0001();
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.SetFlag(CompiledPOUFlags.TopLevel, true);
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, icompiledPOU);
			istatement.Accept(ivisit);
			TypeCheckerVisitor ivisit2 = new TypeCheckerVisitor(scope, \u0002);
			istatement.Accept(ivisit2);
			icompiledPOU.SetParseTree(istatement);
			icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			\u0003.\u0002(icompiledPOU);
			ushort u = 0;
			int u2 = 0;
			if (MemoryCompiler.\u0003(\u0002.DataManager, ref u, ref u2, \u0002.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, \u0002.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				icompiledPOU.CompiledCode.Location = \u0019.\u0003.\u0001(u, u2);
				return;
			}
			string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
			{
				icompiledPOU.Name,
				icompiledPOU.CompiledCode.CodeSize
			});
			_ICompilerMessage message = \u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
			APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
		}

		// Token: 0x060036DD RID: 14045 RVA: 0x000DFA7C File Offset: 0x000DDC7C
		private static ValueTuple<uint, uint> \u0001(Guid \u0002)
		{
			ILicensedSoftwareMetricInformationProvider2 licensedSoftwareMetricInformationProvider = APEnvironmentFacade.Instance.LicensedSoftwareMetricInformationProvider as ILicensedSoftwareMetricInformationProvider2;
			IEnumerable<ILicensedSoftwareMetricCheckableByGeneratedCode> enumerable;
			if (licensedSoftwareMetricInformationProvider != null)
			{
				enumerable = licensedSoftwareMetricInformationProvider.GetMetricsForApplicationWithVersioned(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0002);
			}
			else
			{
				enumerable = APEnvironmentFacade.Instance.LicensedSoftwareMetricInformationProvider.GetMetricsForApplication(APEnvironmentFacade.Instance.PrimaryProjectHandle, \u0002);
			}
			enumerable = enumerable.Where(new Func<ILicensedSoftwareMetricCheckableByGeneratedCode, bool>(LicenseCheckGenerator.<>c.<>9.\u0001)).OrderBy(new Func<ILicensedSoftwareMetricCheckableByGeneratedCode, int>(LicenseCheckGenerator.<>c.<>9.\u0001));
			ChecksumStream checksumStream = new MyChecksumStream(false);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			ChecksumStream checksumStream2 = new MyChecksumStream(false);
			BinaryWriter binaryWriter2 = new BinaryWriter(checksumStream2);
			foreach (ILicensedSoftwareMetricCheckableByGeneratedCode licensedSoftwareMetricCheckableByGeneratedCode in enumerable)
			{
				binaryWriter2.Write(licensedSoftwareMetricCheckableByGeneratedCode.ProductCode);
				binaryWriter2.Write(licensedSoftwareMetricCheckableByGeneratedCode.Value);
				binaryWriter2.Write(licensedSoftwareMetricCheckableByGeneratedCode.ConversionFactor);
				IVersionedLicensedSoftwareMetricCheckableByGeneratedCode versionedLicensedSoftwareMetricCheckableByGeneratedCode = licensedSoftwareMetricCheckableByGeneratedCode as IVersionedLicensedSoftwareMetricCheckableByGeneratedCode;
				if (versionedLicensedSoftwareMetricCheckableByGeneratedCode != null)
				{
					binaryWriter2.Write(versionedLicensedSoftwareMetricCheckableByGeneratedCode.Version);
					binaryWriter2.Write((int)versionedLicensedSoftwareMetricCheckableByGeneratedCode.Flags);
				}
				else
				{
					binaryWriter2.Write(1);
					binaryWriter2.Write(0);
					binaryWriter.Write(licensedSoftwareMetricCheckableByGeneratedCode.ProductCode);
					binaryWriter.Write(licensedSoftwareMetricCheckableByGeneratedCode.Value);
					binaryWriter.Write(licensedSoftwareMetricCheckableByGeneratedCode.ConversionFactor);
				}
			}
			binaryWriter.Flush();
			checksumStream.Close();
			binaryWriter2.Flush();
			checksumStream2.Close();
			return new ValueTuple<uint, uint>(checksumStream.Checksum, checksumStream2.Checksum);
		}
	}
}
