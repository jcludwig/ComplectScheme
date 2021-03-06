﻿module Metadata
    open System
    open System.Reflection
    open System.Reflection.Emit

    open Assembly
    open Symbols
    open Expressions

    type CompilerInfo = {
        Domain : AppDomain;
        AsmName : AssemblyName;
        AsmBuilder : AssemblyBuilder;
        ModuleBuilder : ModuleBuilder;
        }

    type CompilerContext = {
        SymGen : SymbolGenerator;
        }

    // TODO -- unconcstructed and constructed *defs should probably not live in the same type
    // (i.e. Builder/Info should not be option types)

    type ParameterDef = {
        Builder : ParameterBuilder option;
        Type : Type;
        Position : int;
        Name : string;
    }
    with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwithf "Failed to get builder for parameter %s" this.Name 

    type CtorDef = {
        Builder : ConstructorBuilder option;
        Body : Expr;
        Parameters : ParameterDef list
        }
        with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwith "Failed to get builder for ctor" 

    type MethodDef = {
        Builder : MethodBuilder option;
        Name : string;
        Body : Expr;
        ReturnType : Type;
        Parameters : ParameterDef list
        IsStatic : bool;
        }
        with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwithf "Failed to get builder for method %s" this.Name 

    type FieldDef = {
        Builder : FieldBuilder option;
        Name : string;
        Type : Type;
        }
        with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwithf "Failed to get builder for field %s" this.Name 

    type LocalVariableDef = {
        Builder : LocalBuilder option;
        Type : Type;
        Name : string;
        Index : int;
    }
    with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwithf "Failed to get builder for local variable %s" this.Name 

    type TypeDef = {
        Builder : TypeBuilder option;
        Name : string;
        Functions : MethodDef list;
        Ctors : CtorDef list;
        NestedTypes : TypeDef list;
        IsNested : bool;
        Fields : FieldDef list;
        // InheritsFrom?
        }
        with
        member this.GetBuilder() =
            match this.Builder with
                | Some(builder) -> builder
                | None -> failwithf "Failed to get builder for type %s" this.Name 

    let getFuncType (paramTypes : Type list) (returnType : Type) =
        // last type parameter is for return type
        let openType = Type.GetType(sprintf "System.Func`%i" (paramTypes.Length + 1))
        openType.MakeGenericType((paramTypes @ [ returnType ]) |> List.toArray)

    let getLambdaFuncType (methodDef : MethodDef) =
        let paramTypes = methodDef.Parameters |> List.map (fun p -> p.Type)
        let returnType = methodDef.ReturnType
        getFuncType paramTypes returnType