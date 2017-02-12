using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConvertToSqlWhereStack
{
    [TestClass]
    public class ConvertToSqlWhereTest
    {
        [TestMethod]
        public void equals_number()
        {
            var target = new ConvertToSql();
            var input = "age:equals(20)";
            var expected = "where age = 20";

            var actual = target.Result(input);

            Assert.AreEqual(expected, actual);
        }
    }
}
