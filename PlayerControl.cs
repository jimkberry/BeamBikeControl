using BeamBackend;

namespace BikeControl
{
    public class PlayerControl : BikeControlBase
    {
        public override void SetupImpl() 
        {
        }

        public void OnPlayerTurnRequest(TurnDir dir)
        {
            RequestTurn(dir);       
        }
    }
}
