﻿using BeamBackend;
using UniLog;

namespace BikeControl
{
    public interface IBikeControl
    {
        void Setup(IBike beBike, IBeamAppCore appCore);
        void Loop(float frameSecs);
        bool RequestTurn(TurnDir dir, bool allowDeferred = false);
    }

    public abstract class BikeControlBase : IBikeControl
    {
        protected BaseBike bb;
        protected IBeamAppCore be;
        protected TurnDir stashedTurn = TurnDir.kUnset; // if turn is requested too late then save it and apply it after the turn is done

        public UniLogger Logger;

        public BikeControlBase()
        {
            Logger = UniLogger.GetLogger("BikeCtrl");
        }

        public void Setup(IBike ibike, IBeamAppCore backend)
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
                if (!bb.CloseToGridPoint())
                {
                    // Turn is requested, and we are not close to a point
                    Logger.Verbose($"{this.GetType().Name} Bike {bb.name} Executing deferred turn.");
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
            if (bb.CloseToGridPoint()) // too close to a grid point to turn
            {
                if (allowDeferred)
                {
                    Logger.Verbose($"{this.GetType().Name} Bike {bb.name} requesting deferred turn.");
                    stashedTurn = dir;
                }
            }
            else
            {
                // cancel anything stashed (can this happen?)
                stashedTurn = TurnDir.kUnset;

                if ((dir == bb.pendingTurn) ||  (dir == TurnDir.kStraight && bb.pendingTurn == TurnDir.kUnset))
                    Logger.Verbose($"RequestTurn() ignoring do-nothing {dir}");
                else
                {
                    be.PostBikeTurn(bb, dir); // this needs to move
                    posted = true;
                }
            }
            return posted;
        }

    }
}