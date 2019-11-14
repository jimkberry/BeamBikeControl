using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BeamBackend;

namespace BikeControl
{
    public class AiControl : BikeControlBase
    {
        public float kMaxBikeSeparation = Ground.gridSize * 6;
        public float turnTime = 2f;

        public float aiCheckTimeout = .7f;

        public float secsSinceLastAiCheck = 0;

        public float maxX = Ground.maxX - 10*Ground.gridSize; // assumes min === -max
        public float maxZ = Ground.maxZ - 10*Ground.gridSize;

        public TurnDir pendingTurn { get => ib.pendingTurn; } // TODOL: Get rid of these? No?
        public Heading heading { get => ib.heading; }    

        public override void SetupImpl() 
        {

        }

        public override void Loop(float frameSecs)
        {
            Vector2 pos2 = ib.position;
            BeamGameData gd = ((BeamGameInstance)be).gameData;
            Ground g = gd.Ground;

                secsSinceLastAiCheck += frameSecs;   // TODO: be consistent with time
                if (secsSinceLastAiCheck > aiCheckTimeout) 
                {         
                    secsSinceLastAiCheck = 0;
                    // If not gonna turn maybe go towards the closest bike?
                    if (pendingTurn == TurnDir.kUnset) {
                        bool closestBikeIsFarAway = false;
                        IBike closestBike = gd.ClosestBike(ib);
                        if (closestBike != null)
                        {
                            Vector2 closestBikePos = gd.ClosestBike(ib).position;
                            if ( Vector2.Distance(pos2, closestBikePos) > kMaxBikeSeparation) // only if it's not really close
                            {
                                closestBikeIsFarAway = true;
                                be.OnTurnReq(ib.bikeId, BikeUtils.TurnTowardsPos( closestBikePos, pos2, heading ));                             
                            }
                        }
                            
                        if (!closestBikeIsFarAway) // Wow. This is some nasty conditional-nesting! TODO: Make it not suck.
                        {
                            bool doTurn = ( Random.value * turnTime <  frameSecs );
                            if (doTurn)    
                                be.OnTurnReq(ib.bikeId, (Random.value < .5f) ? TurnDir.kLeft : TurnDir.kRight);   
                        }               
                    }

                    // Do some looking ahead - maybe 
                    Vector2 nextPos = BikeUtils.UpcomingGridPoint(pos2, heading);

                    List<Vector2> othersPos = gd.CloseBikePositions(ib, 2); // up to 2 closest

                    BikeUtils.MoveNode moveTree = BikeUtils.BuildMoveTree(g, nextPos, heading, 4, othersPos);
                    List<DirAndScore> dirScores = BikeUtils.TurnScores(moveTree);
                    DirAndScore best =  SelectGoodTurn(dirScores); 
                    if (  pendingTurn == TurnDir.kUnset || dirScores[(int)pendingTurn].score < best.score) 
                    {
                        //Debug.Log(string.Format("New Turn: {0}", best.turnDir));                    
                        be.OnTurnReq(ib.bikeId, best.turnDir);
                    }
                }   
        } 


        protected DirAndScore SelectGoodTurn(List<DirAndScore> dirScores) {
            int bestScore = dirScores.OrderBy( ds => ds.score).Last().score;
            // If you only take the best score you will almost always just go forwards.
            // But never select a 1 if there is anything better
            // &&& jkb - I suspect this doesn;t do exactly what I think it does.
            List<DirAndScore> turns = dirScores.Where( ds => (bestScore > 2) ? (ds.score > bestScore * .5) : (ds.score == bestScore)).ToList(); 
            int sel = (int)(Random.value * (float)turns.Count);
            //Debug.Log(string.Format("Possible: {0}, Sel Idx: {1}", turns.Count, sel));
            return turns[sel];
        }
    }

}