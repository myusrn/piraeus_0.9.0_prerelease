namespace SkunkLab.Protocols.Coap
{
    public interface ICoapRequestDispatch
    {
        CoapMessage Post(CoapMessage message);

        CoapMessage Get(CoapMessage message);

        CoapMessage Put(CoapMessage message);

        CoapMessage Delete(CoapMessage message);

        CoapMessage Observe(CoapMessage message);
    }
}
