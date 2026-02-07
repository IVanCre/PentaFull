namespace Penta_ClientLib.Interfaces
{
    public class ContactInfo(
        string name,
        string contactID)
    {
        public string UserName { get; private set; } = name;
        public string UserContactID { get; private set; } = contactID;
    }
}
