//! Transformation from PLCopen XML schema to IronPLC DSL
//!
//! This module transforms parsed PLCopen XML structures into
//! IronPLC's internal DSL representation.

use ironplc_dsl::{
    common::{
        next_block_id, ArrayBounds, ArrayDeclaration, ArrayElementType, ArraySubranges, Attributes,
        Boolean, BooleanLiteral, ConstantKind, DataTypeDeclarationKind, DeclarationQualifier,
        ElementaryTypeName, EnumeratedSpecificationInit, EnumeratedSpecificationValues,
        EnumeratedValue, EnumerationDeclaration, FunctionBlockBodyKind, FunctionBlockDeclaration,
        FunctionDeclaration, FunctionReturnType, InitialValueAssignmentKind, Integer, Library,
        LibraryElementKind, ProgramDeclaration, SignedInteger, SignedIntegerRef, SimpleDeclaration,
        SimpleInitializer, SpecificationKind, StructureDeclaration, StructureElementDeclaration,
        Subrange, SubrangeDeclaration, SubrangeSpecification, TypeName, VarDecl,
        VariableIdentifier, VariableType,
    },
    configuration::{
        ConfigurationDeclaration, ProgramConfiguration, ResourceDeclaration, TaskConfiguration,
    },
    core::{FileId, Id, SourceSpan},
    diagnostic::{Diagnostic, Label},
    textual::{ExprKind, Statements, StmtKind},
    time::DurationLiteral,
};
use ironplc_parser::options::CompilerOptions;
use ironplc_problems::text;

mod sfc;

use sfc::transform_sfc_body;

use super::schema::{
    ArrayType, Configuration, DataType, DataTypeDecl, Dimension, EnumType, Instances, Interface,
    LocatedString, Pou, PouInstance, PouType, Project, Resource, StructType, SubrangeSigned,
    SubrangeUnsigned, Task, VarList, Variable,
};

/// Create an Id from a LocatedString with source position information.
fn make_id(located: &LocatedString, file_id: &FileId) -> Id {
    Id::from(located.as_str()).with_position(
        SourceSpan::range(located.range.start, located.range.end).with_file_id(file_id),
    )
}

/// Create a TypeName from a LocatedString with source position information.
fn make_type_name(located: &LocatedString, file_id: &FileId) -> TypeName {
    TypeName::from_id(&make_id(located, file_id))
}

/// Create a SourceSpan for the given file with no position info
fn file_span(file_id: &FileId) -> SourceSpan {
    SourceSpan::range(0, 0).with_file_id(file_id)
}

