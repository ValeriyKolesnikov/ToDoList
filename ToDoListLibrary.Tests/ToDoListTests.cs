namespace ToDoListLibrary.Tests
{
    public class ToDoListTests
    {
        private ToDoListRepository repo;    
        [SetUp]
        public void Setup()
        {
            repo = new ToDoListRepository(ProjectType.TEST); 
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}