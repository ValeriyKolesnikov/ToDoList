using System.Linq;

namespace ToDoListLibrary.Tests
{
    public class ToDoListTests
    {
        private ToDoListRepository _repo;
        private readonly DateTime _today = DateTime.Today;

        [SetUp]
        public void Setup()
        {
            _repo = new ToDoListRepository("testUser"); 
        }

        [Test]
        public void AddListTest()
        {
            var list = new List<ToDo>()
            { new ToDo ("Дело 1", TimeOnly.Parse("15:00")) };
            _repo.AddList(_today,list);
            Assert.IsTrue(_repo.GetList(_today)
                .Any(x => x.Name.Equals("Дело 1") && x.StartTime.ToString().Equals("15:00")));
            Assert.That(_repo.GetList(_today).Count().Equals(1));
            list = new List<ToDo>()
            { new ToDo ("Дело 2", TimeOnly.Parse("14:00")),
              new ToDo ("Дело 3", TimeOnly.Parse("13:00"))};
            _repo.AddList(_today, list);
            Assert.That(_repo.GetList(_today).Count().Equals(2));
            Assert.IsFalse(_repo.GetList(_today)
                .Any(x => x.Name.Equals("Дело 1")));
        }

        [Test]
        public void AddListAsYesterdayTest()
        {
            var yesterday = _today.AddDays(-1);
            var list = new List<ToDo>()
            { new ToDo ("Дело 4", TimeOnly.Parse("12:00")) };
            _repo.AddList(yesterday, list);
            _repo.AddListAsYesterday();
            var listToday = _repo.GetList(_today).ToList();
            var listYesterday = _repo.GetList(yesterday).ToList();
            Assert.IsTrue(listToday.Count.Equals(listYesterday.Count));
            for (int i = 0; i < listToday.Count; i++)
            {
                Assert.IsTrue(listToday[i].Equals(listYesterday[i]));
            }
        }

        [Test]
        public void AddItemTest()
        {
            var toDo = new ToDo("Дело 5", TimeOnly.Parse("11:00"));
            _repo.AddToDo(toDo, _today);
            var list = _repo.GetList(_today).ToList();
            Assert.That(list.Count().Equals(1));
            Assert.IsTrue(list[0].Equals(toDo));
            _repo.DeleteList(_today);
            _repo.AddToDo(toDo, _today);
            Assert.That(list.Count().Equals(1));
            Assert.IsTrue(list[0].Equals(toDo));
        }

        [Test]
        public void ReadItemTest()
        {
            var toDo = new ToDo("Дело 6", TimeOnly.Parse("10:00"));
            var list = new List<ToDo>() {toDo};
            _repo.AddList(_today,list);
            Assert.IsTrue(_repo.Read(toDo.Name).Equals(toDo));
        }

        [Test]
        public void DeleteItemTest()
        {
            var toDo = new ToDo("Дело 7", TimeOnly.Parse("19:00"));
            var list = new List<ToDo>() { toDo };
            _repo.AddList(_today, list);
            Assert.That(_repo.GetList(_today).ToList().Count().Equals(1));
            _repo.Delete(toDo.Name);
            Assert.That(_repo.GetList(_today).ToList().Count().Equals(0));
        }

        [Test]
        public void DeleteListTest()
        {
            var yesterday = _today.AddDays(-1);
            var list = new List<ToDo>()
            { new ToDo ("Дело 9", TimeOnly.Parse("05:00")) };
            _repo.AddList(_today, list);
            _repo.AddList(yesterday, list);
            _repo.DeleteList(_today);            
            Assert.IsFalse(_repo.GetMap().ContainsKey(_today));
            Assert.IsTrue(_repo.GetMap().ContainsKey(yesterday));
        }

        [Test]
        public void ChangeStatusTest()
        {
            var toDo = new ToDo("Дело 10", TimeOnly.Parse("10:00"));
            var list = new List<ToDo>() { toDo };
            _repo.AddList(_today, list);
            var firstStatus = _repo.Read(toDo.Name).Status;
            _repo.ChangeStatus(toDo.Name);
            var secondStatus = _repo.Read(toDo.Name).Status;
            Assert.IsFalse(firstStatus.Equals(secondStatus));
            _repo.ChangeStatus(toDo.Name);
            var thirdStatus = _repo.Read(toDo.Name).Status;
            Assert.IsTrue(firstStatus.Equals(thirdStatus));
        }

        [Test]
        public void CloseAllItemTest()
        {
            var list = new List<ToDo>()
            { new ToDo ("Дело 11", TimeOnly.Parse("20:00")),
              new ToDo ("Дело 12", TimeOnly.Parse("16:00")),
              new ToDo ("Дело 13", TimeOnly.Parse("18:00"))};
            _repo.AddList(_today, list);
            Assert.IsTrue(_repo.GetList(_today).Any(toDo => toDo.Status.Equals(ToDoStatus.OPEN)));
            _repo.CloseAll();
            Assert.IsFalse(_repo.GetList(_today).Any(toDo => toDo.Status.Equals(ToDoStatus.OPEN)));
        }

        [Test]
        public void CancelItemTest()
        {
            var toDo = new ToDo("Дело 14", TimeOnly.Parse("14:00"));
            var list = new List<ToDo>() { toDo };
            _repo.AddList(_today, list);
            Assert.IsTrue(_repo.GetList(_today).Any(toDo => toDo.Status.Equals(ToDoStatus.OPEN)));
            _repo.CancelToDo(toDo.Name);
            Assert.IsTrue(_repo.GetList(_today).Any(toDo => toDo.Status.Equals(ToDoStatus.NO)));
        }

        [TearDown]
        public void TearDown()
        {
            if (_repo == null)
                return;
            _repo.GetMap().Keys.ToList().ForEach(date => _repo.DeleteList(date));
        }
    }
}