/// Transform a parsed PLCopen XML project into an IronPLC Library
///
/// This is the main entry point for XML -> DSL transformation.
pub fn transform_project(
    project: &Project,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<Library, Diagnostic> {
    let mut library = Library::new();

    // Transform data types
    for dt in &project.types.data_types.data_type {
        let decl = transform_data_type_decl(dt, file_id)?;
        library
            .elements
            .push(LibraryElementKind::DataTypeDeclaration(decl));
    }

    // Transform POUs
    for pou in &project.types.pous.pou {
        let elem = transform_pou(pou, file_id, compiler_options)?;
        library.elements.push(elem);
    }

    // Transform instances (configurations)
    if let Some(ref instances) = project.instances {
        let configs = transform_instances(instances, file_id)?;
        for config in configs {
            library
                .elements
                .push(LibraryElementKind::ConfigurationDeclaration(config));
        }
    }

    Ok(library)
}

/// Transform a data type declaration
fn transform_data_type_decl(
    decl: &DataTypeDecl,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let type_name = make_type_name(&decl.name, file_id);

    match &decl.base_type {
        DataType::Enum(enum_type) => transform_enum_decl(&type_name, enum_type, file_id),
        DataType::Array(array_type) => transform_array_decl(&type_name, array_type, file_id),
        DataType::Struct(struct_type) => transform_struct_decl(&type_name, struct_type, file_id),
        DataType::SubrangeSigned(subrange) => {
            transform_subrange_signed_decl(&type_name, subrange, file_id)
        }
        DataType::SubrangeUnsigned(subrange) => {
            transform_subrange_unsigned_decl(&type_name, subrange, file_id)
        }
        // Elementary types as type aliases
        _ => {
            let base_type_name = transform_data_type(&decl.base_type, file_id)?;
            Ok(DataTypeDeclarationKind::Simple(SimpleDeclaration {
                type_name,
                spec_and_init: InitialValueAssignmentKind::Simple(SimpleInitializer {
                    type_name: base_type_name,
                    initial_value: None,
                }),
                attributes: Attributes::default(),
            }))
        }
    }
}

/// Transform an enumeration declaration
fn transform_enum_decl(
    type_name: &TypeName,
    enum_type: &EnumType,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let values: Vec<EnumeratedValue> = enum_type
        .values
        .value
        .iter()
        .map(|v| EnumeratedValue {
            type_name: Some(type_name.clone()),
            value: make_id(&v.name, file_id),
            explicit_value: None,
        })
        .collect();

    let spec = SpecificationKind::Inline(EnumeratedSpecificationValues { values });

    Ok(DataTypeDeclarationKind::Enumeration(
        EnumerationDeclaration {
            type_name: type_name.clone(),
            spec_init: EnumeratedSpecificationInit {
                spec,
                default: None,
                underlying_type: None,
            },
            attributes: Attributes::default(),
        },
    ))
}

/// Transform an array declaration
fn transform_array_decl(
    type_name: &TypeName,
    array_type: &ArrayType,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let base_type_name = transform_data_type(&array_type.base_type, file_id)?;
    let subranges = transform_dimensions(&array_type.dimension, file_id)?;

    Ok(DataTypeDeclarationKind::Array(ArrayDeclaration {
        type_name: type_name.clone(),
        spec: SpecificationKind::Inline(ArraySubranges {
            bounds: ArrayBounds::Ranges(subranges),
            type_name: ArrayElementType::Named(base_type_name),
            ref_to: None,
        }),
        init: vec![],
        attributes: Attributes::default(),
    }))
}

/// Transform array dimensions to subranges
fn transform_dimensions(
    dimensions: &[Dimension],
    file_id: &FileId,
) -> Result<Vec<Subrange>, Diagnostic> {
    dimensions
        .iter()
        .map(|dim| {
            let span = file_span(file_id);
            let lower = dim
                .lower
                .parse::<i128>()
                .map_err(|_| invalid_value_error(&dim.lower, "array lower bound", file_id))?;
            let upper = dim
                .upper
                .parse::<i128>()
                .map_err(|_| invalid_value_error(&dim.upper, "array upper bound", file_id))?;

            Ok(Subrange {
                start: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer {
                        span: span.clone(),
                        value: lower.unsigned_abs(),
                    },
                    is_neg: lower < 0,
                }),
                end: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer {
                        span,
                        value: upper.unsigned_abs(),
                    },
                    is_neg: upper < 0,
                }),
            })
        })
        .collect()
}

/// Transform a structure declaration
fn transform_struct_decl(
    type_name: &TypeName,
    struct_type: &StructType,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let elements: Result<Vec<_>, _> = struct_type
        .variable
        .iter()
        .map(|member| {
            let member_type = transform_data_type(&member.member_type, file_id)?;
            Ok(StructureElementDeclaration {
                name: make_id(&member.name, file_id),
                init: InitialValueAssignmentKind::Simple(SimpleInitializer {
                    type_name: member_type,
                    initial_value: None,
                }),
                attributes: Attributes::default(),
            })
        })
        .collect();

    Ok(DataTypeDeclarationKind::Structure(StructureDeclaration {
        type_name: type_name.clone(),
        elements: elements?,
        attributes: Attributes::default(),
    }))
}

/// Transform a signed subrange declaration
fn transform_subrange_signed_decl(
    type_name: &TypeName,
    subrange: &SubrangeSigned,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let base_type = transform_base_type_to_elementary(&subrange.base_type, file_id)?;
    let span = file_span(file_id);

    let lower = subrange
        .lower
        .parse::<i128>()
        .map_err(|_| invalid_value_error(&subrange.lower, "subrange lower bound", file_id))?;
    let upper = subrange
        .upper
        .parse::<i128>()
        .map_err(|_| invalid_value_error(&subrange.upper, "subrange upper bound", file_id))?;

    Ok(DataTypeDeclarationKind::Subrange(SubrangeDeclaration {
        type_name: type_name.clone(),
        spec: SpecificationKind::Inline(SubrangeSpecification {
            type_name: base_type,
            subrange: Subrange {
                start: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer {
                        span: span.clone(),
                        value: lower.unsigned_abs(),
                    },
                    is_neg: lower < 0,
                }),
                end: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer {
                        span,
                        value: upper.unsigned_abs(),
                    },
                    is_neg: upper < 0,
                }),
            },
        }),
        default: None,
        attributes: Attributes::default(),
    }))
}

