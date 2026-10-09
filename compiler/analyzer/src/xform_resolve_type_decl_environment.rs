//! Transformation rule that resolves declarations and builds
//! a type environment for all types in the source. This rule
//! handles types that are:
//!
//! * defined in the language
//! * defined by particular implementations
//! * defined by users
//!
//! This rules also transforms late bound declarations (those
//! that are ambiguous during parsing).
//!
//! A declaration that does not resolve to a declared type is reported and
//! stays as it was; the declarations around it resolve.
use crate::intermediate_type::{FunctionBlockVarType, IntermediateStructField, IntermediateType};
use crate::intermediates::*;
use crate::pass_runner::Outcome;
use crate::resolution::Failure;
use crate::type_environment::TypeEnvironment;
use ironplc_dsl::common::*;
use ironplc_dsl::core::{Id, Located};
use ironplc_dsl::diagnostic::{Diagnostic, Label};
use ironplc_dsl::fold::Fold;
use ironplc_problems::Problem;
use std::convert::Infallible;

/// Populates the type environment (this also transforms late bound
/// declarations).
///
/// A declaration that cannot be resolved (its base type is not declared, its
/// bounds are in the wrong order) is reported and stays as it was, and is not
/// entered in the environment; every other declaration still resolves. A
/// repeated type or function block name is reported too: the environment keeps
/// the first declaration.
pub fn apply(lib: Library, type_environment: &mut TypeEnvironment) -> Outcome {
    let mut declaration = Declaration {
        environment: type_environment,
        diagnostics: Vec::new(),
    };
    let Ok(library) = declaration.fold_library(lib);
    let mut diagnostics = declaration.diagnostics;
    diagnostics.extend(type_environment.take_duplicates());
    Outcome::new(library, diagnostics)
}

/// The fold that enters the declarations of a library in a type environment.
struct Declaration<'a> {
    environment: &'a mut TypeEnvironment,
    /// The declarations that could not be entered.
    diagnostics: Vec<Diagnostic>,
}

impl Declaration<'_> {
    /// `node`, whether or not it could be entered: a declaration that could
    /// not be entered stays as it was. It is reported when the failure is its
    /// own, and it is entered as a declaration with an error either way, so
    /// that its name stays declared and a declaration made from it fails
    /// without a message of its own (see [`crate::resolution`]).
    fn kept<T>(&mut self, name: &TypeName, node: T, entered: Result<(), Failure>) -> T {
        if let Err(failure) = entered {
            self.environment.insert_failed(name);
            self.diagnostics.extend(failure.into_diagnostic());
        }
        node
    }

    /// [`Self::kept`] for a function block, which is a program organization
    /// unit as well as a type.
    fn kept_function_block(
        &mut self,
        node: FunctionBlockDeclaration,
        entered: Result<(), Failure>,
    ) -> FunctionBlockDeclaration {
        if let Err(failure) = entered {
            self.environment
                .insert_failed_function_block(&TypeName::from_id(&node.name.name));
            self.diagnostics.extend(failure.into_diagnostic());
        }
        node
    }
}

