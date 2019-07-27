using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BudgetService
{
    [TestClass]
    public class UnitTest1
    {
        private BudgetService _budgetService;

        [TestMethod]
        public void SingleDate()
        {
            //Arrange
            var budget = new List<Budget>()
            {
                new Budget()
                {
                    Amount = 3100,
                    YearMonth = "201901"
                },
            };

            SetUp(budget);
            ShouldBe(100, new DateTime(2019, 1, 1), new DateTime(2019, 1, 1));
        }

        private void SetUp(List<Budget> budget)
        {
            IBudgetRepo budgetRepo = Substitute.For<IBudgetRepo>();
            budgetRepo.GetAll().Returns(budget);
            _budgetService = new BudgetService(budgetRepo);
        }

        [TestMethod]
        public void SingleMonth()
        {
            //Arrange
            var budget = new List<Budget>()
            {
                new Budget()
                {
                    Amount = 3100,
                    YearMonth = "201901"
                },
            };
            SetUp(budget);
            ShouldBe(3100, new DateTime(2019, 1, 1), new DateTime(2019, 1, 31));
        }

        [TestMethod]
        public void CrossMonth()
        {
            //Arrange
            var budget = new List<Budget>()
            {
                new Budget()
                {
                    Amount = 3100,
                    YearMonth = "201901"
                },
                new Budget()
                {
                    Amount = 56,
                    YearMonth = "201902"
                },
            };
            SetUp(budget);
            ShouldBe(2806, new DateTime(2019, 1, 4), new DateTime(2019, 2, 3));
        }

        [TestMethod]
        public void CrossMonths()
        {
            //Arrange
            var budget = new List<Budget>()
            {
                new Budget()
                {
                    Amount = 3100,
                    YearMonth = "201901"
                },
                new Budget()
                {
                    Amount = 56,
                    YearMonth = "201902"
                },
                new Budget()
                {
                    Amount = 62,
                    YearMonth = "201903"
                },
                new Budget()
                {
                    Amount = 300,
                    YearMonth = "201904"
                },
                new Budget()
                {
                    Amount = 300,
                    YearMonth = "201905"
                }
            };
            SetUp(budget);
            ShouldBe(2948, new DateTime(2019, 1, 4), new DateTime(2019, 4, 3));
        }

        [TestMethod]
        public void CrossYear()
        {
            //Arrange
            var budget = new List<Budget>()
            {
                new Budget()
                {
                    Amount = 31,
                    YearMonth = "201901"
                },
                new Budget()
                {
                    Amount = 28 * 2,
                    YearMonth = "201902"
                },
                new Budget()
                {
                    Amount = 31 * 3,
                    YearMonth = "201903"
                },
                new Budget()
                {
                    Amount = 30 * 4,
                    YearMonth = "201904"
                },
                new Budget()
                {
                    Amount = 31 * 5,
                    YearMonth = "201905"
                },
                new Budget()
                {
                    Amount = 31 * 50,
                    YearMonth = "202005"
                },
                new Budget()
                {
                    Amount = 30 * 60,
                    YearMonth = "202006"
                }
            };
            SetUp(budget);
            ShouldBe(2182, new DateTime(2019, 1, 4), new DateTime(2020, 6, 3));
        }

        private void ShouldBe(int expected, DateTime startDate, DateTime EndDate)
        {
            Assert.AreEqual(expected, _budgetService.Query(startDate, EndDate));
        }
    }

    public interface IBudgetRepo
    {
        List<Budget> GetAll();
    }

    public class BudgetService
    {
        private IBudgetRepo _repo;

        public BudgetService(IBudgetRepo repo)
        {
            _repo = repo;
        }

        public double Query(DateTime start, DateTime end)
        {
            var allBudget = this._repo.GetAll();
            var perDayInFirstMonth = GetAmountPerDay(start, allBudget, out var days1, out var key1);
            var perDayInLastMonth = GetAmountPerDay(end, allBudget, out var days2, out var key2);

            if (end.Month - start.Month > 1||end.Year - start.Year > 0)
            {
                var foo = new[] { key1, key2 };
                var sum = allBudget.Where(o => Convert.ToInt32(o.YearMonth) > Convert.ToInt32(key1) &&
                                               Convert.ToInt32(o.YearMonth) < Convert.ToInt32(key2))
                    .Sum(c => c.Amount);
                return sum + (days1 - start.Day + 1) * perDayInFirstMonth + end.Day * perDayInLastMonth;
            }

            if (end.Month - start.Month == 1)
            {
                return (days1 - start.Day + 1) * perDayInFirstMonth + end.Day * perDayInLastMonth;
            }

            if (start.Day != end.Day)
            {
                var diffDays = (end - start).Days + 1;
                return perDayInFirstMonth * diffDays;
            }

            return perDayInFirstMonth;
        }

        private double GetAmountPerDay(DateTime dt, List<Budget> allBudget, out int days, out string key)
        {
            days = DateTime.DaysInMonth(dt.Year, dt.Month);
            var keyTemp = dt.Year.ToString("#0000") + dt.Month.ToString("#00");
            var amount = allBudget.SingleOrDefault(c => c.YearMonth == keyTemp)?.Amount ?? 0;
            key = keyTemp;
            return amount / (double)days;
        }
    }

    public class Budget
    {
        public int Amount { get; set; }
        public string YearMonth { get; set; }
    }
}