using Xunit;
using Jhulis.Core.Helpers.Extensions;

namespace Jhulis.Core.Test
{
    public class EnumExtensions
    {
        private enum Test
        {
            [System.ComponentModel.Description("One")]
            One,
            Two
        }
        [Fact]
        public void GetEnumDescription()
        {
            var test = Test.One;
            Assert.Equal("One", test.GetEnumDescription());
            
            test = Test.Two;
            Assert.Equal("Two", test.GetEnumDescription());
        }
    }
}