impl TypeEnvironment {
    fn transform_late_bound_declaration(
        &mut self,
        node: LateBoundDeclaration,
    ) -> Result<DataTypeDeclarationKind, Failure> {
        // At this point we should have a type for the late bound declaration
        // so we can replace the late bound declaration with the correct type
        let existing = self.lookup(&node.base_type_name).or_failure(|| {
            Diagnostic::problem(
                Problem::ParentTypeNotDeclared,
                Label::span(node.data_type_name.span(), "Type alias"),
            )
            .with_secondary(Label::span(node.base_type_name.span(), "Base type"))
        })?;

        if existing.representation.is_primitive() {
            Ok(DataTypeDeclarationKind::Simple(SimpleDeclaration {
                type_name: node.data_type_name,
                spec_and_init: InitialValueAssignmentKind::Simple(SimpleInitializer {
                    type_name: node.base_type_name,
                    initial_value: None,
                }),
            }))
        } else {
            match existing.representation {
                IntermediateType::Enumeration { underlying_type: _ } => Ok(
                    DataTypeDeclarationKind::Enumeration(EnumerationDeclaration {
                        type_name: node.data_type_name,
                        spec_init: EnumeratedSpecificationInit {
                            spec: SpecificationKind::Named(node.base_type_name),
                            default: None,
                            underlying_type: None,
                        },
                    }),
                ),
                // A structure alias keeps the name it declares, with the structure
                // it names as its base, so it stays a type of its own.
                IntermediateType::Structure { fields: _ } => {
                    Ok(DataTypeDeclarationKind::Simple(SimpleDeclaration {
                        type_name: node.data_type_name,
                        spec_and_init: InitialValueAssignmentKind::Structure(
                            StructureInitializationDeclaration {
                                type_name: node.base_type_name,
                                elements_init: vec![],
                            },
                        ),
                    }))
                }
                IntermediateType::Array { .. } => {
                    Ok(DataTypeDeclarationKind::Array(ArrayDeclaration {
                        type_name: node.data_type_name,
                        spec: SpecificationKind::Named(node.base_type_name),
                        init: vec![],
                    }))
                }
                IntermediateType::Subrange { .. } => {
                    Ok(DataTypeDeclarationKind::Subrange(SubrangeDeclaration {
                        type_name: node.data_type_name,
                        spec: SpecificationKind::Named(node.base_type_name),
                        default: None,
                    }))
                }
                IntermediateType::Reference { .. } => {
                    // Reference type alias: treat as a simple alias
                    Ok(DataTypeDeclarationKind::Reference(
                        ironplc_dsl::common::ReferenceDeclaration {
                            type_name: node.data_type_name,
                            target: ironplc_dsl::common::ReferenceTarget::Named(
                                node.base_type_name,
                            ),
                            // Resolved alias; original surface keyword not preserved.
                            syntax: ironplc_dsl::common::RefSyntax::RefTo,
                        },
                    ))
                }
                // FunctionBlock and Function types are POUs (Program Organization Units),
                // not TYPE declarations, so they should never appear in the type environment.
                // If we reach this branch, it indicates a bug in the compiler.
                IntermediateType::FunctionBlock { .. } | IntermediateType::Function { .. } => {
                    Err(Diagnostic::internal_error().into())
                }
                // Primitive types are handled by the is_primitive() check above,
                // so reaching this branch indicates a bug in the compiler
                IntermediateType::Bool
                | IntermediateType::Int { .. }
                | IntermediateType::UInt { .. }
                | IntermediateType::Real { .. }
                | IntermediateType::Bytes { .. }
                | IntermediateType::Time { .. }
                | IntermediateType::Date { .. }
                | IntermediateType::TimeOfDay { .. }
                | IntermediateType::DateAndTime { .. }
                | IntermediateType::String { .. } => Err(Diagnostic::internal_error().into()),
            }
        }
    }
}

