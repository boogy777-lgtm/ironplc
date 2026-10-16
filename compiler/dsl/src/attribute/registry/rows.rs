//! The declaration list: every attribute the compiler knows, once.
//!
//! One entry makes the typed constant of the row (`PACK_MODE`, whose reader
//! `PACK_MODE.of(&attributes)` gives a `PackMode`) and the place of the row in
//! [`ALL`], the list of all rows without the type of the value. A row cannot
//! exist outside the list, and the list holds no row twice by construction of
//! the guard test of the registry.
//!
//! The rows are what CODESYS documents: where the attribute may stand, what
//! it takes, and what it does. `state: Waiting` says that no part of IronPLC
//! acts on the attribute yet; `state: Honoured` says that a part does.

use super::row::{Family, Info, Row, State};
use super::targets::{Targets, EVERY, TYPES};
use super::value::{
    DisplayMode, Flag, MonitoringEncoding, MonitoringMode, NamingRegion, Number, Optional,
    OptionalText, PackMode, RuleNumbers, SymbolAccess, Text,
};
use crate::attribute::census::TargetKind as T;
use crate::common::VariableType;

/// The set of the kinds named, for a row that stands before those kinds alone.
macro_rules! kinds {
    ($($kind:ident),+ $(,)?) => {
        Targets::of(&[$(T::$kind),+])
    };
}

/// Declares the constant of each row and [`ALL`] from one list of entries.
macro_rules! rows {
    ($(
        $const:ident : $value:ty = {
            name: $name:literal,
            aliases: [$($alias:literal),*],
            targets: $targets:expr,
            family: $family:ident,
            summary: $summary:literal,
            state: $state:ident,
            $(forbids: $forbids:expr,)?
            $(region: $region:literal,)?
        }
    )*) => {
        $(
            #[doc = concat!("The attribute `", $name, "`.")]
            pub const $const: Row<$value> = Row::new(
                $name,
                &[$($alias),*],
                $targets,
                Family::$family,
                $summary,
                State::$state,
            )$(.forbidding($forbids))?$(.with_region($region))?;
        )*

        /// Every row the compiler knows, in the order of the declaration list.
        pub static ALL: &[Info] = &[$($const.info()),*];
    };
}

