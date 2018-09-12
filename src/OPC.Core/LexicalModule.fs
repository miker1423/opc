namespace OPC.Core

open System
open System.Text
open System.Linq
open System.Collections.Generic

open OPC.Core.Types

module LexicalModule =
    let isPunctuation str = 
        match str with
        | "(" | ")" | " " | "\n" | "{" | "}" | "," | ";" | "\t" | "\r" -> Tokens.Punctuation(str)
        | _ -> Tokens.None
        
    let isOperator str checkEquals =
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
        | _ -> Tokens.Operator(operator)

    let isKeyword str = 
        match str with 
        | "mientras" | "regresa" | "si"  -> Tokens.Keyword(str, DataTypes.None)
        | "verdadero" | "falso" -> Tokens.Keyword(str, DataTypes.LogicConstant)
        | "principal" -> Tokens.Keyword(str, DataTypes.Main)
        | "logico"  -> Tokens.Keyword(str, DataTypes.Logic)
        | "entero" -> Tokens.Keyword(str, DataTypes.Integer)
        | "real" -> Tokens.Keyword(str, DataTypes.Real)
        | _ -> Tokens.None

    let addAndReset(buffer:StringBuilder, tokenList:List<Tokens>, token:Tokens) = 
        buffer.Clear() |> ignore
        tokenList.Add(token)
        LastAction.Added

    let checkKeyword(buffer:StringBuilder, tokenList:List<Tokens>, str:string) =
        let keyword = isKeyword str
        match keyword with
        | Tokens.Keyword _ -> addAndReset(buffer, tokenList, keyword) 
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkOperator(buffer:StringBuilder, tokenList:List<Tokens>, str:string, checkAgain:bool) =
        let operator = isOperator str checkAgain
        match operator with 
        | Tokens.Operator op ->
            match op with
            | Operators.CheckAgain -> LastAction.Again
            | _ -> addAndReset(buffer, tokenList, operator)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None
        
    let checkPunctuation(buffer:StringBuilder, tokenList:List<Tokens>, str) =
        let puntuation = isPunctuation str
        match puntuation with
        | Tokens.Punctuation _ -> addAndReset(buffer, tokenList, puntuation)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let isIdentifier(line:string) = 
        let firstChar = line.[0]
        if Char.IsNumber(firstChar) 
        then Tokens.Error(line, 0, 0)
        else if Char.IsSymbol(firstChar) 
        then Tokens.Error(line, 0, 0)
        else Tokens.Identifier(line)

    let checkIdentier(buffer:StringBuilder, tokenList:List<Tokens>, str:string) =
        let isId = isIdentifier str
        match isId with
        | Tokens.Identifier _ -> addAndReset(buffer, tokenList, isId)
        | _ -> LastAction.None

    let isNumber(line:string) = 
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
        | _ -> Tokens.Constant(result)

    let checkNumber(buffer:StringBuilder, tokenList:List<Tokens>, str:String) =
        let number = isNumber(str)
        match number with
        | Tokens.Constant _ -> addAndReset(buffer, tokenList, number)
        | Tokens.None -> LastAction.None
        | _ -> LastAction.None

    let checkFullOperator(buffer:StringBuilder, tokenList:List<Tokens>, i:byref<int>, text:ReadOnlySpan<char>) =
        let shouldCheck = false
        let operator = checkOperator(buffer, tokenList, buffer.ToString(), shouldCheck)
        if(operator = LastAction.Again) then
            let shouldAppend = isPunctuation(text.[i+1].ToString())
            if(shouldAppend = Tokens.None) then
                buffer.Append(text.[i+1]) |> ignore
                i <- i + 1
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck)
            else 
                checkOperator(buffer, tokenList, buffer.ToString(), not shouldCheck)
        else
            operator

    let getFullStr(buffer:StringBuilder, i:byref<int>, text:ReadOnlySpan<char>, charPtr:byref<int>) = 
        let mutable punctuation = isPunctuation(text.[i].ToString())
        buffer.Clear() |> ignore
        while ((punctuation = Tokens.None) && (i < text.Length - 1)) do
            buffer.Append(text.[i]) |> ignore
            i <- i + 1
            charPtr <- charPtr + 1
            punctuation <- isPunctuation(text.[i].ToString())

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
            let punctuation = checkPunctuation(buffer, tokenList, buffer.ToString())
            if(punctuation <> LastAction.Added) then
                let result = checkFullOperator(buffer, tokenList, &i, text)
                if(result <> LastAction.Added) then
                    let id = getFullStr(buffer, &i, text, &currentCharPtr)
                    let keyword = checkKeyword(buffer, tokenList, id)
                    if(keyword <> LastAction.Added) then
                        let isNum = checkNumber(buffer, tokenList, id)
                        if(isNum <> LastAction.Added) then
                            let isId = checkIdentier(buffer, tokenList, id)
                            if isId = LastAction.None then
                                buildError(buffer, tokenList, id, currentLinePtr, currentCharPtr) 
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
            | Tokens.Error (id, line, _) -> printfn "Error at line %i, id %A" line id
            | _ -> ()


    let whiteSpace = [ " "; "\n"; "\t" ]
    let isWhiteSpace(token:Tokens) =
        match token with 
        | Tokens.Punctuation str -> whiteSpace.Contains(str)
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

        (tokenList |> removeWhiteTokens, tokenList |> extractIdentifiers)