impl Declaration<'_> {
    fn enter_simple_declaration(&mut self, node: &SimpleDeclaration) -> Result<(), Failure> {
        // A simple declaration consists of a type name followed by specification/initialization.
        match &node.spec_and_init {
            InitialValueAssignmentKind::None(source_span) => {
                // Simple declarations must have a type specification
                // Example: TYPE MY_TYPE : INT; END_TYPE (valid)
                // Example: TYPE MY_TYPE; END_TYPE (invalid - this case)
                return Err(Diagnostic::problem(
                    Problem::InvalidSimpleTypeDecl,
                    Label::span(node.type_name.span(), "Type declaration"),
                )
                .with_secondary(Label::span(
                    source_span.clone(),
                    "Type specification required (e.g., ': INT')",
                ))
                .into());
            }
            // A value that is an expression of constants is folded after this
            // environment is first derived, so it is the base type that counts
            // here; the environment is derived again from the folded value.
            InitialValueAssignmentKind::Simple(SimpleInitializer {
                type_name: base, ..
            })
            | InitialValueAssignmentKind::SimpleExpr(SimpleExprInitializer {
                type_name: base,
                ..
            }) => {
                // If the base type is known, then the type is valid this type
                // will have the same attributes as the base type. If the base
                // type is not declared, then this is not valid.
                self.environment.lookup(base).or_failure(|| {
                    Diagnostic::problem(
                        Problem::ParentTypeNotDeclared,
                        Label::span(node.type_name.span(), "Derived type"),
                    )
                    .with_secondary(Label::span(base.span(), "Base type"))
                })?;
                self.environment.insert_alias(&node.type_name, base)?;
            }
            InitialValueAssignmentKind::String(string_initializer) => {
                self.environment
                    .insert_type(&node.type_name, string::from(string_initializer));
            }
            InitialValueAssignmentKind::EnumeratedValues(enumerated_values_initializer) => {
                let attributes = enumeration::try_from_values(enumerated_values_initializer, None)?;
                self.environment.insert_type(&node.type_name, attributes);
            }
            InitialValueAssignmentKind::EnumeratedType(_enumerated_initial_value_assignment) => {
                // I don't think this is needed because this should refer to a declared type, not declare a type.
            }
            InitialValueAssignmentKind::Params(_) => {
                // A PARAMS type declaration is a `DataTypeDeclarationKind::Params`,
                // folded by `fold_params_declaration`; it never reaches the
                // `TYPE Name : <spec>` form this fold handles.
                return Err(Diagnostic::internal_error().into());
            }
            InitialValueAssignmentKind::FunctionBlock(fb_init) => {
                // Handle function block type aliases like: TYPE MyFBAlias : ExistingFB := (input := 10); END_TYPE
                // This creates an alias to an existing function block type
                self.environment.lookup(&fb_init.type_name).or_failure(|| {
                    Diagnostic::problem(
                        Problem::ParentTypeNotDeclared,
                        Label::span(node.type_name.span(), "Function block type alias"),
                    )
                    .with_secondary(Label::span(fb_init.type_name.span(), "Base type"))
                })?;
                self.environment
                    .insert_alias(&node.type_name, &fb_init.type_name)?;
            }
            InitialValueAssignmentKind::FunctionBlockCall(fb_call) => {
                // The call-style FB initializer (`X : FB(args)`) is only
                // produced by the parser for VAR declarations, not type
                // declarations, so this arm is effectively unreachable here.
                // Handle it like the FunctionBlock alias arm for robustness:
                // treat it as an alias to the referenced FB type, ignoring
                // the (codegen-unsupported) constructor arguments.
                self.environment.lookup(&fb_call.type_name).or_failure(|| {
                    Diagnostic::problem(
                        Problem::ParentTypeNotDeclared,
                        Label::span(node.type_name.span(), "Function block type alias"),
                    )
                    .with_secondary(Label::span(fb_call.type_name.span(), "Base type"))
                })?;
                self.environment
                    .insert_alias(&node.type_name, &fb_call.type_name)?;
            }
            InitialValueAssignmentKind::Subrange(spec) => {
                // Handle subrange specifications like: TYPE MY_RANGE : INT (1..100); END_TYPE
                let result = subrange::try_from(&node.type_name, &spec.spec, self.environment)?;
                match result {
                    subrange::IntermediateResult::Type(attributes) => {
                        self.environment.insert_type(&node.type_name, attributes);
                    }
                    subrange::IntermediateResult::Alias(base_type_name) => {
                        self.environment
                            .insert_alias(&node.type_name, &base_type_name)?;
                    }
                }
            }
            InitialValueAssignmentKind::Structure(structure_init) => {
                // Handle structure type aliases like: TYPE MyAlias : ExistingStruct := (field := 10); END_TYPE
                // This creates an alias to an existing structure type
                self.environment
                    .lookup(&structure_init.type_name)
                    .or_failure(|| {
                        Diagnostic::problem(
                            Problem::ParentTypeNotDeclared,
                            Label::span(node.type_name.span(), "Structure type alias"),
                        )
                        .with_secondary(Label::span(structure_init.type_name.span(), "Base type"))
                    })?;
                self.environment
                    .insert_alias(&node.type_name, &structure_init.type_name)?;
            }
            InitialValueAssignmentKind::Array(array_init) => {
                // Handle array specifications like: TYPE MY_ARRAY : ARRAY [1..10] OF INT; END_TYPE
                let result = array::try_from(&node.type_name, &array_init.spec, self.environment)?;
                match result {
                    array::IntermediateResult::Type(attributes) => {
                        self.environment.insert_type(&node.type_name, attributes);
                    }
                    array::IntermediateResult::Alias(base_type_name) => {
                        self.environment
                            .insert_alias(&node.type_name, &base_type_name)?;
                    }
                }
            }
            InitialValueAssignmentKind::Reference(_) => {
                // Reference types are not resolved through simple declarations
            }
            InitialValueAssignmentKind::LateResolvedType(_type_name) => {
                return Err(Diagnostic::internal_error().into());
            }
        }
        // A declared default is part of the type: a declaration against the
        // type that states no value starts at it.
        self.environment
            .set_initial_value(&node.type_name, node.spec_and_init.stated_value());

        Ok(())
    }

    fn enter_enumeration_declaration(
        &mut self,
        node: &EnumerationDeclaration,
    ) -> Result<(), Failure> {
        // Enumeration declaration can define a set of values
        // or rename another enumeration.
        match &node.spec_init.spec {
            SpecificationKind::Named(base_type_name) => {
                // Alias of another enumeration: base must already exist because we sort the items
                self.environment.lookup(base_type_name).or_failure(|| {
                    Diagnostic::problem(
                        Problem::ParentEnumNotDeclared,
                        Label::span(node.type_name.span(), "Enumeration"),
                    )
                    .with_secondary(Label::span(base_type_name.span(), "Base type name"))
                })?;
                // Use explicit alias insertion to avoid duplicating representation logic
                self.environment
                    .insert_alias(&node.type_name, base_type_name)?;
            }
            SpecificationKind::Inline(spec_values) => {
                let attributes = enumeration::try_from_values(
                    spec_values,
                    node.spec_init.underlying_type.clone(),
                )?;
                self.environment.insert_type(&node.type_name, attributes);
            }
        }

        Ok(())
    }

    fn enter_string_declaration(&mut self, node: &StringDeclaration) -> Result<(), Failure> {
        self.environment
            .insert_type(&node.type_name, string::from_decl(node));
        Ok(())
    }

    fn enter_structure_declaration(&mut self, node: &StructureDeclaration) -> Result<(), Failure> {
        // Use the structure processing module to create the structure type
        let attrs =
            crate::intermediates::structure::try_from(&node.type_name, node, self.environment)?;
        self.environment.insert_type(&node.type_name, attrs);
        Ok(())
    }

    fn enter_union_declaration(&mut self, node: &UnionDeclaration) -> Result<(), Failure> {
        // A union is registered with the members' structure layout for now;
        // overlaying them at offset 0 is not implemented yet.
        let attrs =
            crate::intermediates::structure::from_union(&node.type_name, node, self.environment)?;
        self.environment.insert_type(&node.type_name, attrs);
        Ok(())
    }

    fn enter_subrange_declaration(&mut self, node: &SubrangeDeclaration) -> Result<(), Failure> {
        let result = subrange::try_from(&node.type_name, &node.spec, self.environment)?;

        match result {
            subrange::IntermediateResult::Type(attributes) => {
                self.environment.insert_type(&node.type_name, attributes);
            }
            subrange::IntermediateResult::Alias(base_type_name) => {
                self.environment
                    .insert_alias(&node.type_name, &base_type_name)?;
            }
        }

        self.environment.set_initial_value(
            &node.type_name,
            InitialValueAssignmentKind::Subrange(SubrangeInitializer {
                spec: node.spec.clone(),
                initial_value: node.default.clone(),
            })
            .stated_value(),
        );
        Ok(())
    }

    fn enter_array_declaration(&mut self, node: &ArrayDeclaration) -> Result<(), Failure> {
        // Use the array processing module to create the array type
        let result = array::try_from(&node.type_name, &node.spec, self.environment)?;

        match result {
            array::IntermediateResult::Type(attributes) => {
                self.environment.insert_type(&node.type_name, attributes);
            }
            array::IntermediateResult::Alias(base_type_name) => {
                self.environment
                    .insert_alias(&node.type_name, &base_type_name)?;
            }
        }

        Ok(())
    }

    fn enter_params_declaration(&mut self, node: &ParamsDeclaration) -> Result<(), Failure> {
        // A PARAMS type is the array its bounds are derived from (see
        // `intermediates::params`), so it resolves through the array module.
        match crate::intermediates::params::try_from(&node.type_name, &node.spec, self.environment)?
        {
            array::IntermediateResult::Type(attributes) => {
                self.environment.insert_type(&node.type_name, attributes);
            }
            array::IntermediateResult::Alias(base_type_name) => {
                self.environment
                    .insert_alias(&node.type_name, &base_type_name)?;
            }
        }

        Ok(())
    }

    fn enter_reference_declaration(&mut self, node: &ReferenceDeclaration) -> Result<(), Failure> {
        let target_type = self
            .environment
            .reference_target(&node.type_name, &node.target)?;

        let attrs = crate::type_attributes::TypeAttributes::new(
            node.type_name.span(),
            IntermediateType::Reference {
                target_type: Box::new(target_type),
            },
        );
        self.environment.insert_type(&node.type_name, attrs);
        Ok(())
    }

    fn enter_function_block_declaration(
        &mut self,
        node: &FunctionBlockDeclaration,
    ) -> Result<(), Failure> {
        // Register the user-defined function block in the type environment
        // so that variable declarations like `myFb : MY_FB` resolve to
        // IntermediateType::FunctionBlock, just like stdlib FBs.
        let mut fields = Vec::new();
        let mut current_offset = 0u32;

        for decl in &node.variables {
            let var_type = match decl.var_type {
                VariableType::Input => Some(FunctionBlockVarType::Input),
                VariableType::Output => Some(FunctionBlockVarType::Output),
                VariableType::InOut => Some(FunctionBlockVarType::InOut),
                // VAR_STAT/VAR_INST/VAR_GENERIC are stored like VAR until
                // their placement rules are implemented, so they are FB
                // fields too. See `VariableType::is_pou_storage`.
                VariableType::Var
                | VariableType::VarStat
                | VariableType::VarInst
                | VariableType::VarGeneric => Some(FunctionBlockVarType::Internal),
                _ => None,
            };
            let var_type = match var_type {
                Some(vt) => vt,
                None => continue,
            };

            if let Some(id) = decl.identifier.symbolic_id() {
                let field_type = match &decl.initializer {
                    InitialValueAssignmentKind::Simple(simple) => self
                        .environment
                        .lookup(&simple.type_name)
                        .or_failure(|| {
                            Diagnostic::problem(
                                Problem::ParentTypeNotDeclared,
                                Label::span(simple.type_name.span(), "Field type"),
                            )
                        })?
                        .representation
                        .clone(),
                    InitialValueAssignmentKind::FunctionBlock(fb_init) => self
                        .environment
                        .lookup(&fb_init.type_name)
                        .or_failure(|| {
                            Diagnostic::problem(
                                Problem::ParentTypeNotDeclared,
                                Label::span(fb_init.type_name.span(), "Field type"),
                            )
                        })?
                        .representation
                        .clone(),
                    _ => {
                        // Skip field types we can't resolve yet (strings, arrays, etc.)
                        continue;
                    }
                };

                let alignment = field_type.alignment_bytes() as u32;
                let aligned_offset = if alignment == 0 {
                    current_offset
                } else {
                    current_offset.div_ceil(alignment) * alignment
                };
                let size = field_type.size_in_bytes().unwrap_or(0);

                fields.push(IntermediateStructField {
                    name: Id::from(id.to_string().as_str()),
                    field_type,
                    offset: aligned_offset,
                    var_type: Some(var_type),
                    initial_value: None,
                });

                current_offset = aligned_offset + size;
            }
        }

        let attrs = crate::type_attributes::TypeAttributes::new(
            node.name.span(),
            IntermediateType::FunctionBlock {
                name: node.name.name.to_string(),
                fields,
            },
        );
        self.environment.insert_type(&node.name, attrs);

        Ok(())
    }

    fn enter_interface_declaration(&mut self, node: &InterfaceDeclaration) -> Result<(), Failure> {
        // Register the interface name as a known type so that variables
        // declared with an interface type (e.g. `pDrv : I_Drivable;`)
        // resolve instead of failing with "type not declared."
        //
        // Modeled as an empty structure: interfaces have no fields in
        // IronPLC's model today (method/property signatures are not yet
        // parsed — see specs/design/beckhoff-twincat-dialect.md §1.3).
        // This is intentionally a placeholder representation, not a claim
        // that interface field/method access works. Any real use beyond
        // "declare a variable of this type" is unreachable: the
        // `InterfaceDeclaration` itself always triggers P9999 via
        // `rule_unsupported_extension`, which blocks codegen for the whole
        // project before this representation could matter.
        let attrs = crate::type_attributes::TypeAttributes::new(
            node.name.span(),
            IntermediateType::Structure { fields: vec![] },
        );
        self.environment
            .insert_type(&TypeName::from_id(&node.name), attrs);
        Ok(())
    }
}

