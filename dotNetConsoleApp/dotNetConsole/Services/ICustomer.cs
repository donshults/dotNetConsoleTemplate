namespace dotNetConsole.Services
{
    public interface ICustomer
    {
        string CustomerName { get; set; }

        void CreateCustomer(string name);
    }
}