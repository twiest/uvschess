/******************************************************************************
* The MIT License
* Copyright (c) 2006 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

namespace UvsChess
{
    /// <summary>
    /// This is the interface that your AI must implement in order to be used by the chess framework.
    /// The framework will call these methods to signal your AI to do work.
    /// </summary>
    public interface IChessAI
    {
        /// <summary>
        /// Returns true if your AI is currently running
        /// </summary>
        bool IsRunning
        {
            get;
        }

        /// <summary>
        /// The name of your AI
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move for the given chess board</returns>
        ChessMove GetNextMove(ChessBoard board, ChessColor yourColor);

        /// <summary>
        /// Validates the opponents move. The framework will have you validate your opponents move.
        /// </summary>
        /// <param name="currentState">ChessState, including previous state, previous move. </param>
        /// <returns>Returns true if the opponents move was valid</returns>
        bool IsValidMove(ChessState currentState);

        /// <summary>
        /// This is called when the framework detects that your turn is over. Once this is called,
        /// you must immidiately exit and return your move to the framework.
        /// </summary>
        void EndTurn();

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);

    }
}
