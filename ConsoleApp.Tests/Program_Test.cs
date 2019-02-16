using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ConsoleApp;
using Xunit;

namespace GhToGhx_TEST
{
    public static class Program_Test
    {
        // ============
        // TEST METHODS
        // ============

        [Fact]
        public static void ConsoleApp_Args_MemberAssigned()
        {
            // ARRANGE
            var consoleapp = new GhToGhxApp();            
            var args = new string[0]; // dummy value
            consoleapp.Args(ref args);
            // ACT
            var actual = consoleapp.Args();
            // ASSERT
            Assert.NotNull(actual);
        }

        [Fact]
        public static void ConsoleApp_Args_0_OutputValueMatchesInputValue()
        {
            // ARRANGE
            var consoleapp = new GhToGhxApp();
            var args = new string[1];
            args[0] = "D:\\DEV";
            consoleapp.Args(ref args);
            var expected = args[0];
            // ACT
            var actual = consoleapp.Args(0);
            // ASSERT
            Assert.Equal(expected, actual);
        }

        
        [Fact]
        public static void ConsoleApp_Args_NoParametersShouldTerminateImmediately()
        {
            // ARRANGE
            var consoleapp = new GhToGhxApp();
            var args = new string[0];
            var expected = false;
            // ACT - starting without any parameters should cause termination = false)
            var actual = consoleapp.RunAsCommandLine || consoleapp.RunAsConsoleWindow; // should be false
            // ASSERT
            Assert.Equal(expected, actual);
        }

        [Fact]
        public static void ConsoleApp_Args_DirectoryParameterOnlyShouldRunConsoleWindowMode()
        {
            // ARRANGE
            var expected = true;
            var consoleapp = new GhToGhxApp();
            var args = new string[1] { "C:\\Windows" };
            // ACT - starting with only a foldername as the first parameter should run 
            // interactively in a ConsoleWindow. Not checking for valid directory here
            var actual = consoleapp.Args(ref args) && consoleapp.RunAsConsoleWindow && consoleapp.ArgsCount== 1;
            // ASSERT
            Assert.True(expected && actual);
        }

        [Fact]
        public static void ConsoleApp_Args_DirectoryPlusOptionsParameterShouldRunCommandlineMode()
        {
            // ARRANGE
            var consoleapp = new GhToGhxApp();
            var args = new string[2] { "C:\\Windows", "+" }; // + = verbose, but never mind
            var expected = true;
            // ACT - starting with only a foldername as the first parameter should run 
            // interactively in a ConsoleWindow. Not checking for valid directory here
            var actual = consoleapp.Args(ref args) && consoleapp.RunAsCommandLine;
            // ASSERT
            Assert.True(expected && actual);
        }

    }
}
