namespace OPC.Cminus.Core

module Types =
    type AritmeticOps =
        | Sum
        | Mult

    type Operators = 
        | Arimetic of AritmeticOps
        | Logic of string
        | Relational of string
        | Asignation of string
        | None

    type IdTypes = 
        | Function
        | Variable
        | None

    type Tokens =
        | Keyword of string
        | Operator of Operators
        | Identifier of string
        | Punctuation of string
        | Constant of string
        | None

