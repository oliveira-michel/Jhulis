using System;
using Xunit;
using Jhulis.Core.Helpers.Extensions;

namespace Safra.Gsa.QaSwagger.Test
{
    public class StringExtensions
    {
        public StringExtensions()
        {
        }

        [Fact]
        public void ToSomeCaseConvertionByOneInvalidCase()
        {
            var exceptionMessage = "String need to be an valid Camel, Pascal, Kebab or Snake case.";

            try { " abcd".ToPascalCase(); }
            catch (ArgumentException ex)
            {
                Assert.Equal(exceptionMessage, ex.Message);
            }
            try { " abcd".ToCamelCase(); }
            catch (ArgumentException ex)
            {
                Assert.Equal(exceptionMessage, ex.Message);
            }
            try { " abcd".ToKebabCase(); }
            catch (ArgumentException ex)
            {
                Assert.Equal(exceptionMessage, ex.Message);
            }
            try { " abcd".ToSnakeCase(); }
            catch (ArgumentException ex)
            {
                Assert.Equal(exceptionMessage, ex.Message);
            }
        }

        [Fact]
        public void CalculateSimilarityWith()
        {
            Assert.Equal(0.43, "bcd".CalculateSimilarityWith("abcdxyz"), 1);
            Assert.Equal(0.33, "Def".CalculateSimilarityWith("AbcDefGhi"), 1);
            Assert.Equal(0.33, "def".CalculateSimilarityWith("AbcDefGhi", true), 1);
            Assert.Equal(0.0, "dEF".CalculateSimilarityWith("AbcDefGhi"), 1);
            Assert.Equal(0.88, "cliente".CalculateSimilarityWith("clientes"), 1);
            Assert.Equal(0.56, "idCliente".CalculateSimilarityWith("clientes"), 1);
            Assert.Equal(0.0, string.Empty.CalculateSimilarityWith(string.Empty), 1);
            Assert.Equal(1.0, "aaa".CalculateSimilarityWith("aaa"), 1);
        }

        [Fact]
        public void IsSomeCase()
        {
            Assert.True("oneCamelOtherCamel".IsCamelCase());
            Assert.False("OneCamelOtherCamel".IsCamelCase());

            Assert.True("OneTwoThree".IsPascalCase());
            Assert.False("oneTwoThree".IsPascalCase());

            Assert.True("meat-vegetable-meat-vegetable".IsKebabCase());
            Assert.False("meat-Vegetable-Meat-vegetable".IsKebabCase());

            Assert.True("snake_case".IsSnakeCase());
            Assert.False("snake_Case".IsSnakeCase());

            Assert.False(" ".IsCamelCase());
            Assert.False(" ".IsPascalCase());
            Assert.False(" ".IsKebabCase());
            Assert.False(" ".IsSnakeCase());

            Assert.True(" @".GetCaseType()[0] == CaseType.None);
        }

        [Fact]
        public void ToSomeCase()
        {
            Assert.Equal("OneCamelOtherCamel", "oneCamelOtherCamel".ToPascalCase());
            Assert.Equal("one-camel-other-camel", "oneCamelOtherCamel".ToKebabCase());
            Assert.Equal("one_camel_other_camel", "oneCamelOtherCamel".ToSnakeCase());

            Assert.Equal("oneTwoThree", "OneTwoThree".ToCamelCase());
            Assert.Equal("one-two-three", "OneTwoThree".ToKebabCase());
            Assert.Equal("one_two_three", "OneTwoThree".ToSnakeCase());

            Assert.Equal("MeatVegetableMeatVegetable", "meat-vegetable-meat-vegetable".ToPascalCase());
            Assert.Equal("meatVegetableMeatVegetable", "meat-vegetable-meat-vegetable".ToCamelCase());
            Assert.Equal("meat_vegetable_meat_vegetable", "meat-vegetable-meat-vegetable".ToSnakeCase());

            Assert.Equal("SnakeCase", "snake_case".ToPascalCase());
            Assert.Equal("snakeCase", "snake_case".ToCamelCase());
            Assert.Equal("snake-case", "snake_case".ToKebabCase());
        }

        [Fact]
        public void SplitCompositeWord()
        {
            string[] resultCamel = "oneTwoThree".SplitCompositeWord();
            if (resultCamel.Length == 3)
            {
                Assert.Equal("one", resultCamel[0]);
                Assert.Equal("Two", resultCamel[1]);
                Assert.Equal("Three", resultCamel[2]);
            }
            else
                Assert.True(false, "The result lengh for 'oneTwoThree' string needs to be 3.");

            string[] resultCamelNumbers = "oneTwo123Three".SplitCompositeWord();
            if (resultCamelNumbers.Length == 4)
            {
                Assert.Equal("one", resultCamelNumbers[0]);
                Assert.Equal("Two", resultCamelNumbers[1]);
                Assert.Equal("123", resultCamelNumbers[2]);
                Assert.Equal("Three", resultCamelNumbers[3]);
            }
            else
                Assert.True(false, "The result lengh for 'oneTwoThree' string needs to be 4.");

            string[] resultPascal = "OneTwoThree".SplitCompositeWord();
            if (resultPascal.Length == 3)
            {
                Assert.Equal("One", resultPascal[0]);
                Assert.Equal("Two", resultPascal[1]);
                Assert.Equal("Three", resultPascal[2]);
            }
            else
                Assert.True(false, "The result lengh for 'OneTwoThree' string needs to be 3.");

            string[] resultPascalNumbers = "OneTwo123Three".SplitCompositeWord();
            if (resultPascalNumbers.Length == 4)
            {
                Assert.Equal("One", resultPascalNumbers[0]);
                Assert.Equal("Two", resultPascalNumbers[1]);
                Assert.Equal("123", resultPascalNumbers[2]);
                Assert.Equal("Three", resultPascalNumbers[3]);
            }
            else
                Assert.True(false, "The result lengh for 'OneTwoThree' string needs to be 4.");

            string[] resultKebab = "one-two-three".SplitCompositeWord();
            if (resultCamel.Length == 3)
            {
                Assert.Equal("one", resultKebab[0]);
                Assert.Equal("two", resultKebab[1]);
                Assert.Equal("three", resultKebab[2]);
            }
            else
                Assert.True(false, "The result lengh for 'one-two-three' string needs to be 3.");

            string[] resultSnake = "one_two_three".SplitCompositeWord();
            if (resultCamel.Length == 3)
            {
                Assert.Equal("one", resultSnake[0]);
                Assert.Equal("two", resultSnake[1]);
                Assert.Equal("three", resultSnake[2]);
            }
            else
                Assert.True(false, "The result lengh for 'one_two_three' string needs to be 3.");

        }
    }
}
