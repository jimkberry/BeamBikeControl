using BeamBackend;

namespace BikeControl
{
    public class PlayerControl : BikeControlBase
    {
        public override void SetupImpl() 
        {
            
        }

        public override void Loop(float frameSecs)
        {
        }  

        public void OnPlayerTurnRequest(TurnDir dir)
        {
            be.PostBikeTurn(ib, dir);        
        }
    }
}
