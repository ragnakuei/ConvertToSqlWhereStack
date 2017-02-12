using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTalk.SpecFlow;

namespace ConvertToSqlWhereStack.features
{
    [Binding]
    public class ConvertToSqlWhereTestSteps
    {
        private ConvertToSql target;

        [BeforeScenario]
        public void BeforeScenario()
        {
            this.target = new ConvertToSql();
        }

        [Given(@"將輸入的語句 (.*)")]
        public void Given將輸入的語句Input(string input)
        {
            ScenarioContext.Current.Set<string>(input, "input");
        }
        
        [When(@"進行轉換後")]
        public void When進行轉換後()
        {
            var input = ScenarioContext.Current.Get<string>("input");
            var actual = target.Result(input);
            ScenarioContext.Current.Set<string>(actual, "actual");
        }
        
        [Then(@"得到 sql where 語句 (.*)")]
        public void Then得到SqlWhere語句Output(string expected)
        {
            var actual = ScenarioContext.Current.Get<string>("actual");
            Assert.AreEqual(expected, actual);
        }
    }
}
