using System.Diagnostics;
namespace Mankalah
{
    public class Bak32Player : Player
    {
        Stopwatch timer = new Stopwatch();
        private int maxComputeTime = 3000;


        public Bak32Player(Position pos, int maxTimePerMove) : base(pos, "Ben", maxTimePerMove)
        {

        }

        public override int chooseMove(Board b)
        {
            int depth = 1;
            timer.Start();
            Result result = null;
            while (timer.ElapsedMilliseconds < maxComputeTime)
            {

                Result currentresult = minimax(b, depth, int.MinValue, int.MaxValue);
                if (timer.ElapsedMilliseconds < maxComputeTime)
                {//update the final result only if the time has not run out. otherwise the program will take result of the search even though it is not completed
                    result = currentresult;
                }
                //Console.WriteLine("Analyzing at depth: " + depth + " --> Current best move: " + result.getMove() + " and value " + result.getValue());
                depth++;
            }
            timer.Stop();
            timer.Reset();
            return result.getMove();
        }

        public override int evaluate(Board b)
        {//consider: 1) number of stones in mancala wells, 2) captures available, 3) any possible go-agains
            int scoreDifference = b.stonesAt(13) - b.stonesAt(6); //should be positive for top, negative for bottom
            int wellValues = 0;
            int numGoAgains = 0;
            int capturePoints = 0;
            int totalScore = 0;
            if (b.whoseMove() == Position.Top) //Top player wants to find max score
            {
                //1) find remaining stones
                if (b.gameOver())
                {//only add the values of the wells if the game is over (in a terminal state). I am not entirely sure this behaves the way I think it does, however
                    wellValues = b.scoreTop();
                }
                //wellValues = b.scoreTop();


                //2) loop to look for go agains
                for (int well = 7; well <= 12; well++)
                {
                    if (well + b.stonesAt(well) == 13)
                    {//if the number of stones in the well + the well number is equal to 13, that means the final stone would be placed in 13 and we can go again
                        numGoAgains++;
                    }

                    //3) look for captures
                    //to perform a capture, we need to do 2 things: 1) check if there are any captures available, 2) check if it is possible for us to make the capture

                    int oppositeWell = 5; //start here --> this is well 7's compliment
                    if (b.stonesAt(well) == 0 && b.stonesAt(oppositeWell) > 0)//is there a capture? our well must be empty and there must be stones in the other person's well
                    {
                        //can we perform the capture?
                        int captureWell = well;
                        for (int currentWell = 7; currentWell <= 12; currentWell++)//we can make the capture one of the wells can land it's final stone in the capture well
                        {
                            if (currentWell < captureWell) //two ways to check -- if capture can come from stones before empty well, or from stones after empty well
                            { //start with looking at wells before capture well
                                if (b.stonesAt(currentWell) + currentWell == captureWell) //if we can land on the capture
                                {
                                    capturePoints = capturePoints + 1 + b.stonesAt(oppositeWell); //add one from the stone you add to your capture well, plus the stones of the capture target
                                }
                            }
                            if (currentWell > captureWell)
                            {//next look at the stones after the capture well
                                int distanceBetweenWells = currentWell - captureWell;
                                if (14 - distanceBetweenWells == b.stonesAt(currentWell))
                                {// since there are 14 total wells, we need enough to go around the other side to make the capture possible, but no more than where the capture well is

                                    capturePoints = capturePoints + 1 + b.stonesAt(oppositeWell);
                                }

                            }

                        }
                    }
                    //at end of loop, decriment the opposite well to keep pace with the loop. pairs are --> (7,5)(8,4)(9,3)(10,2)(11,1)(12,0)
                    oppositeWell--;
                }
                totalScore = scoreDifference + wellValues + numGoAgains + capturePoints;
            }

            else
            {////Bottom player wants to find min score

                //1) find remaining stones
                if (b.gameOver())
                {
                    wellValues = -b.scoreBot();//the more negative, the better for the min player
                }


                //2) loop for go agains
                for (int well = 0; well <= 5; well++)
                {
                    if (well + b.stonesAt(well) == 6)
                    {
                        numGoAgains--; //want to minimize score for bottom --> subtract, not add
                    }
                    //3) look for captures
                    //to perform a capture, we need to do 2 things: 1) check if there are any captures available, 2) check if it is possible for us to make the capture

                    int oppositeWell = 12; //start here --> this is well 7's compliment
                    if (b.stonesAt(well) == 0 && b.stonesAt(oppositeWell) > 0)//is there a capture? our well must be empty and there must be stones in the other person's well
                    {
                        //can we perform the capture?
                        int captureWell = well;
                        for (int currentWell = 0; currentWell <= 6; currentWell++)//we can make the capture one of the wells can land it's final stone in the capture well
                        {
                            if (currentWell < captureWell) //two ways to check -- if capture can come from stones before empty well, or from stones after empty well
                            { //start with looking at wells before capture well
                                if (b.stonesAt(currentWell) + currentWell == captureWell) //if we can land on the capture
                                {

                                    capturePoints = capturePoints - 1 - b.stonesAt(oppositeWell); //add one from the stone you add to your capture well, plus the stones of the capture target
                                }
                            }
                            if (currentWell > captureWell)
                            {//next look at the stones after the capture well
                                int distanceBetweenWells = currentWell - captureWell;
                                if (14 - distanceBetweenWells == b.stonesAt(currentWell))
                                {// since there are 14 total wells, we need enough to go around the other side to make the capture possible, but no more than where the capture well is

                                    capturePoints = capturePoints - 1 - b.stonesAt(oppositeWell);

                                }

                            }

                        }
                    }
                    //at end of loop, decriment the opposite well to keep pace with the loop. pairs are --> (7,5)(8,4)(9,3)(10,2)(11,1)(12,0)
                    oppositeWell--;
                }
                totalScore = scoreDifference + wellValues + numGoAgains + capturePoints;
            }

            return totalScore;
        }

