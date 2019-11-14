using BeamBackend;

namespace BikeControl
{
    public interface IBikeControl
    {
        void Setup(IBike beBike, IBeamBackend backend);   
        void Loop(float frameSecs);
    }

    public abstract class BikeControlBase : IBikeControl
    {
        protected IBike ib;
        protected IBeamBackend be;

        public BikeControlBase()  { }

        public void Setup(IBike ibike, IBeamBackend backend)
        {
            ib = ibike;
            be = backend;
            SetupImpl();
        }

        public abstract void SetupImpl(); // do any implmentation-specific setup

        public abstract void Loop(float frameSecs);

    }
}