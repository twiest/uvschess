# Introduction #

Developing your chess AI will be challenging. Here is a list of useful tips and trick to help you in your coding.

**Create FEN boards**

In order to test certain situations, you will want to avoid the repetitive playing just to get to a certain state. You can create a [FEN board](http://en.wikipedia.org/wiki/Forsyth-Edwards_Notation) that will describe the layout of the chess pieces, and whose turn it is.

In UvsChess, a FEN board is saved as a single line text file with the file extension ".fen"