impl Fold<Infallible> for Declaration<'_> {
    fn fold_simple_declaration(
        &mut self,
        node: SimpleDeclaration,
    ) -> Result<SimpleDeclaration, Infallible> {
        let entered = self.enter_simple_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_enumeration_declaration(
        &mut self,
        node: EnumerationDeclaration,
    ) -> Result<EnumerationDeclaration, Infallible> {
        let entered = self.enter_enumeration_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_string_declaration(
        &mut self,
        node: StringDeclaration,
    ) -> Result<StringDeclaration, Infallible> {
        let entered = self.enter_string_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_structure_declaration(
        &mut self,
        node: StructureDeclaration,
    ) -> Result<StructureDeclaration, Infallible> {
        let entered = self.enter_structure_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_union_declaration(
        &mut self,
        node: UnionDeclaration,
    ) -> Result<UnionDeclaration, Infallible> {
        let entered = self.enter_union_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_subrange_declaration(
        &mut self,
        node: SubrangeDeclaration,
    ) -> Result<SubrangeDeclaration, Infallible> {
        let entered = self.enter_subrange_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_array_declaration(
        &mut self,
        node: ArrayDeclaration,
    ) -> Result<ArrayDeclaration, Infallible> {
        let entered = self.enter_array_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_params_declaration(
        &mut self,
        node: ParamsDeclaration,
    ) -> Result<ParamsDeclaration, Infallible> {
        let entered = self.enter_params_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_reference_declaration(
        &mut self,
        node: ReferenceDeclaration,
    ) -> Result<ReferenceDeclaration, Infallible> {
        let entered = self.enter_reference_declaration(&node);
        let name = node.type_name.clone();
        Ok(self.kept(&name, node, entered))
    }

    fn fold_function_block_declaration(
        &mut self,
        node: FunctionBlockDeclaration,
    ) -> Result<FunctionBlockDeclaration, Infallible> {
        let entered = self.enter_function_block_declaration(&node);
        Ok(self.kept_function_block(node, entered))
    }

    fn fold_interface_declaration(
        &mut self,
        node: InterfaceDeclaration,
    ) -> Result<InterfaceDeclaration, Infallible> {
        let entered = self.enter_interface_declaration(&node);
        let name = TypeName::from_id(&node.name);
        Ok(self.kept(&name, node, entered))
    }

    fn fold_data_type_declaration_kind(
        &mut self,
        node: DataTypeDeclarationKind,
    ) -> Result<DataTypeDeclarationKind, Infallible> {
        // Although most of the folding is handled by element-specific methods,
        // we need to handle folding of late bound at declaration kind level
        // because this will change the type of the declaration.
        let late_bound = match &node {
            DataTypeDeclarationKind::LateBound(late_bound) => late_bound.clone(),
            _ => return node.recurse_fold(self),
        };
        match self
            .environment
            .transform_late_bound_declaration(late_bound)
        {
            // The transformed declaration declares the alias when it is folded.
            Ok(resolved) => resolved.recurse_fold(self),
            Err(failure) => {
                let name = match &node {
                    DataTypeDeclarationKind::LateBound(late_bound) => {
                        late_bound.data_type_name.clone()
                    }
                    _ => return Ok(node),
                };
                Ok(self.kept(&name, node, Err(failure)))
            }
        }
    }

    fn fold_library_element_kind(
        &mut self,
        node: LibraryElementKind,
    ) -> Result<LibraryElementKind, Infallible> {
        match node {
            LibraryElementKind::DataTypeDeclaration(kind) => {
                let kind = self.fold_data_type_declaration_kind(kind)?;
                Ok(LibraryElementKind::DataTypeDeclaration(kind))
            }
            LibraryElementKind::FunctionBlockDeclaration(fb) => {
                let fb = self.fold_function_block_declaration(fb)?;
                Ok(LibraryElementKind::FunctionBlockDeclaration(fb))
            }
            _ => node.recurse_fold(self),
        }
    }
}

#[cfg(test)]
mod tests;
