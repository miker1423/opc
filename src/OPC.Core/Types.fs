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
        | LogicConstant
        | None

    type Tokens = 
        | Keyword of string * DataTypes
        | Operator of Operators
        | Identifier of string
        | Punctuation of string
        | Constant of Numbers
        | Error of string * int * int
        | None
    
    type IdTypes = 
        | Function
        | Variable
        | None
        
    type Symbol = { Type:DataTypes; Token:Tokens; IdType:IdTypes } 
