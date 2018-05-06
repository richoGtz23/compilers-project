/*
Luis Ricardo Gutierrez A01376121
Josep Romagosa A01374637
 */
using System;

namespace DeepLingo {
    class Prog : Node { }
    class GlobalVarDef : Node { }
    class VarDef : Node { }
    class VarList : Node { }
    class FunDef : Node { }
    class FunList : Node { }
    class IdList : Node { }
    class StmtList : Node { }
    class Stmt : Node { }
    class StmtCall : Node { }
    class If : Node { }
    class ElseIfList: Node { }
    class ElseIf: Node { }
    class Else: Node { }
    class Loop : Node { }
    class Break : Node { }
    class Assignment : Node { }
    class Return : Node { }
    class Increment : Node { }
    class Decrement : Node { }
    class FunCall : Node { }
    class Expression : Node { }
    class ExpressionUnary : Node { }
    class ExpressionAdd : Node { }
    class ExpressionMul : Node { }
    class ExpressionEquality : Node { }
    class ExpressionComparison : Node { }
    class ExpressionList : Node { }
    class Array : Node { }
    class Identifier : Node { }
    class Not: Node { } 
    // Operator Bool
    class Or : Node { }
    class And : Node { }
    //Literals
    class VarInt : Node { }
    class VarChar : Node { };
    class VarString : Node { }
    class True: Node { }
}