/// Transform an unsigned subrange declaration
fn transform_subrange_unsigned_decl(
    type_name: &TypeName,
    subrange: &SubrangeUnsigned,
    file_id: &FileId,
) -> Result<DataTypeDeclarationKind, Diagnostic> {
    let base_type = transform_base_type_to_elementary(&subrange.base_type, file_id)?;
    let span = file_span(file_id);

    let lower = subrange
        .lower
        .parse::<u128>()
        .map_err(|_| invalid_value_error(&subrange.lower, "subrange lower bound", file_id))?;
    let upper = subrange
        .upper
        .parse::<u128>()
        .map_err(|_| invalid_value_error(&subrange.upper, "subrange upper bound", file_id))?;

    Ok(DataTypeDeclarationKind::Subrange(SubrangeDeclaration {
        type_name: type_name.clone(),
        spec: SpecificationKind::Inline(SubrangeSpecification {
            type_name: base_type,
            subrange: Subrange {
                start: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer {
                        span: span.clone(),
                        value: lower,
                    },
                    is_neg: false,
                }),
                end: SignedIntegerRef::Literal(SignedInteger {
                    value: Integer { span, value: upper },
                    is_neg: false,
                }),
            },
        }),
        default: None,
        attributes: Attributes::default(),
    }))
}

/// Transform a DataType to an ElementaryTypeName for subranges
fn transform_base_type_to_elementary(
    data_type: &DataType,
    file_id: &FileId,
) -> Result<ElementaryTypeName, Diagnostic> {
    let span = file_span(file_id);
    match data_type {
        DataType::SInt => Ok(ElementaryTypeName::SINT),
        DataType::Int => Ok(ElementaryTypeName::INT),
        DataType::DInt => Ok(ElementaryTypeName::DINT),
        DataType::LInt => Ok(ElementaryTypeName::LINT),
        DataType::USInt => Ok(ElementaryTypeName::USINT),
        DataType::UInt => Ok(ElementaryTypeName::UINT),
        DataType::UDInt => Ok(ElementaryTypeName::UDINT),
        DataType::ULInt => Ok(ElementaryTypeName::ULINT),
        _ => Err(Diagnostic::text(
            span,
            text::xml_schema_violation_subrange_base_type(data_type.type_name()),
        )),
    }
}