rows! {
    STRICT: Flag = {
        name: "strict",
        aliases: [],
        targets: kinds![Enumeration],
        family: Enumeration,
        summary: "A strict type test for the enumeration: no arithmetic with its values, no assignment of a value that is not one of them",
        state: Honoured,
    }

    TO_STRING: Flag = {
        name: "to_string",
        aliases: [],
        targets: kinds![Enumeration],
        family: Enumeration,
        summary: "`TO_STRING` of a value of the enumeration gives the name of the value",
        state: Waiting,
    }

    QUALIFIED_ONLY: Flag = {
        name: "qualified_only",
        aliases: [],
        targets: kinds![Enumeration, Variable],
        family: NameScope,
        summary: "A value of the enumeration, or a variable of the global variable list, is addressed only with the name of the type or the list",
        state: Waiting,
    }

    PACK_MODE: PackMode = {
        name: "pack_mode",
        aliases: [],
        targets: kinds![Structure, Union, Array],
        family: Layout,
        summary: "How the members of the structure are aligned in memory",
        state: Honoured,
    }

    SUBSEQUENT: Flag = {
        name: "subsequent",
        aliases: [],
        targets: kinds![Program, Variable],
        family: Layout,
        summary: "The variables of the program or the global variable list are allocated one after another in memory",
        state: Honoured,
        forbids: &[VariableType::VarTemp],
    }

    HIDE: Flag = {
        name: "hide",
        aliases: [],
        targets: EVERY,
        family: Visibility,
        summary: "The declaration is not shown in monitoring, debugging, input assistance and the symbol configuration; it stays usable by name",
        state: Waiting,
    }

    HIDE_ALL_LOCALS: Flag = {
        name: "hide_all_locals",
        aliases: [],
        targets: kinds![Function, FunctionBlock, Program, Method, Property],
        family: Visibility,
        summary: "Every local variable of the block is hidden as by `hide`",
        state: Waiting,
    }

    CONDITIONALSHOW: OptionalText = {
        name: "conditionalshow",
        aliases: [],
        targets: EVERY,
        family: Visibility,
        summary: "The declaration of a compiled library is hidden unless the library is opened with the matching option",
        state: Waiting,
    }

    CONDITIONALSHOW_ALL_LOCALS: OptionalText = {
        name: "conditionalshow_all_locals",
        aliases: [],
        targets: kinds![Function, FunctionBlock, Program, Method, Property],
        family: Visibility,
        summary: "Every local variable of the library block is hidden as by `conditionalshow`",
        state: Waiting,
    }

    DISPLAYMODE: DisplayMode = {
        name: "displaymode",
        aliases: [],
        targets: kinds![Variable, Member],
        family: Display,
        summary: "The number format of the value while monitoring",
        state: Waiting,
    }

    MONITORING: MonitoringMode = {
        name: "monitoring",
        aliases: [],
        targets: kinds![Property],
        family: Display,
        summary: "How the value of the property is read while monitoring",
        state: Waiting,
    }

    MONITORING_DISPLAY: Text = {
        name: "monitoring_display",
        aliases: [],
        targets: kinds![FunctionBlock, Structure],
        family: Display,
        summary: "The member whose value is shown in the top line of a monitored instance",
        state: Waiting,
    }

    MONITORING_ENCODING: MonitoringEncoding = {
        name: "monitoring_encoding",
        aliases: [],
        targets: kinds![Variable, Member],
        family: Display,
        summary: "The string value is decoded as UTF-8 while monitoring",
        state: Waiting,
    }

    OBSOLETE: Text = {
        name: "obsolete",
        aliases: [],
        targets: EVERY,
        family: Messages,
        summary: "A warning with the given text at each use of the declaration",
        state: Waiting,
    }

    SUPPRESS_WARNING: Text = {
        name: "suppress_warning",
        aliases: [],
        targets: TYPES.and(&[T::Function, T::FunctionBlock, T::Program, T::Interface, T::Method, T::Property, T::Action]),
        family: Messages,
        summary: "The listed compiler warnings are not reported for the declaration",
        state: Waiting,
    }

    NO_ASSIGN_WARNING: Flag = {
        name: "no_assign_warning",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: Messages,
        summary: "Assigning an instance of the block to another instance is a warning",
        state: Waiting,
    }

    ESTIMATED_STACK_USAGE: Number = {
        name: "estimated-stack-usage",
        aliases: [],
        targets: kinds![Method, Function],
        family: Messages,
        summary: "The stack size in bytes that the stack check assumes for a recursive call",
        state: Waiting,
    }

    NO_ASSIGN: Flag = {
        name: "no_assign",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: CompileChecks,
        summary: "Assigning an instance of the block to another instance is an error",
        state: Waiting,
    }

    NO_INSTANCE_IN_RETAIN: Flag = {
        name: "no_instance_in_retain",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: CompileChecks,
        summary: "An instance of the block in retain memory is an error",
        state: Waiting,
    }

    ENABLE_DYNAMIC_CREATION: Flag = {
        name: "enable_dynamic_creation",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: CompileChecks,
        summary: "`__NEW` may create an instance of the block",
        state: Waiting,
    }

    NAMING: NamingRegion = {
        name: "naming",
        aliases: [],
        targets: EVERY,
        family: StaticAnalysis,
        summary: "`off` and `on` bracket the declarations that are left out of the naming-convention check, and `omit` leaves out the next line only; IronPLC has no naming-convention check",
        state: Waiting,
        region: true,
    }

    ANALYSIS: RuleNumbers = {
        name: "analysis",
        aliases: [],
        targets: EVERY,
        family: StaticAnalysis,
        summary: "The listed rules of the static analysis are not checked for the declaration",
        state: Waiting,
    }

    NOINIT: Flag = {
        name: "noinit",
        aliases: ["no_init", "no-init"],
        targets: kinds![Variable, Member],
        family: Initialisation,
        summary: "The variable keeps its value when the application is reset",
        state: Waiting,
    }

    INITIALIZE_ON_CALL: Flag = {
        name: "initialize_on_call",
        aliases: [],
        targets: kinds![FunctionBlock, Variable],
        family: Initialisation,
        summary: "The input variables of the block are initialised on each call",
        state: Waiting,
    }

    GLOBAL_INIT_SLOT: Number = {
        name: "global_init_slot",
        aliases: [],
        targets: kinds![Variable, Function, FunctionBlock, Program],
        family: Initialisation,
        summary: "The place of the block or the global variable list in the order of initialisation",
        state: Waiting,
    }

    CALL_AFTER_INIT: Flag = {
        name: "call_after_init",
        aliases: [],
        targets: kinds![FunctionBlock, Method],
        family: Initialisation,
        summary: "The method runs after `FB_Init` and after the initial values are assigned",
        state: Waiting,
    }

    CALL_AFTER_GLOBAL_INIT_SLOT: Number = {
        name: "call_after_global_init_slot",
        aliases: [],
        targets: kinds![Function, Program, Method],
        family: Initialisation,
        summary: "The function runs after the global initialisation, in the order of the slot",
        state: Waiting,
    }

    CALL_BEFORE_GLOBAL_EXIT_SLOT: Number = {
        name: "call_before_global_exit_slot",
        aliases: [],
        targets: kinds![Function, Program, Method],
        family: Initialisation,
        summary: "The function runs before the global exit, in the order of the slot",
        state: Waiting,
    }

    NO_EXIT: Flag = {
        name: "no-exit",
        aliases: [],
        targets: kinds![Variable],
        family: Initialisation,
        summary: "`FB_Exit` is not called for this instance",
        state: Waiting,
    }

    INIT_NAMESPACE: Flag = {
        name: "init_namespace",
        aliases: [],
        targets: kinds![Variable],
        family: Initialisation,
        summary: "The string variable is initialised with the namespace of the library",
        state: Waiting,
    }

    INSTANCE_PATH: Flag = {
        name: "instance-path",
        aliases: [],
        targets: kinds![Variable],
        family: Initialisation,
        summary: "The string variable is initialised with the path of the instance",
        state: Waiting,
    }

    IS_CONNECTED: Text = {
        name: "is_connected",
        aliases: [],
        targets: kinds![Variable],
        family: Initialisation,
        summary: "The Boolean variable tells whether the named input was assigned in the call",
        state: Waiting,
    }

    REFLECTION: Flag = {
        name: "reflection",
        aliases: [],
        targets: kinds![Function, FunctionBlock, Program],
        family: Initialisation,
        summary: "The compiler looks in the block for `instance-path` and `is_connected` variables",
        state: Waiting,
    }

    INIT_ON_ONLCHANGE: Flag = {
        name: "init_on_onlchange",
        aliases: [],
        targets: kinds![Variable],
        family: OnlineChange,
        summary: "The variable is initialised by each online change",
        state: Waiting,
    }

    NO_COPY: Flag = {
        name: "no_copy",
        aliases: [],
        targets: kinds![Variable],
        family: OnlineChange,
        summary: "An online change initialises the variable and does not copy its value",
        state: Waiting,
    }

    CALL_AFTER_ONLINE_CHANGE_SLOT: Number = {
        name: "call_after_online_change_slot",
        aliases: [],
        targets: kinds![Function, Program, Method],
        family: OnlineChange,
        summary: "The function runs after an online change, in the order of the slot",
        state: Waiting,
    }

    CALL_ON_TYPE_CHANGE: Text = {
        name: "call_on_type_change",
        aliases: [],
        targets: kinds![Method],
        family: OnlineChange,
        summary: "The method runs when the type of one of the named function blocks changes",
        state: Waiting,
    }

    LINKALWAYS: Flag = {
        name: "linkalways",
        aliases: [],
        targets: TYPES.and(&[T::Function, T::FunctionBlock, T::Program, T::Interface, T::Method, T::Variable]),
        family: CodeGeneration,
        summary: "The declaration is compiled and downloaded even when nothing uses it",
        state: Waiting,
    }

    NO_CHECK: Flag = {
        name: "no_check",
        aliases: [],
        targets: kinds![Function, FunctionBlock, Program, Method, Property],
        family: CodeGeneration,
        summary: "The implicit check functions are not called in the block",
        state: Waiting,
    }

    CONST_REPLACED: Flag = {
        name: "const_replaced",
        aliases: [],
        targets: kinds![Variable],
        family: CodeGeneration,
        summary: "The constant is replaced by its value in the code",
        state: Waiting,
    }

    CONST_NON_REPLACED: Flag = {
        name: "const_non_replaced",
        aliases: [],
        targets: kinds![Variable],
        family: CodeGeneration,
        summary: "The constant is not replaced by its value and stays a variable",
        state: Waiting,
    }

    NO_VIRTUAL_ACTIONS: Flag = {
        name: "no_virtual_actions",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: Inheritance,
        summary: "The actions of the SFC block are not overridden by a block that extends it",
        state: Waiting,
    }

    SYMBOL: Optional<SymbolAccess> = {
        name: "symbol",
        aliases: [],
        targets: kinds![Variable, Member, Program, FunctionBlock],
        family: Symbols,
        summary: "Whether the variable is exported as a symbol and with which access",
        state: Waiting,
    }

    IO_FUNCTION_BLOCK: Flag = {
        name: "io_function_block",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: IoMapping,
        summary: "The block is offered for a channel in the device configuration",
        state: Waiting,
    }

    IO_FUNCTION_BLOCK_MAPPING: Flag = {
        name: "io_function_block_mapping",
        aliases: [],
        targets: kinds![Variable],
        family: IoMapping,
        summary: "The parameter that is connected to the channel when the block is mapped",
        state: Waiting,
    }

    PINGROUP: Text = {
        name: "pingroup",
        aliases: [],
        targets: kinds![Variable],
        family: GraphicalEditors,
        summary: "The group of pins the parameter belongs to in FBD and LD",
        state: Waiting,
    }

    PIN_PRESENTATION_ORDER_INPUTS: Text = {
        name: "pin_presentation_order_inputs",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: GraphicalEditors,
        summary: "The order of the input pins of the block in CFC, FBD and LD",
        state: Waiting,
    }

    PIN_PRESENTATION_ORDER_OUTPUTS: Text = {
        name: "pin_presentation_order_outputs",
        aliases: [],
        targets: kinds![FunctionBlock],
        family: GraphicalEditors,
        summary: "The order of the output pins of the block in CFC, FBD and LD",
        state: Waiting,
    }

    DATAFLOW: Flag = {
        name: "dataflow",
        aliases: [],
        targets: kinds![Variable],
        family: GraphicalEditors,
        summary: "The input and the output through which the data flows in FBD, LD and IL",
        state: Waiting,
    }

    PROCESS_VALUE: Flag = {
        name: "ProcessValue",
        aliases: [],
        targets: kinds![Member],
        family: GraphicalEditors,
        summary: "The member is offered by the CFC command that uses attributed members as inputs",
        state: Waiting,
    }

    EXPAND_FULLY: Flag = {
        name: "ExpandFully",
        aliases: [],
        targets: kinds![Variable, Member],
        family: GraphicalEditors,
        summary: "The members of the array are shown in the properties of a visualisation",
        state: Waiting,
    }
}
