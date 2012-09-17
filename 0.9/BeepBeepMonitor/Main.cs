using System;

namespace BlooBuzzTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
         Atom x2 = new Atom("x2");
         Atom x1 = new Atom("x1");
         OperatorEquals f1 = new OperatorEquals(x2, new Constant("ElephantUpgrade"));
         OperatorNot f2 = new OperatorNot();
         f2.setOperand(f1);

         FOForAll fo = new FOForAll();
         fo.setOperand(f2);
         fo.setQuantifiedVariable(x2);
         fo.setQualifier("/event/name");
         OperatorG f3 = new OperatorG();
         f3.setOperand(fo);
         OperatorX secondPart = new OperatorX();
         secondPart.setOperand(f3);
         OperatorEquals f4 = new OperatorEquals(new Atom(x1), new Constant("ElephantUpgrade"));
         FOForAll fo2 = new FOForAll();
         fo2.setOperand(f4);
	     //fo2.setOperand(secondPart);
         fo2.setQuantifiedVariable(x1);
         fo2.setQualifier("/event/name");

         OperatorImplies f5 = new OperatorImplies(fo2,secondPart);
         OperatorG formula = new OperatorG();
         formula.setOperand(f5);
			
			OperatorImplies oi = new OperatorImplies();
			oi.setLeftOperand(Constant.m_trueAtom);
			oi.setRightOperand (fo);
			
			SymbolicWatcher w = new SymbolicWatcher();
			w.setFormula(oi);
			w.reset();
			w.update ("<event><name>ElephantUpgrade</name></event>");
			w.update ("<event><name>ElephantUpgrade</name></event>");
		}
	}
}
