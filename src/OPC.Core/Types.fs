namespace OPC.Core

module Types =
    type LastAction = 
        | Added
        | Again
        | None

    type Numbers =
        | Real of string
        | Integer of string
        | Error of string
        | None

    type Operators =
        | Arimetic of string
        | Logic of string
        | Relational of string
        | Asignation of string
        | CheckAgain
        | None

    type DataTypes =
        | Integer
        | Real
        | Logic
        | Main
        | Return
        | LogicConstant
        | None

    type Tokens = 
        | Keyword of string * DataTypes * int * int
        | Operator of Operators * int * int
        | Identifier of string * int * int
        | Punctuation of string * int * int
        | Constant of Numbers * int * int
        | Error of string * int * int
        | None
    
    type IdTypes = 
        | Function
        | Variable
        | None
        
    type Symbol = { Type:DataTypes; Token:Tokens; IdType:IdTypes } 
