namespace CalculatorNS
{
    public abstract class Creator
    {
        public abstract IProduct FactoryMethod();
    }

    public class FileServiceCreator : Creator
    {
        public override IProduct FactoryMethod()
        {
            return new FileService();
        }
    }

    public class ConsoleServiceCreator : Creator
    {
        public override IProduct FactoryMethod()
        {
            return new ConsoleService();
        }
    }

    public interface IProduct
    {
        void WriteCalculatedNumber();
    }
}