        public override string getImage()
        {
            //override to provide image of yourself for tournament
            return "BenMancala.png";
        }

        public override string gloat()
        {

            return "Easy... AI will take over your job!";
        }

        public Result minimax(Board b, int depth, int alpha, int beta)
        {//want to return both the best move and the score associated with the move
            if (timer.ElapsedMilliseconds >= maxComputeTime || b.gameOver() || depth == 0)
            {//if depth is 0, we are as far as we should look -- call evaluate
                return new Result(0, evaluate(b));
            }
            int bestVal;
            Result val;
            int bestMove;
            if (b.whoseMove() == Position.Top)
            {
                bestMove = 7;
                bestVal = int.MinValue;
                for (int move = 7; move <= 12; move++)
                {//for each of the 6 wells that the player can choose from
                    if (b.legalMove(move))
                    {
                        if (timer.ElapsedMilliseconds >= maxComputeTime)
                        {
                            return new Result(bestMove, bestVal);
                        }
                        //store the results (in a list?) so that when the timer runs out, the process is terminated but we still have a result to give
                        Board b1 = new Board(b); //call copy constructor to make a new board
                        b1.makeMove(move, false);
                        val = minimax(b1, depth - 1, alpha, beta);
                        if (val.getValue() > bestVal)
                        {//top is max, so we want anything better than negative infinity -- the higher, the better
                            bestVal = val.getValue();
                            bestMove = move;
                        }
                        alpha = Math.Max(alpha, bestVal); //the alpha value should be the best score that the max player should be able to achieve
                        if (beta <= alpha)
                        {//if beta is less than alpha, the max player will not look down the rest of the path because there is already a better option
                         //for them, and it would not make sense for them to attempt to find a position that could end up benefitting the min player instead
                            break;
                        }
                    }
                }
            }
            else
            {//it must be the bottom player
                bestMove = 0;
                bestVal = int.MaxValue;
                for (int move = 0; move <= 5; move++)
                {
                    if (b.legalMove(move)) //also check if 3 seconds have passed -- exit if so
                    {
                        if (timer.ElapsedMilliseconds >= maxComputeTime)
                        {
                            return new Result(bestMove, bestVal);

                        }
                        Board b1 = new Board(b);
                        b1.makeMove(move, false);
                        val = minimax(b1, depth - 1, alpha, beta);
                        if (val.getValue() < bestVal)
                        {//bottom is the min player, so we want as low as possible
                            bestVal = val.getValue();
                            bestMove = move;
                        }
                        beta = Math.Min(beta, bestVal); //the beta values should be the best score that the min player should be able to achieve(smallest possible value)
                        if (beta <= alpha)
                        { //if the min player finds a time where the beta value is less than or equal to the alpha value, that means there is a better option available for the min player 
                            // and there is a chance this tree has the possiblility of giving the max player a better chance, so don't look down the rest of the tree
                            break;
                        }
                    }
                }
            }
            //Console.WriteLine("best value: " + bestVal);
            return new Result(bestMove, bestVal);
        }
    }
}
public class Result
{
    private int move;
    private int value;

    public Result(int bestMove, int bestValue)
    {
        this.move = bestMove;
        this.value = bestValue;
    }

    public int getMove() { return move; }
    public int getValue() { return value; }

    public string toString()
    {
        return "The best move is: " + move + " and the best value is: " + value;
    }
}


