namespace VErp.Commons.GlobalObject
{
    public interface ICurrentContextFactory
    {
        void SetCurrentContext(ICurrentContextService currentContext);
        ICurrentContextService GetCurrentContext();
    }
}
