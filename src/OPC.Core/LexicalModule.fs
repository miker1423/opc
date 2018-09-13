namespace OPC.Core

open System
open System.Text
open System.Linq
open System.Collections.Generic

open OPC.Core.Types

module LexicalModule =
    let isPunctuation str line char = 
        match str with
        | "(" | ")" | " " | "\n" | "{" | "}" | "," | ";" | "\t" | "\r" -> Tokens.Punctuation(str, line, char - 1)
        | _ -> Tokens.None
        
    let isOperator str checkEquals line char =
        let operator = 
            match str with
            | "+" | "-" | "*" | "/" | "^" -> Operators.Arimetic(str)
            | "&" | "|" | "!" -> Operators.Logic(str)
            | "<" | ">" -> Operators.Relational(str)
            | "==" when checkEquals -> Operators.Relational(str)
            | "=" when checkEquals -> Operators.Asignation(str)
            | "=" when (not checkEquals) -> Operators.CheckAgain
            | _ -> Operators.None

        match operator with
        | Operators.None -> Tokens.None
        | _ -> Tokens.Operator(operator, line, char)

    let isKeyword str line char = 
        match str with 
        | "mientras"  | "si"  -> Tokens.Keyword(str, DataTypes.None, line, char - 1)
        | "verdadero" | "falso" -> Tokens.Keyword(str, DataTypes.LogicConstant, line, char - 1)
        | "principal" -> Tokens.Keyword(str, DataTypes.Main, line, char - 1)
        | "regresa" -> Tokens.Keyword(str, DataTypes.Return, line, char - 1)
        | "logico"  -> Tokens.Keyword(str, DataTypes.Logic, line, char - 1)
        | "entero" -> Tokens.Keyword(str, DataTypes.Integer, line, char - 1)
        | "real" -> Tokens.Keyword(str, DataTypes.Real, line, char - 1)
        | _ -> Tokens.None

    let addAndReset(buffer:StringBuilder, tokenList:List<Tokens>, token:Tokens) = 
        buffer.Clear() |> ignore
        tokenList.Add(token)
        LastAction.Added

    let checkKeyword(buffer:StringBuilder, tokenList:List<Tokens>, str:string, line, char) =
        let keyword = isKeyword str line char
        match keyword with
        | Tokens.Keyword _-> addAndReset(buffer, tokenList, keyword) 
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkOperator(buffer:StringBuilder, tokenList:List<Tokens>, str:string, checkAgain:bool, line, char) =
        let operator = isOperator str checkAgain line char
        match operator with 
        | Tokens.Operator (op, _, _) ->
            match op with
            | Operators.CheckAgain -> LastAction.Again
            | _ -> addAndReset(buffer, tokenList, operator)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None
        
    let checkPunctuation(buffer:StringBuilder, tokenList:List<Tokens>, str, line, char) =
        let puntuation = isPunctuation str line char
        match puntuation with
        | Tokens.Punctuation _ -> addAndReset(buffer, tokenList, puntuation)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let isIdentifier(line:string, lineInt, char) =
        if Char.IsNumber(line.[0]) || Char.IsSymbol(line.[0]) || Char.IsPunctuation(line.[0])
        then Tokens.Error(line, 0, 0)
        else 
            let count = line.Count(fun x -> Char.IsLetter(x) || Char.IsNumber(x))
            if count = line.Length then
                Tokens.Identifier(line, lineInt, char - line.Length - 1)
            else Tokens.Error(line, 0, 0)

    let checkIdentier(buffer:StringBuilder, tokenList:List<Tokens>, str:string, line, char) =
        let isId = isIdentifier(str, line, char)
        match isId with
        | Tokens.Identifier _ -> addAndReset(buffer, tokenList, isId)
        | _ -> LastAction.None

    let isNumber(line:string, lineInt, char) = 
        let mutable result = Numbers.Integer(line)
        for ch in line do
            let isNumber = Char.IsNumber(ch)
            if not isNumber then
                if ch.Equals('.') then
                    match result with
                    | Numbers.Real _ -> result <- Numbers.Error(line)
                    | _ -> result <- Numbers.Real(line)
                else result <- Numbers.Error(line)

        match result with 
        | Numbers.None -> Tokens.None
        | Numbers.Error _ -> Tokens.None
        | _ -> Tokens.Constant(result, lineInt, char - 1)

    let checkNumber(buffer:StringBuilder, tokenList:List<Tokens>, str:String, line, char) =
        let number = isNumber(str, line, char)
        match number with
        | Tokens.Constant _ -> addAndReset(buffer, tokenList, number)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkFullOperator(buffer:StringBuilder, tokenList:List<Tokens>, i:byref<int>, text:ReadOnlySpan<char>, line, char:byref<int>) =
        let shouldCheck = false
        let operator = checkOperator(buffer, tokenList, buffer.ToString(), shouldCheck, line, char)
        if(operator = LastAction.Again) then
            let str = text.[i+1].ToString()
            let shouldAppend = isPunctuation str line char
            if(shouldAppend = Tokens.None) then
                buffer.Append(text.[i+1]) |> ignore
                i <- i + 1
                char <- char + 1
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck, line, char)
            else 
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck, line, char)
        else
            operator

    let getFullStr(buffer:StringBuilder, i:byref<int>, text:ReadOnlySpan<char>, charPtr:byref<int>, line) = 
        let mutable str = text.[i].ToString()
        let mutable punctuation = isPunctuation str line charPtr
        buffer.Clear() |> ignore
        while ((punctuation = Tokens.None) && (i < text.Length - 1)) do
            buffer.Append(text.[i]) |> ignore
            i <- i + 1
            charPtr <- charPtr + 1
            str <- text.[i].ToString()
            punctuation <- isPunctuation str line charPtr

        buffer.ToString()

    let buildError(buffer:StringBuilder, tokenList:List<Tokens>, str:string, line:int, char:int) =
        let error = Tokens.Error(str, line, char)
        addAndReset(buffer, tokenList, error)

    let processSpan(text:ReadOnlySpan<char>, buffer:StringBuilder, tokenList:List<Tokens>) =
        let mutable i = 0
        let mutable currentLinePtr = 1
        let mutable currentCharPtr = 1
        while i < text.Length - 1 do
            buffer.Append(text.[i]) |> ignore
            let punctuation = checkPunctuation(buffer, tokenList, buffer.ToString(), currentLinePtr, currentCharPtr)
            if(punctuation <> LastAction.Added) then
                let result = checkFullOperator(buffer, tokenList, &i, text, currentLinePtr, &currentCharPtr)
                if(result <> LastAction.Added) then
                    let id = getFullStr(buffer, &i, text, &currentCharPtr, currentLinePtr)
                    let keyword = checkKeyword(buffer, tokenList, id, currentLinePtr, currentCharPtr)
                    if(keyword <> LastAction.Added) then
                        let isNum = checkNumber(buffer, tokenList, id, currentLinePtr, currentCharPtr)
                        if(isNum <> LastAction.Added) then
                            let isId = checkIdentier(buffer, tokenList, id, currentLinePtr, currentCharPtr)
                            if isId = LastAction.None then
                                buildError(buffer, tokenList, id, currentLinePtr, currentCharPtr - buffer.Length) 
                                |> ignore
                else 
                    i <- i + 1
                    currentCharPtr <- currentCharPtr + 1
            else
                let currentChar = text.[i]
                if currentChar.Equals('\n') then
                    currentLinePtr <- currentLinePtr + 1
                    currentCharPtr <- 1
                else 
                    currentCharPtr <- currentCharPtr + 1

                i <- i + 1
                
    let matchIdentifiers(token:Tokens) =
        match token with 
        | Tokens.Identifier _ -> true
        | _ -> false;

    let extractIdentifiers(tokens:List<Tokens>) =
        tokens |> Seq.filter matchIdentifiers

    let printErrors(tokens:List<Tokens>) = 
        for token in tokens do
            match token with
            | Tokens.Error (id, line, char) -> printfn "Error at line %i, char %i, id %A" line char id
            | _ -> ()


    let whiteSpace = [ " "; "\n"; "\t" ]
    let isWhiteSpace(token:Tokens) =
        match token with 
        | Tokens.Punctuation (str,_,_) -> whiteSpace.Contains(str)
        | _ -> false

    let removeWhiteTokens(tokens:List<Tokens>) = 
        let newList = new List<Tokens>()
        for token in tokens do
            let isWhite = isWhiteSpace(token)
            if not isWhite then newList.Add(token)
        newList
        
    let getTokens(text:ReadOnlySpan<char>) =
        let buffer = StringBuilder()
        let tokenList = List<Tokens>()
        processSpan(text, buffer, tokenList)

        printErrors tokenList

        let symbolsTable = 
            tokenList 
            |> extractIdentifiers
            |> Seq.map(fun x-> { Type = DataTypes.None; Token = x; IdType = IdTypes.None })

        (tokenList |> removeWhiteTokens, symbolsTable)