/// Transform a PLCopen DataType to a DSL TypeName
fn transform_data_type(data_type: &DataType, file_id: &FileId) -> Result<TypeName, Diagnostic> {
    match data_type {
        // Elementary types
        DataType::Bool => Ok(ElementaryTypeName::BOOL.into()),
        DataType::Byte => Ok(ElementaryTypeName::BYTE.into()),
        DataType::Word => Ok(ElementaryTypeName::WORD.into()),
        DataType::DWord => Ok(ElementaryTypeName::DWORD.into()),
        DataType::LWord => Ok(ElementaryTypeName::LWORD.into()),
        DataType::SInt => Ok(ElementaryTypeName::SINT.into()),
        DataType::Int => Ok(ElementaryTypeName::INT.into()),
        DataType::DInt => Ok(ElementaryTypeName::DINT.into()),
        DataType::LInt => Ok(ElementaryTypeName::LINT.into()),
        DataType::USInt => Ok(ElementaryTypeName::USINT.into()),
        DataType::UInt => Ok(ElementaryTypeName::UINT.into()),
        DataType::UDInt => Ok(ElementaryTypeName::UDINT.into()),
        DataType::ULInt => Ok(ElementaryTypeName::ULINT.into()),
        DataType::Real => Ok(ElementaryTypeName::REAL.into()),
        DataType::LReal => Ok(ElementaryTypeName::LREAL.into()),
        DataType::Time => Ok(ElementaryTypeName::TIME.into()),
        DataType::Date => Ok(ElementaryTypeName::DATE.into()),
        DataType::DateAndTime => Ok(ElementaryTypeName::DateAndTime.into()),
        DataType::TimeOfDay => Ok(ElementaryTypeName::TimeOfDay.into()),
        DataType::String { .. } => Ok(ElementaryTypeName::STRING.into()),
        DataType::WString { .. } => Ok(ElementaryTypeName::WSTRING.into()),

        // Derived type (reference to another type)
        DataType::Derived(derived) => Ok(make_type_name(&derived.name, file_id)),

        // Complex types that need context
        DataType::Array(_) => Err(unsupported_data_type(file_id, "Array data type")),
        DataType::Enum(_) => Err(unsupported_data_type(file_id, "Enumeration data type")),
        DataType::Struct(_) => Err(unsupported_data_type(file_id, "Structure data type")),

        // Generic types (usually for library functions)
        DataType::Any => Ok(TypeName::from("ANY")),
        DataType::AnyDerived => Ok(TypeName::from("ANY_DERIVED")),
        DataType::AnyElementary => Ok(TypeName::from("ANY_ELEMENTARY")),
        DataType::AnyMagnitude => Ok(TypeName::from("ANY_MAGNITUDE")),
        DataType::AnyNum => Ok(TypeName::from("ANY_NUM")),
        DataType::AnyReal => Ok(TypeName::from("ANY_REAL")),
        DataType::AnyInt => Ok(TypeName::from("ANY_INT")),
        DataType::AnyBit => Ok(TypeName::from("ANY_BIT")),
        DataType::AnyString => Ok(TypeName::from("ANY_STRING")),
        DataType::AnyDate => Ok(TypeName::from("ANY_DATE")),

        // Subranges and pointers
        DataType::SubrangeSigned(_) | DataType::SubrangeUnsigned(_) => {
            Err(unsupported_data_type(file_id, "Subrange data type"))
        }
        DataType::Pointer(_) => Err(unsupported_data_type(file_id, "Pointer data type")),
    }
}

/// Builds the P9999 for a PLCopen XML data type the importer does not
/// transform yet.
///
/// The XML model records no position for a data type node, so the label
/// names the file rather than a span within it.
#[track_caller]
fn unsupported_data_type(file_id: &FileId, what: &str) -> Diagnostic {
    Diagnostic::not_implemented(Label::file(file_id.clone(), what))
}

