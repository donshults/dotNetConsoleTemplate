namespace dotNetConsole1.Services
{
    public interface ICustomer
    {
        string CustomerName { get; set; }

        void CreateCustomer(string name);
    }
}