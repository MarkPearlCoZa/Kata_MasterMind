using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Minesweeper
{
    public static class ExtensionMethods
    {
        public static bool DoesNotContain(this IEnumerable<PinColor> collection, PinColor pin)
        {
            return !collection.Contains(pin);
        }
    }
    public enum PinColor
    {
        Red,
        Blue,
        Green,
        White,
        Yellow,
        Orange
    }

    public class Mastermind
    {
        private readonly TextToPinColorConverter _textToPinColorConverter = new TextToPinColorConverter();
        private readonly IReadOnlyCollection<PinColor> _actual;

        public Mastermind(IReadOnlyCollection<PinColor> actual)
        {
            _actual = actual;
        }

        public Mastermind(IEnumerable<string> actual)
        {
            _actual = actual.Select(item => _textToPinColorConverter.Convert(item)).ToImmutableList();
        }

        public IEnumerable<PinResult> Check(IReadOnlyCollection<String> guess)
        {
            var guessAsPinColors = guess.Select(item => _textToPinColorConverter.Convert(item)).ToImmutableList();
            return Check(guessAsPinColors);
        }
        
        public IEnumerable<PinResult> Check(IReadOnlyCollection<PinColor> guess)
        {
            var unused = new List<bool> {true, true, true, true};

            var blackPins = CalculateBlackPins(guess, unused);
            var whitePins = CalclateWhitePins(guess, unused);

            return blackPins.Concat(whitePins);
        }

        private IEnumerable<PinResult> CalculateBlackPins(IReadOnlyCollection<PinColor> guess, List<bool> unused)
        {
            var blackPins = new List<PinResult>();
            for (var i = 0; i < _actual.Count(); i++)
            {
                if (guess.ElementAt(i) != _actual.ElementAt(i)) continue;
                blackPins.Add(PinResult.Black);
                unused[i] = false;
            }

            return blackPins;
        }

        private IEnumerable<PinResult> CalclateWhitePins(IReadOnlyCollection<PinColor> guess, List<bool> unused)
        {
            var remaining = GetMaskedPins(_actual, unused).ToList();

            var whitePins = new List<PinResult>();
            for (var i = 0; i < remaining.Count; i++)
            {
                if (!unused[i]) continue;
                var guessedPinColor = guess.ElementAt(i);
                if (remaining.DoesNotContain(guessedPinColor)) continue;
                
                whitePins.Add(PinResult.White);
                unused[i] = false;
            }

            return whitePins;
        }

        public IEnumerable<PinColor> GetMaskedPins(IReadOnlyCollection<PinColor> pins, IReadOnlyList<bool> mask)
        {
            var result = new List<PinColor>();
            for (var i = 0; i < mask.Count; i++)
            {
                if (mask[i])
                {
                    result.Add(pins.ElementAt(i));
                }
            }

            return result;
        }
    }

    public class MastermindResultTests
    {
        [Fact]
        public void ReturnCorrectUsageMask()
        {
            var computerPinColors = new[] {PinColor.Green, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var sut = new Mastermind(computerPinColors);
            List<bool> unused = new List<bool>() {true, true, true, true};
            IEnumerable<PinColor> result = sut.GetMaskedPins(computerPinColors, unused);
            var expected = new List<PinColor>() {PinColor.Green, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            Assert.Equal(result, expected);
        }

        [Fact]
        public void PassingOneCorrectColorInIncorrectPositionReturnsOneWhitePeg()
        {
            var expected = new[] {PinResult.White};
            var computerPinColors = new[] {PinColor.Green, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var userPinColors = new[] {PinColor.Red, PinColor.Green, PinColor.Red, PinColor.Red};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PassingTwoCorrectColorsReturnsTwoBlackPegs()
        {
            var expected = new[] {PinResult.Black, PinResult.Black};
            var computerPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var userPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Red, PinColor.Red};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PassingTwoCorrectColorsInCorrectPositionReturnsTwoBlackPegs()
        {
            var expected = new[] {PinResult.Black, PinResult.Black};
            var computerPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var userPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Red, PinColor.Red};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PassingOneCorrectColorsInCorrectPositionReturnsOneBlackPeg()
        {
            var expected = new[] {PinResult.Black};
            var computerPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var userPinColors = new[] {PinColor.Blue, PinColor.Red, PinColor.Red, PinColor.Red};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PassingNoCorrectColorsReturnsEmptyResults()
        {
            var expected = new PinResult[] { };
            var computerPinColors = new[] {PinColor.Blue, PinColor.Blue, PinColor.Blue, PinColor.Blue};
            var userPinColors = new[] {PinColor.Red, PinColor.Red, PinColor.Red, PinColor.Red};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PassingAllCorrectColorsInCorrectOrderReturnsAllBlack()
        {
            var expected = new[] {PinResult.Black, PinResult.Black, PinResult.Black, PinResult.Black};
            var computerPinColors = new[] {PinColor.Blue, PinColor.Green, PinColor.Red, PinColor.White};
            var userPinColors = new[] {PinColor.Blue, PinColor.Green, PinColor.Red, PinColor.White};
            var sut = new Mastermind(computerPinColors);
            var result = sut.Check(userPinColors);
            Assert.Equal(expected, result);
        }
    }

    public class TextToPinColorConverterTests
    {
        [Theory]
        [InlineData("Red", PinColor.Red)]
        [InlineData("red", PinColor.Red)]
        [InlineData("reD", PinColor.Red)]
        [InlineData("RED", PinColor.Red)]
        [InlineData("Blue", PinColor.Blue)]
        [InlineData("Orange", PinColor.Orange)]
        [InlineData("Green", PinColor.Green)]
        [InlineData("White", PinColor.White)]
        [InlineData("Yellow", PinColor.Yellow)]
        public void ConvertRedAsTextToRedAsPinColor(string textAsColor, PinColor expectedPinColor)
        {
           var sut = new TextToPinColorConverter();
           Assert.Equal(expectedPinColor, sut.Convert(textAsColor));
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("Black")]
        public void ThrowExceptionForInvalidColor(string textAsColor)
        {
            var sut = new TextToPinColorConverter();
            Assert.Throws<Exception>(() => sut.Convert(textAsColor));
        }
    }

    public class TextToPinColorConverter
    {
        public PinColor Convert(string colorAsText)
        {
            var normalizedText = colorAsText.ToLowerInvariant();
            switch (normalizedText)
            {
                case "red": return PinColor.Red;
                case "blue": return PinColor.Blue;
                case "orange": return PinColor.Orange;
                case "green": return PinColor.Green;
                case "white": return PinColor.White;
                case "yellow": return PinColor.Yellow;
                default: throw new Exception("Invalid color text");
            }
            
        }
    }

    public class Game
    {
        
    }


    public class MasterMindAcceptanceTests
    {
        [Fact]
        public void SolveScenario()
        {
            var computerSelection = new List<string>(){"Red", "Blue", "Green", "Yellow"};
            var sut = new Mastermind(computerSelection);

            Assert(sut, new List<string>{"Red", "Orange", "Yellow", "Orange"}, new[]{PinResult.Black, PinResult.White});
            Assert(sut, new List<string>{"Blue", "Red", "Yellow", "Green"}, new[]{PinResult.White, PinResult.White, PinResult.White, PinResult.White});
            Assert(sut, new List<string>{"Red", "Red", "Red", "Red"}, new[]{PinResult.White});
            Assert(sut, new List<string>{"White", "White", "White", "White"}, new PinResult[]{});
            Assert(sut, new List<string>{"Red", "Blue", "Green", "Yellow"}, new[]{PinResult.Black, PinResult.Black, PinResult.Black, PinResult.Black});
        }

        private static void Assert(Mastermind sut, IReadOnlyCollection<string> readOnlyCollection, IEnumerable<PinResult> pinResults)
        {
            var result1 = sut.Check(readOnlyCollection);
            Xunit.Assert.Equal(pinResults, result1);
        }
    }

    public enum PinResult
    {
        Black,
        White
    }
}