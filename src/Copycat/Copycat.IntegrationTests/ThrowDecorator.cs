namespace Copycat.IntegrationTests
{
    public interface IAmPartiallyUseful
    {   
        void DoSomethingUseful();
        void DoSomething();
        void DoSomethingElse();
    }

    [Decorate]
    public partial class ThrowDecorator : IAmPartiallyUseful
    {
        public void DoSomethingUseful() => Console.WriteLine("I did some work!");

        [Template]
        private void Throw(Action action) => throw new NotImplementedException();
    }
}