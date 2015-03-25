# Introduction #
Writing a chess AI can be intimidating for many students, especially when they're given everything all at once. This program is meant to last only a few weeks, but could be reasonably extended to much longer. Allowing more time allows the students to explore and experiment even more with artificial intelligence and high performance techniques. But for the original three week time frame, we want to break it down and give students weekly deliverables. By doing so, it will help them know where to start and in what order things need to be done.

## Week 1 - Random AI ##

See: [Suggested Assignment 1](SuggestedAssignment_1_RandomAI.md)

## Week 2 - Greedy AI ##

See: [Suggested Assignment 2](SuggestedAssignment_2_GreedyAI.md)

## Preliminary Competition ##


The students should bring their dll to the competition. UvsChess requires the opposing dlls to be on the local machine. We chose this design because we felt that a networked approach might allow some injustice in competition. Some students might have more high end hardware than others.

The students will have to copy their assemblies to the computer that will host the competition. The assemblies should be place in the bin directory where UvsChess is built.

The preliminary competition should occur after the students Greedy AI. This should allow the students to test out their AI against other students. Students should not look to win necessarily, but should look to gain as much info from the experience as possible, via profiling and logging. The instructor may wish to extend the Seconds Per Turn value in order to counter the overhead of profiling and debugging code.

The instructor and students may decide to hold more than one preliminary competition.

## Week 3 - Mini-Max AI ##

See: [Suggested Assignment 3](SuggestedAssignment_3_MiniMaxAI.md)

## Semi-Final Competition ##

The Semi-Final competition is to test your AIs Mini-Max against others and to find out what areas need to be optimized.

## Week 4 - Optimization ##

Final optimizations. Everything should already be working and debugged. This week is for optimization only.

Tip: Use the built-in profiler to find out what methods you are executing the most, and optimize those.

## Final Competition ##

The prelim and final competitions are run exactly the same, with the students compiling their dll and copying it to the host or officiating computer. The final competition should occur after the students have finished implementing Mini-Max. Students should remember to compile their AI in release mode.