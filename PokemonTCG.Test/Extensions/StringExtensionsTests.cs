using PokemonTCG.SDK.Extensions;

namespace PokemonTCG.Test.Extensions
{
    public class StringExtensionsTests
    {
        // ToInt
        [Fact]
        public void ToInt_ValidNumber_ReturnsInt() => Assert.Equal(42, "42".ToInt());

        [Fact]
        public void ToInt_InvalidString_ReturnsZero() => Assert.Equal(0, "abc".ToInt());

        [Fact]
        public void ToInt_EmptyString_ReturnsZero() => Assert.Equal(0, "".ToInt());

        // ToDecimal
        [Fact]
        public void ToDecimal_ValidDecimal_ReturnsDecimal() => Assert.Equal(3.14m, "3.14".ToDecimal());

        [Fact]
        public void ToDecimal_InvalidString_ReturnsZero() => Assert.Equal(0m, "xyz".ToDecimal());

        [Fact]
        public void ToDecimal_EmptyString_ReturnsZero() => Assert.Equal(0m, "".ToDecimal());

        // ToBool
        [Fact]
        public void ToBool_TrueString_ReturnsTrue() => Assert.True("true".ToBool());

        [Fact]
        public void ToBool_FalseString_ReturnsFalse() => Assert.False("false".ToBool());

        [Fact]
        public void ToBool_InvalidString_ReturnsFalse() => Assert.False("abc".ToBool());

        // Append
        [Fact]
        public void Append_AddsTextWithNewLine()
        {
            var result = "Hello".Append("World");
            Assert.Contains("Hello", result);
            Assert.Contains("World", result);
        }

        // WithFallback
        [Fact]
        public void WithFallback_NullPrimary_ReturnsFallback() =>
            Assert.Equal("fallback", ((string)null).WithFallback("fallback"));

        [Fact]
        public void WithFallback_EmptyPrimary_ReturnsFallback() =>
            Assert.Equal("fallback", "".WithFallback("fallback"));

        [Fact]
        public void WithFallback_WhitespacePrimary_ReturnsFallback() =>
            Assert.Equal("fallback", "   ".WithFallback("fallback"));

        [Fact]
        public void WithFallback_ValidPrimary_ReturnsPrimary() =>
            Assert.Equal("primary", "primary".WithFallback("fallback"));

        // JoinStrings
        [Fact]
        public void JoinStrings_ValidList_JoinsWithDelimiter() =>
            Assert.Equal("a, b, c", new[] { "a", "b", "c" }.JoinStrings());

        [Fact]
        public void JoinStrings_NullInput_ReturnsNull() =>
            Assert.Null(((IEnumerable<string>)null).JoinStrings());

        [Fact]
        public void JoinStrings_CustomDelimiter_UsesDelimiter() =>
            Assert.Equal("a|b", new[] { "a", "b" }.JoinStrings("|"));

        [Fact]
        public void JoinStrings_SkipsEmptyEntries()
        {
            var result = new[] { "a", "", "b", null }.JoinStrings();
            Assert.Equal("a, b", result);
        }

        // IsEmpty / IsNotEmpty
        [Fact]
        public void IsEmpty_NullString_ReturnsTrue() => Assert.True(((string)null).IsEmpty());

        [Fact]
        public void IsEmpty_EmptyString_ReturnsTrue() => Assert.True("".IsEmpty());

        [Fact]
        public void IsEmpty_WhitespaceString_ReturnsTrue() => Assert.True("   ".IsEmpty());

        [Fact]
        public void IsEmpty_ValidString_ReturnsFalse() => Assert.False("text".IsEmpty());

        [Fact]
        public void IsNotEmpty_ValidString_ReturnsTrue() => Assert.True("text".IsNotEmpty());

        [Fact]
        public void IsNotEmpty_EmptyString_ReturnsFalse() => Assert.False("".IsNotEmpty());

        // DefaultIfEmpty
        [Fact]
        public void DefaultIfEmpty_EmptyString_ReturnsDefault() =>
            Assert.Equal("default", "".DefaultIfEmpty("default"));

        [Fact]
        public void DefaultIfEmpty_ValidString_ReturnsOriginal() =>
            Assert.Equal("value", "value".DefaultIfEmpty("default"));

        // SafeSplit
        [Fact]
        public void SafeSplit_EmptyString_ReturnsEmptyArray() =>
            Assert.Empty("".SafeSplit(","));

        [Fact]
        public void SafeSplit_NullString_ReturnsEmptyArray() =>
            Assert.Empty(((string)null).SafeSplit(","));

        [Fact]
        public void SafeSplit_ValidString_SplitsCorrectly()
        {
            var result = "a,b,c".SafeSplit(",");
            Assert.Equal(3, result.Length);
        }

        // HasSpaces
        [Fact]
        public void HasSpaces_StringWithSpaces_WrapsInQuotes() =>
            Assert.Equal("\"hello world\"", "hello world".HasSpaces());

        [Fact]
        public void HasSpaces_StringWithTO_DoesNotWrap() =>
            Assert.Equal("60 TO 120", "60 TO 120".HasSpaces());

        [Fact]
        public void HasSpaces_StringWithoutSpaces_ReturnsOriginal() =>
            Assert.Equal("pokemon", "pokemon".HasSpaces());

        // ToCamelCase
        [Fact]
        public void ToCamelCase_CapitalizedString_LowersFirstChar() =>
            Assert.Equal("pokemon", "Pokemon".ToCamelCase());

        [Fact]
        public void ToCamelCase_EmptyString_ReturnsEmpty() =>
            Assert.Equal("", "".ToCamelCase());

        [Fact]
        public void ToCamelCase_NullString_ReturnsNull() =>
            Assert.Null(((string)null).ToCamelCase());

        [Fact]
        public void ToCamelCase_AlreadyLowercase_ReturnsUnchanged() =>
            Assert.Equal("pokemon", "pokemon".ToCamelCase());
    }
}
