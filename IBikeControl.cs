using BeamBackend;
using UnityEngine;

namespace BikeControl
{
    public interface IBikeControl
    {
        void Setup(IBike beBike, IBeamBackend backend);   
        void Loop(float frameSecs);
        bool RequestTurn(TurnDir dir, bool allowDeferred = false);     
    }

    public abstract class BikeControlBase : IBikeControl
    {
        protected BaseBike bb;
        protected IBeamBackend be;

        protected TurnDir stashedTurn = TurnDir.kUnset; // if turn is requested too late then save it and apply it after the turn is done

        public BikeControlBase()  { }

        public void Setup(IBike ibike, IBeamBackend backend)
        {
            bb = ibike as BaseBike;
            be = backend;
            SetupImpl();
        }

        public abstract void SetupImpl(); // do any implmentation-specific setup

        public virtual void Loop(float frameSecs) 
        {
            if (stashedTurn != TurnDir.kUnset)
            {
                Vector2 nextPt = bb.UpcomingGridPoint(Ground.gridSize);
                float dist = Vector2.Distance(bb.position, nextPt);
                if (( dist >= BaseBike.length) && (dist <=  Ground.gridSize-BaseBike.length))
                {
                    // Turn is requested, and we are not close to a point
                    bb.logger.Info($"Bike {bb.name} Executing deferred turn.");
                    be.PostBikeTurn(bb, stashedTurn);
                    stashedTurn = TurnDir.kUnset;                    
                }
            }
        }

        public virtual bool RequestTurn(TurnDir dir, bool allowDeferred = false)
        {
            // If we are too close to the upcoming point to be able to turn then assign it to the next point,
            // otherwise send out a request.
            // Current limit is 1 bike length
            bool posted = false;
            Vector2 nextPt = bb.UpcomingGridPoint(Ground.gridSize);
            float dist = Vector2.Distance(bb.position, nextPt);
            if (( dist < BaseBike.length) || (dist >  Ground.gridSize-BaseBike.length)) // too close to a grid point to turn
            {
                if (allowDeferred)
                {
                    bb.logger.Info($"Bike {bb.name} requesting deferred turn.");                
                    stashedTurn = dir;
                }
            }
            else
            {
                // cancel anything stashed (can this happen?)
                stashedTurn = TurnDir.kUnset;                
                be.PostBikeTurn(bb, dir); // this needs to move
                posted = true;
            }
            return posted;
        }

    }
}