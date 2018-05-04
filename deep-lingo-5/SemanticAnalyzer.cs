/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */

using System;
using System.Collections.Generic;

namespace DeepLingo{
    class SemanticAnalyzer{
        //-----------------------------------------------------------
        public SymbolTable functionsTable{
            get;
            private set;
        }
        public SymbolTable globalVariables{
            get;
            private set;
        }
        public Boolean isSecondRun{
            get;
            private set;
        }
        public int loopCounter{
            get;
            private set;
        }
        public int ifCounter{
            get;
            private set;
        }
        public SymbolTable tempTable{
            get;
            private set;
        }
        //-----------------------------------------------------------
        public SemanticAnalyzer(){
            functionsTable = new SymbolTable();
            globalVariables = new SymbolTable();
            tempTable = new SymbolTable();
            loopCounter = 0;
            ifCounter = 0;
        }
        public Boolean varExistsInTables(Identifier node){
            var variableName = node.AnchorToken.Lexeme;
            if (!tempTable.Contains(variableName) && !globalVariables.Contains(variableName)){
                throw new SemanticError(
                    "Variable " + variableName + " those not exist in Scope", node.AnchorToken);
            }
            return true;
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
            Console.WriteLine("Prog");
            APIFuncitons();
            isSecondRun = false;
            Visit((dynamic)node[0]);
            Visit((dynamic)node[1]);
            if(!functionsTable.Contains("main")){
                throw new SemanticError("No main function found",node[1][0].AnchorToken);
            }else{
                isSecondRun = true;
                Visit((dynamic)node[0]);
                Visit((dynamic)node[1]);
            }
        }
        public void Visit(GlobalVarDef node){
            Console.WriteLine("GlobalVarDef");
            if(!isSecondRun)
                VisitChildren((dynamic)node);
            
            
        }
        public void Visit(VarDef node){
            Console.WriteLine("VarDef");
            if(isSecondRun){
                Visit((dynamic)node[0],tempTable);
            }else{
                Visit((dynamic)node[0]);
            }
            
        }
        public void Visit(VarList node){
            Console.WriteLine("VarList");
            VisitChildren((dynamic)node);
        }
        public void Visit(FunList node){
            Console.WriteLine("FunList");
            VisitChildren((dynamic)node);
        }
        public void Visit(Identifier node, SymbolTable table = null){
            var variableName = node.AnchorToken.Lexeme;
            Console.WriteLine(variableName);
            if(table == null && !isSecondRun){
                if (globalVariables.Contains(variableName)){
                    throw new SemanticError(
                        "Duplicated variable: " + variableName,node.AnchorToken);

                }
                else{
                    globalVariables[variableName] = 1;
                }
            }else{
                if(isSecondRun){
                    if (table.Contains(variableName)){
                        throw new SemanticError(
                            "Duplicated variable: " + variableName, node.AnchorToken);
                    }
                    else
                    {
                        table[variableName] = 1;
                    }
                }

            }
            
        }
        public void Visit(FunDef node){
            Console.Write("Fundef ");
            var functionName = node.AnchorToken.Lexeme;
            Console.WriteLine(functionName);
            var arity = 0;
            tempTable = new SymbolTable();
            foreach (var n in node){
                if (n.GetType() == typeof(IdList)){
                    arity = Visit((IdList)n,tempTable);
                }else{
                    if(!isSecondRun){
                        continue;
                    }
                    Visit((dynamic)n); 
                }
            }
            if(!isSecondRun){
                if (functionsTable.Contains(functionName))
                {
                    throw new SemanticError(
                        "Duplicated Function: " + functionName, node.AnchorToken);
                }
                else
                {
                    functionsTable[functionName] = arity;
                }
            }
        }
        public int Visit(IdList node, SymbolTable table = null){
            var arityCounter = 0;
            Console.WriteLine("IdList");
            foreach (var n in node){
                Visit((dynamic)n, table);
                arityCounter++;
            }
            return arityCounter;
        }

