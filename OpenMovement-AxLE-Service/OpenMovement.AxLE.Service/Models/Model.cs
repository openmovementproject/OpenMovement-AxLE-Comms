namespace OpenMovement.AxLE.Service.Models
{
    public abstract class Model
    {
        public string Id { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == GetHashCode();
        }
    }
}
