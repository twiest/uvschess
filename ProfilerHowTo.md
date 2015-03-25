# Introduction #

Your chess AI will call some methods hundreds of thousands, if not millions, of times. Profiling is essential to make your AI as fast as possible. Remember, the faster your AI is, the more nodes it can visit, and the smarter it will be.
Profiling lets you know how many times your AI executes a certain block of code in a single run. By knowing what block of code (ie methods) are called most, you can spend time optimizing those methods to make them as fast as possible.
The Profiler allows you to have up 1000 profile tags.


## Using #if ##
Before we get started with the profiler, know that using the profiler adds some overhead to your AI. This means that your AI will run slower and reach less nodes than if the profiler were not there. With that in mind, you will want to wrap all of your profiling code in `#if` statements. This will allow you to easily compile your AI without the profiling overhead when it comes time to compete.

```
#if DEBUG
	//Some code that only gets compiled when DEBUG is defined
#endif
```

## Create the Enum ##

The profiler tags are defined using an enum. This enum has a value for every profiler tag in your code. Remember, this enum cannot contain more than 1000 values.
The enum for ExampleAI defines 5 tags, each named after the method that the tag is profiling. The naming of the tags is completely arbitrary, but it's best if you name them after the method or block of code they're profiling. Of course, you can always add more tags as your AI becomes more and more complex.

```
        private enum ExampleAIProfilerTags
        {
            AddAllPossibleMovesToDecisionTree,
            GetAllMoves,
            GetNextMove,
            IsValidMove,
            MoveAPawn
        }
```

## Initialize the profiler ##


In `GetNextMove()` before anything else, set `Profiler.KeyNames` to the tags in your enum.
```
   Profiler.TagNames = Enum.GetNames(typeof(ExampleAIProfilerTags));
```


## Add Profiler calls ##
In your code, you need to add profiling tags whereever you want to profile your code. A profiling tag looks like this. Don't forget to wrap each call in `#if` statements.

```
	private void DoSomeSeriousAIStuff()
	{
#if DEBUG
		Profiler.IncrementTagCount((int)ExampleAIProfilerTags.DoSomeSeriousAIStuff);
#endif

		/*
			Some serious AI code
		*/

		// You can also profile blocks of code as well, like a loop
		for(int i = 0; i < numMoves; ++i)
		{

#if DEBUG
			Profiler.IncrementTagCount((int)ExampleAIProfilerTags.DoSomeSeriousAIStuff_ForLoop);
#endif
			/*  
				More serious AI code
			*/
		}
	}
```

This call will tell the profiler to increment the counter for this tag. So at the end of the run, the profiler will output that the tag 'DoSomeSeriousAIStuff' was called N times, and the `for` loop in `DoSomeSeriousAISutff()` was called M times.

## Add Nodes per Minute ##
The Profiler has two special properties that are used to calculate the number of nodes visited per second. You will need to set these properties properly in order to have this functionality. First, you need to create two profiler tags, one for `Mini()` and one for `Max()`.  Then tell the profiler what tags to use in its calculation by setting the Profiler property in `GetNextMove()` right after the key names.

```
#if DEBUG
	Profiler.TagNames = Enum.GetNames(typeof(ExampleAIProfilerTags));

	Profiler.MinisProfilerTag = (int)ExampleAIProfilerTags.MiniMoves;
	Profiler.MaxsProfilerTag = (int)ExampleAIProfilerTags.MaxMoves;
#endif

```

The Profiler automatically logs the time (in milliseconds) for each turn. The profiler will use the two tags that you define to calculate the nodes per second rate. Finally, add the profiling tags in your Mini and Max methods, such that the tag will get called once per node visited.
```
	private Mini()
	{
#if DEBUG
		Profiler.IncrementTagCount((int)ExampleAIProfilerTags.MiniMoves);
#endif
		/*
			your mini-max code
		*/
	}
	private Max()
	{
#if DEBUG
		Profiler.IncrementTagCount((int)ExampleAIProfilerTags.MaxMoves);
#endif
		/*
			your mini-max code
		*/
	}
```


## Setting the depth reached ##
As you keep track of the depth that Mini-Max hits, you will want to set that in the profiler.
Set this method as you run through Mini-Max. This data will be output with the rest of the profiler data

```
#if DEBUG
	Profiler.SetDepthReachedDuringThisTurn(depth);
#endif
```


## Viewing Profiler Results ##

The profiler data is output to the main log window when a game is stopped. The data is output in csv format to make it easier to import the data into a spreadsheet.
The csv data is marked by `---Begin CSV---` and `---End CSV---` markers. The columns in the csv begin with Color, AI or Fx Framework, Method Name (Tag Name), Total, Average, Move 1, Move 2, ..., Move N.

In the columns Move n are listed the number of times each profiler tag was called in that move. The number of calls per move are totaled and averaged in the columns "Total" and "Average" (obviously).

Profiler output from ExampleAI
```
---Begin CSV---
"Date of Profile:","8/11/2008 5:07:49 PM"
"White's AI Name:","ExampleAI (Debug)"
"White's Nodes/Sec:","70.24"
"White's Moves:","5"
"Color","Fx or AI","Method Name","Total","Average","Move 1","Move 2","Move 3","Move 4","Move 5"
"White","AI","Move Depths","","2","2","2","2","2","2"
"White","AI","Move Times","00:00:00.6406250","00:00:00.1280000","00:00:00.1250000","00:00:00.1250000","00:00:00.1250000","00:00:00.1406250","00:00:00.1250000"
"White","AI","AddAllPossibleMovesToDecisionTree","5","1","1","1","1","1","1"
"White","AI","GetAllMoves","45","9","9","9","9","9","9"
"White","AI","GetNextMove","5","1","1","1","1","1","1"
"White","AI","IsValidMove","4","1","0","1","1","1","1"
"White","AI","MoveAPawn","5","1","1","1","1","1","1"

...

---End CSV---
```

Importing the data into a spreadsheet will help you analyze the data.
![http://uvschess.googlecode.com/svn/wiki/profiler_spreadsheet.png](http://uvschess.googlecode.com/svn/wiki/profiler_spreadsheet.png)






