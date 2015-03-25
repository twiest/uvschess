# Introduction #

The purpose of this assignment is for the student to create a heuristic method that can value certain moves above other moves.

When the student is done with this assignment, the AI will be able to pick the best immediate move. In other words, the AI will only look 1 move ahead and whichever move the heuristic says is best, it will pick.


# Requirements #

  * Create a method that takes a board and a move and sets **ChessMove.ValueOfMove** based on some heuristic.
  * Return the move with the highest value to the framework as your next move. If there are multiple moves with the same highest value, just return any of them.
  * **All** chess moves should show up in the DecisionTree with their values (Nothing extra should need to be done for this to happen).
  * The Chosen move should show up with the "->" next to it in the DecisionTree viewer. Set **DecisionTree.BestChildMove** to the chosen move for this to happen.
  * Rename your AI's DLL to something other than StudentAI.dll. This can be done in Visual Studio under the project's build options.
  * Be able to play a full game with `RuntAI_Greedy1`
  * Do not use static variables anywhere. Static classes and methods are fine.

# Resources #
  * DecisionTree HowTo: [http://code.google.com/p/uvschess/wiki/DecisionTreeHowTo](http://code.google.com/p/uvschess/wiki/DecisionTreeHowTo)