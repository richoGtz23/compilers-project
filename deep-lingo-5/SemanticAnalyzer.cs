/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */

using System;
using System.Collections.Generic;

namespace DeepLingo{
    class SemanticAnalyzer{
        //-----------------------------------------------------------
        public FunctionTable functionsTable;
        public SymbolTable globalVariables;
        public Boolean isSecondRun;
        public Boolean insideFunction;
        public int loopCounter;
        public SymbolTable tempTable;
        public IDictionary<string, SymbolTable> localFunctionsTables = new Dictionary<string, SymbolTable>();
        public Boolean broke;
        //-----------------------------------------------------------
        public SemanticAnalyzer(){
            functionsTable = new FunctionTable();
            globalVariables = new SymbolTable();
            tempTable = new SymbolTable();
            loopCounter = 0;
            isSecondRun = false;
            insideFunction = false;
            broke = false;
        }
        public void APIFuncitons(){
            functionsTable["printi"] = 1;
            functionsTable["printc"] = 1;
            functionsTable["prints"] = 1;
            functionsTable["println"] = 0;
            functionsTable["readi"] = 0;
            functionsTable["reads"] = 0;
            functionsTable["new"] = 1;
            functionsTable["size"] = 1;
            functionsTable["add"] = 2;
            functionsTable["get"] = 2;
            functionsTable["set"] = 3;
        }
        void VisitChildren(Node node){
            foreach (var n in node){
                Visit((dynamic)n);
            }
        }
        //-----------------------------------------------------------
        public void Visit(Prog node){
           
            APIFuncitons();
            VisitChildren((dynamic)node);
            if(!functionsTable.Contains("main")){
                throw new SemanticError("No main function found",node[1][0].AnchorToken);
            }
            isSecondRun = true;
            VisitChildren((dynamic)node);
        }
        public void Visit(VarDef node){
           
            //Visit((dynamic)node[0]);
            foreach (var n in node[0]){
                var variableName = n.AnchorToken.Lexeme;
               
                if(!insideFunction && !isSecondRun){
                    if (globalVariables.Contains(variableName)){
                        throw new SemanticError(
                            "Duplicated variable: " + variableName,node.AnchorToken);

                    }
                    else{
                        globalVariables.Add(variableName);
                    }
                }else{
                    if(isSecondRun){
                        if (tempTable.Contains(variableName)){
                            throw new SemanticError(
                                "Duplicated variable: " + variableName, node.AnchorToken);
                        }
                        else
                        {
                            tempTable.Add(variableName);
                        }
                    }

                }
            }
            
        }
        public void Visit(VarList node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(Identifier node){
            var variableName = node.AnchorToken.Lexeme;
            if (!tempTable.Contains(variableName) && !globalVariables.Contains(variableName)){
                throw new SemanticError(
                    "Variable " + variableName + " those not exist in Scope", node.AnchorToken);
            }
            
        }
        public void Visit(FunDef node){
            var functionName = node.AnchorToken.Lexeme;
           
            if(!isSecondRun){
                if(functionsTable.Contains(functionName)){
                    throw new SemanticError("Function Already defined : "+functionName, node.AnchorToken);
                }else{
                    int arity = 0;
                    if(node[0].GetType() == typeof(IdList)){
                        foreach(var n in node[0]){
                            arity++;
                        }
                    }
                    functionsTable[functionName] = arity;
                }
            }else{
                tempTable = new SymbolTable();
                insideFunction = true;
                VisitChildren((dynamic)node);
                insideFunction = false;
                localFunctionsTables[functionName] = tempTable;
                tempTable = new SymbolTable();
            }
        }
        public void Visit(IdList node){
           
            if(isSecondRun){
                foreach (var n in node){
                    var variableName = n.AnchorToken.Lexeme;
                    if(tempTable.Contains(variableName)){
                        throw new SemanticError(
                            "Duplicated variable: " + variableName,node.AnchorToken);
                    }else{
                        tempTable.Add(variableName);
                    }
                }
            }
        }
        public void Visit(ExpressionList node){
            VisitChildren(node);
        }

        public void Visit(StmtList node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(If node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(Else node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(ElseIf node)
        {
           
            VisitChildren((dynamic)node);
        }
        public void Visit(ElseIfList node){
           
            VisitChildren((dynamic)node);
        }
        
        public void Visit(Or node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(And node)
        {
           
            VisitChildren((dynamic)node);
        }
        public void Visit(Assignment node){
            var variableName = node.AnchorToken.Lexeme;
            if(!tempTable.Contains(variableName) && !globalVariables.Contains(variableName)){
                throw new SemanticError("it is not possible to assign to variable"+ variableName+
                "because it is not defined ",node.AnchorToken);    
            }
        }
        
        public void Visit(VarChar node){
           
        }
        public void Visit(VarString node){
           
        }
        public void Visit(VarInt node){
           
            string inputString = node.AnchorToken.Lexeme;
            int numValue;
            bool parsed = Int32.TryParse(inputString, out numValue);

            if (!parsed)
                throw new SemanticError("Int32. could not parse "+inputString+" to an int.", node.AnchorToken);
        }
        public void Visit(Array node){
           
            VisitChildren((dynamic)node);
        }
        public void Visit(Loop node){
           
            loopCounter++;
            VisitChildren((dynamic)node);
            if(!broke)
                throw new SemanticError("infinite loop ",node.AnchorToken);
            loopCounter--;
        }
        public void Visit(FunCall node){
           
            var funName = node.AnchorToken.Lexeme;
            if(!functionsTable.Contains(funName)){
                throw new SemanticError("Function: "+funName+"does not exist", node.AnchorToken);
            }else{
                int arity = 0;
                foreach (var n in node[0]){
                    arity++;
                   
                    Visit((dynamic)n);
                }
                if(arity!=functionsTable[funName]){
                    throw new SemanticError("Function: " + funName + " wrong arity of parameters", node.AnchorToken);
                }
            }
        }
        public void Visit(Break node){
            if(loopCounter>0){
               broke = true;
            }else{
                throw new SemanticError("Break statements not in loop",node.AnchorToken);
            }
            
        }
        public void Visit(Increment node){
           
            var variableName = node.AnchorToken.Lexeme;
            if(!tempTable.Contains(variableName) && !globalVariables.Contains(variableName)){
                throw new SemanticError("it is not possible to assign to variable"+ variableName+
                "because it is not defined ",node.AnchorToken);    
            }
        }
        public void Visit(Decrement node){
           
            var variableName = node.AnchorToken.Lexeme;
            if(!tempTable.Contains(variableName) && !globalVariables.Contains(variableName)){
                throw new SemanticError("it is not possible to assign to variable"+ variableName+
                "because it is not defined ",node.AnchorToken);    
            }
        }
        public void Visit(Return node){
            if(loopCounter>0){
                broke = true;
            }
        }
        public void Visit(Not node){
            VisitChildren((dynamic)node);
        }
        public void Visit(Stmt node){
            VisitChildren((dynamic)node);
        }
        public void Visit(ExpressionEquality node){
            VisitChildren((dynamic)node);
        }
        public void Visit(ExpressionUnary node){
            VisitChildren((dynamic)node);
        }
        public void Visit(ExpressionAdd node){
            VisitChildren((dynamic)node);
        }
        public void Visit(ExpressionMul node){
            VisitChildren((dynamic)node);
        }
        public void Visit(ExpressionComparison node){
            VisitChildren((dynamic)node);
        }


    }
}