/// Transform a POU (Program Organization Unit)
fn transform_pou(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<LibraryElementKind, Diagnostic> {
    match pou.pou_type {
        PouType::Function => transform_function(pou, file_id, compiler_options),
        PouType::FunctionBlock => transform_function_block(pou, file_id, compiler_options),
        PouType::Program => transform_program(pou, file_id, compiler_options),
    }
}

/// Transform a function declaration
fn transform_function(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<LibraryElementKind, Diagnostic> {
    let name = make_id(&pou.name, file_id);
    let span = file_span(file_id);

    // Get return type (required for functions)
    let return_type = if let Some(ref interface) = pou.interface {
        if let Some(ref rt) = interface.return_type {
            FunctionReturnType::Named(transform_data_type(rt, file_id)?)
        } else {
            // Functions must have a return type
            return Err(Diagnostic::text(
                span,
                text::xml_schema_violation_function_missing_return_type(pou.name.as_str()),
            ));
        }
    } else {
        return Err(Diagnostic::text(
            span,
            text::xml_schema_violation_function_missing_interface(pou.name.as_str()),
        ));
    };

    let variables = transform_interface(pou.interface.as_ref(), file_id)?;
    let body = transform_body_statements(pou, file_id, compiler_options)?;

    Ok(LibraryElementKind::FunctionDeclaration(
        FunctionDeclaration {
            name,
            return_type,
            variables,
            body,
            attributes: Attributes::default(),
        },
    ))
}

/// Transform a function block declaration
fn transform_function_block(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<LibraryElementKind, Diagnostic> {
    let name = make_type_name(&pou.name, file_id);
    let span = file_span(file_id);

    let variables = transform_interface(pou.interface.as_ref(), file_id)?;
    let body = transform_body(pou, file_id, compiler_options)?;

    Ok(LibraryElementKind::FunctionBlockDeclaration(
        FunctionBlockDeclaration {
            name,
            variables,
            body,
            span,
            // PLCopen XML has no EXTENDS/IMPLEMENTS/ABSTRACT concept.
            oop: None,
            // TwinCAT's .TcPOU XML stores each method as a separate
            // <Method> element, not nested under the POU body handled
            // here. Wiring those into `methods` is a follow-up slice.
            methods: vec![],
            properties: vec![],
            attributes: Attributes::default(),
        },
    ))
}

/// Transform a program declaration
fn transform_program(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<LibraryElementKind, Diagnostic> {
    let name = make_id(&pou.name, file_id);

    let variables = transform_interface(pou.interface.as_ref(), file_id)?;
    let body = transform_body(pou, file_id, compiler_options)?;

    Ok(LibraryElementKind::ProgramDeclaration(ProgramDeclaration {
        name,
        variables,
        access_variables: vec![],
        body,
        attributes: Attributes::default(),
    }))
}

/// Transform a POU interface (variables)
fn transform_interface(
    interface: Option<&Interface>,
    file_id: &FileId,
) -> Result<Vec<VarDecl>, Diagnostic> {
    let Some(interface) = interface else {
        return Ok(vec![]);
    };

    let mut vars = vec![];

    // Input variables
    for var_list in &interface.input_vars {
        vars.extend(transform_var_list(var_list, VariableType::Input, file_id)?);
    }

    // Output variables
    for var_list in &interface.output_vars {
        vars.extend(transform_var_list(var_list, VariableType::Output, file_id)?);
    }

    // In/Out variables
    for var_list in &interface.in_out_vars {
        vars.extend(transform_var_list(var_list, VariableType::InOut, file_id)?);
    }

    // Local variables
    for var_list in &interface.local_vars {
        vars.extend(transform_var_list(var_list, VariableType::Var, file_id)?);
    }

    // Temp variables
    for var_list in &interface.temp_vars {
        vars.extend(transform_var_list(
            var_list,
            VariableType::VarTemp,
            file_id,
        )?);
    }

    // External variables
    for var_list in &interface.external_vars {
        vars.extend(transform_var_list(
            var_list,
            VariableType::External,
            file_id,
        )?);
    }

    // Global variables (at POU level)
    for var_list in &interface.global_vars {
        vars.extend(transform_var_list(var_list, VariableType::Global, file_id)?);
    }

    Ok(vars)
}

/// Transform a variable list
fn transform_var_list(
    var_list: &VarList,
    var_type: VariableType,
    file_id: &FileId,
) -> Result<Vec<VarDecl>, Diagnostic> {
    let qualifier = if var_list.constant {
        DeclarationQualifier::Constant
    } else if var_list.retain {
        DeclarationQualifier::Retain
    } else if var_list.nonretain {
        DeclarationQualifier::NonRetain
    } else if var_list.persistent {
        DeclarationQualifier::Persistent
    } else {
        DeclarationQualifier::Unspecified
    };

    var_list
        .variable
        .iter()
        .map(|v| transform_variable(v, var_type.clone(), qualifier.clone(), file_id))
        .collect()
}

/// Transform a variable declaration
fn transform_variable(
    var: &Variable,
    var_type: VariableType,
    qualifier: DeclarationQualifier,
    file_id: &FileId,
) -> Result<VarDecl, Diagnostic> {
    let identifier = VariableIdentifier::Symbol(make_id(&var.name, file_id));
    let type_name = transform_data_type(&var.var_type, file_id)?;

    let initializer = InitialValueAssignmentKind::Simple(SimpleInitializer {
        type_name,
        initial_value: None, // TODO: Handle initial values
    });

    Ok(VarDecl {
        identifier,
        var_type,
        qualifier,
        initializer,
        block: next_block_id(),
        type_id: None,
        edge: None,
        attributes: Attributes::default(),
    })
}

/// Transform POU body to FunctionBlockBodyKind
fn transform_body(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<FunctionBlockBodyKind, Diagnostic> {
    let Some(ref body) = pou.body else {
        return Ok(FunctionBlockBodyKind::Empty);
    };

    if let Some(st_body) = body.st_body() {
        let stmts = parse_st_body(
            &st_body.text,
            file_id,
            st_body.line_offset,
            st_body.col_offset,
            compiler_options,
        )?;
        Ok(FunctionBlockBodyKind::Statements(Statements {
            body: stmts,
        }))
    } else if let Some(ref sfc_body) = body.sfc {
        // Transform SFC body
        let sfc = transform_sfc_body(sfc_body, pou, file_id, compiler_options)?;
        Ok(FunctionBlockBodyKind::Sfc(sfc))
    } else {
        Ok(FunctionBlockBodyKind::Empty)
    }
}

/// Transform POU body to Vec<StmtKind> (for functions)
fn transform_body_statements(
    pou: &Pou,
    file_id: &FileId,
    compiler_options: &CompilerOptions,
) -> Result<Vec<StmtKind>, Diagnostic> {
    let Some(ref body) = pou.body else {
        return Ok(vec![]);
    };

    if let Some(st_body) = body.st_body() {
        parse_st_body(
            &st_body.text,
            file_id,
            st_body.line_offset,
            st_body.col_offset,
            compiler_options,
        )
    } else {
        Ok(vec![])
    }
}

// ============================================================================
// Configuration transforms
// ============================================================================

/// Transform instances container to configuration declarations
fn transform_instances(
    instances: &Instances,
    file_id: &FileId,
) -> Result<Vec<ConfigurationDeclaration>, Diagnostic> {
    instances
        .configurations
        .configuration
        .iter()
        .map(|config| transform_configuration(config, file_id))
        .collect()
}

/// Transform a configuration declaration
fn transform_configuration(
    config: &Configuration,
    file_id: &FileId,
) -> Result<ConfigurationDeclaration, Diagnostic> {
    let name = make_id(&config.name, file_id);

    // Transform global variables
    let global_var = transform_global_vars(&config.global_vars, file_id)?;

    // Transform resources
    let resource_decl: Result<Vec<_>, _> = config
        .resource
        .iter()
        .map(|resource| transform_resource(resource, file_id))
        .collect();

    Ok(ConfigurationDeclaration {
        name,
        global_var,
        resource_decl: resource_decl?,
        fb_inits: vec![],
        located_var_inits: vec![],
    })
}

/// Transform a resource declaration
fn transform_resource(
    resource: &Resource,
    file_id: &FileId,
) -> Result<ResourceDeclaration, Diagnostic> {
    let name = make_id(&resource.name, file_id);

    // Transform global variables
    let global_vars = transform_global_vars(&resource.global_vars, file_id)?;

    // Transform tasks
    let tasks: Result<Vec<_>, _> = resource
        .task
        .iter()
        .map(|task| transform_task(task, file_id))
        .collect();

    // Transform program instances from the resource level
    let mut programs: Vec<ProgramConfiguration> = resource
        .pou_instance
        .iter()
        .map(|inst| transform_pou_instance(inst, None, file_id))
        .collect();

    // Also include program instances from tasks
    for task in &resource.task {
        for inst in &task.pou_instance {
            programs.push(transform_pou_instance(inst, Some(&task.name), file_id));
        }
    }

    Ok(ResourceDeclaration {
        name: name.clone(),
        resource: name, // Use the same name for resource identifier
        global_vars,
        tasks: tasks?,
        programs,
    })
}

/// Transform a task configuration
fn transform_task(task: &Task, file_id: &FileId) -> Result<TaskConfiguration, Diagnostic> {
    let name = make_id(&task.name, file_id);

    // Parse priority
    let priority = task.priority.parse::<u32>().map_err(|_| {
        Diagnostic::text(
            file_span(file_id),
            text::xml_schema_violation_task_priority_invalid(&task.priority),
        )
    })?;

    // Parse interval if present
    let interval = if let Some(ref interval_str) = task.interval {
        Some(parse_duration(interval_str, file_id)?)
    } else {
        None
    };

    Ok(TaskConfiguration {
        name,
        priority,
        interval,
        single: None,
    })
}

/// Transform a POU instance (program configuration)
fn transform_pou_instance(
    instance: &PouInstance,
    task_name: Option<&LocatedString>,
    file_id: &FileId,
) -> ProgramConfiguration {
    ProgramConfiguration {
        name: make_id(&instance.name, file_id),
        storage: None,
        task_name: task_name.map(|t| make_id(t, file_id)),
        type_name: make_id(&instance.type_name, file_id),
        fb_tasks: vec![],
        sources: vec![],
        sinks: vec![],
    }
}

/// Transform global variables from VarList
fn transform_global_vars(
    var_lists: &[VarList],
    file_id: &FileId,
) -> Result<Vec<VarDecl>, Diagnostic> {
    let mut vars = vec![];
    for var_list in var_lists {
        vars.extend(transform_var_list(var_list, VariableType::Global, file_id)?);
    }
    Ok(vars)
}

/// Parse a duration string (e.g., "T#100ms", "T#1s") to DurationLiteral
fn parse_duration(duration_str: &str, file_id: &FileId) -> Result<DurationLiteral, Diagnostic> {
    use time::Duration;

    let span = file_span(file_id);

    // Strip the T# prefix if present
    let value_part = duration_str
        .strip_prefix("T#")
        .or_else(|| duration_str.strip_prefix("t#"))
        .unwrap_or(duration_str);

    // Try to parse common duration formats
    // Supports: Xms, Xs, Xm, Xh, Xd (and fractional values)
    if let Some(ms) = value_part.strip_suffix("ms") {
        let value: f64 = ms
            .parse()
            .map_err(|_| invalid_duration_error(duration_str, file_id))?;
        let micros = (value * 1000.0) as i64;
        return Ok(DurationLiteral::new(span, Duration::microseconds(micros)));
    }
    if let Some(s) = value_part.strip_suffix('s') {
        let value: f64 = s
            .parse()
            .map_err(|_| invalid_duration_error(duration_str, file_id))?;
        let micros = (value * 1_000_000.0) as i64;
        return Ok(DurationLiteral::new(span, Duration::microseconds(micros)));
    }
    if let Some(m) = value_part.strip_suffix('m') {
        let value: f64 = m
            .parse()
            .map_err(|_| invalid_duration_error(duration_str, file_id))?;
        let micros = (value * 60.0 * 1_000_000.0) as i64;
        return Ok(DurationLiteral::new(span, Duration::microseconds(micros)));
    }
    if let Some(h) = value_part.strip_suffix('h') {
        let value: f64 = h
            .parse()
            .map_err(|_| invalid_duration_error(duration_str, file_id))?;
        let micros = (value * 3600.0 * 1_000_000.0) as i64;
        return Ok(DurationLiteral::new(span, Duration::microseconds(micros)));
    }
    if let Some(d) = value_part.strip_suffix('d') {
        let value: f64 = d
            .parse()
            .map_err(|_| invalid_duration_error(duration_str, file_id))?;
        let micros = (value * 86400.0 * 1_000_000.0) as i64;
        return Ok(DurationLiteral::new(span, Duration::microseconds(micros)));
    }

    Err(invalid_duration_error(duration_str, file_id))
}

/// Create an error diagnostic for invalid duration values
fn invalid_duration_error(value: &str, file_id: &FileId) -> Diagnostic {
    Diagnostic::text(
        file_span(file_id),
        text::xml_schema_violation_duration_invalid(value),
    )
}

/// Parse ST condition expression
///
/// Note: Full expression parsing from embedded ST is not yet implemented.
/// This currently returns a simple TRUE literal as a placeholder.
fn parse_st_condition(
    _st_text: &str,
    _file_id: &FileId,
    _line_offset: usize,
    _col_offset: usize,
) -> Result<ExprKind, Diagnostic> {
    // TODO: Implement proper ST expression parsing for transition conditions
    // For now, return TRUE as placeholder
    Ok(ExprKind::Const(ConstantKind::Boolean(BooleanLiteral::new(
        Boolean::True,
    ))))
}

/// Parse ST body text using the ST parser
///
/// Uses the position information embedded in the StBody struct to provide
/// accurate line/column offsets for error reporting.
fn parse_st_body(
    st_text: &str,
    file_id: &FileId,
    line_offset: usize,
    col_offset: usize,
    compiler_options: &CompilerOptions,
) -> Result<Vec<StmtKind>, Diagnostic> {
    ironplc_parser::parse_st_statements(st_text, file_id, compiler_options, line_offset, col_offset)
}

/// Create an error diagnostic for invalid values
fn invalid_value_error(value: &str, context: &str, file_id: &FileId) -> Diagnostic {
    Diagnostic::text(
        file_span(file_id),
        text::xml_schema_violation_value_invalid(context, value),
    )
}

#[cfg(test)]
mod tests;
