# Introduction #
The decision tree is very useful in debugging your AI. Your AI will undoubtedly make decisions that you may think are wrong. Using the decision tree, you can look at the AI's thought process in selecting a chess move. Often times, the [horizon effect](http://en.wikipedia.org/wiki/Horizon_effect) is the cause of questionable moves. The decision tree will help you to see if a questionable move is caused by the [horizon effect](http://en.wikipedia.org/wiki/Horizon_effect) (which is not a bug), or if it is caused by a bug in your code.

## Using #if ##
As with the profiler, using the decision tree will add unwanted overhead to your AI. Be sure to wrap all decision tree code in #if statements so you can recompile without that overhead for the competition.

```
#if DEBUG
	// Some code that only gets compiled when DEBUG is defined
#endif
```



## Initialize the DecisionTree ##


In `GetNextMove()` create the DecisionTree object that you will use inside MiniMax. It's best to do this before your call to the MiniMax methods.
```

#if DEBUG
        // Create a new decision tree object using the current board
	DecisionTree dt = new DecisionTree(currentBoard);

	// Tell UvsChess about the decision tree object
	SetDecisionTree(dt);
#endif

```

## Adding the DecisionTree to RandomAI ##

The DecisionTree was created specifically for Mini-Max. However, it can still be of great use for earlier assignments like RandomAI. For instance, you can see if your AI is generating all of the possible valid moves given a current board.

```
function randomAI(board)
{
	allMoves = GetAllMoves(board);
	foreach (curMove in allMoves)
	{
		// Add the current move to the decision tree
		// Make sure to add _all_ moves to the decision tree
		DT.AddChild(board, curMove);
	}
}
```

## Adding the DecisionTree to GreedyAI ##

For GreedyAI, the only change for the DecisionTree is to show which move has been selected. Do this by setting DecisionTree.BestChildMove to the selected move.

```
function greedyAI(board)
{
	allMoves = GetAllMoves(board);
        bestMove = GetBestMove(allMoves);
        DT.BestChildMove = bestMove;

        // You still need to add all of the moves to the DecisionTree
	foreach (curMove in allMoves)
	{
		// Add the current move to the decision tree
		// Make sure to add _all_ moves to the decision tree
		DT.AddChild(board, curMove);
	}
}
```

## Adding the DecisionTree to Mini-Max ##

The DecisionTree class is a tree structure containing other DecisionTree objects. As your AI traverses Mini-max, add the decisions made by the AI to the DecisionTree. Below is the standard pseudo code for the Mini max algorithm. Decision Tree code has been inserted approximately in the proper places for the tree to work.


```


function minValue(state,A,B)
{
	if (terminal)
	{
		return value;
	}

	for s in succ(state)
	{	    	
                // Add the current move to the decision tree
                // Make sure to add _all_ moves to the decision tree
	    	DT.AddChild(boardBeforeMove, s.Move);

		// Since MiniMax is going to descend into the move, the
                // decision tree needs to as well.
		DT = DT.LastChild;

		v = min(maxValue(s,A,B), v);

                // This move will eventually be valued at v, so set that
                // in the decision tree.
                DT.EventualMoveValue = v.ToString();
		
		// We've investigated that tree, now let's go back to the parent
		DT = DT.Parent;

		if (v <= A)
		{
                        // Here we've decided what the best move is, so set the
                        // best move in the decision tree.
                        DT.BestChildMove = s.Move;
			return v;
		}

		B = min(B,v);
	}

	return v;
}
         

```