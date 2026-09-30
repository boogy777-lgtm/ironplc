using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedClass]
	public abstract class CGConstants
	{
		public static readonly int DEVICE_AREA = 0;

		public static readonly int APPLICATION_AREA = 1;

		public static readonly string TRG_MOTOROLABYTEORDER = "codegenerator\\Motorola Byte Order";

		public static readonly string TRG_CESTACKFRAME = "codegenerator\\CE-Stackframe";

		public static readonly string TRG_FPU = "codegenerator\\Floating Point Unit";

		public static readonly string TRG_VECTORUNIT = "codegenerator\\Vector Unit";

		public static readonly string TRG_BREAKPOINT_SIZE = "codegenerator\\Breakpoint Size";

		public static readonly string TRG_RESERVED_REGISTERS = "codegenerator\\reserved-registers";

		public static readonly string TRG_WINDOWS_STACK_ALLOCATION = "codegenerator\\windows-stack-allocation";

		public static readonly string TRG_OPERATING_SYSTEM = "codegenerator\\operating-system";

		public static readonly string TRG_SYSTEMV_CALLING_CONVENTION = "codegenerator\\systemv-calling-convention";

		public static readonly string TRG_HEXFILE = "codegenerator\\hexfile";

		public static readonly string TRG_BYTESUPPORT = "codegenerator\\byte-support";

		public static readonly string TRG_RTS_GLOBALDATAPOINTER_AREA = "codegenerator\\rts-globaldatapointer-area";

		public static readonly string TRG_RTS_OPERATING_SYSTEM = "codegenerator\\rts-operating-system";

		public static readonly string TRG_C_CALLING_CONVENTION = "codegenerator\\c-calling-convention";

		public static readonly string TRG_SSE2_UNIT = "codegenerator\\sse2-unit";

		public static readonly string TRG_AVX_UNIT = "codegenerator\\avx-unit";

		public static readonly string TRG_MISALIGNED_ACCESS = "codegenerator\\misaligned-access";

		public static readonly string TRG_BREAKPOINT_MARK_AFTER_EXTERNAL_CALL = "codegenerator\\breakpoint-mark-after-external-call";

		public static readonly string TRG_ATOMIC_READ_WRITE_64BIT = "codegenerator\\atomic-read-write-64-bit";

		public static readonly string TRG_GENERATE_DIV = "codegenerator\\generate-div";

		public static readonly string TRG_LTICK = "codegenerator\\ltick";

		public static readonly string TRG_LOAD_STORE_REGISTER_EXCLUSIVE = "codegenerator\\load-store-register-exclusive";

		public static readonly string TRG_GENERATE_TRAP_FOR_DIV_BY_ZERO = "codegenerator\\generate-trap-for-div-by-zero";

		public static readonly string TRG_UNALIGNED_MEMORY_ACCESS = "codegenerator\\unaligned-memory-access";

		public static readonly string TRG_LOCK_BIT_WRITE_ACCESS = "codegenerator\\lock-bit-write-access";

		public static readonly string string_to_real64 = "string__to__real64";

		public static readonly string string_to_real32 = "string__to__real32";

		public static readonly string wstring_to_real64 = "wstring__to__real64";

		public static readonly string wstring_to_real32 = "wstring__to__real32";

		public static readonly string real64_to_string = "real64__to__string";

		public static readonly string real64_to_wstring = "real64__to__wstring";

		public static readonly string real32_to_string = "real32__to__string";

		public static readonly string real32_to_wstring = "real32__to__wstring";

		public static readonly string new_real32_to_string = "new__real32__to__string";

		public static readonly string new_real32_to_wstring = "new__real32__to__wstring";

		public static readonly string string_to_any32 = "string__to__any32";

		public static readonly string string_to_any64 = "string__to__any64";

		public static readonly string wstring_to_any32 = "wstring__to__any32";

		public static readonly string wstring_to_any64 = "wstring__to__any64";

		public static readonly string any32_to_string = "any32__to__string";

		public static readonly string any64_to_string = "any64__to__string";

		public static readonly string any32_to_wstring = "any32__to__wstring";

		public static readonly string any64_to_wstring = "any64__to__wstring";

		public static readonly string string_to_wstring = "string__to__wstring";

		public static readonly string wstring_to_string = "wstring__to__string";

		public static readonly string int64_to_any32 = "int64__to__any32";

		public static readonly string any32_to_int64 = "any32__to__int64";

		public static readonly string real32_to_any32 = "real32__to__any32";

		public static readonly string any32_to_real32 = "any32__to__real32";

		public static readonly string real32_to_any64 = "real32__to__any64";

		public static readonly string any64_to_real32 = "any64__to__real32";

		public static readonly string real64_to_any32 = "real64__to__any32";

		public static readonly string any32_to_real64 = "any32__to__real64";

		public static readonly string real64_to_any64 = "real64__to__any64";

		public static readonly string any64_to_real64 = "any64__to__real64";

		public static readonly string real64_to_real32 = "real64__to__real32";

		public static readonly string real32_to_real64 = "real32__to__real64";

		public static string get_time = "get__time";

		public static string get_ltime = "get__ltime";

		public static readonly string real32_eq = "real32__eq";

		public static readonly string real32_ne = "real32__ne";

		public static readonly string real32_lt = "real32__lt";

		public static readonly string real32_le = "real32__le";

		public static readonly string real32_gt = "real32__gt";

		public static readonly string real32_ge = "real32__ge";

		public static readonly string real32_add = "real32__add";

		public static readonly string real32_sub = "real32__sub";

		public static readonly string real32_mul = "real32__mul";

		public static readonly string real32_div = "real32__div";

		public static readonly string real32_min = "real32__min";

		public static readonly string real32_max = "real32__max";

		public static readonly string real32_limit = "real32__limit";

		public static readonly string real32_trunc = "real32__trunc";

		public static readonly string real32_tan = "real32__tan";

		public static readonly string real32_sin = "real32__sin";

		public static readonly string real32_cos = "real32__cos";

		public static readonly string real32_atan = "real32__atan";

		public static readonly string real32_asin = "real32__asin";

		public static readonly string real32_acos = "real32__acos";

		public static readonly string real32_ln = "real32__ln";

		public static readonly string real32_log = "real32__log";

		public static readonly string real32_exp = "real32__exp";

		public static readonly string real32_sqrt = "real32__sqrt";

		public static readonly string real32_abs = "real32__abs";

		public static readonly string real32_expt = "real32__expt";

		public static readonly string int32_div = "int32__div";

		public static readonly string int32_mod = "int32__mod";

		public static readonly string int32_abs = "int32__abs";

		public static readonly string int32_limit = "int32__limit";

		public static readonly string int32_mul = "int32__mul";

		public static readonly string int32_shr = "int32__shr";

		public static readonly string uint32_mod = "uint32__mod";

		public static readonly string uint32_div = "uint32__div";

		public static readonly string uint32_limit = "uint32__limit";

		public static readonly string uint32_mul = "uint32__mul";

		public static readonly string uint32_rol = "uint32__rol";

		public static readonly string uint32_ror = "uint32__ror";

		public static readonly string uint32_shl = "uint32__shl";

		public static readonly string uint32_shr = "uint32__shr";

		public static readonly string real64_add = "real64__add";

		public static readonly string real64_sub = "real64__sub";

		public static readonly string real64_mul = "real64__mul";

		public static readonly string real64_div = "real64__div";

		public static readonly string real64_abs = "real64__abs";

		public static readonly string real64_min = "real64__min";

		public static readonly string real64_max = "real64__max";

		public static readonly string real64_limit = "real64__limit";

		public static readonly string real64_trunc = "real64__trunc";

		public static readonly string real64_tan = "real64__tan";

		public static readonly string real64_sin = "real64__sin";

		public static readonly string real64_cos = "real64__cos";

		public static readonly string real64_atan = "real64__atan";

		public static readonly string real64_asin = "real64__asin";

		public static readonly string real64_acos = "real64__acos";

		public static readonly string real64_ln = "real64__ln";

		public static readonly string real64_log = "real64__log";

		public static readonly string real64_exp = "real64__exp";

		public static readonly string real64_sqrt = "real64__sqrt";

		public static readonly string real64_expt = "real64__expt";

		public static readonly string real64_eq = "real64__eq";

		public static readonly string real64_ne = "real64__ne";

		public static readonly string real64_lt = "real64__lt";

		public static readonly string real64_le = "real64__le";

		public static readonly string real64_gt = "real64__gt";

		public static readonly string real64_ge = "real64__ge";

		public static readonly string int64_add = "int64__add";

		public static readonly string int64_sub = "int64__sub";

		public static readonly string int64_mul = "int64__mul";

		public static readonly string int64_div = "int64__div";

		public static readonly string int64_mod = "int64__mod";

		public static readonly string int64_abs = "int64__abs";

		public static readonly string int64_min = "int64__min";

		public static readonly string int64_max = "int64__max";

		public static readonly string int64_limit = "int64__limit";

		public static readonly string int64_eq = "int64__eq";

		public static readonly string int64_ne = "int64__ne";

		public static readonly string int64_lt = "int64__lt";

		public static readonly string int64_le = "int64__le";

		public static readonly string int64_gt = "int64__gt";

		public static readonly string int64_ge = "int64__ge";

		public static readonly string int64_shr = "int64__shr";

		public static readonly string uint64_add = "uint64__add";

		public static readonly string uint64_sub = "uint64__sub";

		public static readonly string uint64_mul = "uint64__mul";

		public static readonly string uint64_div = "uint64__div";

		public static readonly string uint64_mod = "uint64__mod";

		public static readonly string uint64_min = "uint64__min";

		public static readonly string uint64_max = "uint64__max";

		public static readonly string uint64_limit = "uint64__limit";

		public static readonly string uint64_ror = "uint64__ror";

		public static readonly string uint64_rol = "uint64__rol";

		public static readonly string uint64_shl = "uint64__shl";

		public static readonly string uint64_shr = "uint64__shr";

		public static readonly string uint64_and = "uint64__and";

		public static readonly string uint64_or = "uint64__or";

		public static readonly string uint64_xor = "uint64__xor";

		public static readonly string uint64_not = "uint64__not";

		public static readonly string uint64_eq = "uint64__eq";

		public static readonly string uint64_ne = "uint64__ne";

		public static readonly string uint64_lt = "uint64__lt";

		public static readonly string uint64_le = "uint64__le";

		public static readonly string uint64_gt = "uint64__gt";

		public static readonly string uint64_ge = "uint64__ge";

		public static readonly string exchange_and_add = "exchange__and__add";

		public static readonly string test_and_set = "test__and__set";

		public static readonly string compare_and_swap = "compare__and__swap";
	}
}
