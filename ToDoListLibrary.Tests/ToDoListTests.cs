using System.Linq;

namespace ToDoListLibrary.Tests
{
    public class ToDoListTests
    {
        private ToDoListRepository repo;
        private DateTime today = DateTime.Today;

        [SetUp]
        public void Setup()
        {
            repo = new ToDoListRepository(); 
        }

        [Test]
        public void AddListTest()
        {
            var list = new List<ToDo>()
            { new ToDo ("Дело 1", TimeOnly.Parse("15:00")) };
            repo.AddList(today,list);
            Assert.IsTrue(repo.GetList(today)
                .Any(x => x.Name.Equals("Дело 1") && x.StartTime.ToString().Equals("15:00")));
            Assert.That(repo.GetList(today).Count().Equals(1));
            list = new List<ToDo>()
            { new ToDo ("Дело 2", TimeOnly.Parse("14:00")),
              new ToDo ("Дело 3", TimeOnly.Parse("13:00"))};
            repo.AddList(today, list);
            Assert.That(repo.GetList(today).Count().Equals(2));
            Assert.IsFalse(repo.GetList(today)
                .Any(x => x.Name.Equals("Дело 1")));
        }

        [Test]
        public void AddListAsYesterdayTest()
        {
            var yesterday = today.AddDays(-1);
            var list = new List<ToDo>()
            { new ToDo ("Дело 4", TimeOnly.Parse("12:00")) };
            repo.AddList(yesterday, list);
            repo.AddListAsYesterday();
            var listToday = repo.GetList(today).ToList();
            var listYesterday = repo.GetList(yesterday).ToList();
            Assert.IsTrue(listToday.Count().Equals(listYesterday.Count()));
            for (int i = 0; i < listToday.Count(); i++)
            {
                Assert.IsTrue(listToday[i].Equals(listYesterday[i]));
            }
        }

        [Test]
        public void AddItemTest()
        {
            var toDo = new ToDo("Дело 5", TimeOnly.Parse("11:00"));
            repo.AddToDo(toDo);
            var list = repo.GetList(today).ToList();
            Assert.That(list.Count().Equals(1));
            Assert.IsTrue(list[0].Equals(toDo));
            repo.DeleteList(today);
            repo.AddToDo(toDo);
            Assert.That(list.Count().Equals(1));
            Assert.IsTrue(list[0].Equals(toDo));
        }

        [Test]
        public void AddItemInListTest()
        {
            var toDo = new ToDo("Дело 14", TimeOnly.Parse("16:00"));
            var list = new List<ToDo>()
            { new ToDo ("Дело 15", TimeOnly.Parse("16:00")),
            new ToDo ("Дело 16", TimeOnly.Parse("17:00"))};            
            repo.AddToDoInList(toDo,list);
            Assert.That(list.Count().Equals(3));
            Assert.IsTrue(list[2].Equals(toDo));
        }

        [Test]
        public void ReadItemTest()
        {
            var toDo = new ToDo("Дело 6", TimeOnly.Parse("10:00"));
            var list = new List<ToDo>() {toDo};
            repo.AddList(today,list);
            Assert.IsTrue(repo.Read(toDo.Name).Equals(toDo));
        }

        [Test]
        public void DeleteItemTest()
        {
            var toDo = new ToDo("Дело 7", TimeOnly.Parse("19:00"));
            var list = new List<ToDo>() { toDo };
            repo.AddList(today, list);
            Assert.That(repo.GetList(today).ToList().Count().Equals(1));
            repo.Delete(toDo.Name);
            Assert.That(repo.GetList(today).ToList().Count().Equals(0));
        }

        [Test]
        public void DeleteListTest()
        {
            var yesterday = today.AddDays(-1);
            var list = new List<ToDo>()
            { new ToDo ("Дело 9", TimeOnly.Parse("05:00")) };
            repo.AddList(today, list);
            repo.AddList(yesterday, list);
            repo.DeleteList(today);            
            Assert.IsFalse(repo.GetMap().ContainsKey(today));
            Assert.IsTrue(repo.GetMap().ContainsKey(yesterday));
        }

        [Test]
        public void ChangeStatusTest()
        {
            var toDo = new ToDo("Дело 10", TimeOnly.Parse("10:00"));
            var list = new List<ToDo>() { toDo };
            repo.AddList(today, list);
            var firstStatus = repo.Read(toDo.Name).Status;
            repo.ChangeStatus(toDo.Name);
            var secondStatus = repo.Read(toDo.Name).Status;
            Assert.IsFalse(firstStatus.Equals(secondStatus));
            repo.ChangeStatus(toDo.Name);
            var thirdStatus = repo.Read(toDo.Name).Status;
            Assert.IsTrue(firstStatus.Equals(thirdStatus));
        }

        [Test]
        public void CloseAllItemTest()
        {
            var list = new List<ToDo>()
            { new ToDo ("Дело 11", TimeOnly.Parse("20:00")),
              new ToDo ("Дело 12", TimeOnly.Parse("16:00")),
              new ToDo ("Дело 13", TimeOnly.Parse("18:00"))};
            repo.AddList(today, list);
            Assert.IsTrue(repo.GetList(today).Any(toDo => toDo.Status.Equals(ToDoStatus.OPEN)));
            repo.CloseAll();
            Assert.IsFalse(repo.GetList(today).Any(toDo => toDo.Status.Equals(ToDoStatus.OPEN)));
        }

        [TearDown]
        public void TearDown()
        {
            if (repo == null)
                return;
            repo.GetMap().Keys.ToList().ForEach(date => repo.DeleteList(date));
        }
    }
}