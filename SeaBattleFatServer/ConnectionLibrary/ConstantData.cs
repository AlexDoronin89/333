using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionLibrary
{
    public class ConstantData
    {
        public static class ConnectionData
        {
            public const string Ip = "127.0.0.1";
            public const int Port = 55555;
        }

        public static class Commands
        {
            public const string GetField = "getfield";
            public const string GetOpponentField = "getopponentfield";
            public const string Step = "step";
            public const string EndStep = "endstep";
        }

        public static class ResponceResults
        {
            public const string Ok = "ok";
            public const string Error = "error";
        }

        public static class GameStates
        {
            public const string Go = "go";
            public const string Wait = "wait";
            public const string End = "end";
        }

        public static class PlayerChars
        {
            public const string Ship = "X";
            public const string First = "1";
            public const string HitFirst = "⓵";
            public const string Second = "2";
            public const string HitSecond = "⓶";
        }

        public static class GameStatus
        {
            public const string Play = "Игра продолжается";
            public const string Draw = "Ничья";
            public const string WinFirst = "Победил игрок " + PlayerChars.First;
            public const string WinSecond = "Победил игрок " + PlayerChars.Second;
        }
    }
}