        public void Visit(StmtList node){
            Console.WriteLine("StmtList ");
            VisitChildren((dynamic)node);
        }
        public void Visit(If node){
            Console.WriteLine("If");
            ifCounter++;
            foreach (var n in node){
                if (n.GetType() == typeof(Identifier)){
                    varExistsInTables((Identifier)n);
                }
                else{
                    Visit((dynamic)n);
                }
            }
            ifCounter--;
        }
        public void Visit(Else node){
            Console.WriteLine("Else");
            if(ifCounter>0){
                foreach (var n in node)
                {
                    if (n.GetType() == typeof(Identifier))
                    {
                        varExistsInTables((dynamic)n);
                    }
                    else
                    {
                        Visit((dynamic)n);
                    }
                }
            }
        }
        public void Visit(ElseIf node)
        {
            Console.WriteLine("ElseIf");
            if (ifCounter > 0)
            {
                foreach (var n in node)
                {
                    if (n.GetType() == typeof(Identifier))
                    {
                        varExistsInTables((dynamic)n);
                    }
                    else
                    {
                        Visit((dynamic)n);
                    }
                }
            }
        }
        public void Visit(ElseIfList node){
            Console.WriteLine("ElseIfList");
            VisitChildren((dynamic)node);
        }
        public void Visit(Equals node){
            Console.WriteLine("Equals ");
            foreach (var n in node){
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Not_Equals node){
            Console.WriteLine("Not_Equals ");
            foreach (var n in node){
                if (n.GetType() == typeof(Identifier)){
                    varExistsInTables((dynamic)n);
                }
                else{
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Gt node)
        {
            Console.WriteLine("Gt ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Goet node)
        {
            Console.WriteLine("Goet ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Lt node)
        {
            Console.WriteLine("Lt ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Loet node)
        {
            Console.WriteLine("Loet ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Or node){
            Console.WriteLine("Or ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(And node)
        {
            Console.WriteLine("And ");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Assignment node){
            Console.WriteLine("Assignment");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Sum node){
            Console.WriteLine("Sum");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Mul node){
            Console.WriteLine("Mul");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Sub node){
            Console.WriteLine("Sub");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Div node){
            Console.WriteLine("Div");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(Mod node)
        {
            Console.WriteLine("Mod");
            foreach (var n in node)
            {
                if (n.GetType() == typeof(Identifier))
                {
                    varExistsInTables((dynamic)n);
                }
                else
                {
                    Visit((dynamic)n);
                }
            }
        }
        public void Visit(VarChar node){
            Console.WriteLine("VarChar");
        }
        public void Visit(VarString node){
            Console.WriteLine("VarString");
        }
        public void Visit(VarInt node){
            Console.WriteLine("VarInt");
            string inputString = node.AnchorToken.Lexeme;
            int numValue;
            bool parsed = Int32.TryParse(inputString, out numValue);

            if (!parsed)
                throw new SemanticError("Int32. could not parse "+inputString+" to an int.", node.AnchorToken);
        }
        public void Visit(Array node){
            Console.WriteLine("Array");
            VisitChildren((dynamic)node);
        }
        public void Visit(Loop node){
            Console.WriteLine("Loop");
            loopCounter++;
            VisitChildren((dynamic)node);
            loopCounter--;
        }
        public void Visit(FunCall node){
            Console.WriteLine("FunCall");
            var funName = node.AnchorToken.Lexeme;
            if(!functionsTable.Contains(funName)){
                throw new SemanticError("Function: "+funName+"does not exist", node.AnchorToken);
            }else{
                int arity = 0;
                foreach (var n in node){
                    arity++;
                    Console.WriteLine("Node: "+n.AnchorToken.Lexeme);
                    if (n.GetType() == typeof(Identifier)){
                        varExistsInTables((Identifier)n);
                    }
                    else{
                        Visit((dynamic)n);
                    }
                }
                if(arity!=functionsTable[funName]){
                    throw new SemanticError("Function: " + funName + "wrong arity of parameters", node.AnchorToken);
                }
            }
        }
        public void Visit(Break node){
            if(loopCounter>0){
                Console.WriteLine("Break");
            }else{
                throw new SemanticError("Break statements not in loop",node.AnchorToken);
            }
            
        }
        public void Visit(Increment node){
            Console.WriteLine("Increment");
            varExistsInTables((dynamic)node[0]);
        }
        public void Visit(Decrement node){
            Console.WriteLine("Decrement");
            varExistsInTables((dynamic)node[0]);
        }
        public void Visit(True node){
            Console.WriteLine("True");
        }
        public void Visit(Return node){
            Console.WriteLine("Return");
            //poner loocounter si hay return;
        }
        public void Visit(Not node){
            if (node[0].GetType() == typeof(Identifier)){
                varExistsInTables((Identifier)node[0]);
            }
            else{
                Visit((dynamic)node[0]);
            }
        }
        public void Visit(Stmt node){
            VisitChildren((dynamic)node);
        }

    }
}
/*
        //-----------------------------------------------------------
        public void Visit(DeclarationList node)
        {
            VisitChildren(node);
            return;
        }

        //-----------------------------------------------------------
        public void Visit(Declaration node)
        {

            var variableName = node[0].AnchorToken.Lexeme;

            if (Table.Contains(variableName))
            {
                throw new SemanticError(
                    "Duplicated variable: " + variableName,
                    node[0].AnchorToken);

            }
            else
            {
                Table[variableName] =
                    voidMapper[node.AnchorToken.Category];
            }

            return void.void;
        }

        //-----------------------------------------------------------
        public void Visit(StatementList node)
        {
            VisitChildren(node);
            return void.void;
        }

        //-----------------------------------------------------------
        public void Visit(Assignment node)
        {

            var variableName = node.AnchorToken.Lexeme;

            if (Table.Contains(variableName))
            {

                var expectedvoid = Table[variableName];

                if (expectedvoid != Visit((dynamic)node[0]))
                {
                    throw new SemanticError(
                        "Expecting void " + expectedvoid
                        + " in assignment statement",
                        node.AnchorToken);
                }

            }
            else
            {
                throw new SemanticError(
                    "Undeclared variable: " + variableName,
                    node.AnchorToken);
            }

            return void.void;
        }

        //-----------------------------------------------------------
        public void Visit(Print node)
        {
            node.Expressionvoid = Visit((dynamic)node[0]);
            return void.void;
        }

        //-----------------------------------------------------------
        public void Visit(If node)
        {
            if (Visit((dynamic)node[0]) != void.BOOL)
            {
                throw new SemanticError(
                    "Expecting void " + void.BOOL
                    + " in conditional statement",
                    node.AnchorToken);
            }
            VisitChildren(node[1]);
            return void.void;
        }

        //-----------------------------------------------------------
        public void Visit(Identifier node)
        {

            var variableName = node.AnchorToken.Lexeme;

            if (Table.Contains(variableName))
            {
                return Table[variableName];
            }

            throw new SemanticError(
                "Undeclared variable: " + variableName,
                node.AnchorToken);
        }

        //-----------------------------------------------------------
        public void Visit(IntLiteral node)
        {

            var intStr = node.AnchorToken.Lexeme;

            try
            {
                Convert.ToInt32(intStr);

            }
            catch (OverflowException)
            {
                throw new SemanticError(
                    "Integer literal too large: " + intStr,
                    node.AnchorToken);
            }

            return void.INT;
        }

        //-----------------------------------------------------------
        public void Visit(True node)
        {
            return void.BOOL;
        }

        //-----------------------------------------------------------
        public void Visit(False node)
        {
            return void.BOOL;
        }

        //-----------------------------------------------------------
        public void Visit(Neg node)
        {
            if (Visit((dynamic)node[0]) != void.INT)
            {
                throw new SemanticError(
                    "Operator - requires an operand of void " + void.INT,
                    node.AnchorToken);
            }
            return void.INT;
        }

        //-----------------------------------------------------------
        public void Visit(And node)
        {
            VisitBinaryOperator('&', node, void.BOOL);
            return void.BOOL;
        }

        //-----------------------------------------------------------
        public void Visit(Less node)
        {
            VisitBinaryOperator('<', node, void.INT);
            return void.BOOL;
        }

        //-----------------------------------------------------------
        public void Visit(Plus node)
        {
            VisitBinaryOperator('+', node, void.INT);
            return void.INT;
        }

        //-----------------------------------------------------------
        public void Visit(Mul node)
        {
            VisitBinaryOperator('*', node, void.INT);
            return void.INT;
        }

        //-----------------------------------------------------------
        

        //-----------------------------------------------------------
        void VisitBinaryOperator(char op, Node node, void void)
        {
            if (Visit((dynamic)node[0]) != void ||
                Visit((dynamic)node[1]) != void)
            {
                throw new SemanticError(
                    String.Format(
                        "Operator {0} requires two operands of void {1}",
                        op,
                        void),
                    node.AnchorToken);
            }
        }
    }
}
*/